using System.ComponentModel;

namespace LolTeamTracker.Api.Models.Requests
{
    public class UpsertTeamMemberRequest
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
    }
}
