using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;

namespace FantasyFootballRobot.Tests.Helpers
{
    public static class TestTransferHelper
    {
        public static IList<Transfer> CreateValidTransfers(Team currentTeam, int transferCount)
        {
            var transfers = new List<Transfer>();
            for (int i = 0; i < transferCount; i++)
            {
                //start at 3rd player so captain and vice captain are untouched
                var playerOut = currentTeam.Players[i + 2];

                var playerIn = playerOut.DeepClone();
                playerIn.Id = 102 + i;

                var transfer = new Transfer
                {
                    PlayerIn = playerIn,
                    PlayerOut = playerOut
                };

                transfers.Add(transfer);
            }
            return transfers;
        }

        public static FitTransferActions CreateFitTransferActions(int transferCount, Team currentTeam, bool playStandardWildcard = false, bool playTransferWindowWildcard = false)
        {
            var transferActions = new FitTransferActions
                                  {
                                      PlayStandardWildcard = playStandardWildcard,
                                      PlayTransferWindowWildcard = playTransferWindowWildcard
                                  };

            for (var i = 0; i < transferCount; i++)
            {
                transferActions.Transfers.Add(new Transfer { PlayerIn = new Player(), PlayerOut = currentTeam.Players.First() });
            }

            return transferActions;
        }
    }
}
