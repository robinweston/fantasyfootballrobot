using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Parameters
{
    public interface IInitialTeamSelectionParameters : IGeneticParameters
    {
        int MinimumPlayerScoreFromPreviousSeasonToBeConsidered { get; }
        int MinimumPlayerMinutesPlayerFromPreviousSeasonToBeConsidered { get; }
    }
}
