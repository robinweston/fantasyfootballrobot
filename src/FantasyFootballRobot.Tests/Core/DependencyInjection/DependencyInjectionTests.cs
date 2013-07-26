using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using NUnit.Framework;
using Microsoft.Practices.Unity;

namespace FantasyFootballRobot.Tests.Core.DependencyInjection
{
    [TestFixture]
    public class DependencyInjectionTests
    {
        private IUnityContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = ContainerBuilder.BuildUnityContainer();
        }

        [Test]
        public void can_create_player_service_correctly()
        {
            //Act
            var playerService = _container.Resolve<IMultiplePlayersService>();

            //Assert
            Assert.That(playerService, Is.InstanceOf(typeof (PlayerService)));
        }

        [Test]
        public void can_create_season_simulator()
        {
            //Act
            var seasonSimulator = _container.Resolve<ISeasonSimulator>();

            //Assert
            Assert.That(seasonSimulator, Is.InstanceOf(typeof(ISeasonSimulator)));
        }

        [Test]
        public void can_create_logger()
        {
            //Act
            var logger = _container.Resolve<ILogger>();

            //Assert
            Assert.That(logger, Is.InstanceOf(typeof(ILogger)));
        }

        [Test]
        public void can_create_strategy()
        {
            //Act
            var strategy = _container.Resolve<IStrategy>();

            //Assert
            Assert.That(strategy, Is.InstanceOf(typeof(IStrategy)));
        }
    }
}
