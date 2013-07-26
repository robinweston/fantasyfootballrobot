using System.Collections.Generic;

namespace FantasyFootballRobot.Core.Strategies
{
    public class TransferActions
    {
        public TransferActions()
        {
            Transfers = new List<Transfer>();
        }

        public IList<Transfer> Transfers { get; set; }

        public bool PlayStandardWildcard { get; set; }

        public bool PlayTransferWindowWildcard { get; set; }
    }
}