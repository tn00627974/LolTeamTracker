using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchAnalyzer _matchAnalyzer;


    public MatchController (IMatchAnalyzer matchAnalyzer)
    {
        _matchAnalyzer = matchAnalyzer;
    }

    /// <summary>
    /// 取得指定玩家的比賽列表
    /// </summary>
    /// <remarks>
    /// 範例：
    /// GET /api/match/Faker/TW2
    /// </remarks>
    /// <param name="gameName">召喚師名稱，例如Faker</param>
    /// <param name="tagLine">地區代碼，例如 TW2</param>
    /// <param name="count">搜尋場次</param>
    /// <returns></returns>
    [HttpGet("{gameName}/{tagLine}")]
    public async Task<IActionResult> GetMatchList(string gameName, string tagLine, int count = 50)
    {

        //if (count <= 0) { count = 50; }
        //else if (count > 100) { count = 100; }
        //var puuid = await _RiotApiClient.GetPuuidAsync(gameName, tagLine); 
        //var matchIds = await _RiotApiClient.GetMatchIdsAsync(puuid,0,count);
        //var result = new List<MatchSummary>();

        // 預設0~100不得超過API限制
        count = Math.Clamp(count, 50, 100);
        var result = await _matchAnalyzer.GetMatchSummariesPlayerAsync(gameName, tagLine, count);
        return Ok(new
        {
            count = result.Count,
            data = result
        });

        #region Old
        //foreach (var matchId in matchIds)
        //{
        //    try
        //    {
        //        var summary = await _matchAnalyzer.GetMatchSummaryAsync(matchId, puuid, gameName, tagLine);
        //        if (summary != null)
        //            result.Add(summary);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"讀取比賽 {matchId} 失敗：{ex.Message}");                    
        //    }
        //}
        //return this.Ok(new
        //{
        //    count = result.Count,
        //    data = result
        //});
        #endregion
    }
    /// <summary>
    /// 取得團隊所有玩家的比賽列表
    /// </summary>
    /// <remarks>
    /// 讀取team.json內的玩家資訊
    /// </remarks>
    [HttpGet("team-analysis")]
    public async Task<IActionResult> GetMatchTeamList()
    {
        var results = await _matchAnalyzer.GetMatchSummariesTeamsAsync();
        return Ok(results);
    }

}
