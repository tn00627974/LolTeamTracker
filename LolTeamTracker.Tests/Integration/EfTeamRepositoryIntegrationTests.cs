using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LolTeamTracker.Tests.Integration
{
    /// <summary>
    /// 接真的 SQL Server，驗證 Repository 層跟資料庫實際互動的行為。
    /// 跟 MatchAnalyzerTests 用 Moq 隔離依賴的目的不同——這裡就是要碰真的 DB，
    /// 驗證 Mock 測不到的東西：EF Core 產生的 SQL 對不對、資料庫約束擋不擋得住。
    /// </summary>
    [TestFixture]
    public class EfTeamRepositoryIntegrationTests
    {
        private TestWebApplicationFactory _factory = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new TestWebApplicationFactory();
            _factory.EnsureDatabaseReady();
        }

        [TearDown]
        public async Task TearDown()
        {
            // 每個測試跑完清空 Players 表，讓測試之間互不影響（不依賴交易復原，
            // 因為併發測試需要真的多條連線各自送出 SaveChangesAsync）。
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Players.RemoveRange(db.Players);
            await db.SaveChangesAsync();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => _factory.Dispose();
        // 驗證 Puuid 唯一索引真的擋得住併發寫入。
        //
        // 這是你在 README 裡寫過的宣稱：「應用層的檢查是效能優化，資料庫約束才是正確性保證」。
        // 這個測試就是要證明那句話——不是紙上談兵。
        //
        // 想法起點（不是答案，是引導）：
        // 1. UpsertPlayerAsync 的邏輯是「先查 puuid 存不存在，不存在才新增」。
        //    如果兩個呼叫「同時」發生，資料庫在兩邊查詢的當下都還是空的，
        //    兩邊都會判斷「不存在」，都會走到新增那條路——這就是競爭條件。
        // 2. 關鍵陷阱：這裡的「同時」必須是兩個獨立的 DbContext（等同兩個真的 HTTP 請求各自
        //    拿到自己的 scope），不能共用同一個 scope 解析出來的 ITeamRepository 去跑兩次——
        //    EF Core 的 DbContext 不是 thread-safe，共用一個實例做併發呼叫，
        //    你測到的會是「DbContext 被同時使用」的錯誤，不是你想驗證的唯一索引競爭。
        //    用 _factory.Services.CreateScope() 各開一個 scope、各自解析一個 ITeamRepository。
        // 3. 用 Task.WhenAll 讓兩個 UpsertPlayerAsync 真的同時送出去，不要 await 分開跑
        //    （分開 await 等於序列執行，兩次查詢就不會撞在一起，測不到問題）。
        // 4. 猜猜看：兩個呼叫都會成功，還是一個成功一個丟例外？丟的話會是什麼型別的例外？
        //    （提示：唯一索引違反在 SQL Server 是哪種例外，EF Core 會包成什麼？）
        // 5. 最後查一下 Players 表：應該只有一筆資料，不是兩筆。

        [Test]
        public async Task UpsertPlayerAsync_ConcurrentInsertsWithSamePuuid_OnlyOneSucceeds()
        {
            // ── Arrange ──
            const string puuid = "test-puuid-001";

            // 兩個獨立 scope = 兩個獨立 DbContext，等同兩個真的 HTTP 請求
            using var scopeA = _factory.Services.CreateScope(); 
            using var scopeB = _factory.Services.CreateScope();
            var repositoryA = scopeA.ServiceProvider.GetRequiredService<ITeamRepository>();
            var repositoryB = scopeB.ServiceProvider.GetRequiredService<ITeamRepository>();

            // ── Act ──
            // 先「啟動」兩個 Task，最後才 await——這樣兩次查詢才會真的重疊。
            // 若寫成 await A; await B; 就是序列執行，B 查詢時 A 早就寫完了，撞不在一起。
            var taskA = repositoryA.UpsertPlayerAsync(puuid, "GameNameA", "TagLineA");
            var taskB = repositoryB.UpsertPlayerAsync(puuid, "GameNameB", "TagLineB");

            // 預期它爆炸，而不是讓例外把測試打成失敗。
            //
            // 為什麼是 DbUpdateException（不是 ArgumentException，也不是 SqlException）：
            //   SQL Server 因唯一索引違反丟出 SqlException
            //     → EF Core 在 SaveChangesAsync 攔下來，包成 DbUpdateException 再往外丟
            //   EF Core 統一用 DbUpdateException 表達「寫入資料庫失敗」，
            //   讓呼叫端不必依賴特定資料庫廠商的例外型別（換成 PostgreSQL 也是這個型別）。
            var ex = Assert.ThrowsAsync<DbUpdateException>(
                async () => await Task.WhenAll(taskA, taskB));

            // 再確認一層：DbUpdateException 只說「寫入失敗」，沒說「為什麼失敗」。
            // 內層的 SqlException 才帶錯誤碼——2601/2627 就是唯一索引／唯一約束違反。
            // 不檢查這層的話，這個測試連「外鍵錯誤」「欄位太長」都會判定通過。
            var sqlEx = ex!.InnerException as SqlException;
            Assert.That(sqlEx, Is.Not.Null, "應該是資料庫層拒絕，而不是其他寫入錯誤");
            Assert.That(sqlEx!.Number, Is.AnyOf(2601, 2627), "應該是唯一索引/唯一約束違反");

            // ── Assert ──
            // 開第三個乾淨的 scope 來查證。
            // 為什麼不能用 scopeA/scopeB：它們的 DbContext 有變更追蹤（change tracking），
            // 其中一個還記著那筆「以為新增成功」的實體。用它查會看到記憶體裡的樣子，
            // 不是資料庫真正的樣子。
            using var scopeVerify = _factory.Services.CreateScope();
            var db = scopeVerify.ServiceProvider.GetRequiredService<AppDbContext>();

            var players = await db.Players
                .Where(p => p.Puuid == puuid)
                .ToListAsync();

            // 核心斷言：資料庫只讓一筆進去。
            // 應用層的 if (player == null) 兩邊都判斷「不存在」，是唯一索引擋下第二筆的——
            // 這就是「應用層檢查是效能優化，資料庫約束才是正確性保證」的證據。
            Assert.That(players, Has.Count.EqualTo(1));

            // GameName 是 A 還是 B？不確定——誰先搶到就是誰，這正是競爭條件的本質。
            // 寫死任何一個都會讓測試時好時壞，斷言「是其中之一」才是誠實的。
            Assert.That(players[0].GameName, Is.AnyOf("GameNameA", "GameNameB"));
        }
    }
}
