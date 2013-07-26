namespace FantasyFootballRobot.Core.Strategies.Genetic
{
    public interface IGeneticParameters
    {
        int PopulationSize { get;  }
        int MaxGenerations { get;  }
        double ElitismPercentage { get;  }
        double MutationProbability { get;  }
        int TopChromosomeFitnessPlateauGameweeksForTermination { get; }
        int EliteChromosomesPerGeneration { get; }
        int? MaxDegreeOfParallelism { get; }
    }
}
