using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Models.Results;
using LolTeamTracker.Api.Repositories;
using System.Text.Json;

namespace LolTeamTracker.Api.Services
{
    public class MatchAnalyzer : IMatchAnalyzer
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<MatchAnalyzer> _logger;
        private readonly IEfQueueDefinitionRepository _queueDefinitionRepository;

        /// <summary>
        /// 遊戲模式對照表（queueId → 名稱），整個請求期間只從資料庫載入一次。
        /// </summary>
        /// <remarks>
        /// </remarks>
        private Dictionary<int, string>? _queueNames;

        public MatchAnalyzer(
            IRiotApiClient riotApiClient,
            ITeamRepository teamRepository,
            IEfQueueDefinitionRepository queueDefinitionRepository,
            ILogger<MatchAnalyzer> logger)
        {
            _riotApiClient = riotApiClient;
            _teamRepository = teamRepository;
            _queueDefinitionRepository = queueDefinitionRepository;
            _logger = logger;
        }

        /// <summary>
        /// 確保模式對照表已載入。已載入過就直接返回，不會重複查詢。
        /// </summary>
        private async Task EnsureQueueNamesLoadedAsync()
        {
            _queueNames ??= (await _queueDefinitionRepository.LoadQueueDefinitionDataAsync())
                            .ToDictionary(q => q.Id, q => q.Name);
        }

        /// <summary>
        /// 用 matchId 查比賽列表細節
        /// </summary>
        /// <param name="matchId">遊戲場次編號</param>
        /// <param name="puuid"></param>
        /// <param name="gameName">遊戲名稱</param>
        /// <param name="tagLine">#標籤</param>
        /// <returns></returns>
        public async Task<MatchSummary?> GetMatchSummaryAsync(string matchId, string puuid, string gameName, string tagLine)
        {
            // 模式對照表只在第一次進來時載入，後續呼叫直接用記憶體裡的字典。
            await EnsureQueueNamesLoadedAsync();

            var data = await _riotApiClient.GetMatchSummaryAsync(matchId);

            // TODO : refactor : 換model裝 GetProperty ..
            var info = data.GetProperty("info");
            var participant = info.GetProperty("participants")
                                  .EnumerateArray()
                                  .FirstOrDefault(p => p.GetProperty("puuid").GetString() == puuid);

           
            if (participant.ValueKind == JsonValueKind.Undefined) // 如果找不到對應的參與者
                return null;

            var champion = participant.GetProperty("championName").GetString() ?? "";
            var kills = participant.GetProperty("kills").GetInt32();
            var deaths = participant.GetProperty("deaths").GetInt32();
            var assists = participant.GetProperty("assists").GetInt32();
            var win = participant.GetProperty("win").GetBoolean();
            string teamPosition = participant.GetProperty("teamPosition").GetString() ?? "未知";
            string laneName = GetLaneName(teamPosition);

            // 處理UTC國際時間格式 => 轉為台灣時間 24小時格式
            var gameStartTimestamp = info.GetProperty("gameStartTimestamp").GetInt64();
            var dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(gameStartTimestamp).UtcDateTime;
            var taiwanZone = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
            var taiwanTime = taiwanZone.ToString("yyyy/MM/dd HH:mm:ss");

            // 遊戲模式
            int queueId = info.GetProperty("queueId").GetInt32(); // 遊戲模式 queueId
            var queueName = GetQueueTypeName(queueId);


            // 擊殺小兵數量
            var laneCS = participant.GetProperty("totalMinionsKilled").GetInt32();
            var jungleCS = participant.GetProperty("neutralMinionsKilled").GetInt32();
            var totalCS = laneCS + jungleCS;

            // 金錢
            int gold = participant.GetProperty("goldEarned").GetInt32();

            return new MatchSummary
            {
                GameName = gameName,
                TagLine = tagLine,
                Champion = champion,
                Kills = kills,
                Deaths = deaths,
                Assists = assists,
                Win = win,
                LaneName = laneName,
                GameDate = taiwanTime,
                GameModeId = queueId,
                GameMode = queueName,
                LaneCS = laneCS,
                JungleCS = jungleCS,
                TotalCS = totalCS,
                Gold = gold
            };
        }

