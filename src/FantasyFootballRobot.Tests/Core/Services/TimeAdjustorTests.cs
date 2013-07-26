using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Exceptions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Services
{
    [TestFixture]
    public class TimeAdjustorTests
    {
        private ITimeAdjustor _timeAdjustor;
        IList<Player> _players;
        private Mock<IConfigurationSettings> _configSettingsMock;

        [SetUp]
        public void SetUp()
        {
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _timeAdjustor = new TimeAdjustor(new Mock<ILogger>().Object, _configSettingsMock.Object);
            _players = GetTestPlayers();
            var unityContainer = ContainerBuilder.BuildUnityContainer();
            MappingConfiguration.Bootstrap(unityContainer);
        }

        private static IList<Player> GetTestPlayers()
        {
            var players = new List<Player>();
            for (int i = 0; i < 15; i++)
            {
                var player = new Player
                             {
                                 Id = i,
                                 PastFixtures = new List<PastFixture>
                                                {
                                                    new PastFixture
                                                    {GameWeek = 1, OppositionClubCode = Club.Wigan.Code, PlayerValueAtTime = 50},
                                                    new PastFixture
                                                    {GameWeek = 2, OppositionClubCode = Club.StokeCity.Code, PlayerValueAtTime = 60},
                                                    new PastFixture
                                                    {GameWeek = 2, OppositionClubCode = Club.Sunderland.Code, PlayerValueAtTime = 70}
                                                },
                                 FutureFixtures = new List<FutureFixture>
                                                  {
                                                      new FutureFixture {GameWeek = 3, OppositionClubCode = Club.QPR.Code},
                                                      new FutureFixture {GameWeek = 3, OppositionClubCode = Club.ManUtd.Code},
                                                      new FutureFixture {GameWeek = 4, OppositionClubCode = Club.ManCity.Code}
                                                  }
                             };

                players.Add(player);
            }

            return players;
        }

        [Test]
        public void game_week_1_all_fixtures_are_in_the_future()
        {
            //Arrange

            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(_players, 1);

            //Assert
            Assert.That(adjustedPlayers.First().PastFixtures.Count, Is.EqualTo(0));
            Assert.That(adjustedPlayers.First().FutureFixtures.Count, Is.EqualTo(6));
            Assert.That(adjustedPlayers.First().FutureFixtures.First().GameWeek, Is.EqualTo(1));
        }

        [Test]
        public void correct_current_gameweek_gets_mapped_correctly()
        {
            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(_players, 3);

            //Assert
            Assert.That(adjustedPlayers.First().PastFixtures.Count, Is.EqualTo(3));
            Assert.That(adjustedPlayers.First().FutureFixtures.Count, Is.EqualTo(3));
            Assert.That(adjustedPlayers.First().PastFixtures.Last().OppositionClubCode, Is.EqualTo("SUN"));
            Assert.That(adjustedPlayers.First().FutureFixtures.Last().OppositionClubCode, Is.EqualTo("MCI"));
        }

        [Test]
        public void player_value_gets_adjusted_correctly()
        {
            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(_players, 2);

            //Assert
            Assert.That(adjustedPlayers.First().NowCost, Is.EqualTo(60));           
        }

        [Test]
        public void player_status_gets_set_correctly()
        {
            //this test is really a temporary measure until we find a way of backdating availability

            //Arrange
            _players.ToList().ForEach(x => x.Status = Status.Unavailable);

            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(_players, 2);

            //Assert
            Assert.That(adjustedPlayers.All(x => x.Status == Status.Available), Is.True);
        }

        [Test]
        [ExpectedException(typeof(TimeAdjustorException))]
        public void cant_set_gameweek_in_the_future()
        {
            //Arrange

            //Act
            _timeAdjustor.AdjustPlayersToGameweek(_players, 4); 
        }

        [Test]
        public void adjusted_players_are_cloned_leaving_original_players_untouched()
        {
            //Arrange

            //Act
            _timeAdjustor.AdjustPlayersToGameweek(_players, 1); 

            //Assert
            Assert.That(_players.All(x => x.PastFixtures.Count == 3), Is.True);
            Assert.That(_players.All(x => x.FutureFixtures.Count == 3), Is.True);
        }

        [Test]
        public void adjusted_team_is_cloned_leaving_original_team_untouched()
        {
            //Arrange
            var oldTeam = TeamCreationHelper.CreateTestTeam();

            //Act
            var adjustedTeam = _timeAdjustor.AdjustTeamToGameweek(oldTeam, _players, 1);

            //Assert
            Assert.That(adjustedTeam, Is.Not.EqualTo(oldTeam));
            Assert.That(adjustedTeam.Players.All(p => !oldTeam.Players.Contains(p)));
        }       

        [Test]
        public void purchased_player_values_are_maintained_when_adjusting_team()
        {
            //Arrange
            var oldTeam = TeamCreationHelper.CreateTestTeam();

            //set the player value at 3 million
            oldTeam.Players.First().NowCost = 30;
            oldTeam.Players.First().OriginalCost = 30;

            //set the player to go up in value in gameweek 2 to 5 million
            _players.Single(p => p.Id == oldTeam.Players.First().Id).PastFixtures.First(f => f.GameWeek == 2).PlayerValueAtTime = 50;

            //Act
            var adjustedTeam = _timeAdjustor.AdjustTeamToGameweek(oldTeam, _players, 2);

            //Assert
            //todo: we are probably misusing original cost. Check JSON from owned player to see what property gets set
            //in addition, make sure orignal cost gets set back when player is sold, and purchased cost resets to 0
            Assert.That(adjustedTeam.Players.First().OriginalCost, Is.EqualTo(30));
            Assert.That(adjustedTeam.Players.First().NowCost, Is.EqualTo(50));
        }

        [Test]
        public void captains_are_cloned_when_adjusting_team()
        {
            //Arrange
            var oldTeam = TeamCreationHelper.CreateTestTeam();

            //Act
            var adjustedTeam = _timeAdjustor.AdjustTeamToGameweek(oldTeam, _players, 1);

            //Assert
            Assert.That(adjustedTeam.Captain.Id, Is.EqualTo(oldTeam.Captain.Id));
            Assert.That(adjustedTeam.Captain, Is.Not.EqualTo(oldTeam.Captain));

            Assert.That(adjustedTeam.ViceCaptain.Id, Is.EqualTo(oldTeam.ViceCaptain.Id));
            Assert.That(adjustedTeam.ViceCaptain, Is.Not.EqualTo(oldTeam.ViceCaptain));
        }

        [Test]
        public void each_player_is_adjusted_when_adjusting_team()
        {
            //Arrange
            var oldTeam = TeamCreationHelper.CreateTestTeam();

            //Act
            var adjustedTeam = _timeAdjustor.AdjustTeamToGameweek(oldTeam, _players, 1);

            //Assert
            Assert.That(adjustedTeam.Players.Count, Is.EqualTo(15));
            Assert.That(oldTeam.Players.All(p => adjustedTeam.Players.Count(ap => ap.Id == p.Id) == 1));
        }

        [Test]
        public void if_player_has_no_game_on_gameweek_set_to_gameweek_before()
        {
            //Arrange
            var players = new List<Player>
                              {
                                  new Player
                                      {
                                          NowCost = 60,
                                          FutureFixtures = new List<FutureFixture>(),
                                          PastFixtures = new List<PastFixture>
                                                             {
                                                                 new PastFixture {GameWeek = 1, PlayerValueAtTime = 50}
                                                             }
                                      }
                              };

            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(players, 2);

            //Assert
            Assert.That(adjustedPlayers.Single().NowCost, Is.EqualTo(50));
        }

        [Test]
        public void if_player_has_no_game_on_gameweek_gameweek1_value_is_adjusted_to_original_cost()
        {
            //Arrange
            var players = new List<Player>
                              {
                                  new Player
                                      {
                                          NowCost = 60,
                                          OriginalCost = 80,
                                          FutureFixtures = new List<FutureFixture>(),
                                          PastFixtures = new List<PastFixture>
                                                             {
                                                                 new PastFixture {GameWeek = 2, PlayerValueAtTime = 50}
                                                             }
                                      }
                              };

            //Act
            var adjustedPlayers = _timeAdjustor.AdjustPlayersToGameweek(players, 1);

            //Assert
            Assert.That(adjustedPlayers.Single().NowCost, Is.EqualTo(80));
        }

        [Test]
        [TestCase(false, 50)]
        [TestCase(true, 30)]
        public void player_values_are_set_correctly_based_on_when_we_make_transfer_decisions(bool makeTransfersAtStartOfNewGameweek, double expectedPlayerValue)
        {
            // Arrange
            _configSettingsMock.SetupGet(x => x.MakeTransfersAtStartOfNewGameweek).Returns(makeTransfersAtStartOfNewGameweek);
            var oldTeam = TeamCreationHelper.CreateTestTeam();

            //set the player value at 3 million
            var player = _players.Single(p => p.Id == oldTeam.Players.First().Id);
            oldTeam.Players.First().NowCost = 30;
            oldTeam.Players.First().OriginalCost = 30;
            player.PastFixtures.First(f => f.GameWeek == 1).PlayerValueAtTime = 30;

            //set the player to go up in value in gameweek 2 to 5 million
            player.PastFixtures.First(f => f.GameWeek == 2).PlayerValueAtTime = 50;

            //Act
            var adjustedTeam = _timeAdjustor.AdjustTeamToGameweek(oldTeam, _players, 2);

            //Assert
            //value will have risen to 5 million as we are making last minute decision
            Assert.That(adjustedTeam.Players.First().NowCost, Is.EqualTo(expectedPlayerValue));

        }
    }
}
