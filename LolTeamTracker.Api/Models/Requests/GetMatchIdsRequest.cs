using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Models.Requests
{
    public class GetMatchIdsRequest
    {
        /// <summary>
        /// 玩家的 PUUID（固定 78 碼英數字），用來查詢該玩家的比賽列表
        /// </summary>
        [FromQuery]
        public string Puuid { get; set; } = string.Empty;

        /// <summary>
        /// 查詢場次數量，預設 10 場，最多 100 場
        /// </summary>
        [FromQuery]
        public int Count { get; set; }
    }
}
