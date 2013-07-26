using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using FantasyFootballRobot.Core.Exceptions;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Utils;

namespace FantasyFootballRobot.Core.Strategies.Genetic
{
    public abstract class GeneticAlgorithmBase<TChromosome, TSeedGenes> : IGeneticAlgorithm<TChromosome, TSeedGenes>, ILoggable
        where TChromosome : class, IChromosome
        where TSeedGenes : class
    {
        protected IRandom Random;
        protected IGeneticParameters GeneticParameters;
        protected ILogger Logger;
        protected TSeedGenes SeedGenes;

        public int GenerationsProcessed { get; private set; }

        protected GeneticAlgorithmBase(IGeneticParameters geneticParameters, IRandom random, ILogger logger)
        {
            GeneticParameters = geneticParameters;
            Logger = logger;
            Random = random;
        }

        public abstract double CalculateFitness(TChromosome chromosome);
        public abstract TChromosome Crossover(TChromosome parent1, TChromosome parent2);
        public abstract TChromosome Mutate(TChromosome parent);
        public abstract TChromosome CreateRandomChromosome();

        public IList<TChromosome> GenerateInitialPopulation()
        {
            var population = new ConcurrentBag<TChromosome>();

            Func<bool> condition = () => population.Count < GeneticParameters.PopulationSize;
            ParallelUtils.While(condition, this.CreateParallelOptions(), () =>
                                               {
                                                   var randomChromosome = CreateRandomChromosome();
                                                   if (randomChromosome != null)
                                                   {
                                                       population.Add(randomChromosome);
                                                   }
                                               });

            return population.Take(GeneticParameters.PopulationSize).ToList();
        }

        public virtual void SetSeedGenes(TSeedGenes seedGenes)
        {
            SeedGenes = seedGenes;
        }


        public IList<TChromosome> Run()
        {
            if (SeedGenes == null) throw new NullReferenceException("seedPlayers genes must be set before running algorithm");

            Logger.Log(Tag.Genetic, "Starting genetic algorithm run");
            Logger.Log(Tag.Genetic, string.Format("Max Generations: {0}, Population Size: {1}, Elititism Percentage: {2}, Mutation Probability: {3}", GeneticParameters.MaxGenerations, GeneticParameters.PopulationSize, GeneticParameters.ElitismPercentage, GeneticParameters.MutationProbability));

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var initialPopulation = GenerateInitialPopulation();

            Logger.Log(Tag.Genetic, string.Format("Time to create initial population: {0}", stopWatch.Elapsed.ToFormatted()));

            var finalGeneration = ProcessGenerations(initialPopulation);

            var orderedResults = finalGeneration.OrderByDescending(c => c.Fitness).ToList();

            stopWatch.Stop();
            Logger.Log(Tag.Genetic, string.Format("{0} Generations Complete. Total time taken: {1}", GenerationsProcessed, stopWatch.Elapsed.ToFormatted()));

            return orderedResults;
        }

        private IEnumerable<TChromosome> ProcessGenerations(IList<TChromosome> initialPopulation)
        {
            var population = initialPopulation;

            GenerationsProcessed = 0;
            bool terminate = false;

            var topChromosomes = new List<TChromosome>();
            while (!terminate)
            {
                var sw = new Stopwatch();
                sw.Start();

                Logger.Log(Tag.Genetic, string.Format("Current generation: {0}", GenerationsProcessed));

                CalculateChromosomesFitness(population);

                var topChromosome = population.OrderByDescending(c => c.Fitness).First();
                topChromosomes.Add(topChromosome);

                Logger.Log(Tag.Genetic, "Current top chromosome fitness details");
                LogChromosomeDetails(topChromosome);

                var maxGenerationsReached = GenerationsProcessed >= GeneticParameters.MaxGenerations;
                var solutionPlateaued = HasSolutionPlateaued(topChromosomes);
                if (maxGenerationsReached)
                {
                    Logger.Log(Tag.Genetic, string.Format("Terminating as reached {0} generations", GenerationsProcessed));
                }
                else if (solutionPlateaued)
                {
                    Logger.Log(Tag.Genetic, string.Format("Top chromosome has had fitness of {0} for last {1} generations so terminating", topChromosome.Fitness.GetValueOrDefault().ToFormatted(), GeneticParameters.TopChromosomeFitnessPlateauGameweeksForTermination));
                }

                terminate = maxGenerationsReached || solutionPlateaued;

                if (!terminate)
                {
                    population = ProcessGeneration(population);
                }

                sw.Stop();
                Logger.Log(Tag.Genetic, string.Format("Generation {0} took {1}", GenerationsProcessed, sw.Elapsed.ToFormatted()));
            }

            return population;
        }

        private IList<TChromosome> ProcessGeneration(IList<TChromosome> currentPopulation)
        {
            var matingPool = CreateMatingPool(currentPopulation);

            Logger.Log(Tag.Genetic, string.Format("Created generation {0} mating pool", GenerationsProcessed));

            var eliteChromosomes = currentPopulation.OrderByDescending(c => c.Fitness).Take(GeneticParameters.EliteChromosomesPerGeneration).ToList();

            var newPopulation = CreateNewPopulation(matingPool, eliteChromosomes);

            Logger.Log(Tag.Genetic, string.Format("Created generation {0} new population", GenerationsProcessed));

            GenerationsProcessed++;

            return newPopulation;
        }

        private bool HasSolutionPlateaued(IList<TChromosome> topChromosomes)
        {
            return topChromosomes.Count >=
                                       GeneticParameters.TopChromosomeFitnessPlateauGameweeksForTermination &&
                                       topChromosomes.LastElements(
                                           GeneticParameters.TopChromosomeFitnessPlateauGameweeksForTermination).Select(
                                               tc => tc.Fitness).Distinct().Count() == 1;
        }

        private void CalculateChromosomesFitness(IList<TChromosome> population)
        {
            if (population.Any(c => c == null))
            {
                throw new GeneticAlgorithmException("One chromosome is null");
            }

            CacheHits = 0;
            CacheMisses = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var chromosomes = population.Where(c => !c.Fitness.HasValue).ToList();
            Parallel.ForEach(
                chromosomes,
                c =>
                {
                    c.Fitness = CalculateFitness(c);
                    if (double.IsNaN(c.Fitness.Value))
                    {
                        throw new GeneticAlgorithmException("One chromosome has a fitness of Not a Number");
                    }
                });

            stopwatch.Stop();
            Logger.Log(Tag.Genetic, string.Format("Time to calculate fitness for all chromosomes: {0}", stopwatch.Elapsed.ToFormatted()));

            var cacheHitPercentage = (double)CacheHits / (CacheHits + CacheMisses);
            Logger.Log(Tag.Genetic, string.Format("{0} cache hits, {1} cache misses when calculating fitness ({2}%)", CacheHits, CacheMisses, cacheHitPercentage));
        }

        public List<TChromosome> CreateNewPopulation(IList<TChromosome> matingPool, IList<TChromosome> eliteChromosomes)
        {
            var newPopulation = new ConcurrentBag<TChromosome>();

            var desiredPopulationSize = GeneticParameters.PopulationSize - eliteChromosomes.Count;

            var mutations = 0;
            var crossovers = 0;

            var s = new Stopwatch();
            s.Start();

            Func<bool> condition = () => newPopulation.Count < desiredPopulationSize;
            ParallelUtils.While(condition, this.CreateParallelOptions(), () =>
                                               {
                                                   TChromosome newChromosome;
                                                   if (Random.NextDouble() < GeneticParameters.MutationProbability)
                                                   {
                                                       var toMutate = GetRandomChromosome(matingPool);
                                                       newChromosome = Mutate(toMutate);
                                                       if (newChromosome != null)
                                                       {
                                                           mutations++;
                                                           newPopulation.Add(newChromosome);
                                                       }
                                                   }
                                                   else
                                                   {
                                                       var parent1 = GetRandomChromosome(matingPool);
                                                       var parent2 = GetRandomChromosome(matingPool);
                                                       newChromosome = Crossover(parent1, parent2);
                                                       if (newChromosome != null)
                                                       {
                                                           crossovers++;
                                                           newPopulation.Add(newChromosome);
                                                       }
                                                   }
                                               });

            s.Stop();
            Logger.Log(Tag.Genetic, string.Format("Creating new population took {0}", s.Elapsed.ToFormatted()));

            Logger.Log(Tag.Genetic, string.Format("New population has {0} elite, {1} mutated, {2} crossover", eliteChromosomes.Count().ToFormatted(), mutations.ToFormatted(), crossovers.ToFormatted()));

            return newPopulation.Take(desiredPopulationSize).Concat(eliteChromosomes).ToList();
        }

        public IList<TChromosome> CreateMatingPool(IList<TChromosome> population)
        {
            //http://en.wikipedia.org/wiki/Selection_(genetic_algorithm)

            var s = new Stopwatch();
            s.Start();

            var orderedNormalisedChromosomes = CreateOrderedNormalisedChromosomeList(population);

            var matingPool = new ConcurrentBag<TChromosome>();

            Func<bool> condition = () => matingPool.Count < GeneticParameters.PopulationSize;
            ParallelUtils.While(condition, this.CreateParallelOptions(), () =>
                                               {
                                                   var randomFitnessValue = Random.NextDouble();

                                                   var selectedChromosome =
                                                       orderedNormalisedChromosomes.FindFirstElementGreaterThan(randomFitnessValue);

                                                   matingPool.Add(selectedChromosome);
                                               });

            s.Stop();
            Logger.Log(Tag.Genetic, string.Format("Creating mating pool took {0}", s.Elapsed.ToFormatted()));

            return matingPool.Take(GeneticParameters.PopulationSize).ToList();
        }

        private IList<Tuple<double, TChromosome>> CreateOrderedNormalisedChromosomeList(IList<TChromosome> chromosomes)
        {
            var totalFitnessScore = chromosomes.Sum(c => c.Fitness.GetValueOrDefault());

            if (totalFitnessScore.IsCloseToZero()) throw new GeneticAlgorithmException("Total fitness is zero. No chromosones will be selected");
            if (double.IsNaN(totalFitnessScore)) throw new GeneticAlgorithmException("Total fitness is Not a Number");

            var averageFitnessScore = totalFitnessScore / chromosomes.Count;

            var orderedChromosomes = chromosomes.OrderByDescending(c => c.Fitness).ToList();

            var topChromosome = orderedChromosomes.First();

            Logger.Log(Tag.Genetic, string.Format("Population Fitness (predicted points per gameweek) -  Total: {0}. Average: {1}. Top: {2}", totalFitnessScore.ToFormatted(), averageFitnessScore.ToFormatted(), topChromosome.Fitness.GetValueOrDefault().ToFormatted()));

            Logger.Log(Tag.CSV, string.Join(",", GenerationsProcessed, averageFitnessScore.ToFormatted(), topChromosome.Fitness.GetValueOrDefault().ToFormatted()));

            var orderedNormalisedChromosomeList = new List<Tuple<double, TChromosome>>();
            var cumulativeNormalizedFitness = 0.0;
            foreach (var chromosome in orderedChromosomes)
            {
                var normalizedFitness = chromosome.Fitness.Value / totalFitnessScore;
                cumulativeNormalizedFitness += normalizedFitness;
                orderedNormalisedChromosomeList.Add(new Tuple<double, TChromosome>(cumulativeNormalizedFitness, chromosome));
            }

            return orderedNormalisedChromosomeList;
        }

        private void LogChromosomeDetails(TChromosome chromosome)
        {
            //calculate fitness again but this time set verbose logging
            var previousLoggingValue = VerboseLoggingEnabled;
            VerboseLoggingEnabled = true;
            CalculateFitness(chromosome);
            VerboseLoggingEnabled = previousLoggingValue;
        }

        private TChromosome GetRandomChromosome(IList<TChromosome> chromosomes)
        {
            return chromosomes[Random.Next(chromosomes.Count)];
        }

        private ParallelOptions CreateParallelOptions()
        {
            return new ParallelOptions { MaxDegreeOfParallelism = GeneticParameters.MaxDegreeOfParallelism.HasValue ? GeneticParameters.MaxDegreeOfParallelism.Value : -1 };
        }

        public bool VerboseLoggingEnabled { get; set; }
        public int CacheHits { get; protected set; }
        public int CacheMisses { get; protected set; }
    }
}
