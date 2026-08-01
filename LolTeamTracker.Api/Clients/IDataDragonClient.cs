namespace LolTeamTracker.Api.Clients
{
    public interface IDataDragonClient
    {
        Task<string> GetLatestVersionAsync();

        Task<string> GetDataFileAsync (string version, string fileName, string lang = "zh_TW");
    }
}