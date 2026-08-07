using LolTeamTracker.Api.Models;
using System.Text.Json;

namespace LolTeamTracker.Api.Repositories
{
    /// <summary>
    /// 從 Data/Team/team.json 讀取戰隊成員（EF Core 導入前的原始實作）。
    /// 保留此實作是為了對照——它與 <see cref="EfTeamRepository"/> 遵循同一個介面契約，
    /// 切換儲存方式時呼叫端不需任何改動。
    /// </summary>
    public class JsonTeamRepository : ITeamRepository
    {
        private readonly IWebHostEnvironment _env;

        public JsonTeamRepository(IWebHostEnvironment env)
        {
            _env = env;
        }
        /// <summary>
        /// 載入檔案中的成員資料
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<List<PlayerInfo>> LoadTeamFromDataAsync()
        {
            string savePath = Path.Combine(_env.ContentRootPath, "Data", "Team", "team.json"); // Path : Data/Team/team.json
            if (!File.Exists(savePath))
            {
                throw new FileNotFoundException($"檔案不存在於 {savePath}");
            }

            // 將 JSON 反序列化為 List<PlayerInfo>，將屬性轉為CamelCase(駝峰)去比對team.json中的屬性名稱
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = await File.ReadAllTextAsync(savePath);
            // 這裡可以解析 JSON 並返回所需的資料
            var team = JsonSerializer.Deserialize<List<PlayerInfo>>(json, options);
            return team ?? new List<PlayerInfo>();
        }
    }
}