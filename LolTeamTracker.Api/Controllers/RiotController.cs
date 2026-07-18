using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using LolTeamTracker.Api.Services;
using System.Net.Http;
using System.Xml.Linq;
using LolTeamTracker.Api.Clients;

namespace LolTeamTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiotController : ControllerBase
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly RiotDataDownloader _riotApiClientDataDownloader;
        private readonly IWebHostEnvironment _env;

        public RiotController(IRiotApiClient riotApiClient, RiotDataDownloader riotDataDownloader, IWebHostEnvironment env)
        {
            _riotApiClient = riotApiClient;
            _riotApiClientDataDownloader = riotDataDownloader;
            _env = env;
        }

        /// <summary>
        /// 根據遊戲名稱和標籤獲取puuid
        /// </summary>
        /// <param name="gameName">遊戲名稱</param>
        /// <param name="tagLine">#標籤</param>
        /// <returns></returns>
        [HttpGet("players/puuid")]
        public async Task<IActionResult> GetPuuid(string gameName, string tagLine)
        {
            var puuid = await _riotApiClient.GetPuuidAsync(gameName, tagLine);
            return Ok(puuid);
        }

        /// <summary>
        /// 根據玩家的puuid獲取遊戲名稱和標籤
        /// </summary>
        /// <param name="puuid"></param>
        /// <returns></returns>
        [HttpGet("players/{puuid}")]
        public async Task<IActionResult> GetGameName(string puuid)
        {
            var playerInfo = await _riotApiClient.GetGameNameAsync(puuid);
            return Ok(playerInfo);
        }

        /// <summary>
        /// 查詢單場詳細資訊
        /// </summary>
        /// <param name="matchId">場次編號</param>
        /// <returns></returns>
        [HttpGet("matchId")]
        public async Task<IActionResult> GetMatchSummary(string matchId)
        {
            var result = await _riotApiClient.GetMatchSummaryAsync(matchId);
            return Ok(result);
        }

        /// <summary>
        /// 查詢單場詳細資訊 (含時間軸)
        /// </summary>
        /// <param name="matchId">場次編號</param>
        /// <returns></returns>
        [HttpGet("matchId-timeline")]
        public async Task<IActionResult> GetMatchIdsTimeList(string matchId)
        {
            var result = await _riotApiClient.GetMatchSummaryTimeLineAsync(matchId);
            return Ok(result);
        }

        /// <summary>
        /// 查該玩家的比賽列表 : 最多100場,預設10場
        /// </summary>
        /// <param name="puuid"></param>
        /// <param name="count">資料比數</param>
        /// <returns></returns>
        [HttpGet("match-ids")]
        public async Task<IActionResult> GetMatchIds(string puuid, int count = 10)
        {
            var matchId = await _riotApiClient.GetMatchIdsAsync(puuid, count);
            return Ok(matchId);
        }

        /// <summary>
        /// 下載所有最新的json資料
        /// </summary>
        [HttpGet("download-all-json")]
        public async Task<IActionResult> DownloadChampionData()
        {
            var resultList = new List<string>();

            #region Old : 若失敗就只回傳單一成功與失敗
            //resultList.Add(await _riotApiClientDataDownloader.DownloadLatestChampionJsonAsync());
            //resultList.Add(await _riotApiClientDataDownloader.DownloadLatestItemJsonAsync());
            //resultList.Add(await _riotApiClientDataDownloader.DownloadLatestSummonerJsonAsync());
            //resultList.Add(await _riotApiClientDataDownloader.DownloadLatestRunesReforgedJsonAsync());
            #endregion

            #region 進階 : 先下載並返回成功與失敗的結果
            async Task TryDownload(Func<Task<string>> downloadFunc, string name)
            {
                try
                {
                    var result = await downloadFunc();
                    resultList.Add($"{name}: ✅ {result}");
                }
                catch (Exception ex)
                {
                    resultList.Add($"{name}: ❌ 錯誤 - {ex.Message}");
                }
            }

            await TryDownload(_riotApiClientDataDownloader.DownloadLatestChampionJsonAsync, "Champion");
            await TryDownload(_riotApiClientDataDownloader.DownloadLatestItemJsonAsync, "Item");
            await TryDownload(_riotApiClientDataDownloader.DownloadLatestSummonerJsonAsync, "Summoner");
            await TryDownload(_riotApiClientDataDownloader.DownloadLatestRunesReforgedJsonAsync, "Runes");
            #endregion

            return Ok(new
            {
                message = "所有檔案處理完畢",
                results = resultList
            });
        }
    }
}
