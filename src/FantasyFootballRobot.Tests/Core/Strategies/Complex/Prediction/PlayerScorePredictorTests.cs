using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Prediction
{
    [TestFixture]
    class PlayerScorePredictorTests
    {

        private int _gameweek;
        private Mock<ITeamStrengthCalculator> _teamStrengthCalculatorMock;
        private Mock<IPredictorParameters> _predictorParametersMock;
        private IList<Player> _allPlayers;
        private Mock<IPlayerFormCalculator> _playerformCalculator;
        private TeamStrength _teamStrength;
        private IPlayerScorePredictor _playerScorePredictor;
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _player = new Player();
         
            _gameweek = 1;

            _teamStrengthCalculatorMock = new Mock<ITeamStrengthCalculator>();
            _teamStrength = new TeamStrength
                            {
                                TeamStrengthMultiplier
                                    = 1.0
                            };

            _teamStrengthCalculatorMock.Setup(
                x => x.CalculateTeamStrength(It.IsAny<string>(), It.IsAny<IList<Player>>())).Returns(_teamStrength);

            _predictorParametersMock = new Mock<IPredictorParameters>();
            _predictorParametersMock.Setup(x => x.HomeAdvantageMultiplier).Returns(1);

            _allPlayers = new List<Player>();

            _playerformCalculator = new Mock<IPlayerFormCalculator>();

            _playerScorePredictor  = new PlayerScorePredictor(_teamStrengthCalculatorMock.Object, _predictorParametersMock.Object, _playerformCalculator.Object);
        }
      

        [Test]
        public void player_predicted_score_is_equal_to_current_form_adjusted_to_opponent_strength()
        {
            // Arrange
            const double pointsPerGame = 4.8;
            _playerformCalculator.Setup(x => x.CalculateCurrentPlayerForm(_player, _allPlayers)).Returns(new PlayerForm { NormalisedPointsPerGame = pointsPerGame });
            _player.FutureFixtures = new List<FutureFixture>{new FutureFixture{OppositionClubCode = "OPP", GameWeek = _gameweek}};
            _teamStrength.TeamStrengthMultiplier = 1.5;

            // Act
            var predictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);

            // Assert
            const double adjustedToTeamStrengthScore = pointsPerGame/1.5;
            _teamStrengthCalculatorMock.Verify(x => x.CalculateTeamStrength("OPP",_allPlayers));
            Assert.That(predictedScore, Is.EqualTo(adjustedToTeamStrengthScore));
        }

        [Test]
        public void predicted_score_is_zero_if_no_game_on_gameweek()
        {
            // Arrange
            _player.FutureFixtures = new List<FutureFixture> { new FutureFixture { GameWeek = 2 } };

            // Act
            var predictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(_player, 1, _allPlayers);

            // Assert
            Assert.That(predictedScore, Is.EqualTo(0));
            
        }

        [Test]
        public void predicted_score_is_calculated_correctly_for_double_gameweeks()
        {
            // Arrange
            const double pointsPerGame = 4.8;
            _player.FutureFixtures = new List<FutureFixture> { new FutureFixture
                                                              {
                                                                  OppositionClubCode = "OPP1",
                                                                  GameWeek = _gameweek
                                                              },
                                                              new FutureFixture
                                                              {
                                                                  OppositionClubCode = "OPP2",
                                                                  GameWeek = _gameweek
                                                              } 
            };
            _teamStrengthCalculatorMock.Setup(x => x.CalculateTeamStrength("OPP1", _allPlayers)).Returns(new TeamStrength{TeamStrengthMultiplier = 0.5});
            _teamStrengthCalculatorMock.Setup(x => x.CalculateTeamStrength("OPP2", _allPlayers)).Returns(new TeamStrength { TeamStrengthMultiplier = 1.5 });
            _playerformCalculator.Setup(x => x.CalculateCurrentPlayerForm(_player, _allPlayers)).Returns(new PlayerForm { NormalisedPointsPerGame = pointsPerGame });

            // Act
            var predictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);

            // Assert
            const double adjustedToTeam1StrengthScore = pointsPerGame / .5;
            const double adjustedToTeam2StrengthScore = pointsPerGame / 1.5;
            Assert.That(predictedScore, Is.EqualTo(adjustedToTeam1StrengthScore + adjustedToTeam2StrengthScore));
            
        }

        [Test]
        public void home_advantage_is_used_when_calculating_expected_player_score()
        {
            // Arrange
            const double homeGamePredictionPointsMultiplier = 1.5;
            const double currentPointsPerGame = 5.3;

            _player.FutureFixtures = new List<FutureFixture>{new FutureFixture{GameWeek = _gameweek, Home = true}};

            _predictorParametersMock.SetupGet(x => x.HomeAdvantageMultiplier).Returns(homeGamePredictionPointsMultiplier);
            _playerformCalculator.Setup(x => x.CalculateCurrentPlayerForm(_player, _allPlayers)).Returns(new PlayerForm
                                                                                                    {
                                                                                                        NormalisedPointsPerGame =
                                                                                                            currentPointsPerGame
                                                                                                    });
            

            // Act
            var predictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);

            // Assert
            const double expectedScore = homeGamePredictionPointsMultiplier * currentPointsPerGame;
            Assert.That(predictedScore, Is.EqualTo(expectedScore));
        }

        [Test]
        public void away_advantage_is_used_when_calculating_expected_score()
        {
            // Arrange
            const double homeGamePredictionPointsMultiplier = 1.5;
            const double currentPointsPerGame = 5.3;

            _player.FutureFixtures = new List<FutureFixture> { new FutureFixture { GameWeek = _gameweek, Home = false } };

            _predictorParametersMock.SetupGet(x => x.HomeAdvantageMultiplier).Returns(homeGamePredictionPointsMultiplier);
            _playerformCalculator.Setup(x => x.CalculateCurrentPlayerForm(_player, _allPlayers)).Returns(new PlayerForm
            {
                NormalisedPointsPerGame =
                    currentPointsPerGame
            });


            // Act
            var predictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);

            // Assert
            const double awayGamePointsMultiplier = 1/homeGamePredictionPointsMultiplier;
            const double expectedScore = awayGamePointsMultiplier * currentPointsPerGame;
            Assert.That(predictedScore, Is.EqualTo(expectedScore));

        }

        [Test]
        public void player_expected_scores_for_gameweeks_are_cached()
        {
            // Arrange
            _playerformCalculator.Setup(x => x.CalculateCurrentPlayerForm(It.IsAny<Player>(), _allPlayers)).Returns(new PlayerForm());
            _player.PastSeasons = new List<Season>
                                     {
                                         new Season { TotalPointsScored = 2, MinutesPlayed = 20, SeasonEndYear = 2008},
                                     };
            _player.FutureFixtures = new List<FutureFixture>{new FutureFixture{GameWeek = _gameweek, OppositionClubCode = "TEST"}};

            // Act
            var predictionPoints1 = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);
            var predictionPoints2 = _playerScorePredictor.PredictPlayerGameweekPoints(_player, _gameweek, _allPlayers);

            // Assert
            Assert.That(predictionPoints1, Is.EqualTo(predictionPoints2));
            _playerformCalculator.Verify(x => x.CalculateCurrentPlayerForm(It.IsAny<Player>(), _allPlayers), Times.Once());
            _teamStrengthCalculatorMock.Verify(x => x.CalculateTeamStrength(It.IsAny<string>(), It.IsAny<IList<Player>>()), Times.Once());
        }
    }
}
