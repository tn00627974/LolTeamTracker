using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace LolTeamTracker.Api.Repositories
{
    public class EfMatchPlayerRepository : IEfMatchPlayerRepository
    {
        private readonly AppDbContext _db;

        public EfMatchPlayerRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 每個隊員最常玩的前三個英雄
        /// </summary>
        /// <returns></returns>
        public async Task<List<PlayerChampionStat>> TopChampions()
        {
            // 查詢 MatchPlayers 並依照玩家x英雄分類
            var query = _db.MatchPlayers.
                GroupBy(g => new
                {
                    g.PlayerId,
                    g.ChampionId
                })
                .Select(g => new
                {
                    PlayerId = g.Key.PlayerId,
                    ChampionId = g.Key.ChampionId,
                    Games = g.Count() // 遊玩次數
                })
                .OrderBy(x => x.PlayerId);

            var sql = query.ToQueryString();
            Console.WriteLine(sql);

            var queryResult = await query.ToListAsync();
            var result = queryResult
                .GroupBy(x => x.PlayerId)
                .SelectMany(player => player
                    .OrderByDescending(x => x.Games)
                    .Take(3))
                .Select(x => new PlayerChampionStat(x.PlayerId, x.ChampionId, x.Games))
                .OrderBy(x => x.PlayerId)
                .ThenByDescending(x => x.Games)
                .ToList();

            return result;
        }

        /// <summary>
        /// 每條路線勝率最高的英雄
        /// </summary>
        /// <returns></returns>
        public async Task<List<LaneBestChampionStat>> TeamPositionHeroWinningRate()
        {
            var query = _db.MatchPlayers
                .GroupBy(g => new
                {
                    g.ChampionId,
                    g.TeamPosition
                })
                .Select(g => new
                {
                    ChampionId = g.Key.ChampionId,
                    TeamPosition = g.Key.TeamPosition,
                    Games = g.Count(), // 遊玩次數
                    WinRatePct = Math.Round((double)g.Average(x => x.Win ? 1 : 0) * 100, 2) // 勝率百分比
                })
                .Where(g => g.Games >= 500); // 樣本門檻：場次太少的組合不列入排名

            var sql = query.ToQueryString();
            Console.WriteLine(sql);

            var queryResult = await query.ToListAsync();
            var result = queryResult
                .GroupBy(x => x.TeamPosition)
                .SelectMany(teamPosition => teamPosition
                    .OrderByDescending(x => x.WinRatePct)
                    .Take(1))
                .Select(x => new LaneBestChampionStat(x.TeamPosition, x.ChampionId, x.Games, x.WinRatePct))
                .OrderByDescending(x => x.WinRatePct)
                .ToList();

            return result;
        }


        /// <summary>
        /// 每個隊員英雄 與 勝率最高的路線
        /// </summary>
        /// <returns></returns>
        public async Task<List<PlayerBestComboStat>> PlayerTeamPositionHeroWinningRate()
        {
            var query = _db.MatchPlayers
                .GroupBy(g => new
                {
                    g.PlayerId,
                    g.ChampionId,
                    g.TeamPosition
                })
                .Select(g => new
                {
                    PlayerId = g.Key.PlayerId,
                    ChampionId = g.Key.ChampionId,
                    TeamPosition = g.Key.TeamPosition,
                    Games = g.Count(), // 遊玩次數
                    WinRatePct = Math.Round((double)g.Average(x => x.Win ? 1 : 0) * 100, 2) // 勝率百分比
                });

            var sql = query.ToQueryString();
            Console.WriteLine(sql);

            var queryResult = await query.ToListAsync();
            var result = queryResult
                .GroupBy(x => x.PlayerId)
                .SelectMany(teamPosition => teamPosition
                    .OrderByDescending(x => x.WinRatePct)
                    .Take(1))
                .Select(x => new PlayerBestComboStat(x.PlayerId, x.ChampionId, x.TeamPosition, x.Games, x.WinRatePct))
                .OrderByDescending(x => x.WinRatePct)
                .ToList();

            return result;
        }
    }
}
