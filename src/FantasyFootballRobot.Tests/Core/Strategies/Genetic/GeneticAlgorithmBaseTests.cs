using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Genetic;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Genetic
{
    [TestFixture]
    public class GeneticAlgorithmBaseTests
    {
        GeneticStub _geneticStub;
        Mock<IGeneticParameters> _parametersMock;
        Mock<IRandom> _randomMock;
        IList<StubChromosome> _initalPopulation;
        IList<StubChromosome> _matingPool;
        IList<StubChromosome> _eliteChromosomes;
            
        [SetUp]
        public void SetUp()
        {                    
            _randomMock = new Mock<IRandom>();

            _initalPopulation = StubChromosome.CreateIntitialPopulation();

            _parametersMock = new Mock<IGeneticParameters>();
            _parametersMock.SetupGet(x => x.PopulationSize).Returns(3);
            // run test as single threaded so we can predict outcome and test logic
            _parametersMock.SetupGet(x => x.MaxDegreeOfParallelism).Returns(1);

            _matingPool = StubChromosome.CreateMatingPool();
            _eliteChromosomes = StubChromosome.CreateEliteChromosomes();

            _geneticStub = new GeneticStub(_parametersMock.Object, _randomMock.Object, new Mock<ILogger>().Object);
            _geneticStub.SetSeedGenes(new object());
        }

        [Test]
        public void initial_population_is_created_when_running_algorithm()
        {
            //Arrange
            _parametersMock.SetupGet(x => x.MaxGenerations).Returns(0);

            //Act
            var finalPopulation = _geneticStub.Run();

            //Assert
            Assert.That(finalPopulation.Count, Is.EqualTo(_initalPopulation.Count));
        }

        [Test]
        public void algorithm_returns_final_population_of_correct_size()
        {
            //Act
            var result = _geneticStub.Run();

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(_initalPopulation.Count));
        }

        [Test]
        public void algorithm_runs_for_correct_numer_of_generations()
        {
            //Arrange
            _parametersMock.SetupGet(x => x.MaxGenerations).Returns(2);

            //Act
            _geneticStub.Run();

            //Assert
            Assert.That(_geneticStub.GenerationsProcessed, Is.EqualTo(2));
        }

        [Test]
        public void mating_pool_is_created_with_correct_size()
        {
            //Arrange

            //Act
            var matingPool = _geneticStub.CreateMatingPool(_initalPopulation);

            //Assert
            Assert.That(matingPool.Count, Is.EqualTo(3));
        }

        [Test]
        public void mating_pool_is_created_according_to_chromosome_fitness()
        {
            //http://en.wikipedia.org/wiki/Selection_(genetic_algorithm)

            //Arrange
            var randomQueue = new Queue<double>(new[]
                              {
                                  0.2, //this will select Chromosone with fitness 3
                                  0.9, //this will select Chromosone with fitness 1
                                  0.49 //this will select Chromosone with fitness 3
                              });

            _randomMock.Setup(x => x.NextDouble()).Returns(randomQueue.Dequeue);

            //Act
            var matingPool = _geneticStub.CreateMatingPool(_initalPopulation);
            
            //Assert
            Assert.That(matingPool[0], Is.EqualTo(_initalPopulation[2]));
            Assert.That(matingPool[1], Is.EqualTo(_initalPopulation[0]));
            Assert.That(matingPool[2], Is.EqualTo(_initalPopulation[2]));

        }

        [Test]
        public void elite_chromosomes_are_added_to_next_generation()
        {
            //Arrange
            _parametersMock.SetupGet(x => x.EliteChromosomesPerGeneration).Returns(1);

            //Act
            var result = _geneticStub.Run();

            //Assert
            Assert.That(result.Any(x => x.IsEliteChromosome));
        }

        [Test]
        public void new_population_is_created_of_correct_size()
        {
            //Arrange

            //Act
            var newPopulation = _geneticStub.CreateNewPopulation(_matingPool, _eliteChromosomes);

            //Assert
            Assert.That(newPopulation.Count, Is.EqualTo(3));
        }

        [Test]
        public void new_population_contains_elite_chromones()
        {
            //Arrange

            //Act
            var newPopulation = _geneticStub.CreateNewPopulation(_matingPool, _eliteChromosomes);

            //Assert
            Assert.That(_eliteChromosomes.All(newPopulation.Contains));
        }

        [Test]
        public void new_chromosomes_are_mutated_or_crossed_over()
        {
            //Arrange
            _parametersMock.SetupGet(x => x.MutationProbability).Returns(0.5);
            _eliteChromosomes.Clear();
            var randomQueue = new Queue<double>(new[]
                              {
                                  0.2,
                                  0.7,
                                  0.51
                              });

            _randomMock.Setup(x => x.NextDouble()).Returns(randomQueue.Dequeue);

            //Act
            var newPopulation = _geneticStub.CreateNewPopulation(_matingPool, _eliteChromosomes);

            //Assert
            Assert.That(newPopulation.Count(x => x.IsMutation), Is.EqualTo(1));
            Assert.That(newPopulation.Count(x => x.IsCrossover), Is.EqualTo(2));
        }
        
        [Test]
        public void null_chromosomes_are_not_carried_into_new_population()
        {
            //Arrange
            _geneticStub.CreateNullMutations = true;
            _eliteChromosomes.Clear();

            //Act
            var newPopulation = _geneticStub.CreateNewPopulation(_matingPool, _eliteChromosomes);

            //Assert
            Assert.That(newPopulation.All(x => x != null));
        }

        [Test]
        public void algorithm_terminates_if_top_solution_fitness_does_not_improve_afer_consecutive_iterations()
        {
            // Arrange
            _parametersMock.Setup(x => x.TopChromosomeFitnessPlateauGameweeksForTermination).Returns(5);
            _parametersMock.Setup(x => x.MaxGenerations).Returns(10);
            _geneticStub.OverrideFitness = 10;

            // Act
            _geneticStub.Run();

            // Assert
            Assert.That(_geneticStub.GenerationsProcessed, Is.EqualTo(5));

        }

    }
}
