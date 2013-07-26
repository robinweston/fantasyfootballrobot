namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class TeamStrength
    {
        public int SamplePlayers { get; set; }
        public int TotalMinutes { get; set; }
        public int TotalPointsScored { get; set; }
        public double TeamStrengthMultiplier { get; set; }
        public double PointsPerMinute { get; set; }

        public bool ArtificalPointsPerMinuteDueToLowSampleSize { get; set; }
    }
}