using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Tests.Core.Strategies.Genetic
{
    class GeneticStub : GeneticAlgorithmBase<StubChromosome, object>
    {
        public bool CreateNullMutations { get; set; }
        public int? OverrideFitness { get; set; }

        private int _lastGeneratedChormosome;

        public GeneticStub(IGeneticParameters geneticParameters, IRandom random, ILogger logger)
            : base(geneticParameters, random, logger)
        {
        }

        public override double CalculateFitness(StubChromosome chromosome)
        {
            return OverrideFitness.HasValue ? OverrideFitness.Value : chromosome.Fitness.Value;
        }

        public override StubChromosome Crossover(StubChromosome parent1, StubChromosome parent2)
        {
            return new StubChromosome{Fitness = OverrideFitness.HasValue? OverrideFitness.Value : parent1.Fitness + parent2.Fitness, IsCrossover = true};
        }

        public override StubChromosome Mutate(StubChromosome parent)
        {
            if(CreateNullMutations)
            {
                return null;
            }

            return new StubChromosome {Fitness = OverrideFitness.HasValue ? OverrideFitness.Value : parent.Fitness + 1, IsMutation = true};
        }

        public override StubChromosome CreateRandomChromosome()
        {
            var c = StubChromosome.CreateIntitialPopulation()[_lastGeneratedChormosome];
            _lastGeneratedChormosome++;
            return c;
        }
    }
}
