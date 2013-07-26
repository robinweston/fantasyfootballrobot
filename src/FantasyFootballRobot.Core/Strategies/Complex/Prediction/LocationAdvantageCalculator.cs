using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class LocationAdvantageCalculator : ILocationAdvantageCalculator
    {
        public HomeAdvantageResult CalculateLocationAdvantage(IList<Player> players)
        {
            var result = new HomeAdvantageResult
                             {
                                 AveragePointsPerMinute = GetPointsPerMinute(players, null),
                                 HomeMatchPointsPerMinute = GetPointsPerMinute(players, true),
                                 AwayMatchPointsPerMinute = GetPointsPerMinute(players, false)
                             };
            return result;
        }

        public double GetPointsPerMinute(IList<Player> players, bool? homeFixtures)
        {
            var fixtures = players.SelectMany(p => p.PastFixtures).ToList();
            if(homeFixtures.HasValue)
            {
                fixtures = fixtures.Where(pf => pf.Home == homeFixtures).ToList();
            }
            var totalMinutes = fixtures.Sum(f => f.MinutesPlayed);
            var totalPoints = fixtures.Sum(f => f.TotalPointsScored);
            return totalPoints/(double)totalMinutes;
        }
    }
}