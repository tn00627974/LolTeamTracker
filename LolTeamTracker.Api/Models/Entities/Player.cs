namespace LolTeamTracker.Api.Models.Entities
{
    /// <summary>
    /// 資料庫中的戰隊成員（Entity）。
    /// 這不是對外回傳用的型別——API 回傳請用 <see cref="PlayerInfo"/>（DTO）。
    /// 兩者分開的理由：它們因為不同原因而改變（API 合約 vs 資料庫結構）。
    /// </summary>
    public class Player
    {
        public int Id { get; set; }

        /// <summary>
        /// Riot 的永久玩家識別碼。新增成員的流程為「先查詢 puuid，取得後才寫入」，
        /// 因此寫入當下必定已知，宣告為不可為 null。
        /// </summary>
        public string Puuid { get; set; } = string.Empty;

        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
