using System;

namespace FantasyFootballRobot.Core.Mapping
{
    public static class FixtureDateHelper
    {
        public static DateTime CalculateFixtureDate(string dateText, int seasonStartYear)
        {
            int fixtureYear = seasonStartYear;

            DateTime fixtureDate;
            if (seasonStartYear == 2011)
            {
                fixtureDate = DateTime.Parse(dateText);
            }
            else
            {
                fixtureDate = DateTime.ParseExact(dateText, "dd MMM H:mm", null).Date;
            }
            if (fixtureDate.Month <=6)
            {
                fixtureYear++;
            }
            fixtureDate = new DateTime(fixtureYear, fixtureDate.Month, fixtureDate.Day);
            return fixtureDate;
        }
    }
}
