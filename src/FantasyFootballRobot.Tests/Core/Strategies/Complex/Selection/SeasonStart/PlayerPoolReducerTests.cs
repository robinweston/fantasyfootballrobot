using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Selection.SeasonStart
{
    [TestFixture]
    class PlayerPoolReducerTests
    {
        private IPlayerPoolReducer _playerPoolReducer;
        private IList<Player> _allPlayers;
        private Mock<IInitialTeamSelectionParameters> _teamSelectionParametersMock;
        private Mock<IConfigurationSettings> _configSettingsMock;

        [SetUp]
        public void SetUp()
        {
            _allPlayers = TeamCreationHelper.CreatePlayerList(2, 2, 2, 2);
            _teamSelectionParametersMock = new Mock<IInitialTeamSelectionParameters>();
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _configSettingsMock.Setup(x => x.SeasonStartYear).Returns(2012);
            _playerPoolReducer = new PlayerPoolReducer(new Mock<ILogger>().Object, _teamSelectionParametersMock.Object, _configSettingsMock.Object);
        }

        [Test]
        public void players_must_have_played_a_minimum_number_of_minutes_the_previous_season_to_be_considered_for_selection()
        {
            //Arrange
            _teamSelectionParametersMock.Setup(x => x.MinimumPlayerMinutesPlayerFromPreviousSeasonToBeConsidered).Returns(10);
            for(var i =  0; i < _allPlayers.Count; i++)
            {
                var minutesPlayed = i < 6 ? 9 : 10;
                var player = _allPlayers[i];
                player.PastSeasons = new List<Season> { new Season { MinutesPlayed = minutesPlayed, TotalPointsScored = 1, SeasonEndYear = 2012 } };
            }

            //Act
            var reducedPool = _playerPoolReducer.ReducePlayerPool(_allPlayers);

            //Assert
            Assert.That(reducedPool.Count, Is.EqualTo(2));
        }

        [Test]
        public void players_must_have_scored_a_minimum_number_of_points_the_previous_season_to_be_considered_for_selection()
        {
            //Arrange
            _teamSelectionParametersMock.Setup(x => x.MinimumPlayerScoreFromPreviousSeasonToBeConsidered).Returns(10);
            for (var i = 0; i < _allPlayers.Count; i++)
            {
                var totalPoints = i < 5 ? 9 : 10;
                var player = _allPlayers[i];
                player.PastSeasons = new List<Season> { new Season { TotalPointsScored =  totalPoints, MinutesPlayed = 1, SeasonEndYear = 2012} };
                player.PastFixtures = new List<PastFixture>{new PastFixture{MinutesPlayed = 0}};
            }

            //Act
            var reducedPool = _playerPoolReducer.ReducePlayerPool(_allPlayers);

            //Assert
            Assert.That(reducedPool.Count, Is.EqualTo(3));
        }

        [Test]
        public void if_player_has_played_this_season_they_are_considered_for_selection()
        {
            //Arrange
            _teamSelectionParametersMock.Setup(x => x.MinimumPlayerScoreFromPreviousSeasonToBeConsidered).Returns(10);
            var player = new Player
                         {
                             PastSeasons = new List<Season>(),
                             PastFixtures = new List<PastFixture> {new PastFixture {MinutesPlayed = 1}}
                         };
            _allPlayers = new List<Player>{player};

            //Act
            var reducedPool = _playerPoolReducer.ReducePlayerPool(_allPlayers);

            //Assert
            Assert.That(reducedPool.Count, Is.EqualTo(1));
            Assert.That(reducedPool.First(), Is.EqualTo(player));
        }

        [Test]
        public void only_player_form_from_last_season_is_considered()
        {
            // Arrange
            _allPlayers = new List<Player>
                              {
                                  new Player{
                                      PastSeasons = new List<Season>
                                                               {
                                                                   new Season{SeasonEndYear = 2013, MinutesPlayed = 1, TotalPointsScored = 0}
                                                               }, PastFixtures = new List<PastFixture>()},
                               new Player{PastSeasons = new List<Season>
                                                               {
                                                                   new Season{SeasonEndYear = 2012, MinutesPlayed = 999, TotalPointsScored = 999}
                                                               }, PastFixtures = new List<PastFixture>()}
                              };

            // Act
            var reducedPool = _playerPoolReducer.ReducePlayerPool(_allPlayers);

            //Assert
            Assert.That(reducedPool.Count, Is.EqualTo(1));
            Assert.That(reducedPool.First(), Is.EqualTo(_allPlayers[1]));

        }
    }
}
