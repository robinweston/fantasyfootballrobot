using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FantasyFootballRobot.Core.Entities;


namespace FantasyFootballRobot.Tests.Helpers
{
    public static class TeamCreationHelper
    {
        public static Team CreateTestTeam()
        {
            return CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender, Position.Midfielder);
        }

        public static IList<Player> CreatePlayersWithPastFixtures(int pastFixtureCount)
        {
            return new List<Player>
                       {
                           new Player
                               {
                                   PastFixtures = new List<PastFixture>
                                                          {
                                                              new PastFixture {GameWeek = pastFixtureCount}
                                                          }
                               }
                       };
        }

        public static Team CreateTestTeam(int startingDefenders, int startingMidfielders, int startingAttackers,
                                          Position sub1Position, Position sub2Position, Position sub3Position)
        {
            var team = new Team();

            AddPlayersToTestTeam(team, Position.Goalkeeper, 1);
            AddPlayersToTestTeam(team, Position.Defender, startingDefenders);
            AddPlayersToTestTeam(team, Position.Midfielder, startingMidfielders);
            AddPlayersToTestTeam(team, Position.Forward, startingAttackers);

            //subs
            AddPlayersToTestTeam(team, sub1Position, 1);
            AddPlayersToTestTeam(team, sub2Position, 1);
            AddPlayersToTestTeam(team, sub3Position, 1);
            AddPlayersToTestTeam(team, Position.Goalkeeper, 1);

            team.Captain = team.Players.First();
            team.ViceCaptain = team.Players[1];

            return team;
        }

        private static IEnumerable<Player> CreatePlayers(int startingId, Position playerPosition, int playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                int playerId = startingId + i;

                yield return new Player
                                 {
                                     Id = playerId,
                                     Position = playerPosition,
                                     Name = "Player " + playerId + " - " + playerPosition,

                                     //Assign each player a different club
                                     ClubCode = playerId.ToString(CultureInfo.InvariantCulture),
                                     PastSeasons = new List<Season>{new Season{MinutesPlayed = 10, TotalPointsScored = 10}},
                                     PastFixtures = new List<PastFixture>()
                                 };

            }
        }

        private static void AddPlayersToTestTeam(Team team, Position playerPosition, int playerCount)
        {
            var lastPlayerId = team.Players.Any() ? team.Players.Last().Id + 1 : 0;
            var players = CreatePlayers(lastPlayerId, playerPosition, playerCount);
            
            foreach(var player in players)
            {
                team.Players.Add(player);
            }
        }

        public static IList<Player> CreateFuturePlayersForTeam(Team team)
        {
            return team.Players.Select(player => new Player
                                                 {
                                                     Id = player.Id, Position = player.Position, Name = player.Name, PastFixtures = new List<PastFixture>()
                                                 }).ToList();
        }

        public static IList<Player> CreatePlayerList(int goalkeepers, int defenders, int midfielders, int forwards, int startingId = 0)
        {
            var players = new List<Player>();

            players.AddRange(CreatePlayers(startingId, Position.Goalkeeper, goalkeepers));
            players.AddRange(CreatePlayers(players.Max(p => p.Id), Position.Defender, defenders));
            players.AddRange(CreatePlayers(players.Max(p => p.Id), Position.Midfielder, midfielders));
            players.AddRange(CreatePlayers(players.Max(p => p.Id), Position.Forward, forwards));

            return players;
        }
    }
}