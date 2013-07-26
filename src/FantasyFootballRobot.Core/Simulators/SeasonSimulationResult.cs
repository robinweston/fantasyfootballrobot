using System.Collections.Generic;
using System.Linq;

namespace FantasyFootballRobot.Core.Simulators
{
    public class SeasonSimulationResult
    {
        public SeasonSimulationResult()
        {
            GameweekResults = new List<TeamGameweekPerformance>();
        }

        public IList<TeamGameweekPerformance> GameweekResults { get; set; }

        public int TotalPointsScored { get { return GameweekResults.Sum(x => x.TotalPointsScored); } }
        public int TotalTransfersMade { get { return GameweekResults.Sum(x => x.TransferResults.TotalTransfersMade); } }
        public int TotalPenalisedTransfersMade { get { return GameweekResults.Sum(x => x.TransferResults.PenalisedTransfersMade); } }
        public int TotalTransferPointsPenalties { get { return GameweekResults.Sum(x => x.TransferResults.TransferPointsPenalty); } }
    }
}