using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Models.Results;
using LolTeamTracker.Api.Repositories;

namespace LolTeamTracker.Api.Services
{
    /// <summary>
    /// StaticDataService 處理靜態資料(如JSON、XML 等 ) 
    /// </summary>
    public class StaticDataService
    {
        private readonly IDataDragonClient _dataDragonClient;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly ILogger<StaticDataService> _logger;


        public StaticDataService(IDataDragonClient dataDragonClient, IStaticDataRepository staticDataRepository, ILogger<StaticDataService> logger) 
        {
            _dataDragonClient = dataDragonClient;
            _staticDataRepository = staticDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// 下載Json的資料 => 放在 Data\\Static 資料夾
        /// </summary>
        /// <param name="version">版本號</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public async Task<DownloadAllResult> DownloadDataFileAsync(string version, string fileName)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var results = new DownloadAllResult();

            try
            {
                var content = await _dataDragonClient.GetDataFileAsync(version, fileName);
                await _staticDataRepository.SaveDataFileAsync(fileName, content);
                results.SuccessCount++;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "下載 {FileName} 失敗，第三方回應狀態碼 {StatusCode}", fileName, httpEx.StatusCode);
                }
                else
                {
                    _logger.LogError(ex, "{FileName}發生錯誤", fileName);
                }
                results.FailedCount++;
            }
            return results; 
        }

        /// <summary>
        /// 下載Json的資料 => 放在 Data\\Static 資料夾
        /// </summary>
        /// <returns></returns>
        public async Task<DownloadAllResult> DownloadAllDataFilesAsync()
        {
            var results = new DownloadAllResult();
            var latestVersion = await _dataDragonClient.GetLatestVersionAsync();

            await FetchDataFileAsync("champion.json");
            await FetchDataFileAsync("item.json");
            await FetchDataFileAsync("summoner.json");
            await FetchDataFileAsync("runesReforged.json");

            return results;

            async Task FetchDataFileAsync(string fileName)
            {
                var downloadResult = await DownloadDataFileAsync(latestVersion, fileName);
                if (downloadResult.SuccessCount > 0)
                {
                    results.SuccessCount++;
                    results.SuccessFiles.Add(fileName);
                }
                else
                {
                    results.FailedCount++;
                    results.FailedFiles.Add(fileName);
                }
            }
        }
    }
}
