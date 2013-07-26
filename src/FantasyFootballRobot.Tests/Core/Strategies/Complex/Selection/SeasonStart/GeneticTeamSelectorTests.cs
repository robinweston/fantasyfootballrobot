using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Constants;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Genetic;
using FantasyFootballRobot.Core.Validation;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Selection.SeasonStart
{
    [TestFixture]
    public class GeneticTeamSelectorTests
    {
        GeneticTeamSelector _geneticTeamSelector;
        Mock<IGeneticParameters> _geneticParametersMock;
        Mock<IInitialTeamSelectionParameters> _initialSelectionParametersMock;
        private Mock<IPredictorParameters> _predictorParametersMock;
        Mock<IRandom> _randomMock;
        IList<Player> _seedData;

        private FitTeam _team1;
        private FitTeam _team2;
        private Mock<ITeamScorePredictor> _scorePredictorMock;

        private Dictionary<int, int> _previousRandomValues;
        private Mock<IPlayerPoolReducer> _playerPoolReducerMock;

        [SetUp]
        public void SetUp()
        {
            _geneticParametersMock = new Mock<IGeneticParameters>();
            _geneticParametersMock.SetupGet(x => x.PopulationSize).Returns(5);
            // run test as single threaded so we can predict outcome and test logic
            _geneticParametersMock.SetupGet(x => x.MaxDegreeOfParallelism).Returns(1);


            _predictorParametersMock = new Mock<IPredictorParameters>();

            _seedData = TeamCreationHelper.CreatePlayerList(10, 20, 25, 10);

            _scorePredictorMock = new Mock<ITeamScorePredictor>();

            _initialSelectionParametersMock = new Mock<IInitialTeamSelectionParameters>();

            _team1 = CreateFitTeam();
            _team2 = CreateFitTeam();

            _randomMock = new Mock<IRandom>();

            _playerPoolReducerMock = new Mock<IPlayerPoolReducer>();
            _playerPoolReducerMock.Setup(x => x.ReducePlayerPool(It.IsAny<IList<Player>>())).Returns(
                (IList<Player> players) => players);

            _geneticTeamSelector = new GeneticTeamSelector(_geneticParametersMock.Object, new Mock<ILogger>().Object, _randomMock.Object, _scorePredictorMock.Object, _playerPoolReducerMock.Object, _predictorParametersMock.Object);
            _geneticTeamSelector.SetSeedGenes(_seedData);
        }

        private FitTeam CreateFitTeam()
        {
            var basicTeam = TeamCreationHelper.CreateTestTeam();
            return new FitTeam
                       {
                           Players = basicTeam.Players,
                           Captain = basicTeam.Captain,
                           ViceCaptain = basicTeam.ViceCaptain
                       };
        }



        private void SetupRandomForTeamSelection()
        {
            _previousRandomValues = new Dictionary<int, int>();
            //set up random numbers so that we always pick a unique player
            _randomMock.Setup(x => x.Next(It.IsAny<int>())).Returns((int max) =>  GetNextRandomValue(max));
        }

        private int GetNextRandomValue(int max)
        {
            int previousValue;
            int currentValue = 0;
            if(_previousRandomValues.TryGetValue(max, out previousValue))
            {
                currentValue = previousValue + 1;
                if(currentValue == max)
                {
                    currentValue = 0;
                }
            }

            _previousRandomValues[max] = currentValue;

            return currentValue;
        }

        [Test]
        public void initial_population_created_of_correct_size()
        {
            //Arrange
            SetupRandomForTeamSelection();

            //Act
            var population = _geneticTeamSelector.GenerateInitialPopulation();

            //Assert
            Assert.That(population.Count, Is.EqualTo(5));
        }
        
        [Test]
        public void invalid_team_is_not_included_in_initial_population()
        {
            //Arrange
            SetupRandomForTeamSelection();

            //set first 5 midfielders to be from same club
            var players = _seedData.Where(x => x.Position == Position.Midfielder).Take(5).ToList();
            foreach(var player in players)
            {
                player.ClubCode = "SAME";
            }

            //Act
            var population = _geneticTeamSelector.GenerateInitialPopulation();

            //Assert
            Assert.That(population.All(t => t.Validity == TeamValidationStatus.Valid));
        }

        [Test]
        public void if_team_is_too_expensive_they_are_not_included_in_initial_population()
        {
            //Arrange
            SetupRandomForTeamSelection();

            //set first 5 forwards to be worth 50 million each
            var players = _seedData.Where(x => x.Position == Position.Forward).Take(5).ToList();
            foreach (var player in players)
            {
                player.OriginalCost = 500;
                player.NowCost = 500;
            }

            //Act
            var population = _geneticTeamSelector.GenerateInitialPopulation();

            //Assert
            Assert.That(population.All(t => t.Players.Sum(p => p.OriginalCost) <= GameConstants.StartingMoney));
        }

        [Test]
        public void crossover_returns_valid_team()
        {
            //Arrange

            //Act
            var childTeam = _geneticTeamSelector.Crossover(_team1, _team2);

            //Assert
            Assert.That(childTeam, Is.Not.Null);
            Assert.That(childTeam.Validity, Is.EqualTo(TeamValidationStatus.Valid));
        }

        [Test]
        public void crossover_returns_new_team()
        {
            //Arrange

            //Act
            var childTeam = _geneticTeamSelector.Crossover(_team1, _team2);

            //Assert
            Assert.That(childTeam, Is.Not.EqualTo(_team1));
            Assert.That(childTeam, Is.Not.EqualTo(_team2));
            Assert.That(childTeam.Players, Is.Not.EqualTo(_team2.Players));
            Assert.That(childTeam.Players, Is.Not.EqualTo(_team2.Players));
        }

        [Test]
        public void crossover_joins_team_at_random_point()
        {
            //Arrange
            const int playersFromParentTeam1 = 3;
            _randomMock.Setup(x => x.Next(14)).Returns(playersFromParentTeam1);

            //Act
            var childTeam = _geneticTeamSelector.Crossover(_team1, _team2);

            //Assert
            var playersBeforeCrossover = childTeam.Players.Take(playersFromParentTeam1 + 1);
            var playersAfterCrossover = childTeam.Players.Skip(playersFromParentTeam1 + 1);
            Assert.That(playersBeforeCrossover.All(x => _team1.Players.Contains(x)));
            Assert.That(playersAfterCrossover.All(x => _team2.Players.Contains(x)));
        }

        [Test]
        public void if_crossover_creates_invalid_team_then_nothing_is_returned()
        {
            //Arrange

            //make all players from same team so child is invalid
            foreach (var player in _team1.Players)
            {
                player.ClubCode = "SAME";
            }
            foreach (var player in _team2.Players)
            {
                player.ClubCode = "SAME";
            }

            //Act
            var childTeam = _geneticTeamSelector.Crossover(_team1, _team2);

            //Assert
            Assert.That(childTeam, Is.Null);
        }

        [Test]
        public void if_crossover_creates_team_that_is_too_expensive_then_nothing_is_returned()
        {
            //Arrange

            //make all players from teams too expensive
            foreach (var player in _team1.Players)
            {
                player.OriginalCost = 100;
            }
            foreach (var player in _team2.Players)
            {
                player.OriginalCost = 100;
            }

            //Act
            var childTeam = _geneticTeamSelector.Crossover(_team1, _team2);

            //Assert
            Assert.That(childTeam, Is.Null);
        }

        [Test]
        public void mutation_returns_valid_team()
        {
            //Arrange

            //Act
            var mutation = _geneticTeamSelector.Mutate(_team1);

            //Assert
            Assert.That(mutation, Is.Not.Null);
            Assert.That(mutation.Validity, Is.EqualTo(TeamValidationStatus.Valid));
        }

        [Test]
        public void mutation_returns_new_team()
        {
            //Arrange

            //Act
            var mutation = _geneticTeamSelector.Mutate(_team1);

            //Assert
            Assert.That(mutation, Is.Not.EqualTo(_team1));
            Assert.That(mutation.Players, Is.Not.EqualTo(_team1.Players));
            Assert.That(mutation.Players, Is.Not.EqualTo(_team2.Players));
        }

        [Test]
        public void mutation_changes_one_random_player_in_team()
        {
            //Arrange
            const int replacedPlayerIndex = 4;
            const int replacementPlayerIndex = 7;

            //this will be the player who gets replaced. 
            var replacedPlayer = _team1.Players[replacedPlayerIndex];
            _randomMock.Setup(x => x.Next(15)).Returns(replacedPlayerIndex);

            //this will be the player replacing them - 7th midfielder in seed data
            var possibleReplacements = _seedData.Where(p => p.Position == replacedPlayer.Position).ToList();
            var replacementPlayer = possibleReplacements[replacementPlayerIndex];
            _randomMock.Setup(x => x.Next(possibleReplacements.Count)).Returns(replacementPlayerIndex);

            //Act
            var mutation = _geneticTeamSelector.Mutate(_team1);

            //Assert
            Assert.That(mutation.Players[replacedPlayerIndex], Is.EqualTo(replacementPlayer));
            
            //assert all other players remained the same
            for(var i=0; i < mutation.Players.Count; i++)
            {
                if(i != replacedPlayerIndex)
                {
                    var retainedPlayer = mutation.Players[i];
                    Assert.That(_team1.Players.Contains(retainedPlayer));
                }
            }
        }

        [Test]
        public void if_mutation_creates_invalid_team_then_nothing_is_returned()
        {
            //Arrange

            //replacement player will be first in seed data, 
            //so make him a player already in the team 
            //that means the mutated team will have the same player at index 0 and 1 and will be invalid
            _seedData[0] = _team1.Players[1];
            _geneticTeamSelector.SetSeedGenes(_seedData);

            //Act
            var mutation = _geneticTeamSelector.Mutate(_team1);

            //Assert
            Assert.That(mutation, Is.Null);
        }

        [Test]
        public void if_mutation_creates_team_that_is_too_expensive_then_nothing_is_returned()
        {
            //Arrange

            //make replacement player worth 120 million
            _seedData[0].OriginalCost = 1200;

            //Act
            var childTeam = _geneticTeamSelector.Mutate(_team1);

            //Assert
            Assert.That(childTeam, Is.Null);
        }

        [Test]
        public void fitness_function_predicts_team_form_for_first_x_games_of_season()
        {
            //Arrange
            _predictorParametersMock.Setup(x => x.FutureGameweeksUsedToCalculatePlayerForm).Returns(5);
            _predictorParametersMock.Setup(x => x.FutureGameweekMultiplier).Returns(0.7);
            _scorePredictorMock.Setup(x => x.PredictTeamPointsForFutureGameweeks(_team1, 1, 5, _seedData, 0.7)).Returns(66);

            //Act
            var fitness = _geneticTeamSelector.CalculateFitness(_team1);

            //Assert
            Assert.That(fitness, Is.EqualTo(66));
        }

        [Test]
        public void player_seed_genes_are_reduced_to_players_with_good_record_last_season()
        {
            // Arrange
            var reducedPlayerPool = new List<Player>();
            _playerPoolReducerMock.Setup(x => x.ReducePlayerPool(_seedData)).Returns(reducedPlayerPool);

            // Act
            _geneticTeamSelector.SetSeedGenes(_seedData);

            // Assert      
            Assert.That(_geneticTeamSelector.ReducedPlayerPool, Is.EqualTo(reducedPlayerPool));
            _playerPoolReducerMock.Verify(x => x.ReducePlayerPool(_seedData));
        }
    } 
}
