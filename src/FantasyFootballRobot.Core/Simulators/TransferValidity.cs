namespace FantasyFootballRobot.Core.Simulators
{
    public enum TransferValidity
    {
        Valid,
        PlayerIsNull,
        PlayerTransferredInNotInTeam,
        PlayerTransferredOutInTeam,
        NotEnoughMoney,
        PlayerTransferredOutNotOriginallyInTeam,
        NoTransfersAllowedInFirstGameweek,
        PlayerTransferredInAlreadyInTeam,
        TransferedPlayersNotInSamePosition,
        PlayerTransferredInMultipleTimes,
        PlayerTransferredOutMultipleTimes,
        WildcardPlayedTwice,
        WildcardPlayedOutsideWindow,
        LeavesTeamInInvalidState
    }
}