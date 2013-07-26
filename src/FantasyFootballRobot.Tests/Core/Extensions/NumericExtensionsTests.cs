using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Extensions;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Extensions
{
    [TestFixture]
    class NumericExtensionsTests
    {
        [Test]
        public void formatted_integer_displays_correctly()
        {
            //Arrange

            //Act
            var result = (12345678).ToFormatted();

            //Assert
            Assert.That(result, Is.EqualTo("12,345,678"));
        }

        [Test]
        public void formatted_double_displays_correctly()
        {
            //Arrange

            //Act
            var result = (1234.5678).ToFormatted();

            //Assert
            Assert.That(result, Is.EqualTo("1,234.57"));
        }
    }
}
