using System;
using System.Linq;
using AutoMapper;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Entities.Json;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Tests.Helpers;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Mapping
{
    [TestFixture]
    class Season1213Mapping
    {
        [Test]
        public void season_12_13_player_fixtures_are_mapped_correctly()
        {
            //Arrange         
            var configMock = new Mock<IConfigurationSettings> { CallBase = true };
            configMock.SetupGet(x => x.SeasonStartYear).Returns(2012);
            SetConfigurationInstance(configMock.Object);

            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.Season1213.SamplePlayerJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.FutureFixtures.Count, Is.EqualTo(0));
            Assert.That(player.PastFixtures.Count, Is.EqualTo(38));        
            Assert.That(player.PastFixtures.Last().OppositionClubCode, Is.EqualTo(Club.Newcastle.Code));
            Assert.That(player.PastFixtures.Last().Date, Is.EqualTo(new DateTime(2013, 5, 19)));
        }

        private void SetConfigurationInstance(IConfigurationSettings configSettings)
        {
            var unityContainer = ContainerBuilder.BuildUnityContainer();
            unityContainer.RegisterInstance(configSettings);
            MappingConfiguration.Bootstrap(unityContainer);
        }
    }
}
