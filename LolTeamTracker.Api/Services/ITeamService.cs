using LolTeamTracker.Api.Models;

namespace LolTeamTracker.Api.Services
{
    public interface ITeamService
    {
        /// <summary>
        /// 查詢使用者的戰隊名單。
        /// </summary>
        /// <returns></returns>
        Task<List<PlayerInfo>> GetUserTeamAsync();

        /// <summary>
        /// 將指定 Riot ID 的玩家加入戰隊名單；若該玩家已在名單中，則更新其名稱。
        /// </summary>
        /// <remarks>
        /// puuid 由本方法向 Riot API 查詢後取得，呼叫端只需提供 Riot ID。
        /// 查無此玩家時，例外由全域例外處理轉為 404。
        /// </remarks>
        Task UpsertMemberAsync(string gameName, string tagLine);
    }
}
