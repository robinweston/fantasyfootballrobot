using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Simulators
{
    public class TransferValidator : ITransferValidator
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly ITransferActioner _transferActioner;

        public TransferValidator(IConfigurationSettings configurationSettings, ITransferActioner transferActioner)
        {
            _configurationSettings = configurationSettings;
            _transferActioner = transferActioner;
        }

        public TransferValidity ValidateTransfers(SeasonState seasonState, TransferActions transferActions)
        {
            if (seasonState.Gameweek == 1 && transferActions.Transfers.Any())
            {
                return TransferValidity.NoTransfersAllowedInFirstGameweek;
            }

            foreach (var transfer in transferActions.Transfers)
            {
                var transferValidity = CheckTransferValidity(transfer, seasonState.CurrentTeam);
                if (transferValidity != TransferValidity.Valid)
                {
                    return transferValidity;
                }
            }

            if (transferActions.Transfers.DistinctBy(t => t.PlayerIn.Id).Count() != transferActions.Transfers.Count)
            {
                return TransferValidity.PlayerTransferredInMultipleTimes;
            }

            if (transferActions.Transfers.DistinctBy(t => t.PlayerOut.Id).Count() != transferActions.Transfers.Count)
            {
                return TransferValidity.PlayerTransferredOutMultipleTimes;
            }

            var wildcardValidity = CheckWildcardValidity(seasonState, transferActions);
            if(wildcardValidity != TransferValidity.Valid)
            {
                return wildcardValidity;
            }

            if(!TeamCanAffordTransfers(seasonState, transferActions))
            {
                return TransferValidity.NotEnoughMoney;
            }

            var teamVailidity = CheckResultingTeamValidity(seasonState, transferActions);
            if(teamVailidity != TeamValidationStatus.Valid)
            {
                return TransferValidity.LeavesTeamInInvalidState;
            }

            return TransferValidity.Valid;
        }

        private bool TeamCanAffordTransfers(SeasonState seasonState, TransferActions transferActions)
        {
            var newMoneyValue = CommonTransferFunctions.CalculateUpdatedTeamMoney(transferActions.Transfers,
                                                                                  seasonState.Money);
            return newMoneyValue >= 0;
        }

        private TeamValidationStatus CheckResultingTeamValidity(SeasonState seasonState, TransferActions transferActions)
        {
            var clonedSeasonState = seasonState.ShallowClone();

            _transferActioner.VerboseLoggingEnabled = false;
            var result = _transferActioner.ApplyTransfers(clonedSeasonState, transferActions);
            return result.UpdatedSeasonState.CurrentTeam.Validity;
        }

        private TransferValidity CheckWildcardValidity(SeasonState seasonState, TransferActions transferActions)
        {
            if (transferActions.PlayStandardWildcard && seasonState.StandardWildCardPlayed)
            {
               return TransferValidity.WildcardPlayedTwice;
            }
            
            if (transferActions.PlayTransferWindowWildcard)
            {
                if (seasonState.TransferWindowWildcardPlayed)
                {
                    return TransferValidity.WildcardPlayedTwice;
                }

                var insideTransferWindowWildcardPeriod = IsInsideTransferWindow(seasonState.Gameweek);
                if (!insideTransferWindowWildcardPeriod)
                {
                    return TransferValidity.WildcardPlayedOutsideWindow;
                }
            }

            return TransferValidity.Valid;
        }

        public bool IsInsideTransferWindow(int gameweek)
        {
            return gameweek >= _configurationSettings.TransferWindowWildcardGameweekStart &&
                   gameweek <= _configurationSettings.TransferWindowWildcardGameweekEnd;
        }

        private static TransferValidity CheckTransferValidity(Transfer transfer, Team currentTeam)
        {
            if (transfer.PlayerIn == null || transfer.PlayerOut == null)
            {
                return TransferValidity.PlayerIsNull;
            }

            if (currentTeam.Players.Any(p => p.Id == transfer.PlayerIn.Id))
            {
                return TransferValidity.PlayerTransferredInAlreadyInTeam;
            }

            //player transferred out must have been in original team
            if (currentTeam.Players.All(p => p.Id != transfer.PlayerOut.Id))
            {
                return TransferValidity.PlayerTransferredOutNotOriginallyInTeam;
            }

            if (transfer.PlayerIn.Position != transfer.PlayerOut.Position)
            {
                return TransferValidity.TransferedPlayersNotInSamePosition;
            }

            return TransferValidity.Valid;
        }
    }
}