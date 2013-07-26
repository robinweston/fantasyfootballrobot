using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    public class GameweekSimulatorTests
    {
        Team _team;

        [SetUp]
        public void SetUp()
        {
            _gameweekSimulator = new GameweekSimulator(new Mock<ILogger>().Object);
            _team = TeamCreationHelper.CreateTestTeam();
        }

        private IGameweekSimulator _gameweekSimulator;

        [Test]
        public void all_player_performances_are_calculated()
        {
            //Arrange
            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, 1, futurePlayers);

            //Assert
            Assert.That(playerPerformances.Count(), Is.EqualTo(11));
            Assert.That(playerPerformances.Distinct().Count(), Is.EqualTo(11));
        }

        [Test]
        public void captain_scores_double()
        {
            //Arrange
            const int gameweek = 1;

            _team.Captain = _team.Players[4];

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            futurePlayers[4].SetGameWeekPerformance(gameweek, 1, 23);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances[4].TotalPointsScored, Is.EqualTo(46));
            Assert.That(playerPerformances[4].IsCaptain, Is.True);
        }


        [Test]
        public void first_choice_goalkeeper_is_replaced_by_sub_keeper_if_he_plays()
        {
            //Arrange
            const int firstChoiceKeeperId = 0;
            const int reserveKeeperId = 14;
            const int gameweek = 3;

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            futurePlayers.Single(x => x.Id == firstChoiceKeeperId).SetGameWeekPerformance(gameweek,
                                                                                                              minutesPlayed
                                                                                                                  : 0,
                                                                                                              points: 0);
            futurePlayers.Single(x => x.Id == reserveKeeperId).SetGameWeekPerformance(gameweek,
                                                                                                          minutesPlayed:
                                                                                                              5,
                                                                                                          points: 0);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.First().Position, Is.EqualTo(Position.Goalkeeper));
            Assert.That(playerPerformances.First().PlayerId, Is.EqualTo(reserveKeeperId));
            Assert.That(playerPerformances.Any(x => x.PlayerId == firstChoiceKeeperId), Is.False);
        }

        [Test]
        public void if_you_run_out_of_subs_then_players_play_with_zero_points()
        {
            //Arrange
            const int firstChoiceKeeperId = 0;
            const int gameweek = 3;

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.First().PlayerId, Is.EqualTo(firstChoiceKeeperId));
            Assert.That(playerPerformances.First().MinutesPlayed, Is.EqualTo(0));
            Assert.That(playerPerformances.First().TotalPointsScored, Is.EqualTo(0));
        }

        [Test]
        public void multiple_games_in_gameweek_are_added_up()
        {
            //Arrange
            const int firstChoiceKeeperId = 0;
            const int gameweek = 3;

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            //set another player to captain so it doesn't mess with our stats
            _team.Captain = _team.Players[3];

            futurePlayers.Single(x => x.Id == firstChoiceKeeperId).PastFixtures = new List
                <PastFixture>
                                                                                                          {
                                                                                                              new PastFixture
                                                                                                                  {
                                                                                                                      GameWeek
                                                                                                                          =
                                                                                                                          gameweek,
                                                                                                                      MinutesPlayed
                                                                                                                          =
                                                                                                                          90,
                                                                                                                      CleanSheets
                                                                                                                          =
                                                                                                                          4,
                                                                                                                      TotalPointsScored
                                                                                                                          =
                                                                                                                          10,
                                                                                                                      Bonus
                                                                                                                          =
                                                                                                                          3
                                                                                                                  },
                                                                                                              new PastFixture
                                                                                                                  {
                                                                                                                      GameWeek
                                                                                                                          =
                                                                                                                          gameweek,
                                                                                                                      MinutesPlayed
                                                                                                                          =
                                                                                                                          72,
                                                                                                                      CleanSheets
                                                                                                                          =
                                                                                                                          0,
                                                                                                                      TotalPointsScored
                                                                                                                          =
                                                                                                                          3
                                                                                                                  }
                                                                                                          };

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.First().PlayerId, Is.EqualTo(firstChoiceKeeperId));
            Assert.That(playerPerformances.First().MinutesPlayed, Is.EqualTo(162));
            Assert.That(playerPerformances.First().TotalPointsScored, Is.EqualTo(13));
            Assert.That(playerPerformances.First().CleanSheets, Is.EqualTo(4));
            Assert.That(playerPerformances.First().Bonus, Is.EqualTo(3));
        }

        [Test]
        public void non_playing_starting_defender_gets_replaced_by_last_sub_defender_if_required()
        {
            //Arrange
            const int gameweek = 5;

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);
            var teamDefenders = futurePlayers.Where(x => x.Position == Position.Defender);

            //set 2nd and 3rd defenders as playing (1st and 4th don't play)
            teamDefenders.ElementAt(1).SetGameWeekPerformance(gameweek, 90, 0);
            teamDefenders.ElementAt(2).SetGameWeekPerformance(gameweek, 90, 0);

            //set last reserve defender as playing
            teamDefenders.Last().SetGameWeekPerformance(gameweek, 90, 0);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.Count(x => x.Position == Position.Defender), Is.EqualTo(3));
            Assert.That(
                playerPerformances.Any(x => x.PlayerId == teamDefenders.First().Id),
                Is.False);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamDefenders.ElementAt(1).Id), Is.True);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamDefenders.ElementAt(2).Id), Is.True);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamDefenders.ElementAt(3).Id), Is.False);
            Assert.That(
                playerPerformances.Any(x => x.PlayerId == teamDefenders.Last().Id),
                Is.True);
        }

        [Test]
        public void non_playing_starting_midfielders_get_replaced_by_two_sub_midfielders()
        {
            //Arrange
            const int gameweek = 5;
            Team team = TeamCreationHelper.CreateTestTeam(4, 3, 3, Position.Midfielder, Position.Midfielder,
                                                          Position.Defender);

            IList<Player> futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(team);
            IEnumerable<Player> teamMidfielders = futurePlayers.Where(x => x.Position == Position.Midfielder);

            //set all the defenders as playing so they don't get replaced
            futurePlayers.Where(x => x.Position == Position.Defender).ToList().ForEach(
                x => x.SetGameWeekPerformance(gameweek, 90, 0));

            //set 2nd, 4th (reserve), and 5th (reserve) midfielders playing (1st and 3rd don't play)
            teamMidfielders.ElementAt(1).SetGameWeekPerformance(gameweek, 90, 0);
            teamMidfielders.ElementAt(3).SetGameWeekPerformance(gameweek, 90, 0);
            teamMidfielders.ElementAt(4).SetGameWeekPerformance(gameweek, 90, 0);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.Count(x => x.Position == Position.Midfielder), Is.EqualTo(3));
            Assert.That(
                playerPerformances.Any(x => x.PlayerId == teamMidfielders.First().Id),
                Is.False);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamMidfielders.ElementAt(1).Id), Is.True);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamMidfielders.ElementAt(2).Id), Is.False);
            Assert.That(
                playerPerformances.Any(
                    x => x.PlayerId == teamMidfielders.ElementAt(3).Id), Is.True);
            Assert.That(
                playerPerformances.Any(x => x.PlayerId == teamMidfielders.Last().Id),
                Is.True);
        }

        [Test]
        public void player_with_no_match_on_gameweek_gets_replaced()
        {
            //Arrange
            const int firstChoiceKeeperId = 0;
            const int reserveKeeperId = 14;
            const int gameweek = 3;

            IList<Player> futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            futurePlayers.Single(x => x.Id == firstChoiceKeeperId).PastFixtures.Clear();

            futurePlayers.Single(x => x.Id == reserveKeeperId).SetGameWeekPerformance(gameweek,
                                                                                                          minutesPlayed:
                                                                                                              5,
                                                                                                          points: 0);
            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.First().Position, Is.EqualTo(Position.Goalkeeper));
            Assert.That(playerPerformances.First().PlayerId, Is.EqualTo(reserveKeeperId));
            Assert.That(playerPerformances.Any(x => x.PlayerId == firstChoiceKeeperId), Is.False);
        }

        [Test]
        public void stats_are_added_up_correctly_for_a_player_playing_multiple_games()
        {
            //Arrange
            const int gameweek = 1;

            var futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            //set another player to captain so it doesn't mess with our stats
            _team.Captain = _team.Players[3];

            //no defenders can play
            futurePlayers.First().PastFixtures = new List<PastFixture>
                                                     {
                                                         new PastFixture
                                                             {
                                                                 GameWeek = gameweek,
                                                                 MinutesPlayed = 1,
                                                                 TotalPointsScored = 2,
                                                                 Assists = 3,
                                                                 Bonus = 4,
                                                                 CleanSheets = 5,
                                                                 GoalsScored = 6,
                                                                 GoalsConceded = 7,
                                                                 OwnGoals = 8,
                                                                 PenaltiesMissed = 9,
                                                                 PenaltiesSaved = 10,
                                                                 RedCards = 11,
                                                                 Saves = 12,
                                                                 YellowCards = 13
                                                             },
                                                         new PastFixture
                                                             {
                                                                 GameWeek = gameweek,
                                                                 MinutesPlayed = 1,
                                                                 TotalPointsScored = 2,
                                                                 Assists = 3,
                                                                 Bonus = 4,
                                                                 CleanSheets = 5,
                                                                 GoalsScored = 6,
                                                                 GoalsConceded = 7,
                                                                 OwnGoals = 8,
                                                                 PenaltiesMissed = 9,
                                                                 PenaltiesSaved = 10,
                                                                 RedCards = 11,
                                                                 Saves = 12,
                                                                 YellowCards = 13
                                                             }
                                                     };

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            var player = playerPerformances.First();
            Assert.That(player.MinutesPlayed, Is.EqualTo(2));
            Assert.That(player.TotalPointsScored, Is.EqualTo(4));
            Assert.That(player.Assists, Is.EqualTo(6));
            Assert.That(player.Bonus, Is.EqualTo(8));
            Assert.That(player.CleanSheets, Is.EqualTo(10));
            Assert.That(player.GoalsScored, Is.EqualTo(12));
            Assert.That(player.GoalsConceded, Is.EqualTo(14));
            Assert.That(player.OwnGoals, Is.EqualTo(16));
            Assert.That(player.PenaltiesMissed, Is.EqualTo(18));
            Assert.That(player.PenaltiesSaved, Is.EqualTo(20));
            Assert.That(player.RedCards, Is.EqualTo(22));
            Assert.That(player.Saves, Is.EqualTo(24));
            Assert.That(player.YellowCards, Is.EqualTo(26));
        }

        [Test]
        public void team_switches_to_3_4_3_if_not_enough_defenders_players_are_available()
        {
            //Arrange
            const int gameweek = 1;

            //create team playing 5_4_1
            Team team = TeamCreationHelper.CreateTestTeam(5, 4, 1, Position.Forward, Position.Midfielder,
                                                          Position.Forward);

            IList<Player> futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(team);

            //no defenders can play

            //all mids apart from 1 can play
            futurePlayers.Where(x => x.Position == Position.Forward).Skip(1).ToList().ForEach(
                x => x.SetGameWeekPerformance(gameweek, 90, 0));

            //all forwards play          
            futurePlayers.Where(x => x.Position == Position.Forward).ToList().ForEach(
                x => x.SetGameWeekPerformance(gameweek, 90, 0));

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances.Count(x => x.Position == Position.Defender), Is.EqualTo(3));
            Assert.That(playerPerformances.Count(x => x.Position == Position.Midfielder), Is.EqualTo(4));
            Assert.That(playerPerformances.Count(x => x.Position == Position.Forward), Is.EqualTo(3));
        }

        [Test]
        public void vice_captain_replaces_captain_if_he_does_not_play()
        {
            //Arrange
            const int gameweek = 1;

            _team.Captain = _team.Players[4];
            _team.ViceCaptain = _team.Players[5];

            IList<Player> futurePlayers = TeamCreationHelper.CreateFuturePlayersForTeam(_team);

            //only the vice captain plays
            futurePlayers[5].SetGameWeekPerformance(gameweek, 1, 33);

            //Act
            var playerPerformances = _gameweekSimulator.CalculatePlayerPerformances(_team, gameweek, futurePlayers);

            //Assert
            Assert.That(playerPerformances[4].TotalPointsScored, Is.EqualTo(0));
            Assert.That(playerPerformances[4].IsCaptain, Is.False);
            Assert.That(playerPerformances[5].TotalPointsScored, Is.EqualTo(66));
            Assert.That(playerPerformances[5].IsCaptain, Is.True);
        }
    }
}