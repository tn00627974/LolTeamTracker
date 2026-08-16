namespace LolTeamTracker.Api.Models.Entities
{
    /// <summary>
    /// 某位玩家在某場比賽的表現（Entity）。
    /// 一場比賽對應多筆——這張表關聯 <see cref="Match"/> 與 <see cref="Player"/>，
    /// 但它自己也帶資料（KDA、補刀、金錢），不是純粹的多對多中介表。
    /// </summary>
    public class MatchPlayer
    {
        public int Id { get; set; }

        public string MatchId { get; set; } = string.Empty;

        /// <summary>
        /// 關聯到 <see cref="Player.Id"/>（代理鍵），不是 puuid 也不是 GameName。
        /// 理由：那兩個都會變。用會變的值當關聯，改名或 Riot 遷移後歷史資料就對不上了。
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Riot 的英雄編號（participants[].championId）。
        /// 存 id 不存名稱：名稱可能改動或需要多語系，id 穩定且窄。
        /// </summary>
        public int ChampionId { get; set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }

        public bool Win { get; set; }

        /// <summary>
        /// Riot 原始值：TOP / JUNGLE / MIDDLE / BOTTOM / UTILITY，
        /// 大亂鬥等無路線概念的模式會是空字串（Riot 就是這樣回的，不是缺漏）。
        /// </summary>
        /// <remarks>
        /// 存原值不存中文——翻譯是呈現層的責任。
        /// 存「上路」的話，出英文版或 Riot 新增位置時都得回頭改歷史資料。
        /// </remarks>
        public string TeamPosition { get; set; } = string.Empty;

        public int LaneCS { get; set; }
        public int JungleCS { get; set; }

        public int Gold { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>導覽屬性：這筆表現所屬的比賽。</summary>
        public Match? Match { get; set; }

        /// <summary>導覽屬性：這筆表現屬於哪位玩家。</summary>
        public Player? Player { get; set; }
    }
}
