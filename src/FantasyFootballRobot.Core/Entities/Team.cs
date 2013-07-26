using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities.Utilities.Cloning;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Entities
{
    public class Team : IDeepCloneable<Team>
    {
        public Team()
        {
            Players = new List<Player>();
        }

        public IList<Player> Players { get; set; }

        public Player Captain { get; set; }

        public Player ViceCaptain { get; set; }

        public TeamValidationStatus Validity 
        {
            get { return TeamValidator.ValidateTeam(this); }
        }

        public void EnsureValidCaptains()
        {
            Captain = Players[0];
            ViceCaptain = Players[1];
        }

        public Team ShallowClone()
        {
            return TeamCloner.ShallowClone(this);
        }

        public Team DeepClone()
        {
            return TeamCloner.DeepClone(this);
        }
    }
}