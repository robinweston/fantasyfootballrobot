using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    class TransferValidatorTests
    {
        private SeasonState _seasonState;
        private TransferActions _transferActions;
        private ITransferValidator _transferValidator;
        private Mock<IConfigurationSettings> _configSettingsMock;
        private Mock<ITransferActioner> _transferActionerMock;


        [SetUp]
        public void SetUp()
        {
            _seasonState = new SeasonState
                               {
                                   CurrentTeam = TeamCreationHelper.CreateTestTeam(), 
                                   AllPlayers = TeamCreationHelper.CreatePlayerList(10,10,10,10, startingId: 100),
                                   Gameweek = 2
                               };
            _transferActions = new TransferActions();

            _transferActionerMock = new Mock<ITransferActioner>();
            _transferActionerMock.Setup(x => x.ApplyTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>())).
                Returns((SeasonState s, TransferActions t) => new TransferActionsResult{UpdatedSeasonState = s});

            _configSettingsMock = new Mock<IConfigurationSettings>();
            _transferValidator = new TransferValidator(_configSettingsMock.Object, _transferActionerMock.Object);
        }

        [Test]
        public void can_not_make_transfers_in_gameweek_one()
        {
            //Arrange
            _seasonState.Gameweek = 1;
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam,1);

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.NoTransfersAllowedInFirstGameweek));
        }

        [Test]
        public void can_not_play_standard_wildcard_twice()
        {
            //Arrange
            _transferActions.PlayStandardWildcard = true;
            _seasonState.StandardWildCardPlayed = true;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.WildcardPlayedTwice));
        }

        [Test]
        public void transfer_window_wildcard_can_only_be_played_once()
        {
            //Arrange
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 12;
            _seasonState.TransferWindowWildcardPlayed = true;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.WildcardPlayedTwice));
        }

        [Test]
        public void can_not_play_transfer_wildcard_too_early()
        {
            //Arrange
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 3;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.WildcardPlayedOutsideWindow));
        }

        [Test]
        public void can_not_play_transfer_window_wildcard_outside_window()
        {
            //Arrange
            _transferActions.PlayTransferWindowWildcard = true;
            _seasonState.Gameweek = 30;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.WildcardPlayedOutsideWindow));
        }

        [Test]
        public void transfers_cant_take_up_too_much_money()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //player bought for 10 million
            _transferActions.Transfers.First().PlayerIn.NowCost = 100;

            //player sold for 8.8 million
            _transferActions.Transfers.First().PlayerOut.OriginalCost = 88;
            _transferActions.Transfers.First().PlayerOut.NowCost = 88;

            //1.1 million in bank
            _seasonState.Money = 11;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.NotEnoughMoney));

        }

        [Test]
        public void transfers_should_have_a_player_out()
        {
            //Arrange
            _transferActions.Transfers = new List<Transfer> { new Transfer { PlayerIn = new Player() } };

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerIsNull));

        }

        [Test]
        public void transfered_player_in_must_not_alread_be_in_team()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);
            _transferActions.Transfers.First().PlayerIn = _seasonState.CurrentTeam.Players.First();

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerTransferredInAlreadyInTeam));

        }

        [Test]
        public void transferred_player_out_must_have_been_in_previously_selected_side()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);
            _transferActions.Transfers.First().PlayerOut = new Player { Id = 200 };

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerTransferredOutNotOriginallyInTeam));
        }

        [Test]
        public void transfers_should_have_a_player_in()
        {
            //Arrange
            _transferActions.Transfers = new List<Transfer> { new Transfer { PlayerOut = new Player() } };

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerIsNull));

        }

        [Test]
        public void valid_transfers_pass_validation()
        {
            //Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.Valid));
            
        }

        [Test]
        public void transfered_players_must_be_in_same_position()
        {
            // Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);
            _transferActions.Transfers.First().PlayerIn.Position = Position.Defender;
            _transferActions.Transfers.First().PlayerOut.Position = Position.Midfielder;

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.TransferedPlayersNotInSamePosition));
            
        }

        [Test]
        public void transfer_must_leave_team_in_valid_state()
        {
            // Arrange           

            //transfer adds 4 players from same club
            _seasonState.CurrentTeam.Players[0].ClubCode = "CLUB";
            _seasonState.CurrentTeam.Players[1].ClubCode = "CLUB";
            _seasonState.CurrentTeam.Players[2].ClubCode = "CLUB";
            _seasonState.CurrentTeam.Players[3].ClubCode = "CLUB";

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.LeavesTeamInInvalidState));
            
        }

        [Test]
        public void transfer_validator_leaves_team_unchanged()
        {
            // Arrange
            _transferActions.Transfers = TestTransferHelper.CreateValidTransfers(_seasonState.CurrentTeam, 1);
            var teamClone = _seasonState.CurrentTeam.DeepClone();

            // Act
            _transferValidator.ValidateTransfers(_seasonState, _transferActions);


            // Assert
            for (var i = 0; i < teamClone.Players.Count; i++)
            {
                var clonePlayer = teamClone.Players[i];
                Assert.That(_seasonState.CurrentTeam.Players[i].Id, Is.EqualTo(clonePlayer.Id));
            }
        }

        [Test]
        public void can_not_transfer_the_same_player_in_multiple_times_in_same_gameweek()
        {
            // Arrange
            var playerIn = _seasonState.AllPlayers.First();
            _transferActions = new TransferActions
                                   {
                                       Transfers = new List<Transfer>
                                                       {
                                                           new Transfer
                                                               {
                                                                   PlayerIn = playerIn,
                                                                   PlayerOut = _seasonState.CurrentTeam.Players.Where(p => p.Position == playerIn.Position).ElementAt(0)
                                                               },
                                                           new Transfer
                                                               {
                                                                   PlayerIn = playerIn,
                                                                   PlayerOut = _seasonState.CurrentTeam.Players.Where(p => p.Position == playerIn.Position).ElementAt(1)
                                                               }
                                                       }
                                   };

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerTransferredInMultipleTimes));
        }

        [Test]
        public void can_not_transfer_the_same_player_out_multiple_times_in_same_gameweek()
        {
            // Arrange
            var playerOut = _seasonState.CurrentTeam.Players.First();
            _transferActions = new TransferActions
            {
                Transfers = new List<Transfer>
                                                       {
                                                           new Transfer
                                                               {
                                                                   PlayerIn = _seasonState.AllPlayers[0],
                                                                   PlayerOut = playerOut
                                                               },
                                                           new Transfer
                                                               {
                                                                   PlayerIn = _seasonState.AllPlayers[1],
                                                                   PlayerOut =playerOut
                                                               }
                                                       }
            };

            //Act
            var transferValidity = _transferValidator.ValidateTransfers(_seasonState, _transferActions);

            //Assert
            Assert.That(transferValidity, Is.EqualTo(TransferValidity.PlayerTransferredOutMultipleTimes));        
        }

    }
}
