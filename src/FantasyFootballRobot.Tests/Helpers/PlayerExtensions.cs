using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Tests.Helpers
{
    public static class PlayerExtensions
    {
        public static void SetGameWeekPerformance(this Player player, int gameweek, int minutesPlayed, int points)
        {
            player.PastFixtures = new List<PastFixture>
                                                                                      {
                                                                                          new PastFixture
                                                                                              {
                                                                                                  GameWeek = gameweek,
                                                                                                  MinutesPlayed = minutesPlayed,
                                                                                                  TotalPointsScored = points
                                                                                              }
                                                                                      };
        }
    }
}
