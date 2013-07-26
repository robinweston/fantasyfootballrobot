using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Exceptions;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;

namespace FantasyFootballRobot.Core.Simulators
{
    public class TimeAdjustor : ITimeAdjustor
    {
        private readonly ILogger _logger;
        private readonly IConfigurationSettings _configSettings;

        public TimeAdjustor(ILogger logger, IConfigurationSettings configSettings)
        {
            _logger = logger;
            _configSettings = configSettings;
        }

        public Team AdjustTeamToGameweek(Team team, IList<Player> allUpToDatePlayers, int gameweek)
        {
            if (team == null) throw new ArgumentNullException("team");
            if (gameweek < 1 || gameweek > 38) throw new ArgumentException("gameweek");
            if (allUpToDatePlayers == null) throw new ArgumentNullException("allUpToDatePlayers");

            _logger.Log(Tag.Adjusting, string.Concat("Adjusting team to gameweek ", gameweek));

            var updatedTeam = new Team();

            foreach (var playerInTeam in team.Players)
            {
                //find player that is up to date to adjust
                Player inTeam = playerInTeam;
                
                var playerToAdjust = allUpToDatePlayers.Single(x => x.Id == inTeam.Id);

                //bring them back to the current gameweek
                var adjustedPlayer = AdjustPlayer(playerToAdjust, gameweek);

                if(adjustedPlayer.NowCost != playerInTeam.NowCost)
                {
                    _logger.Log(Tag.Adjusting, string.Format("{0} value has {1} from {2} to {3}", playerInTeam.Name, adjustedPlayer.NowCost > playerInTeam.NowCost ? "increased" : "decreased", playerInTeam.NowCost.ToMoney(), adjustedPlayer.NowCost.ToMoney()));
                }

                //now copy over anything that is special about a player being in your team

                //todo: check this is correct when we find out property for purchase cost
                adjustedPlayer.OriginalCost = playerInTeam.OriginalCost;

                updatedTeam.Players.Add(adjustedPlayer);
            }

            //now switch over captains
            updatedTeam.Captain = updatedTeam.Players.Single(p => p.Id == team.Captain.Id);
            updatedTeam.ViceCaptain = updatedTeam.Players.Single(p => p.Id == team.ViceCaptain.Id);

            return updatedTeam;
        }

        public IList<Player> AdjustPlayersToGameweek(IList<Player> allUpToDatePlayers, int gameweek)
        {
            if(allUpToDatePlayers == null)throw new ArgumentNullException("allUpToDatePlayers");
            if (gameweek < 1 || gameweek > 38) throw new ArgumentException("gameweek");

            _logger.Log(Tag.Adjusting, string.Concat("Adjusting ", allUpToDatePlayers.Count, " players to gameweek ", gameweek));

            return allUpToDatePlayers.Select(player => AdjustPlayer(player, gameweek)).ToList();
        }

        /// <summary>
        /// Adjusting to a gameweek means to the start of that Gameweek, before any matches are played
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gameweek"></param>
        /// <returns></returns>
        private Player AdjustPlayer(Player player, int gameweek)
        {           
            if (player.FutureFixtures.Any(x => x.GameWeek < gameweek))
            {
                throw new TimeAdjustorException("Can't adjust player to a fixture that is yet to happen");
            }

            var clonedPlayer = player.DeepClone();

            AdjustPlayerValue(clonedPlayer, gameweek);
            AdjustFixtures(clonedPlayer, gameweek);
            clonedPlayer.Status = Status.Available;
            return clonedPlayer;
        }

        private void AdjustPlayerValue(Player player, int gameweek)
        {
            //if we are making decisions at start of gameweek 2, then the player values will still be the same as gameweek 1
            //if we are making decisions at end of gameweek 2, then the values will have risen to gameweek 2 values
            var gameweekToSetTo = _configSettings.MakeTransfersAtStartOfNewGameweek ? gameweek - 1 : gameweek;

            gameweekToSetTo = Math.Max(gameweekToSetTo, 1);

            //the value is taken from Gameweek X itself
            var pastFixture = player.PastFixtures.Where(x => x.GameWeek <= gameweekToSetTo).OrderByDescending(x => x.GameWeek).FirstOrDefault();
            player.NowCost = pastFixture != null ? pastFixture.PlayerValueAtTime : player.OriginalCost;
        }

        private static void AdjustFixtures(Player player, int gameweek)
        {
            //if we are adjusting to gameweek X, then all fixtures from gameweek X in the JSON will be in the future
            var pastFixturesToBecomeFuture = player.PastFixtures.Where(x => x.GameWeek >= gameweek).ToList();
            foreach(var fixtureToConvert in pastFixturesToBecomeFuture)
            {
                player.PastFixtures.Remove(fixtureToConvert);
                var convertedFixture = Mapper.Map<PastFixture, FutureFixture>(fixtureToConvert);
                player.FutureFixtures.Add(convertedFixture);
            }
            player.FutureFixtures = player.FutureFixtures.OrderBy(x => x.GameWeek).ThenBy(x => x.Date).ToList();
        }        
    }
}