using System;
using System.IO;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Services
{
    [TestFixture]
    public class LocalPlayerJsonServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            string testDataDir = GetTestDataDirectory();
            _configSettingsMock = new Mock<IConfigurationSettings>();
            _configSettingsMock.SetupGet(x => x.DataDirectory).Returns(testDataDir);
            

            _childJsonServiceMock = new Mock<IPlayerJsonService>();

            _service = new LocalPlayerJsonService(_configSettingsMock.Object, _childJsonServiceMock.Object, new Mock<ILogger>().Object);
        }

        private LocalPlayerJsonService _service;
        private Mock<IConfigurationSettings> _configSettingsMock;
        private Mock<IPlayerJsonService> _childJsonServiceMock;

        private static string GetTestDataDirectory()
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData");
        }

        [Test]
        public void service_can_return_json_if_file_exists()
        {
            //Arrange
            _configSettingsMock.SetupGet(x => x.ValidPlayerJsonCacheHours).Returns(1);
            const string testJson = "{json:true}";
            string filePath = Path.Combine(GetTestDataDirectory(), "514.json");
            File.WriteAllText(filePath, testJson);

            //Act
            var playerJson = _service.GetPlayerJson(514);

            //Assert
            Assert.That(playerJson, Is.EqualTo(testJson));
        }

        [Test]
        public void service_can_return_valid_player_json_if_file_exists()
        {
            //Arrange
            _configSettingsMock.SetupGet(x => x.ValidPlayerJsonCacheHours).Returns(1);
            string filePath = Path.Combine(GetTestDataDirectory(), "514.json");
            File.WriteAllText(filePath, "{json:true}");

            //Act
            var playerJson = _service.GetPlayerJson(514);

            //Assert
            _childJsonServiceMock.Verify(x => x.GetPlayerJson(It.IsAny<int>()), Times.Never());
            Assert.That(playerJson, Is.EqualTo("{json:true}"));
        }

        [Test]
        public void service_can_save_to_file_system()
        {
            //Arrange
            var testJson = "{json:true}";
            _childJsonServiceMock.Setup(x => x.GetPlayerJson(It.IsAny<int>())).Returns(testJson);
            string tempFile = Path.Combine(GetTestDataDirectory(), "666.json");

            //Act          
            _service.GetPlayerJson(666);

            //Assert
            string fileContents = File.ReadAllText(tempFile);
            Assert.That(fileContents, Is.EqualTo(testJson));
        }

        [Test]
        public void service_does_not_store_null_responses()
        {
            //Arrange
            _childJsonServiceMock.Setup(x => x.GetPlayerJson(It.IsAny<int>())).Returns(string.Empty);

            //Act
            _service.GetPlayerJson(1);

            //Assert
            string filePath = Path.Combine(GetTestDataDirectory(), "1.json");
            Assert.That(File.Exists(filePath), Is.False);
        }

        [Test]
        public void if_local_service_has_no_json_it_is_fetched_remotely()
        {
            //Arrange

            //Act
            _service.GetPlayerJson(514);

            //Assert
            _childJsonServiceMock.Verify(x => x.GetPlayerJson(514));
        }

        [Test]
        public void player_json_pulled_remotely_if_local_cached_file_is_too_old()
        {
            //Arrange
            _configSettingsMock.SetupGet(x => x.ValidPlayerJsonCacheHours).Returns(9);
            string filePath = Path.Combine(GetTestDataDirectory(), "514.json");
            File.WriteAllText(filePath, "{json:true}");
            File.SetLastWriteTime(filePath, DateTime.Now.AddHours(-10));

            //Act
            _service.GetPlayerJson(514);

            //Assert
            _childJsonServiceMock.Verify(x => x.GetPlayerJson(514));
        }
    }
}