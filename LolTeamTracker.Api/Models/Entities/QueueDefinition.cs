namespace LolTeamTracker.Api.Models.Entities
{
    /// <summary>
    /// 遊戲模式對照表（queueId → 名稱）。
    /// 資料來源：https://static.developer.riotgames.com/docs/lol/queues.json
    /// </summary>
    /// <remarks>
    /// Riot 新增遊戲模式(可從這個表添加)。
    /// 存進資料庫只要 INSERT 一列。
    /// </remarks>
    public class QueueDefinition
    {
        /// <summary>
        /// Riot 定義的模式編號，直接當主鍵（自然鍵）。
        /// 與 <see cref="Player"/> 選代理鍵的差別：這個值窄（int）、穩定、
        /// 且寫入當下必定已知——Player 的 puuid 三條都不滿足。
        /// </summary>
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 可為 null：queues.json 裡部分模式沒有描述欄位。
        /// 「沒有描述」與「描述是空字串」語意不同，不用魔術值代替。
        /// </summary>
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>導覽屬性：這個模式底下的所有場次。</summary>
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
