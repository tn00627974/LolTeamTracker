using LolTeamTracker.Api.Models;

namespace LolTeamTracker.Api.Services
{
    public interface IMatchAnalyzer
    {
        public string GetLaneName(string teamPosition);
        public string GetQueueTypeName(int queueId);
        Task<List<MatchSummary>> GetMatchSummariesPlayerAsync(string gameName, string tagLine, int count);
        Task<List<MatchSummary>> GetMatchSummariesTeamsAsync();
        Task<MatchSummary?> GetMatchSummaryAsync(string matchId, string puuid, string gameName, string tagLine);
    }
}