using System;
using System.IO;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Core.Services;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.IntegrationTests.Services
{
    [TestFixture]
    class MultiplePlayersServiceTests
    {
        IMultiplePlayersService _multiplePlayersService;
        Mock<IConfigurationSettings> _configSettingsMock;

        [SetUp]
        public void SetUp()
        {
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _configSettingsMock.SetupGet(x => x.DataDirectory).Returns(GetTestDataDirectory());
            _configSettingsMock.SetupGet(x => x.ValidPlayerJsonCacheHours).Returns(48);

            var remotePlayerService = new RemotePlayerJsonService();

            var localPlayerService = new LocalPlayerJsonService(_configSettingsMock.Object, remotePlayerService, new Mock<ILogger>().Object);

            var jsonParsingService = new JsonParsingSinglePlayerService(localPlayerService, new Mock<ILogger>().Object);

            var playerService = new PlayerService(jsonParsingService);
            _multiplePlayersService = playerService;

            var unityContainer = ContainerBuilder.BuildUnityContainer();
            MappingConfiguration.Bootstrap(unityContainer);
        }

        private static string GetTestDataDirectory()
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData");
        }

        [Test]
        public void can_retrieve_json_for_all_players_and_store_locally()
        {
            //Arrange
            var testDir = GetTestDataDirectory();
            foreach(var file in Directory.GetFiles(testDir))
            {
                File.Delete(file);
            }

            //Act
            var players = _multiplePlayersService.GetAllPlayers();

            //Assert
            Assert.That(players.Count, Is.GreaterThan(100));
            Assert.That(Directory.GetFiles(testDir).Count(), Is.EqualTo(players.Count));
        }
    }
}
