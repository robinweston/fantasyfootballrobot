using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;

namespace FantasyFootballRobot.Core.Logging
{
    static class LogHelper
    {
        public static void LogTeam(Team team, ILogger logger)
        {       
            LogTeamFormation(team, logger);

            var orderedPlayers = team.Players.OrderBy(p => p.Position).ToList();
            for (var i = 0; i < orderedPlayers.Count; i++)
            {
                if(i == 11)
                {
                    logger.Log(Tag.Team, "---");
                }
                var player = orderedPlayers[i];
                LogPlayer(player, team, logger);
            }
            LogTeamCost(team, logger);   
        }

        public static void LogTeamCost(Team team, ILogger logger, bool important = false)
        {
            logger.Log(Tag.Team, string.Concat("Total team cost: ", team.Players.Sum(p => p.NowCost).ToMoney()), important);
        }

        public static void LogTeamFormation(Team team, ILogger logger)
        {
            var formation = GetFormation(team);
            logger.Log(Tag.Team, string.Concat("Formation: ", formation));
        }

        public static void LogPlayer(Player player, Team team, ILogger logger, string extraData = "", bool important = false)
        {
            var captainText = string.Empty;
            if(player == team.Captain)
            {
                captainText = " (c)";
            } else if(player == team.ViceCaptain)
            {
                captainText = " (v)";
            }

            logger.Log(Tag.Team, string.Concat(player.Position.ToString().Substring(0, 1), " ", player.Name, "(", player.Id, ")", captainText, " ", player.ClubCode, " ", player.NowCost.ToMoney(), extraData), important);
        }

        public static string GetFormation(Team team)
        {
            var defenders = team.Players.Take(11).Count(p => p.Position == Position.Defender);
            var midfielders = team.Players.Take(11).Count(p => p.Position == Position.Midfielder);
            var forwards = team.Players.Take(11).Count(p => p.Position == Position.Forward);

            return string.Format("{0}-{1}-{2}", defenders, midfielders, forwards);
        }

        public static string GetFormation(IList<PlayerGameweekPerformance> performances)
        {
            var defenders = performances.Take(11).Count(p => p.Position == Position.Defender);
            var midfielders = performances.Take(11).Count(p => p.Position == Position.Midfielder);
            var forwards = performances.Take(11).Count(p => p.Position == Position.Forward);

            return string.Format("{0}-{1}-{2}", defenders, midfielders, forwards);
        }

        public static void LogTeamSelection(TeamSelection teamSelection, int gameweek, List<PredictedPlayerScore> predictedPlayerPoints, ILogger logger, bool important = false)
        {
            logger.Log(Tag.Prediction, string.Format("Selected team for gameweek {0}", gameweek), important);
            logger.Log(Tag.Prediction, string.Format("Predicted points: {0}", teamSelection.PredictedTotalTeamScore.ToFormatted()), important);
            LogTeamFormation(teamSelection.Team, logger);

            for (var i = 0; i < teamSelection.Team.Players.Count; i++)
            {
                if (i == 11)
                {
                    logger.Log(Tag.Team, "---");
                }

                var player = teamSelection.Team.Players[i];
                var fixtures = player.FutureFixtures.Where(x => x.GameWeek == gameweek);
                var extraData = new StringBuilder(" ");
                foreach (var fixture in fixtures)
                {
                    extraData.Append(fixture.OppositionClubCode);
                    extraData.Append(fixture.Home ? "(H) " : "(A) ");
                }
                var playerGameweekPoints = predictedPlayerPoints.Single(p => p.Player == player).PredictedScore;
                extraData.Append(playerGameweekPoints.ToFormatted());
                if (teamSelection.Team.Captain == player)
                {
                    extraData.Append("(");
                    extraData.Append((playerGameweekPoints * 2).ToFormatted());
                    extraData.Append(")");
                }

                LogPlayer(player, teamSelection.Team, logger, extraData.ToString(), important);
            }

            LogTeamCost(teamSelection.Team, logger, important);
        }

        public static void LogTransferActions(FitTransferActions transferActions, ILogger logger)
        {
            logger.Log(Tag.Strategy, string.Format("Fitness (predicted points for next gameweeks) : {0}", transferActions.Fitness.GetValueOrDefault().ToFormatted()));
            for (var i = 0; i < transferActions.Transfers.Count; i++)
            {
                var transfer = transferActions.Transfers[i];

                logger.Log(Tag.Transfers, string.Format("Transfer {0} of {1}", i + 1, transferActions.Transfers.Count));
                LogTransfer(transfer, logger);                          
            }

            if (transferActions.PlayStandardWildcard)
            {
                logger.Log(Tag.Transfers, "Playing standard wildcard");
            }

            if (transferActions.PlayTransferWindowWildcard)
            {
                logger.Log(Tag.Transfers, "Playing transfer window wildcard");
            }
        }

        public static void LogTransfer(Transfer transfer, ILogger logger, bool important = false)
        {
            logger.Log(Tag.Transfers, string.Format("Transferring in {0} ({1}) for {2} ({3})", transfer.PlayerIn.Name, transfer.PlayerIn.NowCost.ToMoney(), transfer.PlayerOut.Name, transfer.PlayerOut.NowCost.ToMoney()), important);         
        }

        public static void LogTopTransferActions(List<FitTransferActions> topTransferActions, ILogger logger)
        {
            for (var i = 0; i < topTransferActions.Count; i++)
            {
                logger.Log(Tag.Transfers, string.Format("Top transfer actions {0}", i + 1));

                var transferActions = topTransferActions[i];
                LogTransferActions(transferActions, logger);

                logger.Log(Tag.Strategy, string.Format("Fitness (predicted points for next gameweeks) : {0}", transferActions.Fitness.GetValueOrDefault().ToFormatted()));
               
            }
        }
    }
}
