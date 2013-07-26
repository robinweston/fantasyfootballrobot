using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Services
{
    [TestFixture]
    public class JsonParsingPlayerServiceTests
    {
        private JsonParsingSinglePlayerService _service;
        private Mock<IPlayerJsonService> _localJsonServiceMock;

        [SetUp]
        public void SetUp()
        {
            var unityContainer = ContainerBuilder.BuildUnityContainer();
            MappingConfiguration.Bootstrap(unityContainer);
            _localJsonServiceMock = new Mock<IPlayerJsonService>();
            _service = new JsonParsingSinglePlayerService(_localJsonServiceMock.Object, new Mock<ILogger>().Object);
        }

        private static string GetTestJson()
        {
            return
                ResourceHelper.GetFromResources(
                    "FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
        }

        [Test]
        public void get_player_retrieves_json_and_parses_as_player()
        {
            //Arrange
            
            _localJsonServiceMock.Setup(x => x.GetPlayerJson(514)).Returns(GetTestJson());

            //Act
            var player = _service.GetPlayer(514);

            //Assert
            _localJsonServiceMock.VerifyAll();
            Assert.That(player, Is.Not.Null);
            Assert.That(player.Id, Is.EqualTo(514));
            Assert.That(player.Name, Is.EqualTo("Michel Vorm"));
        }

        [Test]
        public void empty_json_returns_null_player()
        {
            //Arrange
            _localJsonServiceMock.Setup(x => x.GetPlayerJson(It.IsAny<int>())).Returns(string.Empty);

            //Act
            var player = _service.GetPlayer(514);

            //Assert
            Assert.That(player, Is.Null);
        }
    }
}
