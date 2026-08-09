using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Repositories;
using LolTeamTracker.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework.Internal;
using System.Text.Json;

namespace LolTeamTracker.Tests.Services
{
    [TestFixture]
    public class MatchAnalyzerTests
    {
        /// <summary>
        /// 一場比賽的 Riot API 回應（精簡版，只保留 MatchAnalyzer 會讀的欄位）。
        /// </summary>
        private const string SampleMatchJson = """
        {
          "info": {
            "gameStartTimestamp": 1722931200000,
            "queueId": 420,
            "participants": [
              {
                "puuid": "TEST_PUUID_001",
                "championName": "Ahri",
                "kills": 10,
                "deaths": 2,
                "assists": 8,
                "win": true,
                "teamPosition": "MIDDLE",
                "totalMinionsKilled": 180,
                "neutralMinionsKilled": 20,
                "goldEarned": 15000
              }
            ]
          }
        }
        """;

        /// <summary>
        /// 建立受測物件。沒傳入的依賴一律給空替身——測試不該為了「編譯得過」
        /// 而去設定它根本不關心的東西。
        /// </summary>
        private static MatchAnalyzer CreateSut(
            Mock<IRiotApiClient> riotApiClient,
            Mock<ITeamRepository>? teamRepository = null)
        {
            return new MatchAnalyzer(
                riotApiClient.Object,
                teamRepository?.Object ?? Mock.Of<ITeamRepository>(),
                NullLogger<MatchAnalyzer>.Instance);   // 日誌不影響斷言，用官方的空實作
        }

        /// <summary>
        /// 當 puuid 對應得到 participants 中的玩家時，應正確解析 KDA、英雄與補兵數。
        /// 其中 TotalCS 是「算」出來的（laneCS + jungleCS），不是直接讀欄位。
        /// </summary>
        [Test]
        public async Task GetMatchSummaryAsync_WithMatchingPuuid_ParsesKdaAndCs()
        {
            // ── Arrange ──
            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            // 回傳 matchData 固定資料
            mockRiotApiClient
                .Setup(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData);

            var sut = CreateSut(mockRiotApiClient);   // sut = System Under Test，受測物件

            // ── Act ──
            var result = await sut.GetMatchSummaryAsync(
                matchId: "TW2_123456",     // Any
                puuid: "TEST_PUUID_001",   // 對應上 SampleMatchJson 裡的 participants
                gameName: "測試玩家",  // Any
                tagLine: "tw2");  // Any

            // ── Assert ──
            Assert.That(result, Is.Not.Null);

            // Assert.Multiple：全部檢查後輸出一份診斷完整報告
            Assert.Multiple(() =>
            {
                Assert.That(result!.Kills, Is.EqualTo(10));
                Assert.That(result.Deaths, Is.EqualTo(2));
                Assert.That(result.Assists, Is.EqualTo(8));
                Assert.That(result.Win, Is.True);
                Assert.That(result.Champion, Is.EqualTo("Ahri"));

                // 這三個才是真正的「邏輯」——TotalCS 是算出來的，不是讀出來的
                Assert.That(result.LaneCS, Is.EqualTo(180));
                Assert.That(result.JungleCS, Is.EqualTo(20));
                Assert.That(result.TotalCS, Is.EqualTo(200), "TotalCS 應為 laneCS + jungleCS");
            });
        }


        /// <summary>
        /// 應把 Riot 回傳的 Unix 毫秒時間戳轉成台灣時間字串。
        /// gameStartTimestamp = 1722931200000 → UTC 2024/08/06 08:00:00 → 台灣 (UTC+8) 16:00:00。
        /// 期望值刻意寫成人工算好的常數，不在測試裡重算——重算等於拿被測邏輯去驗自己。
        /// </summary>
        [Test]
        public async Task GetMatchSummaryAsync_WithUtcTimestamp_ConvertsToTaipeiTime()
        {
            // ── Arrange ──
            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            // 回傳 matchData 固定資料
            mockRiotApiClient
                .Setup(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData);

            var sut = CreateSut(mockRiotApiClient);   // sut = System Under Test，受測物件

            // ── Act ──
            var result = await sut.GetMatchSummaryAsync(
                matchId: "TW2_123456",     // Any
                puuid: "TEST_PUUID_001",   // 對應上 SampleMatchJson 裡的 participants
                gameName: "測試玩家",  // Any
                tagLine: "tw2");  // Any

            // ── Assert ──
            Assert.That(result!.GameDate, Is.EqualTo("2024/08/06 16:00:00"));
        }

