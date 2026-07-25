using LolTeamTracker.Api.Models.Requests;
using System.Text.Json;

namespace LolTeamTracker.Api.Clients
{
    public interface IRiotApiClient
    {
        Task<string> GetPuuidAsync(string gameName, string tagLine);

        Task<string> GetGameNameAsync(string puuid);

        Task<JsonElement> GetMatchSummaryAsync(string matchId);

        Task<JsonElement> GetMatchSummaryTimeLineAsync(string matchId);

        Task<List<string>> GetMatchIdsAsync(string puuid, int start, int count);
    }
}
