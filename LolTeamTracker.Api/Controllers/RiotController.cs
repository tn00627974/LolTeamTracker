using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Models.Requests;
using LolTeamTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiotController : ControllerBase
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly RiotDataDownloader _riotApiClientDataDownloader;

        public RiotController(IRiotApiClient riotApiClient, RiotDataDownloader riotDataDownloader )
        {
            _riotApiClient = riotApiClient;
            _riotApiClientDataDownloader = riotDataDownloader;
        }

        /// <summary>
        /// 根據遊戲名稱和標籤獲取puuid
        /// </summary>
        /// <returns></returns>
        [HttpGet("players/{gameName}/{tagLine}")] 
        public async Task<IActionResult> GetPuuid([FromRoute] GetPuuidRequest request)
        {
            var puuid = await _riotApiClient.GetPuuidAsync(request.GameName, request.TagLine);
            return Ok(puuid);
        }

        /// <summary>
        /// 根據玩家的puuid獲取遊戲名稱和標籤
        /// </summary>
        /// <returns></returns>
        [HttpGet("players/{puuid}")]
        public async Task<IActionResult> GetGameName([FromRoute] GetGameNameRequest request)
        {
            var playerInfo = await _riotApiClient.GetGameNameAsync(request.Puuid);
            return Ok(playerInfo);
        }

        /// <summary>
        /// 查該玩家的比賽列表 : 最多100場,預設10場
        /// </summary>
        /// <returns></returns>
        [HttpGet("players/match-ids")] 
        public async Task<IActionResult> GetMatchIds(GetMatchIdsRequest request)
        {
            var result = await _riotApiClient.GetMatchIdsAsync(request.Puuid, 0,request.Count);
            return Ok(result);
        }

        /// <summary>
        /// 查詢單場詳細資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet("matches/{matchId}")]
        public async Task<IActionResult> GetMatchSummary([FromRoute] GetMatchIdRequest request)
        {
            var result = await _riotApiClient.GetMatchSummaryAsync(request.MatchId);
            return Ok(result);
        }

        /// <summary>
        /// 查詢單場詳細資訊 (含時間軸)
        /// </summary>
        /// <returns></returns>
        [HttpGet("matches/{matchId}/timeline")]
        public async Task<IActionResult> GetMatchIdsTimeList([FromRoute] GetMatchIdRequest request)
        {
            var result = await _riotApiClient.GetMatchSummaryTimeLineAsync(request.MatchId);
            return Ok(result);
        }

        /// <summary>
        /// 下載所有最新的json資料
        /// </summary>
        [HttpPost("download-all-json")]
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
