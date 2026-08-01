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
        var result = await _matchAnalyzer.GetMatchSummariesPlayerAsync(request.GameName, request.TagLine, request.Count);
        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }
    /// <summary>
    /// 取得團隊所有玩家的比賽列表
    /// </summary>
    /// <remarks>
    /// 讀取 team.json 內的玩家資訊
    /// </remarks>
    [HttpGet("team-analysis")]
    public async Task<IActionResult> GetMatchTeamList()
    {
        var results = await _matchAnalyzer.GetMatchSummariesTeamsAsync();
        return Ok(results);
    }

}
