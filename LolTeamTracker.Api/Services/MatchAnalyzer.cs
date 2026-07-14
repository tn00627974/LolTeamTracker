using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Models;
using System.Text.Json;
using LolTeamTracker.Api.Repositories;

namespace LolTeamTracker.Api.Services
{
    public class MatchAnalyzer : IMatchAnalyzer
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly ITeamRepository _teamRepository;

        public MatchAnalyzer( IRiotApiClient riotApiClient, ITeamRepository teamRepository)
        {
            _riotApiClient = riotApiClient;
            _teamRepository = teamRepository;
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
            var data = await _riotApiClient.GetMatchSummaryAsync(matchId);

            // TODO : refactor : 換model裝 GetProperty ..
            var info = data.GetProperty("info");
            var participant = info.GetProperty("participants")
                                  .EnumerateArray()
                                  .FirstOrDefault(p => p.GetProperty("puuid").GetString() == puuid);
            
            if (participant.ValueKind == JsonValueKind.Undefined) // 如果找不到對應的參與者
                return null;

            var champion = participant.GetProperty("championName").GetString();
            var kills = participant.GetProperty("kills").GetInt32();
            var deaths = participant.GetProperty("deaths").GetInt32();
            var assists = participant.GetProperty("assists").GetInt32();
            var win = participant.GetProperty("win").GetBoolean();
            string teamPosition = participant.GetProperty("teamPosition").GetString();
            string laneName = GetLaneName(teamPosition);

            // 處理UTC國際時間格式 => 轉為台灣時間 24小時格式
            var dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(info.GetProperty("gameStartTimestamp").GetInt64()).UtcDateTime;
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
        public async Task<List<MatchSummary>> GetMatchSummariesTeamsAsync()
        {
            var players = await _teamRepository.LoadTeamFromDataAsync();
            var allResults = new List<MatchSummary>();

            // 要先用 riotApiClient 取得成員的 puuid 
            foreach (var player in players)
            {
                // 取得玩家的 puuid
                var summaries = await GetMatchSummariesPlayerAsync(player.gameName, player.tagLine ,10);
                if (summaries != null)
                    allResults.AddRange(summaries); // 將每個玩家的比賽列表加入到總結果中

                #region Old 
                //var puuid = await _riotApiClient.GetPuuidAsync(player.gameName, player.tagLine);
                //if (string.IsNullOrEmpty(puuid))
                //{
                //    Console.WriteLine($"無法取得 {player.gameName} 的 puuid");
                //    continue;
                //}
                //// 取得玩家的比賽列表
                //var matchIds = await _riotApiClient.GetMatchIdsAsync(puuid, 0, 10);
                //foreach (var matchId in matchIds)
                //{
                //    // 分析每場比賽
                //    var summary = await _matchAnalyzer.GetMatchSummaryAsync(matchId, puuid, player.gameName, player.tagLine);
                //    if (summary != null)
                //    {
                //        // 在這裡處理比賽摘要，例如儲存到資料庫或進行其他分析
                //        Console.WriteLine($"分析比賽 {matchId} 成功");
                //        allResults.Add(summary);
                //    }
                //}
                #endregion
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
        public async Task<List<MatchSummary>> GetMatchSummariesPlayerAsync(string gameName, string tagLine, int count)
        {
            var result = new List<MatchSummary>();
            var puuid = await _riotApiClient.GetPuuidAsync(gameName, tagLine);
            var matchIds = await _riotApiClient.GetMatchIdsAsync(puuid, 0, count);

            foreach (var matchId in matchIds)
            {
                try
                {
                    var summary = await GetMatchSummaryAsync(matchId, puuid, gameName, tagLine);
                    if (summary != null)
                        result.Add(summary);
                }
                catch (Exception ex)
                {
                    // log or skip ?
                }
            }

            return result;
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
            switch (queueId)
            {
                case 400: return "Normal Draft Pick（一般選角）";
                case 420: return "單雙積分 Solo/Duo Ranked";
                case 430: return "Normal Blind Pick（一般盲選）";
                case 440: return "彈性積分 Flex Ranked";
                case 450: return "大亂鬥 ARAM";
                case 480: return "一般(超速衝點)";
                case 750: return "Clash 盃";
                default: return $"未知模式 (QueueId={queueId})";
            }
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
                //case "MID":
                    return "中路";
                case "BOTTOM":
                //case "BOT":
                    return "下路";
                case "UTILITY":
                //case "SUPPORT":
                    return "輔助";
                default:
                    return $"未知路線 ({teamPosition})";
            }
        }
    }
}
