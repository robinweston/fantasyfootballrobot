using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;
using FantasyFootballRobot.Core.Strategies.Genetic;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Transfers
{
    [TestFixture]
    class GeneticTransferSelectorTests
    {
        private Team _team;
        private GeneticTransferSelector _transferSelector;
        private Mock<IGeneticParameters> _geneticParametersMock;
        private Mock<IRandom> _randomMock;
        private Mock<ITeamScorePredictor> _teamScorePredictorMock;
        private Mock<ITransferActioner> _transferActionerMock;
        private Mock<ITransferValidator> _transferValidatorMock;
        private Mock<IPlayerPoolReducer> _playerPoolReducerMock;
        private Mock<IPredictorParameters> _predictorParametersMock;

        private SeasonState _seasonState;

        [SetUp]
        public void SetUp()
        {
            _geneticParametersMock = new Mock<IGeneticParameters>();
            _geneticParametersMock.SetupGet(x => x.PopulationSize).Returns(5);
            // run test as single threaded so we can predict outcome and test logic
            _geneticParametersMock.SetupGet(x => x.MaxDegreeOfParallelism).Returns(1);

            _teamScorePredictorMock = new Mock<ITeamScorePredictor>();

            _team = TeamCreationHelper.CreateTestTeam();

            _seasonState = new SeasonState
                           {
                               CurrentTeam = _team,
                               AllPlayers = TeamCreationHelper.CreatePlayerList(10, 10, 10, 10)
                           };

            _transferActionerMock = new Mock<ITransferActioner>();
            _transferActionerMock.Setup(x => x.ApplyTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>())).
               Returns(new TransferActionsResult { UpdatedSeasonState = _seasonState });

            _randomMock = new Mock<IRandom>();

            _transferValidatorMock = new Mock<ITransferValidator>();

            _predictorParametersMock = new Mock<IPredictorParameters>();

            _playerPoolReducerMock = new Mock<IPlayerPoolReducer>();
            _playerPoolReducerMock.Setup(x => x.ReducePlayerPool(_seasonState.AllPlayers)).Returns(
                _seasonState.AllPlayers);

            _transferSelector = new GeneticTransferSelector(_geneticParametersMock.Object,
                _randomMock.Object,
                new Mock<ILogger>().Object,
                _transferValidatorMock.Object,
                _teamScorePredictorMock.Object,
                _predictorParametersMock.Object,
                _transferActionerMock.Object,
                _playerPoolReducerMock.Object);


            _transferSelector.SetSeedGenes(_seasonState);

        }

        [Test]
        public void transfer_selector_returns_transfers()
        {
            // Arrange
            _teamScorePredictorMock.Setup(
                x => x.PredictTeamPointsForFutureGameweeks(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IList<Player>>(), It.IsAny<double>()))
                .Returns(10);

            // Act
            var result = _transferSelector.Run();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void initial_population_created_of_correct_size()
        {
            //Arrange

            //Act
            var population = _transferSelector.GenerateInitialPopulation();

            //Assert
            Assert.That(population.Count, Is.EqualTo(5));
        }

        [Test]
        public void initial_population_does_not_generate_invalid_transfers()
        {
            // Arrange

            // Act
            var population = _transferSelector.GenerateInitialPopulation();
            var transferValidator = new TransferValidator(new ConfigurationSettings(),
                                                          new TransferActioner(new Mock<ILogger>().Object));

            // Assert
            foreach (var transferActions in population)
            {
                var transferValidity = transferValidator.ValidateTransfers(_seasonState, transferActions);
                Assert.That(transferValidity, Is.EqualTo(TransferValidity.Valid));
            }
        }

        [Test]
        public void crossover_returns_combined_transfers()
        {
            // Arrange

            //select all transfers to combine
            _randomMock.Setup(x => x.Next(2)).Returns(1);
            var transfers1 = TestTransferHelper.CreateFitTransferActions(1, _team);
            var transfers2 = TestTransferHelper.CreateFitTransferActions(1, _team);

            // Act
            var combinedTransfers = _transferSelector.Crossover(transfers1, transfers2);

            // Assert
            Assert.That(combinedTransfers.Transfers.Count, Is.EqualTo(2));
            Assert.That(combinedTransfers.Transfers.Contains(transfers1.Transfers.First()));
            Assert.That(combinedTransfers.Transfers.Contains(transfers2.Transfers.First()));
        }

        [Test]
        public void crossover_returns_null_if_combined_transfers_are_invalid()
        {
            //Arrange
            var transfers1 = TestTransferHelper.CreateFitTransferActions(1, _team);
            var transfers2 = TestTransferHelper.CreateFitTransferActions(1, _team);
            _transferValidatorMock.Setup(x => x.ValidateTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>()))
                .Returns(TransferValidity.NotEnoughMoney);

            // Act
            var combinedTransfers = _transferSelector.Crossover(transfers1, transfers2);

            // Assert
            Assert.That(combinedTransfers, Is.Null);
        }

        [Test]
        public void mutation_changes_existing_transfers()
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(3, _team);

            // Act
            var mutation = _transferSelector.Mutate(transferActions);

            // Assert
            Assert.That(mutation.Transfers.Count, Is.EqualTo(3));
            var mutatedTransfer = mutation.Transfers.Single(t => !transferActions.Transfers.Contains(t));
            Assert.That(mutatedTransfer.PlayerIn, Is.EqualTo(_seasonState.AllPlayers.First()));
            Assert.That(mutatedTransfer.PlayerOut, Is.EqualTo(_team.Players.First()));
        }

        [Test]
        public void mutation_returns_null_if_mutated_transfers_are_invalid()
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(3, _team);
            _transferValidatorMock.Setup(x => x.ValidateTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>()))
                .Returns(TransferValidity.NotEnoughMoney);

            // Act
            var mutation = _transferSelector.Mutate(transferActions);

            // Assert
            Assert.That(mutation, Is.Null);
        }

        [Test]
        public void fitness_is_equal_to_future_gameweeks_score()
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(1, _team);
            _predictorParametersMock.Setup(x => x.FutureGameweeksUsedToCalculatePlayerForm).Returns(5);
            _predictorParametersMock.Setup(x => x.FutureGameweekMultiplier).Returns(0.7);
            _teamScorePredictorMock.Setup(
                x => x.PredictTeamPointsForFutureGameweeks(_team, _seasonState.Gameweek, 5, _seasonState.AllPlayers, 0.7)).
                Returns(2.5);

            // Act
            var fitness = _transferSelector.CalculateFitness(transferActions);

            // Assert
            Assert.That(fitness, Is.EqualTo(2.5));

        }

        [Test]
        public void fitness_is_cached()
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(1, _team);
            _teamScorePredictorMock.Setup(
                x => x.PredictTeamPointsForFutureGameweeks(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IList<Player>>(), It.IsAny<double>())).
                Returns(2.5);

            // Act
            var fitness1 = _transferSelector.CalculateFitness(transferActions);
            var fitness2 = _transferSelector.CalculateFitness(transferActions);

            // Assert
            Assert.That(fitness1, Is.EqualTo(2.5));
            Assert.That(fitness2, Is.EqualTo(2.5));
            _teamScorePredictorMock.Verify(
                x =>
                x.PredictTeamPointsForFutureGameweeks(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<int>(),
                                                      It.IsAny<IList<Player>>(), It.IsAny<double>()), Times.Once());
        }

        [Test]
        public void original_population_contains_actions_playing_standard_wildcard_if_available()
        {
            // Arrange
            

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.Any(ta => ta.PlayStandardWildcard), Is.True);
        }

        [Test]
        public void original_population_does_not_contain_actions_playing_standard_wildcard_if_it_has_already_been_played()
        {
            // Arrange
            _seasonState.StandardWildCardPlayed = true;
            _transferSelector.SetSeedGenes(_seasonState);

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.All(ta => ta.PlayStandardWildcard), Is.False);
        }

        [Test]
        public void original_population_contains_actions_playing_transfer_wildcard_if_available()
        {
            // Arrange
            _transferValidatorMock.Setup(x => x.IsInsideTransferWindow(_seasonState.Gameweek)).Returns(true);

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.Any(ta => ta.PlayTransferWindowWildcard), Is.True);
        }

        [Test]
        public void original_population_does_not_contain_actions_playing_transfer_wildcard_if_it_has_been_played_before()
        {
            // Arrange
            _transferValidatorMock.Setup(x => x.IsInsideTransferWindow(_seasonState.Gameweek)).Returns(true);
            _seasonState.TransferWindowWildcardPlayed = true;
            _transferSelector.SetSeedGenes(_seasonState);

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.All(ta => ta.PlayTransferWindowWildcard), Is.False);
        }

        [Test]
        public void original_population_does_not_contain_actions_playing_transfer_wildcard_if_it_is_outside_transfer_window()
        {
            // Arrange
            _transferValidatorMock.Setup(x => x.IsInsideTransferWindow(_seasonState.Gameweek)).Returns(false);

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.All(ta => ta.PlayTransferWindowWildcard), Is.False);
        }

        [Test]
        public void both_wildcards_are_not_played_at_same_time_even_if_they_both_are_available()
        {
            // Arrange
            _transferValidatorMock.Setup(x => x.IsInsideTransferWindow(_seasonState.Gameweek)).Returns(true);

            // Act
            var population = _transferSelector.GenerateInitialPopulation();

            // Assert
            Assert.That(population.Any(ta => ta.PlayStandardWildcard && ta.PlayTransferWindowWildcard), Is.False);
        }

        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        public void crossovers_play_standard_wildcard_onlyif_both_parents_do(bool transfer1StandardWildcardPlayed, bool transfer2StandardWildcardPlayed, bool expectedCrossoverStandardWildardPlayed)
        {
            // Arrange
            var transferActions1 = TestTransferHelper.CreateFitTransferActions(1, _team, transfer1StandardWildcardPlayed);
            var transferActions2 = TestTransferHelper.CreateFitTransferActions(1, _team, transfer2StandardWildcardPlayed);

            // Act
            var crossover = _transferSelector.Crossover(transferActions1, transferActions2);

            // Assert
            Assert.That(crossover.PlayStandardWildcard, Is.EqualTo(expectedCrossoverStandardWildardPlayed));
        }

        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        public void crossovers_play_transfer_window_wildcard_onlyif_both_parents_do(bool transfer1TransferWindowWildcardPlayed, bool transfer2TransferWindowWildcardPlayed, bool expectedCrossoverTransferWindowWildardPlayed)
        {
            // Arrange
            var transferActions1 = TestTransferHelper.CreateFitTransferActions(1, _team, false, transfer1TransferWindowWildcardPlayed);
            var transferActions2 = TestTransferHelper.CreateFitTransferActions(1, _team, false, transfer2TransferWindowWildcardPlayed);

            // Act
            var crossover = _transferSelector.Crossover(transferActions1, transferActions2);

            // Assert
            Assert.That(crossover.PlayTransferWindowWildcard, Is.EqualTo(expectedCrossoverTransferWindowWildardPlayed));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void mutated_transfer_actions_keeps_standard_wildcard_value_of_parent(bool standardWildcardPlayed)
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(1, _team, standardWildcardPlayed);

            // Act
            var mutation = _transferSelector.Mutate(transferActions);

            // Assert
            Assert.That(mutation.PlayStandardWildcard, Is.EqualTo(standardWildcardPlayed));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void mutated_transfer_actions_keeps_transfer_window_wildcard_value_of_parent(bool transferWindowWildcardPlayed)
        {
            // Arrange
            var transferActions = TestTransferHelper.CreateFitTransferActions(1, _team, false, transferWindowWildcardPlayed);

            // Act
            var mutation = _transferSelector.Mutate(transferActions);

            // Assert
            Assert.That(mutation.PlayTransferWindowWildcard, Is.EqualTo(transferWindowWildcardPlayed));
        }

    }
}
