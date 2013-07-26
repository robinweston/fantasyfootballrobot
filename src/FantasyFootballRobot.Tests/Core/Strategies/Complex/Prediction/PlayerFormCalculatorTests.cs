using System.Collections.Generic;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Prediction
{
    [TestFixture]
    class PlayerFormCalculatorTests
    {
        private IPlayerFormCalculator _playerFormCalculator;
        private Player _player;
        private Mock<IConfigurationSettings> _configSettingsMock;
        private Mock<IPredictorParameters> _predictorParametersMock;
        private Mock<ITeamStrengthCalculator> _teamStrengthCalculatorMock;
        private IList<Player> _allPlayers;


        [SetUp]
        public void SetUp()
        {
            _allPlayers = new List<Player>();

            _player = new Player
                      {
                          PastSeasons = new List<Season>(),
                          PastFixtures = new List<PastFixture>()
                      };
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _configSettingsMock.Setup(x => x.SeasonStartYear).Returns(2012);
            
            _predictorParametersMock = new Mock<IPredictorParameters>();
            _predictorParametersMock.SetupGet(x => x.HomeAdvantageMultiplier).Returns(1.0);
            _predictorParametersMock.SetupGet(x => x.AwayAdvantageMultiplier).Returns(1.0);
            _predictorParametersMock.SetupGet(x => x.PastGamesUsedToCalculatePlayerForm).Returns(5);
            _predictorParametersMock.SetupGet(x => x.PreviousGameMultiplier).Returns(1.0);

            _teamStrengthCalculatorMock = new Mock<ITeamStrengthCalculator>();
            _teamStrengthCalculatorMock.Setup(
                x => x.CalculateTeamStrength(It.IsAny<string>(), It.IsAny<IList<Player>>())).Returns(new TeamStrength
                                                                                                     {
                                                                                                         TeamStrengthMultiplier
                                                                                                             = 1.0
                                                                                                     });

            _playerFormCalculator = new PlayerFormCalculator(_predictorParametersMock.Object, _teamStrengthCalculatorMock.Object, _configSettingsMock.Object);
        }

        [Test]
        public void extrapolate_player_scores_from_most_recent_season_to_predict_current_form_if_no_games_played()
        {
            // Arrange
            //player scored 150 points last season. 
            _player.PastSeasons .Add(new Season { TotalPointsScored = 150, MinutesPlayed = 820, SeasonEndYear = 2012});

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert

            // (total points scored / minutes played) * 90 minutes
            const double expectedForm = (150.0 / 820) * 90;
            Assert.That(currentForm.NormalisedPointsPerGame, Is.EqualTo(expectedForm));

        }

        [Test]
        public void if_player_has_no_past_seasons_then_current_form_is_0()
        {
            // Arrange

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            Assert.That(currentForm.NormalisedPointsPerGame, Is.EqualTo(0));

        }

        [Test]
        public void if_player_has_played_no_minutes_in_a_season_then_points_per_game_is_0()
        {
            // Arrange
            _player.PastSeasons.Add(new Season{MinutesPlayed = 0});

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            Assert.That(currentForm.NormalisedPointsPerGame, Is.EqualTo(0));

        }

        [Test]
        public void only_most_recent_season_is_used_to_calculate_form()
        {
            // Arrange
            _player.PastSeasons.Add(new Season {TotalPointsScored = 99, MinutesPlayed = 130, SeasonEndYear = 2011});

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            Assert.That(currentForm.NormalisedPointsPerGame, Is.EqualTo(0));
        }

        [Test]
        public void if_player_has_previous_home_fixture_then_score_is_normalised_correctly()
        {
            // Arrange
            _player.PastFixtures.Add(new PastFixture{Home = true, TotalPointsScored = 10});
            _predictorParametersMock.SetupGet(x => x.HomeAdvantageMultiplier).Returns(1.5);


            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 10/1.5);
        }

        [Test]
        public void if_player_has_previous_away_fixture_then_score_is_normalised_correctly()
        {
            // Arrange
            _player.PastFixtures.Add(new PastFixture { Home = false, TotalPointsScored = 10 });
            _predictorParametersMock.SetupGet(x => x.AwayAdvantageMultiplier).Returns(0.5);

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 10 / 0.5);
        }

        [Test]
        public void if_player_has_previous_fixture_then_score_is_normalised_correctly_based_on_opposition_strength()
        {
            // Arrange
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 10, OppositionClubCode = "CLUB"});
            _teamStrengthCalculatorMock.Setup(x => x.CalculateTeamStrength("CLUB", It.IsAny<IList<Player>>())).Returns(new TeamStrength{TeamStrengthMultiplier = 2.0});

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 10 * 2.0);
        }

        [Test]
        public void if_player_has_previous_fixture_then_score_is_normalised_correctly_based_on_combined_opposition_strength_and_location()
        {
            // Arrange
            _player.PastFixtures.Add(new PastFixture { Home = true, TotalPointsScored = 10, OppositionClubCode = "CLUB" });
            _teamStrengthCalculatorMock.Setup(x => x.CalculateTeamStrength("CLUB", It.IsAny<IList<Player>>())).Returns(new TeamStrength { TeamStrengthMultiplier = 0.5 });
            _predictorParametersMock.SetupGet(x => x.HomeAdvantageMultiplier).Returns(1.5);

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 10 / 1.5 * 0.5);
        }

        [Test]
        public void games_player_has_not_played_in_are_calculated_in_form()
        {
            // Arrange
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 10, OppositionClubCode = "CLUB" });
            _player.PastFixtures.Add(new PastFixture ());

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, (10 + 0) / 2.0);
        }


        [Test]
        public void player_form_calculator_uses_the_correct_past_number_of_games_when_calculating_form()
        {
            // Arrange
            _predictorParametersMock.SetupGet(x => x.PastGamesUsedToCalculatePlayerForm).Returns(2);
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 9999, OppositionClubCode = "CLUB", GameWeek = 1});
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 10, OppositionClubCode = "CLUB", GameWeek = 2});
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 20, OppositionClubCode = "CLUB", GameWeek = 3});

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, (10 + 20) / 2.0);
        }

        [Test]
        public void player_form_calculator_produces_weighted_mean_of_recent_scores()
        {
            // Arrange
            _predictorParametersMock.SetupGet(x => x.PreviousGameMultiplier).Returns(0.75);
            _predictorParametersMock.SetupGet(x => x.PastGamesUsedToCalculatePlayerForm).Returns(3);
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 30, OppositionClubCode = "CLUB", GameWeek = 1 });    
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 10, OppositionClubCode = "CLUB", GameWeek = 2 });
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 20, OppositionClubCode = "CLUB", GameWeek = 3 });
            
            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert

            //weighted mean = ((1 * 20) + (0.75 * 10) + (0.75 * 0.75 * 30)) / (1 + 0.75 + (0.75 * 0.75))
            // = (20 + 7.5 + 16.875) / 2.3125 = 19.1892
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 19.1892);
        }

        [Test]
        public void previous_season_score_is_used_when_calculating_player_form_early_on_in_season()
        {
            // Arrange
            _predictorParametersMock.SetupGet(x => x.PreviousGameMultiplier).Returns(0.75);
            _predictorParametersMock.SetupGet(x => x.PastGamesUsedToCalculatePlayerForm).Returns(2);
            _player.PastSeasons.Add(new Season { TotalPointsScored = 5, MinutesPlayed = 90, SeasonEndYear = 2012 });
            _player.PastFixtures.Add(new PastFixture { TotalPointsScored = 30, OppositionClubCode = "CLUB", GameWeek = 1 });

            // Act
            var currentForm = _playerFormCalculator.CalculateCurrentPlayerForm(_player, _allPlayers);

            // Assert
            //weighted mean = ((1 * 30) + (0.75 * 5)) / (1 + 0.75)
            // = (30 + 3.75) / 1.75 = 13.571
            AssertIsCloseTo(currentForm.NormalisedPointsPerGame, 19.2857);
            
        }

        private void AssertIsCloseTo(double actual, double expected)
        {
            const double error = 0.0001;
            Assert.That(actual, Is.InRange(expected - error, expected + error));
        }
    }
}
