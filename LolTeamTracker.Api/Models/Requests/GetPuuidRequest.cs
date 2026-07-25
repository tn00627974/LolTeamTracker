using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Models.Requests
{
    public class GetPuuidRequest
    {
        /// <summary>
        /// 召喚師名稱，例如 Faker
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 地區代碼，例如 TW2
        /// </summary>
        public string TagLine { get; set; } = string.Empty;
    }
}
