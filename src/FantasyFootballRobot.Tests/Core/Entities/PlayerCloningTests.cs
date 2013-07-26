using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Entities
{
    [TestFixture]
    public class PlayerCloningTests
    {
        [Test]
        public void basic_player_properties_cloned()
        {
            //Arrange
            var player = new Player
                             {
                                 Id = 1,
                                 ClubCode = "Test",
                                 Name = "Test Player",
                                 Position = Position.Defender,
                                 Status = Status.Available
                             };

            //Act
            var clonedPlayer = player.DeepClone();

            //Assert
            Assert.That(clonedPlayer, Is.Not.Null);
            Assert.That(clonedPlayer.Id, Is.EqualTo(1));
            Assert.That(clonedPlayer.ClubCode, Is.EqualTo("Test"));
            Assert.That(clonedPlayer.Name, Is.EqualTo("Test Player"));
            Assert.That(clonedPlayer.Position, Is.EqualTo(Position.Defender));
            Assert.That(clonedPlayer.Status, Is.EqualTo(Status.Available));
        }

        [Test]
        public void future_fixtures_cloned_correctly()
        {
            //Arrange
            var player = new Player
            {
                FutureFixtures = new List<FutureFixture>
                                     {
                                         new FutureFixture{
                                             Date = new DateTime(2010,1, 1),
                                             GameWeek = 1,
                                             Home = true,
                                             OppositionClubCode = "Test"
                                         }
                                     }
            };

            //Act
            var clonedPlayer = player.DeepClone();

            //Assert
            Assert.That(clonedPlayer.FutureFixtures.Count, Is.EqualTo(1));
            Assert.That(clonedPlayer.FutureFixtures.First().Date, Is.EqualTo(new DateTime(2010, 1, 1)));
            Assert.That(clonedPlayer.FutureFixtures.First().GameWeek, Is.EqualTo(1));
            Assert.That(clonedPlayer.FutureFixtures.First().Home, Is.True);
            Assert.That(clonedPlayer.FutureFixtures.First().OppositionClubCode, Is.EqualTo("Test"));
        }

        [Test]
        public void past_fixtures_cloned_correctly()
        {
            //Arrange
            var player = new Player
            {
                PastFixtures = new List<PastFixture>
                                     {
                                         new PastFixture{
                                             Date = new DateTime(2010,1, 1),
                                             GameWeek = 1,
                                             Home = true,
                                             OppositionClubCode = "Test"
                                         }
                                     }
            };

            //Act
            var clonedPlayer = player.DeepClone();

            //Assert
            Assert.That(clonedPlayer.PastFixtures.Count, Is.EqualTo(1));
            Assert.That(clonedPlayer.PastFixtures.First().Date, Is.EqualTo(new DateTime(2010, 1, 1)));
            Assert.That(clonedPlayer.PastFixtures.First().GameWeek, Is.EqualTo(1));
            Assert.That(clonedPlayer.PastFixtures.First().Home, Is.True);
            Assert.That(clonedPlayer.PastFixtures.First().OppositionClubCode, Is.EqualTo("Test"));
        }

        [Test]
        public void original_player_is_not_changed_when_altering_clone()
        {
            var player = new Player
            {
                Id = 1,
                ClubCode = "Test",
                Name = "Test Player",
                Position = Position.Defender,
                Status = Status.Available
            };

            //Act
            var clonedPlayer = player.DeepClone();
            clonedPlayer.Id = 2;
            clonedPlayer.Name = "New Name";

            //Assert
            Assert.That(player.Id, Is.EqualTo(1));
            Assert.That(player.Name, Is.EqualTo("Test Player"));
        }

        [Test]
        public void original_player_future_fixtures_not_changed_when_altering_clone()
        {
            var player = new Player
            {
                FutureFixtures = new List<FutureFixture>
                                     {
                                         new FutureFixture{
                                             Date = new DateTime(2010,1, 1),
                                             GameWeek = 1,
                                             Home = true,
                                             OppositionClubCode = "Test"
                                         }
                                     }
            };

            //Act
            var clonedPlayer = player.DeepClone();
            clonedPlayer.FutureFixtures.First().GameWeek = 2;
            clonedPlayer.FutureFixtures.Add(new FutureFixture());

            //Assert
            Assert.That(player.FutureFixtures.First().GameWeek, Is.EqualTo(1));
            Assert.That(player.FutureFixtures.Count, Is.EqualTo(1));
        }

        [Test]
        public void original_player_past_fixtures_not_changed_when_altering_clone()
        {
            //Arrange
            var player = new Player
            {
                PastFixtures = new List<PastFixture>
                                     {
                                         new PastFixture{
                                             Date = new DateTime(2010,1, 1),
                                             GameWeek = 1,
                                             Home = true,
                                             OppositionClubCode = "Test"
                                         }
                                     }
            };

            //Act
            var clonedPlayer = player.DeepClone();
            clonedPlayer.PastFixtures.First().GameWeek = 2;
            clonedPlayer.PastFixtures.Add(new PastFixture());

            //Assert
            Assert.That(player.PastFixtures.First().GameWeek, Is.EqualTo(1));
            Assert.That(player.PastFixtures.Count, Is.EqualTo(1));
        }
    }
}
