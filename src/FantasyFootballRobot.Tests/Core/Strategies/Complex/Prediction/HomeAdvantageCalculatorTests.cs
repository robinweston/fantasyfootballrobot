using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Prediction
{
    [TestFixture]
    class HomeAdvantageCalculatorTests
    {
        private ILocationAdvantageCalculator _locationAdvantageCalculator;

        [SetUp]
        public void SetUp()
        {
            _locationAdvantageCalculator = new LocationAdvantageCalculator();
        }

        [Test]
        public void home_advantage_multiplier_is_calculated_correctly()
        {
            // Arrange
            var players = new List<Player>
                              {
                                  CreatePlayerWithPastFixture(true, 10),
                                  CreatePlayerWithPastFixture(true, 20),
                                  CreatePlayerWithPastFixture(true, 30),
                                  CreatePlayerWithPastFixture(false, 10),
                                  CreatePlayerWithPastFixture(false, 20)
                              };

            //60 points at home, 30 away. Total points is 90. Total minutes is 450 

            // Act
            var locationAdvantage = _locationAdvantageCalculator.CalculateLocationAdvantage(players);

            // Assert
            const double average = 90.0/450;
            const double homeAverage = 60.0/270;
            Assert.That(locationAdvantage.HomeMatchPointsPerMinute, Is.EqualTo(homeAverage));
            Assert.That(locationAdvantage.HomeMatchMultiplier, Is.EqualTo(homeAverage / average));
        }

        [Test]
        public void away_advantage_multiplier_is_calculated_correctly()
        {
            // Arrange
            var players = new List<Player>
                              {
                                  CreatePlayerWithPastFixture(true, 10),
                                  CreatePlayerWithPastFixture(true, 20),
                                  CreatePlayerWithPastFixture(true, 30),
                                  CreatePlayerWithPastFixture(false, 10),
                                  CreatePlayerWithPastFixture(false, 20)
                              };

            //60 points at home, 30 away. Ratio is 60/30 = 2

            // Act
            var locationAdantage = _locationAdvantageCalculator.CalculateLocationAdvantage(players);

            // Assert
            const double average = 90.0 / 450;
            const double awayAverage = 30.0 / 180;
            Assert.That(locationAdantage.AwayMatchPointsPerMinute, Is.EqualTo(awayAverage));
            Assert.That(locationAdantage.AwayMatchMultiplier, Is.EqualTo(awayAverage / average));
        }

        private Player CreatePlayerWithPastFixture(bool home, int pointScored)
        {
            return new Player
                       {
                           PastFixtures = new List<PastFixture>
                                              {
                                                  new PastFixture{Home = home, TotalPointsScored = pointScored, MinutesPlayed = 90}
                                              }
                       };
        }
    }
}
