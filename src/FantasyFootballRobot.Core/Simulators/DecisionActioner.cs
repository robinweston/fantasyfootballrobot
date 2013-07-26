using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Constants;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Exceptions;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Simulators
{

    public class DecisionActioner : IDecisionActioner
    {
        private readonly ITransferActioner _transferActioner;
        private readonly ITransferValidator _transferValidator;

        public DecisionActioner(ITransferActioner transferActioner, ITransferValidator transferValidator)
        {
            _transferActioner = transferActioner;
            _transferValidator = transferValidator;
        }

        public TransferActionsResult ValidateAndApplyTransfers(SeasonState seasonState, TransferActions transferActions)
        {        
            if (seasonState == null) throw new ArgumentNullException("seasonState");
            if (transferActions == null) throw new ArgumentNullException("transferActions");

            var transferValidity = _transferValidator.ValidateTransfers(seasonState, transferActions);
            if(transferValidity != TransferValidity.Valid)
            {
                throw new InvalidTransferException(transferValidity);
            }

            _transferActioner.VerboseLoggingEnabled = true;
            var result = _transferActioner.ApplyTransfers(seasonState, transferActions);

            return result;
        }

        public SeasonState ValidateAndApplyGameweekTeamSelection(SeasonState seasonState, Team selectedTeam)
        {
            if(seasonState == null)throw new ArgumentNullException("seasonState");
            if(selectedTeam == null)throw new ArgumentNullException("selectedTeam");

            if (selectedTeam.Validity != TeamValidationStatus.Valid)
            {
                throw new InvalidTeamException(selectedTeam.Validity);
            }

            //team now valid so update
            seasonState.CurrentTeam = selectedTeam;

            return seasonState;
        }

        public SeasonState ValidateAndApplyStartingTeam(Team startingTeam, IList<Player> allPlayers)
        {
            if(startingTeam == null)throw new ArgumentNullException("startingTeam");
            if (allPlayers == null) throw new ArgumentNullException("allPlayers");

            if (startingTeam.Validity != TeamValidationStatus.Valid)
            {
                throw new InvalidTeamException(startingTeam.Validity);
            }

            var teamCost = startingTeam.Players.Sum(x => x.NowCost);
            if(teamCost > GameConstants.StartingMoney)
            {
                throw new InvalidTeamException(TeamValidationStatus.StartingTeamTooExpensive);
            }

            return new SeasonState
            {
                AllPlayers = allPlayers,
                CurrentTeam = startingTeam,
                FreeTransfers = GameConstants.StartingFreeTransfers,
                Money = GameConstants.StartingMoney - teamCost,
            };
        }

        
    }
}
