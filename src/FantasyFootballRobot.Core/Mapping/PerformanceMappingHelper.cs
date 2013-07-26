using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    internal static class PerformanceMappingHelper
    {
        public static PerformanceBase UpdatePastPerformanceStats(PerformanceBase performance, object[] arr, int startingIndex)
        {
            performance.MinutesPlayed = (int) ((long) arr[startingIndex]);
            performance.GoalsScored = (int)((long)arr[++startingIndex]);
            performance.Assists = (int)((long)arr[++startingIndex]);
            performance.CleanSheets = (int)((long)arr[++startingIndex]);
            performance.GoalsConceded = (int)((long)arr[++startingIndex]);
            performance.OwnGoals = (int)((long)arr[++startingIndex]);
            performance.PenaltiesSaved = (int)((long)arr[++startingIndex]);
            performance.PenaltiesMissed = (int)((long)arr[++startingIndex]);
            performance.YellowCards = (int)((long)arr[++startingIndex]);
            performance.RedCards = (int)((long)arr[++startingIndex]);
            performance.Saves = (int)((long)arr[++startingIndex]);
            performance.Bonus = (int)((long)arr[++startingIndex]);

            return performance;
        }

        public static int ParsePlayerValueAtTime(long l)
        {
            return ((int)l);
        }

        public static PlayerGameweekPerformance CreatePlayerPerformanceFromFixtures(int playerId, string playerName, Position playerPosition, IEnumerable<PastFixture> fixtures)
        {
            var performance = new PlayerGameweekPerformance
            {
                Position = playerPosition,
                PlayerId = playerId,
                PlayerName = playerName,
                Assists = fixtures.Sum(x => x.Assists),
                Bonus = fixtures.Sum(x => x.Bonus),
                CleanSheets = fixtures.Sum(x => x.CleanSheets),
                GoalsConceded = fixtures.Sum(x => x.GoalsConceded),
                GoalsScored = fixtures.Sum(x => x.GoalsScored),
                MinutesPlayed = fixtures.Sum(x => x.MinutesPlayed),
                OppositionGoals = fixtures.Sum(x => x.OppositionGoals),
                OwnGoals = fixtures.Sum(x => x.OwnGoals),
                PenaltiesMissed = fixtures.Sum(x => x.PenaltiesMissed),
                PenaltiesSaved = fixtures.Sum(x => x.PenaltiesSaved),
                RedCards = fixtures.Sum(x => x.RedCards),
                Saves = fixtures.Sum(x => x.Saves),
                TotalPointsScored = fixtures.Sum(x => x.TotalPointsScored),
                YellowCards = fixtures.Sum(x => x.YellowCards)
            };

            return performance;
        }
    }

}
