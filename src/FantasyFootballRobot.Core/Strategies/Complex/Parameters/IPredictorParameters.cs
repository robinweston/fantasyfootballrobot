namespace FantasyFootballRobot.Core.Strategies.Complex.Parameters
{
    public interface IPredictorParameters
    {
        double HomeAdvantageMultiplier { get; }
        double AwayAdvantageMultiplier { get; }
        int MinMinutesPlayedLastSeasonToCalculateClubForm { get; }
        int MinMinutesPlayedLastSeasonToCalculatePlayerForm { get; }
        int PastGamesUsedToCalculatePlayerForm { get; }
        int FutureGameweeksUsedToCalculatePlayerForm { get; }
        double FutureGameweekMultiplier { get; }
        double PreviousGameMultiplier { get; }
    }
}