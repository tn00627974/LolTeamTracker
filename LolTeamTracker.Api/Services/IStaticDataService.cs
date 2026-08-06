using LolTeamTracker.Api.Models.Results;

namespace LolTeamTracker.Api.Services
{
    public interface IStaticDataService
    {
        Task<DownloadAllResult> DownloadAllDataFilesAsync();
        Task<bool> DownloadDataFileAsync(string version, string fileName);
    }
}