using System.Collections.Generic;

namespace FantasyFootballRobot.Core.Simulators
{
    public class SeasonSimulationOptions
    {
        public bool ChooseInitialTeamOnly { get; set; }
        public bool CalculateTeamStrengthOnly { get; set; }
        public bool CalculatePlayerFormOnly { get; set; }

        public bool ListUsage { get; set; }
        public bool CalculateHomeAdvantageRatioOnly { get; set; }

        public bool UseSavedInitialTeam { get; set; }

        public int? MaximumGameweek { get; set; }

        public IList<int> InitialTeamPlayerIds
        {
            get
            {
                return new List<int>
                       {
                           334,460,340,189,152,447,105,200,12,21,227,475,435,7,314
                       };
            }
        }

        public string StrategyName { get; set; }
    }
}