using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Mapping
{
    [TestFixture]
    public class AutoMapperConfigurationTests
    {
        [Test]
        public void check_auto_mapper_configuration_is_valid()
        {
            AutoMapper.Mapper.AssertConfigurationIsValid();
        }
    }
}
