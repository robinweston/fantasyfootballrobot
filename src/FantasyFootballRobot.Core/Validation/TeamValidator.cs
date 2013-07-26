using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Constants;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Validation
{
    public static class TeamValidator
    {
        public static TeamValidationStatus ValidateTeam(Team team)
        {
            const int playerInTeam = 15;

            if (team.Players.Count < playerInTeam)
            {
                return TeamValidationStatus.TooFewPlayers;
            }

            if (team.Players.Count > playerInTeam)
            {
                return TeamValidationStatus.TooManyPlayers;
            }

            if (TeamContainsDuplicates(team))
            {
                return TeamValidationStatus.DuplicatePlayers;
            }

            if (!IsCaptainValid(team))
            {
                return TeamValidationStatus.InvalidCaptain;
            }

            if (!IsViceCaptainValid(team))
            {
                return TeamValidationStatus.InvalidViceCaptain;
            }

            if (!SquadDistributionValid(team))
            {
                return TeamValidationStatus.InvalidPlayerDistribution;
            }

            if (!TeamFormationValid(team.Players.Select(x => x.Position)))
            {
                return TeamValidationStatus.InvalidFormation;
            }

            if (TeamContainsTooManyPlayersFromOneClub(team))
            {
                return TeamValidationStatus.TooManyPlayersFromOneClub;
            }

            if (TeamContainsTooManyPlayersFromOneClubInSamePosition(team))
            {
                return TeamValidationStatus.TooManyPlayersFromOneClubInSamePosition;
            }

            return TeamValidationStatus.Valid;
        }

        private static bool TeamContainsTooManyPlayersFromOneClubInSamePosition(Team team)
        {
            var playersGroupedByPosition = team.Players.GroupBy(p => p.Position);
            return playersGroupedByPosition.Any(@group => @group.GroupBy(p => p.ClubCode).Count() < @group.Count());
        }

        private static bool TeamContainsTooManyPlayersFromOneClub(Team team)
        {
            return team.Players.GroupBy(i => i.ClubCode)
                .Any(g => g.Count() > 3);
        }

        public static bool TeamFormationValid(IEnumerable<Position> positions)
        {
            var startingPositions = positions.Take(11).ToList();
            return startingPositions.Count(x => x == Position.Goalkeeper) == 1
                   && startingPositions.Count(x => x == Position.Defender) >= GameConstants.MinimumDefendersInStartingTeam
                   && startingPositions.Count(x => x == Position.Forward) >= GameConstants.MinimumForwardsInStartingTeam;
        }

        private static bool SquadDistributionValid(Team team)
        {
            return team.Players.Count(x => x.Position == Position.Goalkeeper) == 2
                   && team.Players.Count(x => x.Position == Position.Defender) == 5
                   && team.Players.Count(x => x.Position == Position.Midfielder) == 5
                   && team.Players.Count(x => x.Position == Position.Forward) == 3;
        }

        private static bool TeamContainsDuplicates(Team team)
        {
            return team.Players.GroupBy(i => i.Id)
                .Any(g => g.Count() > 1);
        }

        private static bool IsCaptainValid(Team team)
        {
            return PlayerIsValidCaptainChoice(team, team.Captain) &&
                   team.Players.Any(x => x.Id == team.Captain.Id);
        }

        private static bool IsViceCaptainValid(Team team)
        {
            return PlayerIsValidCaptainChoice(team, team.ViceCaptain) &&
                   team.ViceCaptain.Id != team.Captain.Id;
        }

        private static bool PlayerIsValidCaptainChoice(Team team, Player player)
        {
            return player != null &&
                   team.Players.Any(x => x.Id == player.Id);
        }
    }
}