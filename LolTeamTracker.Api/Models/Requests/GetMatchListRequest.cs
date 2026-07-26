using System.ComponentModel;

namespace LolTeamTracker.Api.Models.Requests
{
    public class GetMatchListRequest
    {
        /// <summary>
        /// 召喚師名稱，例如 Faker
        /// </summary>
        [DefaultValue("Faker")]
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 地區代碼，例如 TW2
        /// </summary>
        [DefaultValue("TW2")]
        public string TagLine { get; set; } = string.Empty;

        /// <summary>
        /// 查詢場次數量，預設 20 場，最多 50 場 ( Riot API 最高上限100場，此處收斂為 50，避免大量查詢觸發限流 )
        /// </summary>
        public int Count { get; set; } = 20;
    }
}
