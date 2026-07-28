namespace LolTeamTracker.Api.Clients
{
    public class DataDragonClient : IDataDragonClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DataDragonClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 取得最新的版本號
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetLatestVersionAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var versionList = await client.GetFromJsonAsync<List<string>>("https://ddragon.leagueoflegends.com/api/versions.json");
            return versionList!.First();// 取得最新版本(不會有 null)
        }

        /// <summary>
        /// 查詢檔案內容
        /// </summary>
        /// <param name="latestVersion">版本號</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="lang">地區格式</param>
        /// <returns></returns>
        public async Task<string> GetDataFileAsync(string latestVersion, string fileName, string lang)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://ddragon.leagueoflegends.com/cdn/{latestVersion}/data/{lang}/{fileName}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}
