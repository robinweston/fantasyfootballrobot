using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Simulators
{
    public class GameweekSimulator : IGameweekSimulator
    {
        readonly ILogger _logger;

        public GameweekSimulator(ILogger logger)
        {
            _logger = logger;
        }

        public IList<PlayerGameweekPerformance> CalculatePlayerPerformances(Team team, int gameweek, IList<Player> allUpToDatePlayers)
        {
            if(team == null)throw new ArgumentNullException("team");
            if (allUpToDatePlayers == null) throw new ArgumentNullException("allUpToDatePlayers");
            if(team.Validity != TeamValidationStatus.Valid)throw new ArgumentException("team.Validity = " + team.Validity);
            if (gameweek < 1 || gameweek > 38) throw new ArgumentException("gameweek");

            _logger.Log(Tag.Result, string.Concat("Calculating team performance for gameweek ", gameweek));

            var playerIds = team.Players.Select(x => x.Id);

            var playerPerformances = RetrievePlayerPerformances(playerIds, gameweek, allUpToDatePlayers).ToList();

            var scoringPerformances = SelectScoringPerformances(playerPerformances);

            AddCaptainPoints(team, scoringPerformances);

            LogPerformances(scoringPerformances, gameweek);

            return scoringPerformances;
        }

        void LogPerformances(IList<PlayerGameweekPerformance> scoringPerformances, int gameweek)
        {
            _logger.Log(Tag.Result, string.Concat("Gameweek ", gameweek, " points "), true);
            
            var formation = LogHelper.GetFormation(scoringPerformances);
            _logger.Log(Tag.Result, string.Concat("Formation: ", formation), true);
            
            foreach(var performance in scoringPerformances)
            {
                _logger.Log(Tag.Result, string.Format("{0}{1} - {2} mins - {3} pt(s)", performance.PlayerName, performance.IsCaptain ? " (c)" : string.Empty, performance.MinutesPlayed, performance.TotalPointsScored), true);
            }

        }

        private void AddCaptainPoints(Team team, IList<PlayerGameweekPerformance> playerPerformances)
        {
            if (!TryAddCaptainPoints(playerPerformances, team.Captain))
            {                
                if(TryAddCaptainPoints(playerPerformances, team.ViceCaptain))
                {
                    _logger.Log(Tag.Result, string.Concat(team.ViceCaptain.Name, " replaces ", team.Captain.Name, " as captain"), true);
                } else
                {
                    _logger.Log(Tag.Result, string.Concat("Both ", team.Captain.Name, " and ", team.ViceCaptain.Name, " unavailable so no captain selected"), true);
                }
            }
        }

        private static bool TryAddCaptainPoints(IEnumerable<PlayerGameweekPerformance> playerPerformances, Player captain)
        {
            var captainPerformance =
                playerPerformances.SingleOrDefault(x => x.PlayerId == captain.Id);
            if (captainPerformance != null && captainPerformance.MinutesPlayed > 0)
            {
                captainPerformance.IsCaptain = true;
                captainPerformance.TotalPointsScored *= 2;
                return true;
            }
            return false;
        }

        private static IEnumerable<PlayerGameweekPerformance> RetrievePlayerPerformances(IEnumerable<int> playerIds, int gameweek, IList<Player> upToDatePlayers)
        {
            foreach (var playerId in playerIds)
            {
                int id = playerId;
                var upToDatePlayer = upToDatePlayers.Single(x => x.Id == id);
                var gameweekFixtures =
                    upToDatePlayer.PastFixtures.Where(x => x.GameWeek == gameweek);
                yield return PerformanceMappingHelper.CreatePlayerPerformanceFromFixtures(upToDatePlayer.Id, upToDatePlayer.Name, upToDatePlayer.Position, gameweekFixtures);
            }
        }

        private IList<PlayerGameweekPerformance> SelectScoringPerformances(IList<PlayerGameweekPerformance> playerGameweekPerformances)
        {
            //start with the first 11 and replace players where we can if they don't play this gameweek
            var scoringPerformances = playerGameweekPerformances.Take(11).ToList();
            var subs = playerGameweekPerformances.Skip(11).ToList();

            //iterate through players and replace where neccessary)
            for (var i = 0; i < 11; i++)
            {
                var performance = scoringPerformances.ElementAt(i);
                if(performance.MinutesPlayed == 0)
                {                   
                    var sub = FindSubPerformance(scoringPerformances, subs, performance.Position);                
                    if(sub != null)
                    {
                        scoringPerformances.RemoveAt(i);
                        scoringPerformances.Insert(i, sub);
                        subs.Remove(sub);

                        _logger.Log(Tag.Result, string.Concat(sub.PlayerName, " subbed in for ", performance.PlayerName), true);  
                    } else
                    {
                        _logger.Log(Tag.Result, string.Concat("No sub found for ", performance.PlayerName), true);  
                    }
                }
            }        

            return scoringPerformances;
        }

        private static PlayerGameweekPerformance FindSubPerformance(IList<PlayerGameweekPerformance> currentScoringPerformances, IEnumerable<PlayerGameweekPerformance> potentialSubs, Position positionBeingReplaced)
        {
            foreach(var potentialSub in potentialSubs.Where(x => x.MinutesPlayed > 0))
            {
                var currentPositions = currentScoringPerformances.Select(x => x.Position).ToList();
                currentPositions.Remove(positionBeingReplaced);
                currentPositions.Add(potentialSub.Position);
                if(TeamValidator.TeamFormationValid(currentPositions))
                {
                    return potentialSub;
                }
            }

            return null;
        }     
    }
}