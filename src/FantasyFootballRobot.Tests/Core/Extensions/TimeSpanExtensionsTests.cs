using System;
using FantasyFootballRobot.Core.Extensions;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Extensions
{
    [TestFixture]
    class TimeSpanExtensionsTests
    {
        [Test]
        public void formatted_timespan_displays_correctly()
        {
            //Act
            var result = new TimeSpan(0, 1, 2, 3, 456).ToFormatted();

            //Assert
            Assert.That(result, Is.EqualTo("01:02:03:4560000"));
        }
    }
}
