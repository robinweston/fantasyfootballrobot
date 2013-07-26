using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class PastFixturesResolver : ValueResolver<object[][], IList<PastFixture>>
    {
        readonly IConfigurationSettings _configSettings;

        public PastFixturesResolver(IConfigurationSettings configSettings)
        {
            _configSettings = configSettings;
        }

        protected override IList<PastFixture> ResolveCore(object[][] source)
        {
            var fixtures = new List<PastFixture>();
            foreach (var arr in source)
            {
                var fixture = new PastFixture
                                  {
                                      Date = ParseFixtureDate((string) arr[0]),
                                      GameWeek = (int) ((long) arr[1]),
                                      TeamGoals = ParseTeamGoals((string)arr[2]),
                                      OppositionGoals = ParseOppositionGoals((string)arr[2]),
                                      OppositionClubCode = ParseFixtureOpposition((string) arr[2]),                                      
                                      Home = ParseFixtureHome((string) arr[2]),
                                      PlayerValueAtTime = PerformanceMappingHelper.ParsePlayerValueAtTime((long)arr[17]),
                                      TotalPointsScored = (int)((long)arr[18])
                                  };

                PerformanceMappingHelper.UpdatePastPerformanceStats(fixture, arr, 3);

                fixtures.Add(fixture);
            }
            return fixtures;
        }

        private static int ParseTeamGoals(string s)
        {
            return int.Parse(s.Split('-').First().Split(' ').Last());
        }

        private static int ParseOppositionGoals(string s)
        {
            return int.Parse(s.Split('-').Last());
        }

        private static bool ParseFixtureHome(string s)
        {
            return s.ToLower().Contains("(h)");
        }

        private static string ParseFixtureOpposition(string s)
        {
            return s.Split('(').First().Trim();
        }

        private DateTime ParseFixtureDate(string dateText)
        {
            return FixtureDateHelper.CalculateFixtureDate(dateText, _configSettings.SeasonStartYear);
        }
    }
}