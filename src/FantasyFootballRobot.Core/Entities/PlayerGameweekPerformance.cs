namespace FantasyFootballRobot.Core.Entities
{
    public class PlayerGameweekPerformance : PerformanceBase
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public Position Position { get; set; }
        public bool IsCaptain { get; set; }
    }
}