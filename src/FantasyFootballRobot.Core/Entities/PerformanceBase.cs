using System;

namespace FantasyFootballRobot.Core.Entities
{
    [Serializable]
    public abstract class PerformanceBase
    {
        public int Bonus { get; set; }

        public int PlayerValueAtTime { get; set; }

        public int MinutesPlayed { get; set; }

        public int TotalPointsScored { get; set; }

        public int TeamGoals { get; set; }

        public int OppositionGoals { get; set; }

        public int GoalsScored { get; set; }

        public int Assists { get; set; }

        public int CleanSheets { get; set; }

        public int GoalsConceded { get; set; }

        public int OwnGoals { get; set; }

        public int PenaltiesSaved { get; set; }

        public int YellowCards { get; set; }

        public int RedCards { get; set; }

        public int Saves { get; set; }

        public int PenaltiesMissed { get; set; }
    }

}