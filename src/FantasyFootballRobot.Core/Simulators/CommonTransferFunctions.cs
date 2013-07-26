using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public static class CommonTransferFunctions
    {
        public static int CalculateUpdatedTeamMoney(IList<Transfer> transfers, int currentMoney)
        {
            //process transfers as a whole
            int totalMoneySpent = transfers.Sum(t => t.PlayerIn.NowCost);

            int totalMoneyIn = 0;
            foreach (var transfer in transfers)
            {
                if (transfer.PlayerOut.NowCost <= transfer.PlayerOut.OriginalCost)
                {
                    totalMoneyIn += transfer.PlayerOut.NowCost;
                }
                else
                {
                    var difference = transfer.PlayerOut.NowCost - transfer.PlayerOut.OriginalCost;

                    //you get 50% of the difference, rounded down to nearest 0.1 mllion
                    totalMoneyIn += transfer.PlayerOut.OriginalCost + (difference / 2);
                }
            }

            var newMoneyValue = currentMoney - totalMoneySpent + totalMoneyIn;

            return newMoneyValue;
        }
    }
}
