using System;

namespace FantasyFootballRobot.Core.Entities
{
    [Serializable]
    public class PastFixture : PerformanceBase
    {
        public bool Home { get; set; }

        public string OppositionClubCode { get; set; }

        public DateTime Date { get; set; }

        public int GameWeek { get; set; }

    }
}