using System;

namespace FantasyFootballRobot.Core.Entities
{
    [Serializable]
    public class FutureFixture
    {
        public string OppositionClubCode { get; set; }
        public bool Home { get; set; }
        public DateTime Date { get; set; }
        public int GameWeek { get; set; }
    }
}
