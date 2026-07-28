namespace LolTeamTracker.Api.Repositories
{
    public interface IStaticDataRepository
    {
        public Task SaveDataFileAsync(string latestVersion, string fileName, string content);
    }
}