namespace LolTeamTracker.Api.Models.Requests
{
    public class GetGameNameRequest
    {
        /// <summary>
        /// 玩家的 PUUID（固定 78 碼英數字），用來查詢對應的召喚師名稱與標籤
        /// </summary>
        public string Puuid { get; set; } = string.Empty;
    }
}
