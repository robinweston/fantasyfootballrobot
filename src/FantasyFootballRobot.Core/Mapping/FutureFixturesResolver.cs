using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class FutureFixturesResolver : ValueResolver<string[][], IList<FutureFixture>>
    {
        readonly IConfigurationSettings _configSettings;

        public FutureFixturesResolver(IConfigurationSettings configSettings)
        {
            _configSettings = configSettings;
        }

        protected override IList<FutureFixture> ResolveCore(string[][] source)
        {
            var fixtures = new List<FutureFixture>();
            foreach(var arr in source)
            {
                var fixture = new FutureFixture
                                  {
                                      Date = ParseFixtureDate(arr[0]),
                                      GameWeek = ParseFixtureGameweek(arr[1]),
                                      OppositionClubCode = ParseFixtureOpposition(arr[2]),
                                      Home = ParseFixtureHome(arr[2])
                                  };
                fixtures.Add(fixture);
            }
            return fixtures;
        }

        private static bool ParseFixtureHome(string s)
        {
            return s.ToLower().Contains("(h)");
        }

        private static string ParseFixtureOpposition(string s)
        {
            var teamName = s.Split('(').First().Trim();
            return Club.GetCodeFromTeamName(teamName);
        }

        private static int ParseFixtureGameweek(string s)
        {
            return int.Parse(s.ToLower().Replace("gameweek ", ""));
        }

        private DateTime ParseFixtureDate(string dateText)
        {
            return FixtureDateHelper.CalculateFixtureDate(dateText, _configSettings.SeasonStartYear);
        }
    }
}