using FantasyFootballRobot.Core.Entities.Json;
using FantasyFootballRobot.Core.Services;
using NUnit.Framework;

namespace FantasyFootballRobot.IntegrationTests.Services
{

    [TestFixture]
    public class RemotePlayerJsonServiceTests
    {
        private IPlayerJsonService _playerJsonService;

        [SetUp]
        public void SetUp()
        {
            _playerJsonService = new RemotePlayerJsonService();
        }

        [Test]
        public void can_retrieve_player_json_from_site()
        {
            //Arrange

            //Act
            var json = _playerJsonService.GetPlayerJson(514);

            //Assert
            Assert.That(json, Is.Not.Null);
            Assert.That(json.Length, Is.GreaterThan(0));

        }

        [Test]
        public void can_retrieve_player_json_from_site_and_parse_as_player()
        {
            //Arrange

            //Act
            var json = _playerJsonService.GetPlayerJson(514);

            //Assert
            var jsonPlayer = new JsonPlayer(json);
            Assert.That(jsonPlayer.Id, Is.EqualTo(514));
            Assert.That(jsonPlayer.MaxCost, Is.GreaterThan(0));

        }

        [Test]
        public void remote_service_returns_null_if_player_does_not_exist()
        {
            //Arrange

            //Act
            var json = _playerJsonService.GetPlayerJson(5000);

            //Assert
            Assert.That(json, Is.Null);
        }
    }
}
