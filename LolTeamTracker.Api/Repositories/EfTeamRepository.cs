using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LolTeamTracker.Api.Repositories
{
    /// <summary>
    /// 從資料庫讀取戰隊成員。與 <see cref="JsonTeamRepository"/> 遵循同一個介面契約，
    /// 因此切換實作時，Services 層不需要任何改動。
    /// </summary>
    public class EfTeamRepository : ITeamRepository
    {
        private readonly AppDbContext _db;

        public EfTeamRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<PlayerInfo>> LoadTeamFromDataAsync()
        {
            return await _db.Players
                .AsNoTracking()
                .Select(p => new PlayerInfo
                {
                    GameName = p.GameName,
                    TagLine = p.TagLine
                })
                .ToListAsync();
        }

        public async Task<List<PlayerInfo>> UpdateTeamFromDataAsync()
        {
            return new List<PlayerInfo>(); 
        }

        public async Task<List<PlayerInfo>> CreateTeamFromDataAsync()
        {
            return new List<PlayerInfo>();
        }

    }
}
