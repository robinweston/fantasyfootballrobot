using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Genetic;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Selection.SeasonStart
{
    [TestFixture]
    class InitialTeamSelectorStrategyTests
    {
        private IInitialTeamSelectorStrategy _selectorStrategy;
        private Mock<IGeneticAlgorithm<FitTeam, IList<Player>>> _geneticAlgorithmMock;
        private IList<FitTeam> _topPopulation;

        [SetUp]
        public void SetUp()
        {
            _geneticAlgorithmMock = new Mock<IGeneticAlgorithm<FitTeam, IList<Player>>>();

            _topPopulation = new List<FitTeam>{new FitTeam()};
            _geneticAlgorithmMock.Setup(x => x.Run()).Returns(_topPopulation);

            _selectorStrategy = new InitialTeamSelectorStrategy(new Mock<ILogger>().Object, _geneticAlgorithmMock.Object);
        }

        [Test]
        public void seed_data_for_genetic_algorithm_is_set()
        {
            // Arrange
            var players = new List<Player>();

            // Act
            _selectorStrategy.SelectTeam(players);

            // Assert
            _geneticAlgorithmMock.Verify(x => x.SetSeedGenes(players));
        }

        [Test]
        public void intial_team_selector_uses_genetic_algorithm_to_find_best_starting_team()
        {
            //Arrange
            var players = new List<Player>();

            //Act
            _selectorStrategy.SelectTeam(players);

            //Asset
            _geneticAlgorithmMock.Verify(x => x.Run());

        }

        [Test]
        public void intial_team_selector_returns_top_result_from_genetic_algorithm_population()
        {
            //Arrange
            var players = new List<Player>();        

            //Act
            var team = _selectorStrategy.SelectTeam(players);

            //Asset
            Assert.That(team, Is.EqualTo(_topPopulation.First()));

        }
    }
}
