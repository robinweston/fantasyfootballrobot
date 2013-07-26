using System;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public class TransferActioner : ITransferActioner
    {
        private readonly ILogger _logger;

        public TransferActioner(ILogger logger)
        {
            _logger = logger;
        }

        public TransferActionsResult ApplyTransfers(SeasonState seasonState, TransferActions transferActions)
        {
            var actionResults = new TransferActionsResult();

            if (VerboseLoggingEnabled)
            {
                _logger.Log(Tag.Transfers, string.Format("Applying gameweek {0} transfers", seasonState.Gameweek), true);
            }

            //set transferred in player cost
            foreach (var transfer in transferActions.Transfers)
            {
                seasonState.CurrentTeam = ProcessTransfer(transfer, seasonState.CurrentTeam);
            }

            seasonState.CurrentTeam.EnsureValidCaptains();

            actionResults.TotalTransfersMade = transferActions.Transfers.Count;          
  
            actionResults.PenalisedTransfersMade = Math.Max(transferActions.Transfers.Count - seasonState.FreeTransfers, 0);    
            seasonState.Money = CommonTransferFunctions.CalculateUpdatedTeamMoney(transferActions.Transfers, seasonState.Money);

            if (VerboseLoggingEnabled)
            {
                _logger.Log(Tag.Transfers, string.Format("Total transfers made: {0}", actionResults.TotalTransfersMade));
                _logger.Log(Tag.Transfers, string.Format("Penalised transfers made: {0}", actionResults.PenalisedTransfersMade));
                _logger.Log(Tag.Transfers, string.Format("Updated money: {0}", seasonState.Money.ToMoney()));
            }
           
            UpdateFreeTransfers(transferActions, seasonState);

            actionResults.UpdatedSeasonState = seasonState;

            actionResults = ApplyWildcards(seasonState, transferActions, actionResults);

            return actionResults;
        }

        Team ProcessTransfer(Transfer transfer, Team currentTeam)
        {
            currentTeam.Players.Remove(transfer.PlayerOut);

            transfer.PlayerIn.OriginalCost = transfer.PlayerIn.NowCost;
            currentTeam.Players.Add(transfer.PlayerIn);

            if (VerboseLoggingEnabled)
            {
                LogHelper.LogTransfer(transfer, _logger, true);
            }

            return currentTeam;
        }

        private static void UpdateFreeTransfers(TransferActions actions, SeasonState seasonState)
        {
            //transfers carry over if you're playing wildcard
            if (!actions.PlayTransferWindowWildcard && !actions.PlayStandardWildcard)
            {
                //deduct the transfers made
                seasonState.FreeTransfers -= actions.Transfers.Count;

                //you can never have negative free transfers
                seasonState.FreeTransfers = Math.Max(seasonState.FreeTransfers, 0);

                //add on one transfer for new game week
                seasonState.FreeTransfers++;

                //add one another free transfer if you didn't make any this week
                if (actions.Transfers.Count == 0)
                {
                    seasonState.FreeTransfers++;
                }

                //you can only ever have a max of two free transfers
                seasonState.FreeTransfers = Math.Min(seasonState.FreeTransfers, 2);
            }
        }


        private TransferActionsResult ApplyWildcards(SeasonState seasonState, TransferActions actions, TransferActionsResult result)
        {
            if (actions.PlayStandardWildcard)
            {
                if (VerboseLoggingEnabled)
                {
                    _logger.Log(Tag.Transfers, "Standard wildcard played", true);
                }
                seasonState.StandardWildCardPlayed = true;
                result.PenalisedTransfersMade = 0;
            }
            else if (actions.PlayTransferWindowWildcard)
            {
                if (VerboseLoggingEnabled)
                {
                    _logger.Log(Tag.Transfers, "Transfer window wildcard played", true);
                }
                seasonState.TransferWindowWildcardPlayed = true;
                result.PenalisedTransfersMade = 0;
            }

            return result;
        }

        public bool VerboseLoggingEnabled { get; set; }
    }
}