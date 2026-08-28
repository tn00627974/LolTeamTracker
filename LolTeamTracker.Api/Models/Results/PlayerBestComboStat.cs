namespace LolTeamTracker.Api.Models.Results
{
    public record PlayerBestComboStat(int PlayerId, int ChampionId, string TeamPosition, int Games, double WinRatePct);
}
