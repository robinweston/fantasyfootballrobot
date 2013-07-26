using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Tests.Helpers;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    class SeasonStateCloningTests
    {
        private SeasonState _originalSeasonState;
        private SeasonState _clonedSeasonState;

        [SetUp]
        public void SetUp()
        {
            _originalSeasonState = new SeasonState
                                   {
                                       AllPlayers = TeamCreationHelper.CreatePlayerList(5, 5, 5, 5),
                                       CurrentTeam = TeamCreationHelper.CreateTestTeam(),
                                       FreeTransfers = 5,
                                       Gameweek = 2,
                                       Money = 200,
                                       StandardWildCardPlayed = true,
                                       TransferWindowWildcardPlayed = true
                                   };

            _clonedSeasonState = _originalSeasonState.ShallowClone();
        }

        [Test]
        public void shallow_cloned_season_state_is_new_object()
        {
            // Assert
            Assert.That(_clonedSeasonState, Is.Not.EqualTo(_originalSeasonState));
        }

        [Test]
        public void shallow_cloned_season_state_copies_player_list_by_reference()
        {
            // Assert
            Assert.That(_clonedSeasonState.AllPlayers, Is.EqualTo(_originalSeasonState.AllPlayers));
        }

        [Test]
        public void shallow_cloned_season_state_shallow_clones_current_team()
        {
            // Assert
            Assert.That(_clonedSeasonState.CurrentTeam, Is.Not.EqualTo(_originalSeasonState.CurrentTeam));

            for(var i=0; i < 15; i++)
            {
                Assert.That(_clonedSeasonState.CurrentTeam.Players[i], Is.EqualTo(_originalSeasonState.CurrentTeam.Players[i]));
            }
        }

        [Test]
        public void shallow_cloned_season_state_maintains_money()
        {
            // Assert
            Assert.That(_clonedSeasonState.Money, Is.EqualTo(_originalSeasonState.Money));
        }

        [Test]
        public void shallow_cloned_season_state_maintains_free_transfers()
        {
            // Assert
            Assert.That(_clonedSeasonState.FreeTransfers, Is.EqualTo(_originalSeasonState.FreeTransfers));
        }

        [Test]
        public void shallow_cloned_season_state_maintains_gameweek()
        {
            // Assert
            Assert.That(_clonedSeasonState.Gameweek, Is.EqualTo(_originalSeasonState.Gameweek));
        }

        [Test]
        public void shallow_cloned_season_state_maintains_standard_wildcard()
        {
            // Assert
            Assert.That(_clonedSeasonState.StandardWildCardPlayed, Is.EqualTo(_originalSeasonState.StandardWildCardPlayed));
        }

        [Test]
        public void shallow_cloned_season_state_maintains_transfer_window_wildcard()
        {
            // Assert
            Assert.That(_clonedSeasonState.TransferWindowWildcardPlayed, Is.EqualTo(_originalSeasonState.TransferWindowWildcardPlayed));
        }
    }
}
