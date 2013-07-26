using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Constants;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek
{
    public class TeamGameweekSelector : ITeamGameweekSelector
    {
        public TeamSelection SelectStartingTeamForGameweek(IList<PredictedPlayerScore> predictedPlayerScores)
        {
            if(predictedPlayerScores.Count != 15)throw new Exception("Incorrect number of players to select starting team from");

            var selection = new TeamSelection
                                {
                                    Team = SelectTeam(predictedPlayerScores)
                                };
            selection.PredictedTotalTeamScore = CalculatePredictedTeamScore(
                predictedPlayerScores.ToDictionary(ps => ps.Player.Id, ps => ps.PredictedScore), 
                selection.Team);

            return selection;
        }

        private static Team SelectTeam(IList<PredictedPlayerScore> predictedPlayerScores)
        {
            var mandatoryPlayers = GetMandatoryPlayers(predictedPlayerScores);

            var playersNotYetSelected = predictedPlayerScores.Where(ps => !mandatoryPlayers.ContainsKey(ps.Player.Id)).ToList();

            mandatoryPlayers = FillRemainingStartingPositions(mandatoryPlayers, playersNotYetSelected);

            var subs = predictedPlayerScores.Where(ps => !mandatoryPlayers.ContainsKey(ps.Player.Id)).OrderByDescending(ps => ps.PredictedScore);

            var players = mandatoryPlayers.Values.Select(p => p.Player).Concat(subs.Select(x => x.Player)).ToList();

            var orderedScores = mandatoryPlayers.Values.OrderByDescending(x => x.PredictedScore).Take(2);
            var captain = orderedScores.First().Player;
            var viceCaptain = orderedScores.Last().Player;

            return new Team
                           {
                               Players = players,
                               Captain = captain,
                               ViceCaptain = viceCaptain
                           };
        }

        private static IDictionary<int, PredictedPlayerScore> GetMandatoryPlayers(IList<PredictedPlayerScore> predictedPlayerScores)
        {
            var mandatoryPlayers = new Dictionary<int, PredictedPlayerScore>(1 + GameConstants.MinimumDefendersInStartingTeam + GameConstants.MinimumForwardsInStartingTeam);

            var goalkeeper = SelectMandatoryPlayer(predictedPlayerScores, Position.Goalkeeper);
            mandatoryPlayers.Add(goalkeeper.Player.Id, goalkeeper);

            var defenders = SelectMandatoryPlayers(predictedPlayerScores, Position.Defender, GameConstants.MinimumDefendersInStartingTeam);
            foreach (var defender in defenders)
            {
                mandatoryPlayers.Add(defender.Player.Id, defender);
            }

            if (GameConstants.MinimumForwardsInStartingTeam != 1)
            {
                throw new ArgumentException("Game rules have changed regarding forwards in team");
            }

            var forward = SelectMandatoryPlayer(predictedPlayerScores, Position.Forward);
            mandatoryPlayers.Add(forward.Player.Id, forward);

            return mandatoryPlayers;
        }

        private static IDictionary<int, PredictedPlayerScore> FillRemainingStartingPositions(IDictionary<int, PredictedPlayerScore> currentSelectedPlayers, IList<PredictedPlayerScore> playersNotYetSelected)
        {
            if (currentSelectedPlayers.Count() != 5) throw new ArgumentException("Should be 5 mandatory players already selected");
            if (playersNotYetSelected.Count() != 10) throw new ArgumentException("Should be 10 players available to complete team");

            const int PlayerCountRequired = 11 - 1 - GameConstants.MinimumDefendersInStartingTeam -
                                            GameConstants.MinimumForwardsInStartingTeam;

            var playersToSelect = playersNotYetSelected.Where(ps => ps.Player.Position != Position.Goalkeeper).OrderByDescending(ps => ps.PredictedScore).Take(PlayerCountRequired);
            foreach (var player in playersToSelect)
            {
                currentSelectedPlayers.Add(player.Player.Id, player);
            }

            return currentSelectedPlayers;
        }

        private static PredictedPlayerScore SelectMandatoryPlayer(IEnumerable<PredictedPlayerScore> predictedPlayerScores, Position position)
        {
            return
                predictedPlayerScores.Where(p => p.Player.Position == position).OrderByDescending(
                    p => p.PredictedScore).First();
        }

        private static IEnumerable<PredictedPlayerScore> SelectMandatoryPlayers(IEnumerable<PredictedPlayerScore> predictedPlayerScores, Position position, int playersRequired)
        {
            return
                predictedPlayerScores.Where(p => p.Player.Position == position).OrderByDescending(
                    p => p.PredictedScore).Take(playersRequired);
        }

        private static double CalculatePredictedTeamScore(IDictionary<int, double> predictedPlayerScores, Team startingTeam)
        {
            return startingTeam.Players.Take(11).Sum(p =>
                                                         {
                                                             var predictedScore = predictedPlayerScores[p.Id];
                                                             return p == startingTeam.Captain
                                                                        ? predictedScore*2
                                                                        : predictedScore;
                                                         });
        }
    }
}