using System.IO;
using System.Linq;
using System.Reflection;
using FantasyFootballRobot.Core.Entities.Json;
using FantasyFootballRobot.Tests.Helpers;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Entities.Json
{

    [TestFixture]
    public class JsonPlayerTests
    {
        [Test]
        public void can_parse_player_properties_from_json()
        {
            //Arrange
            var testJson = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");

            //Act
            var player = new JsonPlayer(testJson);

            //Assert
            Assert.That(player.Id, Is.EqualTo(514));
            Assert.That(player.EventCost, Is.EqualTo(52));
            Assert.That(player.LastSeasonPoints, Is.EqualTo(0));
            Assert.That(player.FirstName, Is.EqualTo("Michel"));
            Assert.That(player.SecondName, Is.EqualTo("Vorm"));
            Assert.That(player.TeamId, Is.EqualTo(16));
            Assert.That(player.TeamName, Is.EqualTo("Swansea"));
            Assert.That(player.TypeName, Is.EqualTo("Goalkeeper"));
        }

        [Test]
        public void can_parse_player_future_fixtures_from_json()
        {
            //Arrange
            var testJson = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");

            //Act
            var player = new JsonPlayer(testJson);

            //Assert
            Assert.That(player.Fixtures.All.Length, Is.EqualTo(18));
            Assert.That(player.Fixtures.All.First()[0], Is.EqualTo("15 Jan"));
            Assert.That(player.Fixtures.All.First()[1], Is.EqualTo("Gameweek 21"));
            Assert.That(player.Fixtures.All.First()[2], Is.EqualTo("Arsenal (H)"));
        }

        [Test]
        public void can_parse_player_fixture_history_from_json()
        {
            //Arrange
            var testJson = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");

            //Act
            var player = new JsonPlayer(testJson);

            //Assert
            Assert.That(player.FixtureHistory.All.Length, Is.EqualTo(20));
            Assert.That(player.FixtureHistory.All.First()[0], Is.EqualTo("15 Aug"));
            Assert.That(player.FixtureHistory.All.First()[1], Is.EqualTo(1));
            Assert.That(player.FixtureHistory.All.First()[2], Is.EqualTo("MCI(A) 0-4"));
            Assert.That(player.FixtureHistory.All.First()[18], Is.EqualTo(3)); 
        }       
    }
}
