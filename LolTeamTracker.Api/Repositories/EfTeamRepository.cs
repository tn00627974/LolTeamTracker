using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LolTeamTracker.Api.Repositories
{
    /// <summary>
    /// 以 EF Core 存取戰隊成員資料。
    /// </summary>
    /// <remarks>
    /// 前身為 JsonTeamRepository（讀取 Data/Team/team.json）。切換儲存實作時
    /// Services 與 Controllers 完全未受影響，因為兩者遵循同一個 ITeamRepository 契約。
    /// </remarks>
    public class EfTeamRepository : ITeamRepository
    {
        private readonly AppDbContext _db;

        public EfTeamRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 取得戰隊成員資料，並轉換為 PlayerInfo 型別的 List。
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 修改更新指定玩家資料，
        /// 1. 若資料庫中找不到該玩家，則新增一筆資料。
        /// 2. 若資料庫中找得到該玩家，則更新該筆資料。
        /// </summary>
        /// <param name="puuid"></param>
        /// <param name="gameName"></param>
        /// <param name="tagLine"></param>
        /// <returns></returns>
        public async Task UpsertPlayerAsync(string puuid, string gameName, string tagLine)
        {
            var player = await _db.Players.SingleOrDefaultAsync(p => p.Puuid == puuid );
            player ??= await _db.Players.SingleOrDefaultAsync(p => p.GameName == gameName && p.TagLine == tagLine);

            // 新增 ( 當DbContext中找不到該玩家時，新增一筆資料 )
            if (player == null)
            {
                _db.Players.Add(player = new Player
                {
                    Puuid = puuid,
                    GameName = gameName,
                    TagLine = tagLine,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 更新 ( 當DbContext中找得到該玩家時，更新該筆資料 )
            else
            {
                player.Puuid = puuid;
                player.GameName = gameName;
                player.TagLine = tagLine;
                player.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }
    }
}
