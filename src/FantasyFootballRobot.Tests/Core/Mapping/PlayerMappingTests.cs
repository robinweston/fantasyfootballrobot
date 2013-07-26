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
    public class PlayerMappingTests
    {
        [SetUp]
        public void SetUp()
        {
            var unityContainer = ContainerBuilder.BuildUnityContainer();
            MappingConfiguration.Bootstrap(unityContainer);
        }

        [Test]
        public void basic_player_attributes_mapped_correctly()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.Id, Is.EqualTo(514));
            Assert.That(player.NowCost, Is.EqualTo(52));
            Assert.That(player.ClubCode, Is.EqualTo(Club.Swansea.Code));
            Assert.That(player.Position, Is.EqualTo(Position.Goalkeeper));
            Assert.That(player.Name, Is.EqualTo("Michel Vorm"));
            Assert.That(player.Status, Is.EqualTo(Status.Available));

        }

        [Test]
        public void player_future_fixtures_are_mapped_correctly()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.FutureFixtures.Count, Is.EqualTo(18));
            Assert.That(player.FutureFixtures.First().Home, Is.True);
            Assert.That(player.FutureFixtures.First().OppositionClubCode, Is.EqualTo(Club.Arsenal.Code));
            Assert.That(player.FutureFixtures[3].OppositionClubCode, Is.EqualTo(Club.WestBrom.Code));
            Assert.That(player.FutureFixtures.First().Date, Is.EqualTo(new DateTime(2012, 1, 15)));
        }

        [Test]
        public void player_past_fixtures_are_mapped_correctly()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.PastFixtures.Count, Is.EqualTo(20));
            Assert.That(player.PastFixtures.First().Home, Is.False);
            Assert.That(player.PastFixtures.First().OppositionClubCode, Is.EqualTo(Club.ManCity.Code));
            Assert.That(player.PastFixtures.First().Date, Is.EqualTo(new DateTime(2011, 8, 15)));
            Assert.That(player.PastFixtures.Last().Date, Is.EqualTo(new DateTime(2012, 1, 2)));
            Assert.That(player.PastFixtures.First().GameWeek, Is.EqualTo(1));
            Assert.That(player.PastFixtures.First().PlayerValueAtTime, Is.EqualTo(40));
            Assert.That(player.PastFixtures.First().MinutesPlayed, Is.EqualTo(90));
            Assert.That(player.PastFixtures.First().TeamGoals, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().OppositionGoals, Is.EqualTo(4));
            Assert.That(player.PastFixtures.First().TotalPointsScored, Is.EqualTo(3));
        }

        [Test]
        public void player_past_fixtures_points_are_mapped()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.PastFixtures.First().TotalPointsScored, Is.EqualTo(3));
            Assert.That(player.PastFixtures.First().GoalsScored, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().Assists, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().CleanSheets, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().GoalsConceded, Is.EqualTo(4));
            Assert.That(player.PastFixtures.First().OwnGoals, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().PenaltiesSaved, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().PenaltiesMissed, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().YellowCards, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().RedCards, Is.EqualTo(0));
            Assert.That(player.PastFixtures.First().Saves, Is.EqualTo(10));
            Assert.That(player.PastFixtures.First().Bonus, Is.EqualTo(0));

        }

        [Test]
        public void player_past_seasons_are_mapped_for_players_with_no_past_seasons()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleGoalkeeperJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.PastSeasons, Is.Empty);
        }

        [Test]
        public void player_past_seasons_are_mapped_for_players()
        {
            //Arrange
            var json = ResourceHelper.GetFromResources("FantasyFootballRobot.Tests.TestData.SampleDefenderJson.txt");
            var jsonPlayer = new JsonPlayer(json);

            //Act
            var player = Mapper.Map<JsonPlayer, Player>(jsonPlayer);

            //Assert
            Assert.That(player.PastSeasons.Count, Is.EqualTo(5));
            Assert.That(player.PastSeasons.Last().SeasonEndYear, Is.EqualTo(2011));
            Assert.That(player.PastSeasons.Last().MinutesPlayed, Is.EqualTo(2961));
            Assert.That(player.PastSeasons.Last().PlayerValueAtTime, Is.EqualTo(74));
            Assert.That(player.PastSeasons.Last().TotalPointsScored, Is.EqualTo(134));
        }     
    }
}
