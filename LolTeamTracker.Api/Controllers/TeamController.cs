using LolTeamTracker.Api.Models.Requests;
using LolTeamTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        /// <summary>
        /// 查詢戰隊成員名單
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetUserTeam() // TODO : 之後改為 UserId (依照登入者) 來查詢戰隊成員
        {
            var results = await _teamService.GetUserTeamAsync();
            return Ok(results);
        }

        /// <summary>
        /// 將玩家加入戰隊名單（已存在則更新名稱）
        /// </summary>
        /// <remarks>
        /// 使用 PUT 而非 POST：本操作是冪等的——以相同參數呼叫多次，
        /// 結果與呼叫一次相同，因此重試是安全的。
        /// </remarks>
        [HttpPut("members")]
        public async Task<IActionResult> UpsertMember([FromBody] UpsertTeamMemberRequest request)
        {
            await _teamService.UpsertMemberAsync(request.GameName, request.TagLine);
            return NoContent();
        }
    }
}
