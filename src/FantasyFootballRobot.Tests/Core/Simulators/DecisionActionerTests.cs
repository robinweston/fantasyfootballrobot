using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Exceptions;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Tests.Helpers;
using Microsoft.Practices.ObjectBuilder2;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    public class DecisionActionerTests
    {
        private IDecisionActioner _decisionActioner;
        private TransferActions _transferActions;
        private SeasonState _seasonState;
        private IList<Player> _allPlayers;
        private Mock<ITransferValidator> _transferValidatorMock;
        private Mock<ITransferActioner> _transferActionerMock; 

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

            _transferValidatorMock = new Mock<ITransferValidator>();
            _transferActionerMock = new Mock<ITransferActioner>();

            _decisionActioner = new DecisionActioner(_transferActionerMock.Object, _transferValidatorMock.Object);

            _allPlayers = new List<Player>();
        }

        [Test]
        [ExpectedException(typeof(InvalidTransferException))]
        public void transfers_are_validated_and_exception_thrown_if_invalid()
        {
            // Arrange
            _transferValidatorMock.Setup(
                x => x.ValidateTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>())).Returns(
                    TransferValidity.NotEnoughMoney);

            // Act
            _decisionActioner.ValidateAndApplyTransfers(_seasonState, _transferActions);

            // Assert
            _transferValidatorMock.VerifyAll();
        }

        [Test]
        public void if_transfers_are_valid_then_they_are_applied()
        {
            // Arrange
            var transferResult = new TransferActionsResult();
            _transferActionerMock.Setup(x => x.ApplyTransfers(_seasonState, _transferActions)).Returns(transferResult);

            // Act
            var result = _decisionActioner.ValidateAndApplyTransfers(_seasonState, _transferActions);

            // Assert
            Assert.That(result, Is.EqualTo(transferResult));
        }


        [Test]
        [ExpectedException(typeof(InvalidTeamException))]
        public void selected_team_is_checked_for_validity_when_selecting_starting_team()
        {
            //Arrange
            //team has too few players
            _seasonState.CurrentTeam.Players.Clear();

            //Act
            _decisionActioner.ValidateAndApplyGameweekTeamSelection(_seasonState, _seasonState.CurrentTeam);
        }

        [Test]
        public void valid_starting_team_is_created()
        {
            //Arrange
            var startingTeam = TeamCreationHelper.CreateTestTeam();

            //Act
            var seasonState = _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, _allPlayers);

            //Assert
            Assert.That(seasonState.CurrentTeam, Is.EqualTo(startingTeam));
        }

        [Test]
        public void starting_money_is_calculated_correctly()
        {
            //Arrange
            var startingTeam = TeamCreationHelper.CreateTestTeam();
            //make each player cost 6.6 million, so total team cost is 99 million
            startingTeam.Players.ForEach(x => x.NowCost = 66);

            //Act
            var seasonState = _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, _allPlayers);

            //Assert
            Assert.That(seasonState.Money, Is.EqualTo(10));
        }

        [Test]
        public void initial_season_state_properties_are_set_correctly()
        {
            //Arrange
            var startingTeam = TeamCreationHelper.CreateTestTeam();

            //Act
            var seasonState = _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, _allPlayers);

            //Assert
            Assert.That(seasonState.FreeTransfers, Is.EqualTo(1));
            Assert.That(seasonState.AllPlayers, Is.EqualTo(_allPlayers));
            Assert.That(seasonState.StandardWildCardPlayed, Is.False);
            Assert.That(seasonState.TransferWindowWildcardPlayed, Is.False);
        }

        [Test]
        [ExpectedException(typeof(InvalidTeamException))]
        public void invalid_starting_team_throws_exception()
        {
            //Arrange
            var startingTeam = TeamCreationHelper.CreateTestTeam();
            startingTeam.Players.RemoveAt(0);

            //Act
            _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, _allPlayers);
        }

        [Test]
        [ExpectedException(typeof(InvalidTeamException), ExpectedMessage = "StartingTeamTooExpensive")]
        public void starting_team_that_is_too_expensive_throws_exception()
        {
            //Arrange
            var startingTeam = TeamCreationHelper.CreateTestTeam();
            //make each player cost 6.7 million, so total team cost is 100.5 million
            startingTeam.Players.ForEach(x => x.NowCost = 67);

            //Act
            _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, _allPlayers);
        }

        
    }
}
