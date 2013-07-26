using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public class InitialTeamSelectorStrategy : IInitialTeamSelectorStrategy
    {
        readonly ILogger _logger;
        readonly IGeneticAlgorithm<FitTeam, IList<Player>> _geneticTeamSelector;

        public InitialTeamSelectorStrategy(ILogger logger, IGeneticAlgorithm<FitTeam, IList<Player>> geneticTeamSelector)
        {
            _logger = logger;
            _geneticTeamSelector = geneticTeamSelector;
        }

        public Team SelectTeam(IList<Player> allPlayers)
        {
            _geneticTeamSelector.SetSeedGenes(allPlayers);
            var finalTeams = _geneticTeamSelector.Run();

            var topTeams = finalTeams.Take(5).ToList();
            LogTopTeams(topTeams);

            return topTeams.First();
        }

        private void LogTopTeams(IList<FitTeam> teams)
        {
            for (var i = 0; i < teams.Count; i++)
            {
                _logger.Log(Tag.Strategy, string.Format("Top initial team {0}", i));

                var team = teams[i];
                LogHelper.LogTeam(team, _logger);

                _logger.Log(Tag.Strategy, string.Format("Fitness (predicted points per gameweek) : {0}", team.Fitness.GetValueOrDefault().ToFormatted()));
            }         
        }
    }
}
