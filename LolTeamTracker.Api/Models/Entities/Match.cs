namespace LolTeamTracker.Api.Models.Entities
{
    /// <summary>
    /// 一場比賽本身的資料（Entity）。
    /// 只放「一場比賽只有一個值」的欄位——每位參與者各自的表現在 <see cref="MatchPlayer"/>。
    /// </summary>
    /// <remarks>
    /// 這個切分直接對應 Riot 回應的結構：
    ///   info.gameStartTimestamp / info.queueId → 一場一個 → 這裡
    ///   info.participants[]                    → 一場十個 → MatchPlayer
    /// 混在一張表的話，同一場的 GameDate、QueueId 會重複十次，
    /// 而且十份有機會不一致（更新時漏改其中幾筆）。
    /// </remarks>
    public class Match
    {
        /// <summary>
        /// Riot 的場次編號（例：TW2_371188339），用自然鍵當主鍵。
        /// </summary>
        /// <remarks>
        /// 為什麼這裡可以用自然鍵，Player 卻不行：
        /// 比賽是「已發生的歷史事實」——寫入當下必定已知，而且永遠不會變。
        /// puuid 則是外部系統對「人」的當前識別，2026-08-06 實測過會被 Riot 換掉。
        /// 判準不是「自然鍵好或壞」，是這個值會不會變、寫入當下在不在。
        /// </remarks>
        public string Id { get; set; } = string.Empty;

        // public int Id { get; set; } 

        public int QueueId { get; set; }

        /// <summary>
        /// 開賽時間，一律存 UTC。
        /// 時區轉換是呈現層的責任——存當地時間會讓資料無法跨時區解釋，
        /// 也讓範圍查詢與排序綁死在某個地區的日曆上。
        /// </summary>
        public DateTime GameDate { get; set; }

        /// <summary>
        /// 比賽長度（秒）。可為 null：中斷或異常結束的場次可能取不到這個值。
        /// </summary>
        public int? GameDuration { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>導覽屬性：這場比賽的模式定義。</summary>
        public QueueDefinition? Queue { get; set; }

        /// <summary>導覽屬性：這場比賽的所有參與者（正常情況為 10 筆）。</summary>
        public ICollection<MatchPlayer> Participants { get; set; } = new List<MatchPlayer>();
    }
}
