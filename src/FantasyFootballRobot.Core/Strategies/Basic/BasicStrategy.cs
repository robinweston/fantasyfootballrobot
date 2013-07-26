using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;

namespace FantasyFootballRobot.Core.Strategies.Basic
{
    public class BasicStrategy : IStrategy
    {
        readonly ILogger _logger;

        public BasicStrategy(ILogger logger)
        {
            _logger = logger;
            _logger.Log(Tag.Strategy, "Using Basic strategy");
        }

        public TransferActions MakeTransfers(SeasonState seasonState)
        {
            //do nothing
            return new TransferActions ();
        }

        public Team PickGameweekTeam(SeasonState seasonState)
        {
            return seasonState.CurrentTeam;
        }

        public Team PickStartingTeam(IList<Player> allPlayers)
        {
            var team = new Team();
            
            //player 3-4-3
            AddPlayerToTeam(team, allPlayers, Position.Goalkeeper);

            AddPlayerToTeam(team, allPlayers, Position.Defender);
            AddPlayerToTeam(team, allPlayers, Position.Defender);
            AddPlayerToTeam(team, allPlayers, Position.Defender);

            AddPlayerToTeam(team, allPlayers, Position.Midfielder);
            AddPlayerToTeam(team, allPlayers, Position.Midfielder);
            AddPlayerToTeam(team, allPlayers, Position.Midfielder);
            AddPlayerToTeam(team, allPlayers, Position.Midfielder);

            AddPlayerToTeam(team, allPlayers, Position.Forward);
            AddPlayerToTeam(team, allPlayers, Position.Forward);
            AddPlayerToTeam(team, allPlayers, Position.Forward);

            AddPlayerToTeam(team, allPlayers, Position.Goalkeeper);
            AddPlayerToTeam(team, allPlayers, Position.Defender);
            AddPlayerToTeam(team, allPlayers, Position.Defender);
            AddPlayerToTeam(team, allPlayers, Position.Midfielder);

            team.Captain = team.Players.First();
            team.ViceCaptain = team.Players[2];

            return team;
        }

        private static void AddPlayerToTeam(Team currentTeam, IEnumerable<Player> allPlayers, Position position)
        {
            var player = allPlayers.Where(p => !p.Name.StartsWith(" ")).OrderBy(p => p.Name).First(p => !currentTeam.Players.Contains(p) && 
                                               p.Position == position &&
                                               currentTeam.Players.Count(p2 => p2.ClubCode == p.ClubCode) < 3);

            currentTeam.Players.Add(player);
        }
    }
}