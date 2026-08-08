namespace LolTeamTracker.Api.Models.Results
{
    /// <summary>
    /// 批次查詢戰績的結果：成功的摘要清單，以及失敗的場次數。
    /// </summary>
    public class MatchSummaryResult
    {
        public List<MatchSummary> MatchSummaryList { get; set; } = [];

        /// <summary>
        /// 成功筆數。由清單長度推導而來，不另外儲存——避免同一份資訊有兩個可能不同步的來源。
        /// </summary>
        public int SuccessCount => MatchSummaryList.Count;

        /// <summary>
        /// 失敗場次數。必須獨立儲存：失敗的場次不會出現在任何清單裡，
        /// 若不記錄，呼叫端無從得知回傳的筆數是否完整。
        /// </summary>
        public int FailedCount { get; set; }
    }
}
