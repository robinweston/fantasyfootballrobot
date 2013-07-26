namespace FantasyFootballRobot.Core.Strategies.Complex.Parameters
{
    internal class PredictorParameters : IPredictorParameters
    {
        public double HomeAdvantageMultiplier
        {
            get
            {
                return 1.18;
            }
        }

        public double AwayAdvantageMultiplier
        {
            get
            {
                return 0.92;
            }
        }

        public int MinMinutesPlayedLastSeasonToCalculateClubForm
        {
            get
            {
                //at least half a team for the whole season
                return 6 * 90 * 38;
            }
        }

        public int MinMinutesPlayedLastSeasonToCalculatePlayerForm
        {
            get
            {
                // at least half a season
                return 90 * 18;
            }
        }

        public int PastGamesUsedToCalculatePlayerForm
        {
            get
            {
                return 3;
            }
        }

        public int FutureGameweeksUsedToCalculatePlayerForm
        {
            get
            {
                return 5;
            }
        }

        public double FutureGameweekMultiplier
        {
            get
            {
                return 0.5;
            }
        }

        public double PreviousGameMultiplier
        {
            get
            {
                return 0.75;
            }
        }
    }
}