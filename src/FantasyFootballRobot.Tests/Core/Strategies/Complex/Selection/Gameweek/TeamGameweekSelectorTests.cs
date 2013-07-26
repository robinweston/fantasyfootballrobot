using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Core.Validation;
using FantasyFootballRobot.Tests.Helpers;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Selection.Gameweek
{
    [TestFixture]
    class TeamGameweekSelectorTests
    {
        private ITeamGameweekSelector _teamGameweekSelector;
        private IList<PredictedPlayerScore> _predictedPlayerScores;
            
        [SetUp]
        public void SetUp()
        {
            _predictedPlayerScores = CreatePredictedPlayerScores();
            _teamGameweekSelector = new TeamGameweekSelector();
        }

        private IList<PredictedPlayerScore> CreatePredictedPlayerScores()
        {
            var players = TeamCreationHelper.CreateTestTeam().Players;

            //assign unique predicted scores to each player
            return players.Select(p => new PredictedPlayerScore{Player = p, PredictedScore = p.Id}).ToList();
        }

        [Test]
        public void selector_returns_valid_team()
        {
            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Validity, Is.EqualTo(TeamValidationStatus.Valid));
        }

        [Test]
        public void top_scoring_goalkeeper_is_selected()
        {
            // Arrange
            var topGk = _predictedPlayerScores.First(ps => ps.Player.Position == Position.Goalkeeper);
            topGk.PredictedScore = 5.71;
            var bottomGk = _predictedPlayerScores.Last(ps => ps.Player.Position == Position.Goalkeeper);
            bottomGk.PredictedScore = 5.70;

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Players.First(p => p.Position == Position.Goalkeeper), Is.EqualTo(topGk.Player));
        }

        [Test]
        public void top_scoring_three_defenders_are_selected()
        {
            // Arrange
            var defenders = _predictedPlayerScores.Where(ps => ps.Player.Position == Position.Defender).ToList();
            defenders[0].PredictedScore = 20;
            defenders[1].PredictedScore = 19;
            defenders[2].PredictedScore = 110;
            defenders[3].PredictedScore = 150;
            defenders[4].PredictedScore = 0;

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Players.Contains(defenders[0].Player));
            Assert.That(selection.Team.Players.Contains(defenders[2].Player));
            Assert.That(selection.Team.Players.Contains(defenders[3].Player));
            
        }

        [Test]
        public void substitutes_are_ordered_by_predicted_score()
        {
            // Arrange

            //assign unique predicted scores to each player

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            var subs = selection.Team.Players.Skip(11).ToList();
            for (int i = 0; i < subs.Count() - 1; i++)
            {
                var thisSubScore = _predictedPlayerScores.Single(ps => ps.Player == subs[i]).PredictedScore;
                var nextSubScore = _predictedPlayerScores.Single(ps => ps.Player == subs[i + 1]).PredictedScore;

                Assert.That(thisSubScore > nextSubScore);
            }
        }

        [Test]
        public void top_scoring_forward_is_selected()
        {
            // Arrange

            //set all non forwards to score highly
            foreach(var score in _predictedPlayerScores.Where(p => p.Player.Position != Position.Forward))
            {
                score.PredictedScore = 50;
            }

            //set one forward to be more than the others
            var topForward = _predictedPlayerScores.Last(p => p.Player.Position == Position.Forward);
            topForward.PredictedScore = 60;

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Players.Take(11).Count(p => p.Position == Position.Forward), Is.EqualTo(1));
            Assert.That(selection.Team.Players.First(p => p.Position == Position.Forward), Is.EqualTo(topForward.Player));          
        }
        

        [Test]
        public void selector_returns_top_predicted_players_after_mandatory_positions_have_been_filled()
        {
            // Arrange
          
            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            var startingPlayers = selection.Team.Players.Take(11).ToList();
            
            //remove all mandatory players
            var topScoringMandatoryPlayers =
                _predictedPlayerScores.Where(ps => ps.Player.Position == Position.Goalkeeper).OrderByDescending(
                    ps => ps.PredictedScore).Take(1).
                    Union(_predictedPlayerScores.Where(ps => ps.Player.Position == Position.Defender).OrderByDescending(
                    ps => ps.PredictedScore).Take(3)).
                    Union(_predictedPlayerScores.Where(ps => ps.Player.Position == Position.Forward).OrderByDescending(
                    ps => ps.PredictedScore).Take(1)).Select(ps => ps.Player);

            foreach(var mandatoryPlayer in topScoringMandatoryPlayers)
            {
                startingPlayers.Remove(mandatoryPlayer);
            }

            Assert.That(startingPlayers.Count, Is.EqualTo(6));

            var nonKeeperSubs = selection.Team.Players.Skip(11).Where(p => p.Position != Position.Goalkeeper);
            var highestNonKeeperSubScore =
                _predictedPlayerScores.Where(ps => nonKeeperSubs.Contains(ps.Player)).Max(ps => ps.PredictedScore);
            foreach(var remainingStartingPlayer in startingPlayers)
            {
                var playerPredictedScore =
                    _predictedPlayerScores.Single(ps => ps.Player == remainingStartingPlayer).PredictedScore;
                Assert.That(playerPredictedScore, Is.GreaterThan(highestNonKeeperSubScore));
            }
        }

        [Test]
        public void selector_will_leave_out_higher_scoring_player_if_it_means_filling_mandatory_position()
        {
            // Arrange

            //make all forwards, goalkeepers and midfielders predicted scores high. Defenders left at 0
            foreach (var prediction in _predictedPlayerScores.Where(ps => ps.Player.Position != Position.Defender))
            {
                prediction.PredictedScore = 80;
            }

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Players.Take(11).Count(p => p.Position == Position.Defender), Is.EqualTo(3));
            Assert.That(selection.Team.Players.Take(11).Count(p => p.Position == Position.Goalkeeper), Is.EqualTo(1));
            Assert.That(selection.Team.Validity, Is.EqualTo(TeamValidationStatus.Valid));
        }
     
        [Test]
        public void selector_returns_correct_expected_points()
        {
            // Arrange

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            var expectedTotal = 0.0;
            foreach(var startingPlayer in selection.Team.Players.Take(11))
            {
                var playerExpectedScore = _predictedPlayerScores.Single(p => p.Player == startingPlayer).PredictedScore;
                if(startingPlayer == selection.Team.Captain)
                {
                    expectedTotal += (playerExpectedScore*2);
                } else
                {
                    expectedTotal += playerExpectedScore;
                }
            }
            Assert.That(selection.PredictedTotalTeamScore, Is.EqualTo(expectedTotal));
        }

        [Test]
        public void top_scoring_player_is_selected_to_be_captain()
        {
            // Arrange

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            var expectedCaptain = _predictedPlayerScores.OrderByDescending(ps => ps.PredictedScore).First().Player;
            Assert.That(selection.Team.Captain, Is.EqualTo(expectedCaptain));
            
        }

        [Test]
        public void second_highest_scoring_player_is_selected_to_be_vice_captain()
        {
            // Arrange

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            var expectedViceCaptain = _predictedPlayerScores.OrderByDescending(ps => ps.PredictedScore).ElementAt(1).Player;
            Assert.That(selection.Team.ViceCaptain, Is.EqualTo(expectedViceCaptain));
        }

        [Test]
        public void only_one_goalkeeper_selected_in_starting_eleven()
        {
            // Arrange
            foreach(var player in _predictedPlayerScores.Where(ps => ps.Player.Position == Position.Goalkeeper))
            {
                player.PredictedScore = 100;
            }

            // Act
            var selection = _teamGameweekSelector.SelectStartingTeamForGameweek(_predictedPlayerScores);

            // Assert
            Assert.That(selection.Team.Players.Take(11).Count(p => p.Position == Position.Goalkeeper), Is.EqualTo(1));
        }
    }
}
