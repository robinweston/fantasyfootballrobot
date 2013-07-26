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
    class TeamStrengthCalculatorTests
    {
        private ITeamStrengthCalculator _teamStrengthCalculator;
        private IList<Player> _allPlayers;
        private Mock<IConfigurationSettings> _configSettingsMock;
        private Mock<IPredictorParameters> _predictorParametersMock;

        [SetUp]
        public void SetUp()
        {
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _configSettingsMock.SetupGet(x => x.SeasonStartYear).Returns(2012);
            _predictorParametersMock = new Mock<IPredictorParameters>();
            _teamStrengthCalculator = new TeamStrengthCalculator(_predictorParametersMock.Object, _configSettingsMock.Object);
            _allPlayers = new List<Player>();
        }

        [Test]
        public void single_player_with_no_games_played_this_season_gives_correct_team_strength()
        {
            // Arrange
            const string clubCode = "CLUB";
            _allPlayers.Add(CreatePlayerWithHistory(clubCode, 10, 10));

            // Act
            var strength = _teamStrengthCalculator.CalculateTeamStrength(clubCode, _allPlayers);

            // Assert
            Assert.That(strength.TeamStrengthMultiplier, Is.EqualTo(1.0));
        }

        [Test]
        public void two_players_from_same_team_with_no_games_played_this_season_gives_correct_team_strength()
        {
            // Arrange
            const string clubCode = "CLUB";
            _allPlayers.Add(CreatePlayerWithHistory(clubCode, 10, 1000));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode, 90, 1000));

            // Act
            var strength = _teamStrengthCalculator.CalculateTeamStrength(clubCode, _allPlayers);

            // Assert
            Assert.That(strength.TeamStrengthMultiplier, Is.EqualTo(1.0));
        }
     
        [Test]
        public void two_players_from_different_teams_with_no_games_played_this_season_gives_correct_team_strengths()
        {
            // Arrange
            const string clubCode1 = "CLUB1";
            const string clubCode2 = "CLUB2";

            //total minutes played is 2000
            //total points is 100
            //team average is 0.05 points per minute
            //team 1 has strength of 0.2 (0.01 / 0.05)
            //team 2 has strength of 1.8 (0.09 / 0.05)
            _allPlayers.Add(CreatePlayerWithHistory(clubCode1, 10, 1000));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode2, 90, 1000));

            // Act
            var teamStrength1 = _teamStrengthCalculator.CalculateTeamStrength(clubCode1, _allPlayers);
            var teamStrength2 = _teamStrengthCalculator.CalculateTeamStrength(clubCode2, _allPlayers);

            // Assert
            Assert.That(teamStrength1.TeamStrengthMultiplier, Is.InRange(0.1999, 2.0001));
            Assert.That(teamStrength2.TeamStrengthMultiplier, Is.InRange(1.7999, 1.80001));
        }

        [Test]
        public void sample_player_stats_recorded_correctly()
        {
            //Arrange
            const string clubCode = "CLUB";
            _allPlayers.Add(CreatePlayerWithHistory(clubCode, 10, 1000));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode, 90, 1000));

            // Act
            var teamStrength = _teamStrengthCalculator.CalculateTeamStrength(clubCode, _allPlayers);

            // Assert
            Assert.That(teamStrength.SamplePlayers, Is.EqualTo(2));
            Assert.That(teamStrength.TotalPointsScored, Is.EqualTo(100));
            Assert.That(teamStrength.TotalMinutes, Is.EqualTo(2000));
            Assert.That(teamStrength.PointsPerMinute, Is.EqualTo(100.0 / 2000));
        }

        [Test]
        public void season_further_ago_than_last_season_have_no_effect()
        {
            // Arrange
            const string clubCode1 = "CLUB1";
            const string clubCode2 = "CLUB2";
         
            _allPlayers.Add(CreatePlayerWithHistory(clubCode1, 10, 1000));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode2, 90, 1000));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode2, 9999, 1000, seasonEndDate: 2011));

            // Act
            var teamStrength1 = _teamStrengthCalculator.CalculateTeamStrength(clubCode1, _allPlayers);
            var teamStrength2 = _teamStrengthCalculator.CalculateTeamStrength(clubCode2, _allPlayers);

            // Assert
            Assert.That(teamStrength1.TeamStrengthMultiplier, Is.InRange(0.1999, 2.0001));
            Assert.That(teamStrength2.TeamStrengthMultiplier, Is.InRange(1.7999, 1.80001));
        }


        [Test]
        public void team_strength_is_calculated_based_on_points_per_minute()
        {
           // Arrange
            const string clubCode1 = "CLUB1";
            const string clubCode2 = "CLUB2";

            //total minutes played is 1600
            //total points is 100
            //team average is 0.0625 points per minute

            //team 1 has 0.02 points per minute
            //team 1 has strength of 0.32 (0.02 / 0.0625)

            //team 2 has 0.0818181 points per minute
            //team 2 has strength of 1.30909
            _allPlayers.Add(CreatePlayerWithHistory(clubCode1, 10, 500));
            _allPlayers.Add(CreatePlayerWithHistory(clubCode2, 90, 1100));

            // Act
            var teamStrength1 = _teamStrengthCalculator.CalculateTeamStrength(clubCode1, _allPlayers);
            var teamStrength2 = _teamStrengthCalculator.CalculateTeamStrength(clubCode2, _allPlayers);

            // Assert
            Assert.That(teamStrength1.TeamStrengthMultiplier, Is.EqualTo(0.32));
            Assert.That(teamStrength2.TeamStrengthMultiplier, Is.InRange(1.30909, 1.31));
            
        }

        [Test]
        public void team_strength_is_cached()
        {
            // Arrange
            var teamStrengthCalculatorMock = new Mock<TeamStrengthCalculator>(_predictorParametersMock.Object, _configSettingsMock.Object);
            teamStrengthCalculatorMock.Setup(x => x.CalculateLastSeasonForm(It.IsAny<IList<Player>>(), It.IsAny<int>())).Returns(
                new TeamStrength());
            _allPlayers.Add(CreatePlayerWithHistory("CLUB1", 10, 10, 2007));

            //add another player in otherwise reduced player list is equal to allPlayers
            _allPlayers.Add(CreatePlayerWithHistory("CLUB2", 10, 10, 2007));

            // Act
            var resultOne = teamStrengthCalculatorMock.Object.CalculateTeamStrength("CLUB1", _allPlayers);
            var resultTwo = teamStrengthCalculatorMock.Object.CalculateTeamStrength("CLUB1", _allPlayers);

            // Assert
            teamStrengthCalculatorMock.Verify(x => x.CalculateLastSeasonForm(_allPlayers, It.IsAny<int>()), Times.Once());
            Assert.That(resultOne, Is.EqualTo(resultTwo));
        }

        [Test]
        public void if_not_enough_data_available_for_previous_season_set_expected_points_to_be_equal_to_worst_team()
        {
            // Arrange
            _predictorParametersMock.SetupGet(x => x.MinMinutesPlayedLastSeasonToCalculateClubForm).Returns(50);

            const string newClubCode = "NEWCLUB";
            const string goodExistingClubCode = "GOOD";
            const string badExistingClubCode = "BAD";

            //total minutes played is 1049
            //total points is 100
            //team average is 0.02 points per minute

            //good team has 0.02 points per minute
            //bad team has 0.002 points per minute
            //bad team has relative points per min of 0.002 / 0.02 = 0.1

            //new club doesnt have enough minutes to give accurate result (49 < 50)
            _allPlayers.Add(CreatePlayerWithHistory(newClubCode, 10, 49));
            _allPlayers.Add(CreatePlayerWithHistory(goodExistingClubCode, 10, 500));
            _allPlayers.Add(CreatePlayerWithHistory(badExistingClubCode, 1, 500));

            // Act
            var newClubStrength = _teamStrengthCalculator.CalculateTeamStrength(newClubCode, _allPlayers);
            var badClubStrength = _teamStrengthCalculator.CalculateTeamStrength(badExistingClubCode, _allPlayers);

            // Assert
            Assert.That(newClubStrength.ArtificalPointsPerMinuteDueToLowSampleSize, Is.True);
            Assert.That(newClubStrength.PointsPerMinute, Is.EqualTo(0.002));          
            Assert.That(newClubStrength.TeamStrengthMultiplier, Is.InRange(0.0999, 0.1));

            Assert.That(newClubStrength.PointsPerMinute, Is.EqualTo(badClubStrength.PointsPerMinute));
            Assert.That(newClubStrength.TeamStrengthMultiplier, Is.EqualTo(badClubStrength.TeamStrengthMultiplier));                 
        }

        //todo: test for team strength game into season

        private Player CreatePlayerWithHistory(string clubCode, int pastSeasonScore, int minutesPlayed, int seasonEndDate = 2012)
        {
            return new Player
            {
                ClubCode = clubCode,
                PastSeasons = new List<Season> { new Season { TotalPointsScored = pastSeasonScore, MinutesPlayed = minutesPlayed, SeasonEndYear = seasonEndDate } }
            };
        }
    }
}
