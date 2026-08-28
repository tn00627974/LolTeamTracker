using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Models.Entities;
using LolTeamTracker.Api.Models.Requests;
using LolTeamTracker.Api.Repositories;
using LolTeamTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchStatsController : Controller
    {
        private readonly IEfMatchPlayerRepository _efMatchPlayerRepository;

        public MatchStatsController(IEfMatchPlayerRepository efMatchPlayerRepository)
        {
            _efMatchPlayerRepository = efMatchPlayerRepository;
        }

        /// <summary>
        /// 每個隊員最常玩的前三個英雄
        /// </summary>
        [HttpGet("top-champions")]
        public async Task<IActionResult> TopChampions()
        {
            var result = await _efMatchPlayerRepository.TopChampions();
            return Ok(result);
        }

        /// <summary>
        /// 每個隊員勝率最高的「英雄 + 路線」組合
        /// </summary>
        [HttpGet("best-combo-per-player")]
        public async Task<IActionResult> BestComboPerPlayer()
        {
            var result = await _efMatchPlayerRepository.PlayerTeamPositionHeroWinningRate();
            return Ok(result);
        }

        /// <summary>
        /// 每條路線勝率最高的英雄（樣本數 &lt; 500 場不列入排名，避免只打 1 場就 100% 勝率排第一）
        /// </summary>
        [HttpGet("best-champion-per-lane")]
        public async Task<IActionResult> BestChampionPerLane()
        {
            var result = await _efMatchPlayerRepository.TeamPositionHeroWinningRate();
            return Ok(result);
        }
    }
}
