using LolTeamTracker.Api.Clients;
using LolTeamTracker.Api.Repositories;

namespace LolTeamTracker.Api.Services
{
    /// <summary>
    /// 戰隊名單的流程編排：只負責「先做什麼、再做什麼」，
    /// 不自己打 HTTP（交給 Clients）、不自己存取資料（交給 Repositories）。
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<TeamService> _logger;

        public TeamService(
            IRiotApiClient riotApiClient,
            ITeamRepository teamRepository,
            ILogger<TeamService> logger)
        {
            _riotApiClient = riotApiClient;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        public async Task UpsertMemberAsync(string gameName, string tagLine)
        {
            string puuid = await _riotApiClient.GetPuuidAsync(gameName, tagLine);
            await _teamRepository.UpsertPlayerAsync(puuid,gameName, tagLine);
            _logger.LogInformation("已更新戰隊成員 {GameName}#{TagLine}", gameName, tagLine);

        }
    }
}
