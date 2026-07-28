using LolTeamTracker.Api.Clients;
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


        public StaticDataService(IDataDragonClient dataDragonClient, IStaticDataRepository staticDataRepository) 
        {
            _dataDragonClient = dataDragonClient;
            _staticDataRepository = staticDataRepository;
        }

        /// <summary>
        /// 下載所有json的資料 => 放在 Data\\Static 資料夾
        /// </summary>
        /// <param name="latestVersion">版本號</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public async Task<string> DownloadDataFileAsync(string latestVersion, string fileName)
        {
            var content = await _dataDragonClient.GetDataFileAsync(latestVersion ,fileName);
            await _staticDataRepository.SaveDataFileAsync(latestVersion, fileName, content);
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{now}:{fileName}下載版本{latestVersion}成功!";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> DownloadAllDataFilesAsync()
        {
            var resultList = new List<string>();
            var latestVersion = await _dataDragonClient.GetLatestVersionAsync();

            await FetchDataFileAsync("champion.json");
            await FetchDataFileAsync("item.json");
            await FetchDataFileAsync("summoner.json");
            await FetchDataFileAsync("runesReforged.json");

            async Task FetchDataFileAsync(string fileName)
            {
                try
                {
                    var result = await DownloadDataFileAsync(latestVersion, fileName);
                    resultList.Add($"✅ {result}");
                }
                catch (Exception ex)
                {
                    resultList.Add($"❌{ex.Message}");
                }
            }

            return resultList;
        }
    }
}
