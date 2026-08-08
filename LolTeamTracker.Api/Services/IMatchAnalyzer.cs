using LolTeamTracker.Api.Models;
using LolTeamTracker.Api.Models.Results;

namespace LolTeamTracker.Api.Services
{
    public interface IMatchAnalyzer
    {
        public string GetLaneName(string teamPosition);
        public string GetQueueTypeName(int queueId);
        Task<MatchSummaryResult> GetMatchSummariesPlayerAsync(string gameName, string tagLine, int count);
        Task<MatchSummaryResult> GetMatchSummariesTeamsAsync();
        Task<MatchSummary?> GetMatchSummaryAsync(string matchId, string puuid, string gameName, string tagLine);
    }
}