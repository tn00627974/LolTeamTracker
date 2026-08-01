namespace LolTeamTracker.Api.Repositories
{
    public interface IStaticDataRepository
    {
        Task SaveDataFileAsync(string fileName, string content);
    }
}