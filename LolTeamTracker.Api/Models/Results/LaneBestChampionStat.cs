namespace LolTeamTracker.Api.Models.Results
{
    public record LaneBestChampionStat(string TeamPosition, int ChampionId, int Games, double WinRatePct);
}
