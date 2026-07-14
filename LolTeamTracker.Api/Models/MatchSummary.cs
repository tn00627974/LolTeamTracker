namespace LolTeamTracker.Api.Models
{
    public class MatchSummary
    {
        public string GameName { get; set; }
        public string TagLine { get; set; }
        public string Champion { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public bool Win { get; set; }
        public string LaneName { get; set; } // 玩家路線
        public string GameDate { get; set; } // 遊戲時間
        public string GameMode { get; set; } // 遊戲模式

        public int LaneCS { get; set; } // totalMinionsKilled 線上小兵
        public int JungleCS { get; set; } // neutralMinionsKilled 野怪

        //public int TotalCS => LaneCS + JungleCS; // 計算總吃兵
        public int TotalCS; // 計算總吃兵

        public int Gold { get; set; } 
    }

}
