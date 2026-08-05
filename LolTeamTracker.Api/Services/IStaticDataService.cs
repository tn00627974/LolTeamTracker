using LolTeamTracker.Api.Models.Results;

namespace LolTeamTracker.Api.Services
{
    public interface IStaticDataService
    {
        Task<DownloadAllResult> DownloadAllDataFilesAsync();
        Task<DownloadAllResult> DownloadDataFileAsync(string version, string fileName);
    }
}