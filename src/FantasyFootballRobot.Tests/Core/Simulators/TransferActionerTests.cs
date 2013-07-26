using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    class TransferActionerTests
    {
        private ITransferActioner _transferActioner;
        private TransferActions _transferActions;
        private SeasonState _seasonState;
        private IList<Player> _allPlayers;

        [SetUp]
        public void SetUp()
        {
            _seasonState = new SeasonState
            {
                Gameweek = 2,
                FreeTransfers = 1,
                CurrentTeam = TeamCreationHelper.CreateTestTeam(4, 3, 3, Position.Defender,
                                                                                        Position.Midfielder,
                                                                                        Position.Midfielder)

            };
            _transferActions = new TransferActions();

            _transferActioner = new TransferActioner(new Mock<ILogger>().Object);

            _allPlayers = new List<Player>();
        }

        [Test]
        public void transfers_made_is_equal_to_the_sum_of_transfers()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TotalTransfersMade, Is.EqualTo(1));
        }

        [Test]
        public void no_points_deducted_for_single_transfer()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.FreeTransfersMade, Is.EqualTo(1));
            Assert.That(result.TotalTransfersMade, Is.EqualTo(1));
            Assert.That(result.PenalisedTransfersMade, Is.EqualTo(0));
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(0));
        }

        [Test]
        public void four_points_deducted_for_two_transfers()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 2);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.FreeTransfersMade, Is.EqualTo(1));
            Assert.That(result.TotalTransfersMade, Is.EqualTo(2));
            Assert.That(result.PenalisedTransfersMade, Is.EqualTo(1));
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(4));
        }

        [Test]
        public void transfer_totals_correctly_calcualted_if_not_all_free_transfers_are_used()
        {
            //Arrange
            _seasonState.FreeTransfers = 2;
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.FreeTransfersMade, Is.EqualTo(1));
            Assert.That(result.TotalTransfersMade, Is.EqualTo(1));
            Assert.That(result.PenalisedTransfersMade, Is.EqualTo(0));
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(0));
        }

        [Test]
        public void eight_points_deducted_for_three_transfers()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(8));
        }

        [Test]
        public void four_points_deducted_for_three_transfers_if_one_free_transfer_existed()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);
            _seasonState.FreeTransfers = 2;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(4));
        }

        [Test]
        public void no_points_deducted_for_three_transfers_if_standard_wildcard_is_played()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);
            _transferActions.PlayStandardWildcard = true;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(0));
        }

        [Test]
        public void no_points_deducted_for_four_transfers_if_transfer_window_wildcard_is_played()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 22;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TransferPointsPenalty, Is.EqualTo(0));
        }


        [Test]
        public void if_no_transfers_are_made_then_team_gets_a_bonus_transfer_next_gameweek()
        {
            //Arrange
            //actions contain no transfers
            _transferActions.Transfers.Clear();

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(2));

        }

        [Test]
        public void if_one_transfer_is_made_made_then_team_gets_one_free_transfer_next_gameweek()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(1));

        }

        [Test]
        public void if_no_transfers_are_made_for_multiple_gameweeks_in_a_row_then_team_only_ever_gets_a_maximum_of_one_bonus_transfer()
        {
            //Arrange
            _seasonState.FreeTransfers = 2;

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(2));
        }

        [Test]
        public void if_player_has_a_bonus_transfer_it_carries_over_if_they_dont_use_it()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);
            _seasonState.FreeTransfers = 2;

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(2));
        }

        [Test]
        public void if_player_plays_standard_wildcard_their_free_transfers_remain_the_same()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);
            _transferActions.PlayStandardWildcard = true;
            _seasonState.FreeTransfers = 2;

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(2));
        }

        [Test]
        public void if_player_plays_transfer_window_wildcard_their_free_transfers_remain_the_same()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 3);
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 22;

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.FreeTransfers, Is.EqualTo(1));
        }


        [Test]
        public void transfer_window_wildcard_can_be_played()
        {
            //Arrange
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 22;

            //Act
            _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(_seasonState.TransferWindowWildcardPlayed, Is.True);
        }

        [Test]
        public void transfers_made_is_set_to_the_sum_of_tranfers()
        {
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.TotalTransfersMade, Is.EqualTo(1));
        }

        [Test]
        public void free_transfers_never_go_below_one()
        {
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 5);

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(result.UpdatedSeasonState.FreeTransfers, Is.EqualTo(1));

        }

        [Test]
        public void team_money_is_updated_correctly_for_player_who_maintained_value_then_transferred()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 10 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 100;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 88;
            _transferActions.Transfers.First().PlayerOut.NowCost = 88;

            //2 million in bank
            _seasonState.Money = 20;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //2 - 10 + 8.8 = 0.8
            Assert.That(result.UpdatedSeasonState.Money, Is.EqualTo(8));
        }

        [Test]
        public void team_money_is_updated_correctly_for_player_who_gained_point_two_million_in_value_then_transferred()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 5 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 50;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 86;
            _transferActions.Transfers.First().PlayerOut.NowCost = 88;

            //nothing in bank
            _seasonState.Money = 0;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //0 - 5 + 8.7 = 3.7
            Assert.That(result.UpdatedSeasonState.Money, Is.EqualTo(37));
        }

        [Test]
        public void team_money_is_updated_correctly_for_player_who_gained_point_one_million_in_value_then_transferred()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 5 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 50;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 87;
            _transferActions.Transfers.First().PlayerOut.NowCost = 88;

            //nothing in bank
            _seasonState.Money = 0;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //0 - 5 + 8.7 = 3.7
            Assert.That(result.UpdatedSeasonState.Money, Is.EqualTo(37));
        }



        [Test]
        public void team_money_is_updated_correctly_for_player_who_gained_point_seven_million_in_value_then_transferred()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 5 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 50;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 80;
            _transferActions.Transfers.First().PlayerOut.NowCost = 87;

            //nothing in bank
            _seasonState.Money = 0;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //0 - 5 + 8.3 = 3.3
            Assert.That(result.UpdatedSeasonState.Money, Is.EqualTo(33));
        }

        [Test]
        public void team_money_is_updated_correctly_for_player_who_lost_value_then_transferred()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 5 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 50;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 80;
            _transferActions.Transfers.First().PlayerOut.NowCost = 60;

            //nothing in bank
            _seasonState.Money = 0;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //0 - 5 + 6 = 1
            Assert.That(result.UpdatedSeasonState.Money, Is.EqualTo(10));
        }

        [Test]
        public void purchased_players_have_their_purchased_cost_set_correctly()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 5 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 50;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 80;
            _transferActions.Transfers.First().PlayerOut.NowCost = 60;

            //nothing in bank
            _seasonState.Money = 0;

            //Act
            var result = _transferActioner.ApplyTransfers(_seasonState, _transferActions);

            //Assert
            //0 - 5 + 6 = 1
            var newTransfer =
                result.UpdatedSeasonState.CurrentTeam.Players.Single(
                    x => x.Id == _transferActions.Transfers.First().PlayerIn.Id);
            Assert.That(newTransfer.OriginalCost, Is.EqualTo(50));
        }
    }
}
