namespace FantasyFootballRobot.Core.Validation
{
    public enum TeamValidationStatus
    {
        Valid,
        TooFewPlayers,
        TooManyPlayers,
        DuplicatePlayers,
        InvalidCaptain,
        InvalidViceCaptain,
        InvalidPlayerDistribution,
        InvalidFormation,
        TooManyPlayersFromOneClub,
        StartingTeamTooExpensive,
        TooManyPlayersFromOneClubInSamePosition
    }
}