        /// <summary>
        /// 當 puuid 在這場比賽的 participants 裡找不到時，應回傳 null。
        /// </summary>
        [Test]
        public async Task GetMatchSummaryAsync_WhenPuuidNotInMatch_ReturnsNull()
        {
            // ── Arrange ──
            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            // 回傳 matchData 固定資料
            mockRiotApiClient
                .Setup(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData);

            var sut = CreateSut(mockRiotApiClient);   // sut = System Under Test，受測物件

            // ── Act ──
            var result = await sut.GetMatchSummaryAsync(
                matchId: "TW2_123456",       // Any
                puuid: "NOT_IN_THIS_MATCH",  // 刻意給一個 SampleMatchJson 裡沒有的 puuid
                gameName: "測試玩家",  // Any
                tagLine: "tw2");  // Any

            // ── Assert ──
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// teamPosition 應轉成中文路線名稱。
        /// 用 [TestCase] 參數化：同一段邏輯、多組資料，NUnit 會展開成多個獨立測試，
        /// 失敗時能精準指出是哪一組資料壞掉——比在同一個方法裡寫五次斷言好得多。
        /// </summary>
        [TestCase("TOP", "上路")]
        [TestCase("JUNGLE", "打野")]
        [TestCase("MIDDLE", "中路")]
        [TestCase("BOTTOM", "下路")]
        [TestCase("UTILITY", "輔助")]
        [TestCase("TEST", "未知路線:TEST")]   // Riot 給了值但對照表沒有 → 原樣顯示代號，提示對照表該補
        [TestCase("", "無路線")]              // Riot 沒給值（大亂鬥無路線概念）→ 正常情況，不是錯誤
        public void GetLaneName_WithGivenPosition_ReturnsChineseName(string teamPosition, string expected)
        {
            // 這個方法是純函式，不碰任何依賴，所以替身完全不用設定行為
            var sut = CreateSut(new Mock<IRiotApiClient>());

            var result = sut.GetLaneName(teamPosition);

            Assert.That(result, Is.EqualTo(expected));
        }

        /// <summary>
        /// 查詢玩家多場戰績時，若其中一場失敗，不應中斷整批查詢。
        /// 這是金融/批次系統最在意的性質：一筆壞資料不能拖垮整個作業。
        /// </summary>
        [Test]
        public async Task GetMatchSummariesPlayerAsync_WhenOneMatchThrows_ShouldReturnRemainingMatches()
        {
            // ── Arrange ──
            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            mockRiotApiClient
                .Setup(x => x.GetPuuidAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("TEST_PUUID_001");

            mockRiotApiClient
                .Setup(x => x.GetMatchIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "M1", "M2", "M3" });

            // SetupSequence：同一個方法，第 1/2/3 次呼叫回傳不同結果。
            // 一般的 Setup 是「每次都回同一個東西」，測不出「第二場壞掉」這種情境。
            mockRiotApiClient
                .SetupSequence(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData)                                        // M1 成功
                .ThrowsAsync(new HttpRequestException("模擬 Riot API 暫時失敗")) // M2 失敗
                .ReturnsAsync(matchData);                                       // M3 成功

            var sut = CreateSut(mockRiotApiClient);

            // ── Act ──
            var result = await sut.GetMatchSummariesPlayerAsync("測試玩家", "tw2", 3);

            // ── Assert ──
            // 三場中壞一場，仍應拿回另外兩場——失敗的那場由 catch 記錄後計入 failedCount，
            // 不向上拋出，避免單一場次的暫時性失敗導致整批查詢無結果。
            Assert.That(result.MatchSummaryList, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// 查詢 N 場戰績時，puuid 應該只查一次，不該每場都重查。
        /// Verify 驗證的是「互動」（有沒有呼叫、呼叫幾次），
        /// 前面幾個測試驗證的是「狀態」（回傳值對不對）——這是兩種不同的斷言。
        /// </summary>
        [Test]
        public async Task GetMatchSummariesPlayerAsync_ShouldQueryPuuidOnlyOnce()
        {
            // ── Arrange ──
            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            mockRiotApiClient
                .Setup(x => x.GetPuuidAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("TEST_PUUID_001");

            mockRiotApiClient
                .Setup(x => x.GetMatchIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "M1", "M2", "M3" });

            mockRiotApiClient
                .Setup(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData);

            var sut = CreateSut(mockRiotApiClient);

            // ── Act ──
            await sut.GetMatchSummariesPlayerAsync("測試玩家", "tw2", 3);

            // ── Assert ──
            // 不論查幾場，puuid 只該查一次。這個性質不體現在回傳值上——
            // 查一次或查三次，result 完全一樣，只有互動驗證抓得到。
            // 若日後有人把查詢搬進 foreach，這個測試會立即失敗。
            mockRiotApiClient.Verify(
                x => x.GetPuuidAsync("測試玩家", "tw2"),
                Times.Once);
        }

        /// <summary>
        /// 全隊分析應把每位成員的戰績彙整成一份清單。
        /// 這個測試同時假造了 IRiotApiClient 與 ITeamRepository——
        /// 意即整條流程可以完全不碰資料庫、不碰 Riot API 就驗證完畢。
        /// </summary>
        [Test]
        public async Task GetMatchSummariesTeamsAsync_ShouldAggregateAllMembers()
        {
            // ── Arrange ──
            var mockTeamRepository = new Mock<ITeamRepository>();
            mockTeamRepository
                .Setup(x => x.LoadTeamFromDataAsync())
                .ReturnsAsync(new List<PlayerInfo>
                {
                    new() { GameName = "隊員一", TagLine = "tw2" },
                    new() { GameName = "隊員二", TagLine = "tw2" }
                });

            var mockRiotApiClient = new Mock<IRiotApiClient>();
            var matchData = JsonDocument.Parse(SampleMatchJson).RootElement;

            mockRiotApiClient
                .Setup(x => x.GetPuuidAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("TEST_PUUID_001");

            // 每位成員各兩場
            mockRiotApiClient
                .Setup(x => x.GetMatchIdsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "M1", "M2" });

            mockRiotApiClient
                .Setup(x => x.GetMatchSummaryAsync(It.IsAny<string>()))
                .ReturnsAsync(matchData);

            var sut = CreateSut(mockRiotApiClient, mockTeamRepository);

            // ── Act ──
            var result = await sut.GetMatchSummariesTeamsAsync();

            // ── Assert ──
            // 2 位成員 × 每人 2 場 = 4 筆。期望值由測試資料推算而來，
            // 不從 result 本身取值——否則又成了自我實現的測試。
            Assert.That(result.MatchSummaryList, Has.Count.EqualTo(4));
        }
    }
}
