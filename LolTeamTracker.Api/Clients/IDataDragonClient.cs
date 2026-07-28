namespace LolTeamTracker.Api.Clients
{
    public interface IDataDragonClient
    {
        public Task<string> GetLatestVersionAsync();

        public Task<string> GetDataFileAsync (string version, string fileName, string lang = "zh_TW");
    }
}