using LolTeamTracker.Api.Models;

namespace LolTeamTracker.Api.Repositories
{
    public interface ITeamRepository
    {
        Task<List<PlayerInfo>> LoadTeamFromDataAsync();
    }
}