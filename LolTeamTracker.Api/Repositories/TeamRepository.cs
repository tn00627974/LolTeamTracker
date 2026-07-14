using LolTeamTracker.Api.Models;
using System.Text.Json;

namespace LolTeamTracker.Api.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly IWebHostEnvironment _env;

        public TeamRepository(IWebHostEnvironment env)
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
            string json = await File.ReadAllTextAsync(savePath);
            // 這裡可以解析 JSON 並返回所需的資料
            var team = JsonSerializer.Deserialize<List<PlayerInfo>>(json);
            return team ?? new List<PlayerInfo>();
        }
    }
}