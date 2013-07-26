using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Validation;
using FantasyFootballRobot.Tests.Helpers;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Validation
{
    [TestFixture]
    public class TeamValidatorTests
    {

        [Test]
        public void valid_team_is_valid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.Valid));
        }

        [Test]
        public void team_with_too_few_players_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.Players.RemoveAt(0);

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.TooFewPlayers));

        }

        [Test]
        public void team_with_too_many_players_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.Players.Add(new Player());

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.TooManyPlayers));

        }

        [Test]
        public void duplicate_players_are_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.Players[4] = team.Players[5];

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.DuplicatePlayers));
        }

        [Test]
        public void team_without_captain_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.Captain = null;
            
                //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidCaptain));
        }

        [Test]
        public void team_without_vice_captain_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.ViceCaptain = null;

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidViceCaptain));
        }

        [Test]
        public void team_with_captain_not_in_team_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.Captain = new Player{Id = 100};

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidCaptain));
        }

        [Test]
        public void team_with_vice_captain_not_in_team_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.ViceCaptain = new Player { Id = 100 };

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidViceCaptain));
        }

        [Test]
        public void vice_captain_the_same_as_captain_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);
            team.ViceCaptain = team.Captain;

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidViceCaptain));
        }

        [Test]
        [TestCase(Position.Defender, Position.Defender, Position.Defender)]
        [TestCase(Position.Goalkeeper, Position.Defender, Position.Defender)]
        [TestCase(Position.Forward, Position.Defender, Position.Midfielder)]
        [TestCase(Position.Midfielder, Position.Midfielder, Position.Defender)]
        public void invalid_player_distribution_is_invalid(Position sub1Position, Position sub2Position, Position sub3Position)
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, sub1Position, sub2Position,
                                                         sub3Position);

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidPlayerDistribution));
        }

        [Test]
        [TestCase(2, 5, 3, Position.Defender, Position.Defender, Position.Defender)]
        [TestCase(5, 5, 0, Position.Forward, Position.Forward, Position.Forward)]
        public void 
            invalid_formation_is_invalid(int defenders, int midfielders, int forwards, Position sub1Position, Position sub2Position, Position sub3Position)
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(defenders, midfielders, forwards, sub1Position, sub2Position,
                                                         sub3Position);

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.InvalidFormation));
        }

        [Test]
        public void more_than_three_players_from_one_club_is_invalid()
        {
            //Arrange
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);

            team.Players.Take(4).ToList().ForEach(x => x.ClubCode = "SUN");

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.TooManyPlayersFromOneClub));       
        }

        [Test]
        public void team_is_invalid_if_it_contains_more_than_one_player_from_same_team_in_same_position()
        {
            var team = TeamCreationHelper.CreateTestTeam(3, 4, 3, Position.Defender, Position.Defender,
                                                         Position.Midfielder);

            team.Players.Where(x => x.Position == Position.Goalkeeper).ToList().ForEach(x => x.ClubCode = "SUN");

            //Act
            var result = TeamValidator.ValidateTeam(team);

            //Assert
            Assert.That(result, Is.EqualTo(TeamValidationStatus.TooManyPlayersFromOneClubInSamePosition ));       
            
        }
    }
}
