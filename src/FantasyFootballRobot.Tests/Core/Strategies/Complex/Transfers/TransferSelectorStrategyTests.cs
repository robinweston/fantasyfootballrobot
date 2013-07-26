using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;
using FantasyFootballRobot.Core.Strategies.Genetic;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Transfers
{
    [TestFixture]
    class TransferSelectorStrategyTests
    {
        private ITransferSelectorStrategy _selectorStrategy;
        private Mock<IGeneticAlgorithm<FitTransferActions, SeasonState>> _geneticAlgorithmMock;
        private SeasonState _seasonState;
        private IList<FitTransferActions> _transferActions;
            
        [SetUp]
        public void SetUp()
        {
            _transferActions = new List<FitTransferActions>
                          {
                              new FitTransferActions(),
                              new FitTransferActions()
                          };
            _seasonState = new SeasonState();
            _geneticAlgorithmMock = new Mock<IGeneticAlgorithm<FitTransferActions, SeasonState>>();
            _geneticAlgorithmMock.Setup(x => x.Run()).Returns(_transferActions);
            _selectorStrategy = new TransferSelectorStrategy(_geneticAlgorithmMock.Object, new Mock<ILogger>().Object);
        }

        [Test]
        public void strategy_sets_seed_genes_for_genetic_algorithm()
        {
            // Act
            _selectorStrategy.SelectTransfers(_seasonState);

            // Assert
            _geneticAlgorithmMock.Verify(x => x.SetSeedGenes(_seasonState));
        }

        [Test]
        public void strategy_returns_top_result_of_genetic_algorithm()
        {
            // Act
            var result = _selectorStrategy.SelectTransfers(_seasonState);

            // Assert
            Assert.That(result, Is.EqualTo(_transferActions.First()));
        }
    }
}
