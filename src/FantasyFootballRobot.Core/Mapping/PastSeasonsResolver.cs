using System.Collections.Generic;
using AutoMapper;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class PastSeasonsResolver : ValueResolver<object[], IList<Season>>
    {
        protected override IList<Season> ResolveCore(object[] source)
        {
            var pastSeasons = new List<Season>();

            foreach(object[] arr in source)
            {
                var pastSeason = new Season
                                     {
                    SeasonEndYear = ParseSeasonEndDate((string)arr[0]),
                    PlayerValueAtTime = PerformanceMappingHelper.ParsePlayerValueAtTime((long)arr[14]),
                    TotalPointsScored = (int)((long)arr[15])
                };

                PerformanceMappingHelper.UpdatePastPerformanceStats(pastSeason, arr, 1);

                pastSeasons.Add(pastSeason);
            }

            return pastSeasons;
        }

        private static int ParseSeasonEndDate(string s)
        {
            //y3k bug!!!
            return 2000 + int.Parse(s.Substring(s.Length - 2));
        }
    }
}