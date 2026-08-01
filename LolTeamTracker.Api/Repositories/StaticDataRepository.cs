namespace LolTeamTracker.Api.Repositories
{
    public class StaticDataRepository : IStaticDataRepository
    {
        private readonly IWebHostEnvironment _env;

        public StaticDataRepository(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task SaveDataFileAsync(string fileName, string content)
        {
            string savePath = Path.Combine(_env.ContentRootPath, "Data", "Static", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            await File.WriteAllTextAsync(savePath, content);
        }
    }
}
