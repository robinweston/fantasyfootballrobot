namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class HomeAdvantageResult
    {
        public double AveragePointsPerMinute { get; set; }
        public double HomeMatchPointsPerMinute { get; set; }
        public double AwayMatchPointsPerMinute { get; set; }

        public double HomeMatchMultiplier
        {
            get { return HomeMatchPointsPerMinute/AveragePointsPerMinute; }
        }

        public double AwayMatchMultiplier
        {
            get { return AwayMatchPointsPerMinute/AveragePointsPerMinute; }
        }
    }
}