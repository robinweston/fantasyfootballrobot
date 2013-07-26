using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public class StrategyDecisionsResult
    {
        public TransferActions TransfersMade { get; set; }

        public TransferActionsResult TransferResults { get; set; }

        public SeasonState UpdatedSeasonState { get; set; }
    }
}