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
        private readonly StaticDataService _staticDataService;
        private readonly ILogger<RiotController> _logger;

        public RiotController(IRiotApiClient riotApiClient, StaticDataService staticDataService, ILogger<RiotController> logger )
        {
            _riotApiClient = riotApiClient;
            _staticDataService = staticDataService;
            _logger = logger;
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
        /// 查該玩家的比賽列表 : 預設20場 ~ 50場 , 最多100場
        /// </summary>
        /// <returns></returns>
        [HttpGet("players/match-ids")] 
        public async Task<IActionResult> GetMatchIds([FromQuery] GetMatchIdsRequest request)
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
        public async Task<IActionResult> DownloadDataAsync() 
        {
            // 進階 : 先下載並返回成功與失敗的結果
            var resultList = await _staticDataService.DownloadAllDataFilesAsync();
            
            return Ok(new
            {
                message = "所有檔案處理完畢",
                results = resultList
            });
        }
    }
}
