namespace LolTeamTracker.Api.Models.Results
{
    public class DownloadAllResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> SuccessFiles { get; set; } = [];
        public List<string> FailedFiles { get; set; } = [];
    }
}
