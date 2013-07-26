using System.Collections.Generic;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Tests.Core.Strategies.Genetic
{
    public class StubChromosome : IChromosome
    {
        public bool IsEliteChromosome { get; set; }
        public bool IsMutation { get; set; }
        public bool IsCrossover { get; set; }

        public static IList<StubChromosome> CreateIntitialPopulation()
        {
            return new List<StubChromosome>
                             {
                                 new StubChromosome {Fitness = 1}, 
                                 new StubChromosome {Fitness = 2},
                                 new StubChromosome {Fitness = 3, IsEliteChromosome = true}
                             };
        }

        public static IList<StubChromosome> CreateMatingPool()
        {
            return new List<StubChromosome>
                             {
                                 new StubChromosome {Fitness = 1}, 
                                 new StubChromosome {Fitness = 2},
                             };
        }

        public static IList<StubChromosome> CreateEliteChromosomes()
        {
            return new List<StubChromosome>
                             {
                                 new StubChromosome {Fitness = 1, IsEliteChromosome = true}, 
                                 new StubChromosome {Fitness = 2, IsEliteChromosome = true},
                             };
        }

        public double? Fitness { get; set; }
    }
}