using LolTeamTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LolTeamTracker.Api.Services
{
    public class RiotApiService 
    {
        private readonly HttpClient _accountClient;
        private readonly HttpClient _matchClient;

        public RiotApiService(IHttpClientFactory httpFactory)
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
            try
            {
                var response = await _accountClient.GetFromJsonAsync<JsonElement>($"riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}");
                var puuid = response.GetProperty("puuid").GetString();
                return puuid ?? "";
            }
            catch (HttpRequestException ex)
            {
                return $"API Error : {ex.StatusCode} - {ex.Message}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 用 puuid 查遊戲名稱 (GameName) 和標籤 (TagLine)
        /// </summary>
        /// <param name="puuid">puuid</param>
        /// <returns></returns>
        public async Task<string> GetGameNameAsync(string puuid)
        {
            try
            {
                var response = await _accountClient.GetFromJsonAsync<JsonElement>($"riot/account/v1/accounts/by-puuid/{puuid}");
                var gameName = response.GetProperty("gameName").GetString();
                var tagLine = response.GetProperty("tagLine").GetString();
                return $"{gameName}#{tagLine}";
            }
            //catch (HttpRequestException ex)
            //{
            //    return $"API Error : {ex.StatusCode} - {ex.Message}"; // TODO 要修正 
            //}
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 查詢單場詳細資訊
        /// </summary>
        /// <param name="matchId">遊戲場次編號</param>
        /// <returns></returns>
        public async Task<string> GetMatchSummary(string matchId)
        {
            var url = $"lol/match/v5/matches/{matchId}";
            var response = await _matchClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return json;
        }


        /// <summary>
        /// 查詢單場詳細資訊 (含時間軸)
        /// </summary>
        /// <param name="matchId">遊戲場次編號</param>
        /// <returns></returns>
        public async Task<string> GetMatchSummaryTimeLine(string matchId)
        {
            var url = $"lol/match/v5/matches/{matchId}/timeline";
            var response = await _matchClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return json;
        }

        /// <summary>
        /// 用 puuid 查比賽列表 count預設為10,最多100上限 ( API限制 )
        /// </summary>
        /// <param name="puuid"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<string>> GetMatchIdsAsync(string puuid,int start=0,int count=10) 
        {
            var url = $"lol/match/v5/matches/by-puuid/{puuid}/ids?start={start}&count={count}";
            var response = await _matchClient.GetFromJsonAsync<List<string>>(url);
            return response ?? [];
        }        
    }
}
