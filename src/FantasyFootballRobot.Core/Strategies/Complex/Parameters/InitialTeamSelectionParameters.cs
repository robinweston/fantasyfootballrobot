namespace FantasyFootballRobot.Core.Strategies.Complex.Parameters
{
    public class InitialTeamSelectionParameters : IInitialTeamSelectionParameters
    {
        public double ElitismPercentage
        {
            get { return 0.05; }
        }

        public int EliteChromosomesPerGeneration
        {
            get { return (int)(ElitismPercentage * PopulationSize); }
        }

        public int? MaxDegreeOfParallelism
        {
            get
            {
                return null;
            }
        }

        public int MaxGenerations
        {
            get { return 50; }
        }

        public double MutationProbability
        {
            get { return 0.03; }
        }

        public int TopChromosomeFitnessPlateauGameweeksForTermination { get { return 10; } }

        public int PopulationSize
        {
            get { return 10000; }
        }

        public int GamesAtStartOfSeasonToPredict
        {
            get { return 10; }
        }

        public int MinimumPlayerScoreFromPreviousSeasonToBeConsidered
        {
            get { return 50; }
        }

        public int MinimumPlayerMinutesPlayerFromPreviousSeasonToBeConsidered
        {
            get
            {
                //15 games
                return 15 * 90;
            }
        }
    }
}