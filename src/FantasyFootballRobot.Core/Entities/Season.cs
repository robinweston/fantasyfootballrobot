using System;

namespace FantasyFootballRobot.Core.Entities
{
    [Serializable]
    public class Season : PerformanceBase
    {
        public int SeasonEndYear { get; set; }
    }
}