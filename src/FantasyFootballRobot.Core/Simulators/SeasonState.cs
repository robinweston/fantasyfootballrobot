using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Entities.Utilities.Cloning;

namespace FantasyFootballRobot.Core.Simulators
{
    //todo: parse season state from current login
    public class SeasonState : IShallowCloneable<SeasonState>
    {
        public Team CurrentTeam { get; set; }
        public int Gameweek { get; set; }
        public int Money { get; set; }

        public int FreeTransfers { get; set; }

        public IList<Player> AllPlayers { get; set; }

        public bool StandardWildCardPlayed { get; set; }

        public bool TransferWindowWildcardPlayed { get; set; }

        public SeasonState ShallowClone()
        {
            var clone = new SeasonState
                            {
                                AllPlayers = AllPlayers,
                                Gameweek = Gameweek,
                                FreeTransfers = FreeTransfers,
                                Money = Money,
                                StandardWildCardPlayed = StandardWildCardPlayed,
                                TransferWindowWildcardPlayed = TransferWindowWildcardPlayed,
                                CurrentTeam = CurrentTeam.ShallowClone()
                            };

            return clone;
        }
        
    }
}