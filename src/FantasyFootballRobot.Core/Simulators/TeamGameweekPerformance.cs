using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public class TeamGameweekPerformance
    {
        public IList<PlayerGameweekPerformance> PlayerPerformances { get; set; }
        public TransferActions TransferActions { get; set; }
        public TransferActionsResult TransferResults { get; set; }

        public int TotalPlayerPointsScored
        {
            get
            {
                return PlayerPerformances.Sum(x => x.TotalPointsScored);
            }
        }

        public int TotalPointsScored
        {
            get
            {
                return TotalPlayerPointsScored - TransferResults.TransferPointsPenalty;
            }
        }

        public int Gameweek { get; set; }
    }
}