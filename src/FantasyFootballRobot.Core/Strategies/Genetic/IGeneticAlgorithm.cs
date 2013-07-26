using System.Collections.Generic;
using FantasyFootballRobot.Core.Caching;

namespace FantasyFootballRobot.Core.Strategies.Genetic
{
    public interface IGeneticAlgorithm<TChromosome, in TSeedGenes> : ICacher
        where TChromosome : class
        where TSeedGenes : class
    {
        void SetSeedGenes(TSeedGenes seedGenes);
        IList<TChromosome> Run();
    }
}