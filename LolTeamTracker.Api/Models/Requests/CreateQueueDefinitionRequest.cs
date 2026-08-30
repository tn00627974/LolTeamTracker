using System.ComponentModel;

namespace LolTeamTracker.Api.Models.Requests
{
    /// <summary>
    /// 新增遊戲模式定義的請求。
    /// 只包含呼叫端「該提供」的欄位——
    /// UpdatedAt 由伺服器決定，Matches 是導覽屬性，兩者都不該出現在 API 合約裡。
    /// </summary>
    public class CreateQueueDefinitionRequest
    {
        /// <summary>
        /// Riot 定義的模式編號，例如 420（單雙排）、440（彈性）。
        /// 不是自動遞增——這個值由 Riot 決定，呼叫端必須提供。
        /// </summary>
        [DefaultValue(420)]
        public int Id { get; set; }

        /// <summary>
        /// 模式名稱，例如 單雙積分 Solo/Duo Ranked
        /// </summary>
        [DefaultValue("單雙積分 Solo/Duo Ranked")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模式說明。可省略——queues.json 裡部分模式沒有描述。
        /// </summary>
        [DefaultValue("5v5 排位，僅限單人或雙人組隊")]
        public string? Description { get; set; }
    }
}
