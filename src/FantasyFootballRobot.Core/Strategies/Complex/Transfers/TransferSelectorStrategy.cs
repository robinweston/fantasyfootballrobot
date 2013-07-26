using System.Linq;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Transfers
{
    public class TransferSelectorStrategy : ITransferSelectorStrategy
    {
        private readonly IGeneticAlgorithm<FitTransferActions, SeasonState> _geneticTransferSelector;
        private readonly ILogger _logger;

        public TransferSelectorStrategy(IGeneticAlgorithm<FitTransferActions, SeasonState> geneticTransferSelector, ILogger logger)
        {
            _geneticTransferSelector = geneticTransferSelector;
            _logger = logger;
        }

        public TransferActions SelectTransfers(SeasonState seasonState)
        {
            _geneticTransferSelector.SetSeedGenes(seasonState);

            var allTransferActions = _geneticTransferSelector.Run();

            var topTransferActions = allTransferActions.Take(1).ToList();

            LogHelper.LogTopTransferActions(topTransferActions, _logger);

            return topTransferActions.First();
        }
     
       
    }
}