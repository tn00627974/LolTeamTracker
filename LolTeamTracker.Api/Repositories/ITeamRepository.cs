using LolTeamTracker.Api.Models;

namespace LolTeamTracker.Api.Repositories
{
    public interface ITeamRepository
    {
        /// <summary>
        /// 提供對玩家資訊的查詢方法。
        /// </summary>
        /// <returns></returns>
        Task<List<PlayerInfo>> LoadTeamFromDataAsync();

        /// <summary>
        /// 確保指定 puuid 的成員存在於名單中：已存在則更新名稱（處理玩家改名），不存在則新增。
        /// </summary>
        /// <remarks>
        /// 此操作是冪等的——以相同參數呼叫多次，結果與呼叫一次相同，因此可安全重試。
        /// 備註 : puuid 並非永遠不變於2026-08-07發現。
        /// </remarks>
        Task UpsertPlayerAsync(string puuid, string gameName, string tagLine);
    }
}