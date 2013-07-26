using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface ISeasonSimulator
    {
        SeasonSimulationResult PerformSimulation(IStrategy strategy, SeasonSimulationOptions simulationOptions);
    }
}