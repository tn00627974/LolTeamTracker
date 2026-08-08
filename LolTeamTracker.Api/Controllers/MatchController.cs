using LolTeamTracker.Api.Models.Requests;
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
    /// 範例：GET /api/match/match-summaries?gameName=Faker&amp;tagLine=TW2&amp;count=20
    /// </remarks>
    [HttpGet("match-summaries")]
    public async Task<IActionResult> GetMatchList([FromQuery] GetMatchListRequest request)
    {
        var results = await _matchAnalyzer.GetMatchSummariesPlayerAsync(request.GameName, request.TagLine, request.Count);

        // 成功
        if (results.FailedCount == 0)
        {
            return Ok(results);
        }

        // 失敗
        if (results.SuccessCount == 0)
        {
            return StatusCode(502, results); // 全部失敗問題
        }

        return StatusCode(207, results);

    }
    /// <summary>
    /// 取得團隊所有玩家的比賽列表
    /// </summary>
    /// <remarks>
    /// 成員名單取自 Players 資料表。個別場次查詢失敗不會中斷整批作業，
    /// 失敗數量由回應中的 failedCount 呈現：全數成功回 200、部分成功回 207、全數失敗回 502。
    /// </remarks>
    [HttpGet("team-analysis")]
    public async Task<IActionResult> GetMatchTeamList()
    {
        var results = await _matchAnalyzer.GetMatchSummariesTeamsAsync();

         // 成功
        if (results.FailedCount == 0)
        {
            return Ok(results);
        }

        // 失敗
        if (results.SuccessCount == 0)
        {
            return StatusCode(502, results); // 全部失敗問題
        }

        return StatusCode(207, results);
    }

}
