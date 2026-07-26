using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Models.Requests
{
    public class GetMatchIdsRequest
    {
        /// <summary>
        /// 玩家的 PUUID（固定 78 碼英數字），用來查詢該玩家的比賽列表
        /// </summary>
        public string Puuid { get; set; } = string.Empty;

        /// <summary>
        /// 查詢場次數量，預設 20 場，最多 50 場 ( Riot API 最高上限100場，此處收斂為 50，避免大量查詢觸發限流 )
        /// </summary>
        public int Count { get; set; } = 20;
    }
}
