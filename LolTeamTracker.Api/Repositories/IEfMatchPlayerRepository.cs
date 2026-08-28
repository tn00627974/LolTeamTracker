using LolTeamTracker.Api.Models.Results;

namespace LolTeamTracker.Api.Repositories
{
    public interface IEfMatchPlayerRepository
    {
        Task<List<PlayerChampionStat>> TopChampions();
        Task<List<PlayerBestComboStat>> PlayerTeamPositionHeroWinningRate();
        Task<List<LaneBestChampionStat>> TeamPositionHeroWinningRate();
    }
}