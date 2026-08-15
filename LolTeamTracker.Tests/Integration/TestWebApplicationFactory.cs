using LolTeamTracker.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LolTeamTracker.Tests.Integration
{
    /// <summary>
    /// 接真的 SQL Server——沿用 docker-compose 起的那個容器，但指向獨立的測試資料庫，
    /// 不會動到本機開發用的 LolTeamTracker 資料庫。
    /// </summary>
    /// <remarks>
    /// 密碼不寫死在程式碼裡，也不需要手動設環境變數——
    /// 直接沿用 docker compose 本來就在用的 .env（已列入 .gitignore，不進版控）。
    /// 執行前只要：
    ///   docker compose up -d mssql
    ///   dotnet test
    /// 連線字串的來源順序見 <see cref="ConnectionString"/>。
    /// </remarks>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        /// <summary>
        /// 測試資料庫連線字串。兩個來源，依序嘗試：
        ///   ① 環境變數 LOLTEAMTRACKER_TEST_DB —— CI 用（GitHub Secrets 注入）
        ///   ② 專案根目錄的 .env —— 本機開發用，密碼跟 docker compose 共用同一份
        /// 兩個都沒有才丟例外。
        /// </summary>
        /// <remarks>
        /// 為什麼要有 ②：密碼只該存在一個地方。若本機也走環境變數，密碼就會同時
        /// 存在 .env 和（登錄檔／runsettings／各人的終端機）裡，改密碼時必然漏掉一處，
        /// 出現「CLI 跑得過、VS 跑不過」這種難查的狀況。
        /// 為什麼 ① 優先：CI 上沒有 .env，且環境變數要能覆寫本機設定。
        /// </remarks>
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("LOLTEAMTRACKER_TEST_DB")
            ?? BuildConnectionStringFromDotEnv()
            ?? throw new InvalidOperationException(
                "找不到測試資料庫連線設定。請確認專案根目錄的 .env 已填入 MSSQL_SA_PASSWORD，"
                + "或設定環境變數 LOLTEAMTRACKER_TEST_DB。");

        /// <summary>
        /// 從專案根目錄的 .env 讀出 SA 密碼，組成測試資料庫連線字串。找不到就回 null。
        /// </summary>
        private static string? BuildConnectionStringFromDotEnv()
        {
            // 測試 dll 在 bin/Debug/net8.0/ 底下，.env 在方案根目錄。
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, ".env")))
            {
                dir = dir.Parent;
            }
            if (dir is null) return null;

            const string key = "MSSQL_SA_PASSWORD=";
            var line = File.ReadAllLines(Path.Combine(dir.FullName, ".env"))
                           .FirstOrDefault(l => l.StartsWith(key, StringComparison.Ordinal));
            if (line is null) return null;

            var password = line[key.Length..].Trim();
            if (string.IsNullOrEmpty(password)) return null;

            // 資料庫名稱刻意跟開發用的 LolTeamTracker 分開——
            // TearDown 會清空整張 Players 表，指錯資料庫會把開發資料刪光。
            return $"Server=localhost,1434;Database=LolTeamTracker_Test;"
                 + $"User Id=sa;Password={password};TrustServerCertificate=True";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 把 Program.cs 裡註冊的 DbContextOptions<AppDbContext> 換掉，
                // 改指向測試專用連線字串，而不是 appsettings／User Secrets 裡本機開發那組。
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(ConnectionString));
            });
        }

        /// <summary>
        /// 確保測試資料庫存在且結構最新（套用所有 Migration）。
        /// 整個測試檔案第一次跑之前呼叫一次即可，不用每個測試都呼叫。
        /// </summary>
        public void EnsureDatabaseReady()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
    }
}
