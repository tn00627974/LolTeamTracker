using System.Text.Json;

namespace LolTeamTracker.Api.Clients
{
    public class RiotApiClient : IRiotApiClient
    {
        private readonly HttpClient _accountClient;
        private readonly HttpClient _matchClient;

        public RiotApiClient(IHttpClientFactory httpFactory)
        {
            _accountClient = httpFactory.CreateClient("Account");
            _matchClient = httpFactory.CreateClient("Match");
        }

        /*         
         ---------------------- Riot ----------------------         
         */

        /// <summary>
        /// 用遊戲名稱 {GameName} 和標籤 {TagLine} 查詢 puuid
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="tagLine"></param>
        /// <returns></returns>
        public async Task<string> GetPuuidAsync(string gameName, string tagLine)
        {
            var url = $"riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
            var response = await _accountClient.GetFromJsonAsync<JsonElement>(url);
            var puuid = response.GetProperty("puuid").GetString();
            return puuid ?? "";
        }

        /// <summary>
        /// 用 puuid 查遊戲名稱 (GameName) 和標籤 (TagLine)
        /// </summary>
        /// <param name="puuid">puuid</param>
        /// <returns></returns>
        public async Task<string> GetGameNameAsync(string puuid)
        {
            var url = $"riot/account/v1/accounts/by-puuid/{puuid}";
            var response = await _accountClient.GetFromJsonAsync<JsonElement>(url);
            var gameName = response.GetProperty("gameName");
            var tagLine = response.GetProperty("tagLine");
            return $"{gameName}#{tagLine}";
        }

        /// <summary>
        /// 查詢單場詳細資訊
        /// </summary>
        /// <param name="matchId">遊戲場次編號</param>
        /// <returns></returns>
        public async Task<JsonElement> GetMatchSummaryAsync(string matchId)
        {
            var url = $"lol/match/v5/matches/{matchId}";
            var response = await _matchClient.GetFromJsonAsync<JsonElement>(url);
            return response;
        }


        /// <summary>
        /// 查詢單場詳細資訊 (含時間軸)
        /// </summary>
        /// <param name="matchId">遊戲場次編號</param>
        /// <returns></returns>
        public async Task<JsonElement> GetMatchSummaryTimeLineAsync(string matchId)
        {
            var url = $"lol/match/v5/matches/{matchId}/timeline";
            var response = await _matchClient.GetFromJsonAsync<JsonElement>(url);

            return response;
        }

        /// <summary>
        /// 用 puuid 查比賽列表 count預設為10,最多100上限 ( API限制 )
        /// </summary>
        /// <param name="puuid"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<string>> GetMatchIdsAsync(string puuid,int start,int count) 
        {
            var url = $"lol/match/v5/matches/by-puuid/{puuid}/ids?start={start}&count={count}";
            var response = await _matchClient.GetFromJsonAsync<List<string>>(url);
            return response ?? [];
        }
    }
}
