using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Transfers
{
    public class FitTransferActions : TransferActions, IChromosome
    {
        public double? Fitness { get; set; }
    }
}