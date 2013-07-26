using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Tests.Helpers;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Entities
{
    [TestFixture]
    class TeamClonerTests
    {
        private Team _originalTeam;
        private Team _shallowClonedTeam;

        [SetUp]
        public void SetUp()
        {
            _originalTeam = TeamCreationHelper.CreateTestTeam();
            _shallowClonedTeam = TeamCloner.ShallowClone(_originalTeam);
        }

        [Test]
        public void shallow_clone_does_not_equal_original_team()
        {
            // Assert
            Assert.That(_originalTeam, Is.Not.EqualTo(_shallowClonedTeam));
        }

        [Test]
        public void shallow_clone_keeps_players_unchanged_but_in_new_list()
        {
            // Assert
            Assert.That(_originalTeam.Players, Is.Not.EqualTo(_shallowClonedTeam));

            for(var i=0; i < 15; i ++)
            {
                Assert.That(_originalTeam.Players[i], Is.EqualTo(_shallowClonedTeam.Players[i]));
            }
        }

        [Test]
        public void shallow_clone_maintains_captain()
        {
            // Assert
            Assert.That(_originalTeam.Captain, Is.EqualTo(_shallowClonedTeam.Captain));
        }

        [Test]
        public void shallow_clone_maintains_vice_captain()
        {
            // Assert
            Assert.That(_originalTeam.ViceCaptain, Is.EqualTo(_shallowClonedTeam.ViceCaptain));
        }
    }
}