        /// <summary>
        /// 載入 JSON 檔案中的隊伍成員資料，並取得每位成員的比賽摘要列表
        /// </summary>
        /// <returns></returns>
        public async Task<MatchSummaryResult> GetMatchSummariesTeamsAsync()
        {
            var players = await _teamRepository.LoadTeamFromDataAsync();
            var allResults = new MatchSummaryResult();

            // 要先用 riotApiClient 取得成員的 puuid 
            foreach (var player in players) 
            {
                // 取得玩家的 puuid
                var summaries = await GetMatchSummariesPlayerAsync(player.GameName, player.TagLine, 10);
                allResults.MatchSummaryList.AddRange(summaries.MatchSummaryList);
                allResults.FailedCount += summaries.FailedCount;
            }
            return allResults;
        }

        /// <summary>
        /// 邏輯化 : 取得指定玩家的比賽列表
        /// </summary>
        /// <param name="gameName">遊戲名稱</param>
        /// <param name="tagLine">#標籤</param>
        /// <param name="count">搜尋場次</param>
        /// <returns></returns>
        public async Task<MatchSummaryResult> GetMatchSummariesPlayerAsync(string gameName, string tagLine, int count)
        {
            var results = new MatchSummaryResult();
            var puuid = await _riotApiClient.GetPuuidAsync(gameName, tagLine);
            var matchIds = await _riotApiClient.GetMatchIdsAsync(puuid, 0, count);

            foreach (var matchId in matchIds)
            {
                try
                {
                    var summary = await GetMatchSummaryAsync(matchId, puuid, gameName, tagLine);
                    if (summary != null)
                    {
                        results.MatchSummaryList.Add(summary);
                    }
                    else
                    {
                        results.FailedCount++;
                    }
                }
                catch (Exception ex)
                {
                    results.FailedCount++; // 當發生ex追蹤錯誤次數。
                    _logger.LogError(ex, "查詢比賽失敗。Player: {GameName}#{TagLine}, MatchId: {MatchId}", gameName, tagLine, matchId);
                }                
            }
            if (results.FailedCount > 0)
            {
                _logger.LogWarning("Player: {GameName}#{TagLine} 查詢完成，成功 {SuccessCount} 場，失敗 {FailedCount} 場",gameName, tagLine, results.SuccessCount, results.FailedCount);
            }

            return results;
        }

        /*         
         ---------------------- Helpers ----------------------         
         */

        /// <summary>
        /// 查詢遊戲模式
        /// </summary>
        /// <param name="queueId">模式編號</param>
        /// <returns>返回模式名稱</returns>
        public string GetQueueTypeName(int queueId)
        {
            // Riot 新增模式時只要 INSERT 一列，不必改程式碼重新部署。
            if (_queueNames is null)
            {
                // 呼叫端沒先載入就用——回退到「未知」而不是丟例外，
                // 與查無此 queueId 的處理一致：對照表缺漏不該讓整場戰績查詢失敗。
                return $"未知模式 (QueueId={queueId})";
            }

            return _queueNames.TryGetValue(queueId, out var name)
                ? name
                : $"未知模式 (QueueId={queueId})";
        }

        /// <summary>
        /// 玩家選擇路線
        /// </summary>
        /// <param name="teamPosition"></param>
        /// <returns>返回玩家路線</returns>
        public string GetLaneName(string teamPosition)
        {
            switch (teamPosition)
            {
                case "TOP":
                    return "上路";
                case "JUNGLE":
                    return "打野";
                case "MIDDLE":
                    return "中路";
                case "BOTTOM":
                    return "下路";
                case "UTILITY":
                    return "輔助";
                case "":
                    return "無路線"; // ARAM
                default:
                    return $"未知路線:{teamPosition}";
            }
        }
    }
}
