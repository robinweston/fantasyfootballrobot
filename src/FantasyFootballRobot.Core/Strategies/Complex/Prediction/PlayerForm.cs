namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class PlayerForm
    {
        public double NormalisedPointsPerGame { get; set; }

        public int PreviousSeasonTotalPointsScored { get; set; }
        public int PreviousSeasonMinutesPlayed { get; set; }
    }
}