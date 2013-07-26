using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Entities.Utilities.Cloning;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public class FitTeam : Team, IChromosome, IShallowCloneable<FitTeam>
    {
        public double? Fitness { get; set; }

        public new FitTeam ShallowClone()
        {
            return TeamCloner.ShallowClone(this);
        }
    }
}
