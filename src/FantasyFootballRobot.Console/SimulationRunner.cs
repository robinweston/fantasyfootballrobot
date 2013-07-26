using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Basic;
using FantasyFootballRobot.Core.Strategies.Complex;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using Microsoft.Practices.Unity;

namespace FantasyFootballRobot.Console
{
    internal static class SimulationRunner
    {
        private static IList<Player> GetAllPlayers(IUnityContainer container, int gameweek)
        {

            var playerService = container.Resolve<IMultiplePlayersService>();
            var timeAdjustor = container.Resolve<ITimeAdjustor>();

            var allPlayers = playerService.GetAllPlayers();

            return timeAdjustor.AdjustPlayersToGameweek(allPlayers, gameweek);
        }

        private static void CalculateTeamStrengths(IUnityContainer container, ILogger logger)
        {
            logger.Log(Tag.Progress, "Calculating team strengths");
            var teamStrengthCalculator = container.Resolve<ITeamStrengthCalculator>();

            var allPlayers = GetAllPlayers(container, 1);

            var teamStrengths = new Dictionary<Club, TeamStrength>();
            foreach (var club in Club.AllClubs)
            {
                var teamStrength = teamStrengthCalculator.CalculateTeamStrength(club.Code, allPlayers);
                teamStrengths[club] = teamStrength;
            }

            logger.Log(Tag.Progress, "Team strengths below in descending order");

            var ranking = 1;
            foreach (var strength in teamStrengths.OrderByDescending(kvp => kvp.Value.TeamStrengthMultiplier))
            {
                logger.Log(Tag.Progress, string.Format("{0} - {1} - Relative Points/Min: {2}. Sample Players: {3}. Total Minutes {4}. Points/Min {5}", ranking, strength.Key.Name, strength.Value.TeamStrengthMultiplier.ToFormatted(), strength.Value.SamplePlayers, strength.Value.TotalMinutes.ToFormatted(), strength.Value.PointsPerMinute.ToFormatted()));

                ranking++;
            }
        }

        public static void RunSimulations(SeasonSimulationOptions simulationOptions, IUnityContainer container, ILogger logger)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!string.IsNullOrWhiteSpace(simulationOptions.StrategyName))
            {
                RegisterStrategyByName(container, simulationOptions.StrategyName);
            }

            if(simulationOptions.ListUsage)
            {
                ListUsage();
            }
            else if (simulationOptions.CalculateTeamStrengthOnly)
            {
                CalculateTeamStrengths(container, logger);
            }
            else if (simulationOptions.CalculateHomeAdvantageRatioOnly)
            {
                CalculateHomeAdvantageRatio(container, logger);
            }
            else if (simulationOptions.CalculatePlayerFormOnly)
            {
                CalculatePlayerForm(container, logger);
            }
            else
            {
                RunSeasonSimulation(container, logger, simulationOptions);
            }

            stopwatch.Stop();
            logger.Log(Tag.Progress, string.Format("Simulation time:  {0}", stopwatch.Elapsed.ToFormatted()));
        }

        private static void RegisterStrategyByName(IUnityContainer container, string strategyName)
        {
            // todo: must be a better way to do this
            switch (strategyName.ToLower())
            {
                case "basic":
                    container.RegisterType<IStrategy, BasicStrategy>();
                    return;
                case "complex":
                    container.RegisterType<IStrategy, ComplexStrategy>();
                    return;
            }

            throw new Exception("Could not find strategy with name " + strategyName);
        }

        private static void CalculateHomeAdvantageRatio(IUnityContainer container, ILogger logger)
        {
            logger.Log(Tag.Progress, "Calculating home advantage points ratio");

            var players = GetAllPlayers(container, 38);
            var locationAdvantageCalculator = container.Resolve<ILocationAdvantageCalculator>();
            var locationAdvantage = locationAdvantageCalculator.CalculateLocationAdvantage(players);

            logger.Log(Tag.Progress, string.Format("Average points per minute: {0}", locationAdvantage.AveragePointsPerMinute.ToFormatted()));
            logger.Log(Tag.Progress, string.Format("Average home points per minute: {0}", locationAdvantage.HomeMatchPointsPerMinute.ToFormatted()));
            logger.Log(Tag.Progress, string.Format("Average away points per minute: {0}", locationAdvantage.AwayMatchPointsPerMinute.ToFormatted()));
            logger.Log(Tag.Progress, string.Format("Home multipler: {0}", locationAdvantage.HomeMatchMultiplier.ToFormatted()));
            logger.Log(Tag.Progress, string.Format("Away multipler: {0}", locationAdvantage.AwayMatchMultiplier.ToFormatted()));

        }

        private static void ListUsage()
        {
            System.Console.WriteLine("Listing usage");
            System.Console.WriteLine("(No parameters) Run full season simulation");
            System.Console.WriteLine("-" + ConsoleOptions.ListUsage + " List Usage");
            System.Console.WriteLine("-" + ConsoleOptions.CalculateHomeAdvantageOnly + " Calculate Home Advantage Ratio Only");
            System.Console.WriteLine("-" + ConsoleOptions.CalculatePlayerFormOnly + " Calculate Player Form Only");
            System.Console.WriteLine("-" + ConsoleOptions.CalculateTeamStrengthsOnly + " Calculate Team Strengths Only");
            System.Console.WriteLine("-" + ConsoleOptions.ChooseInitialTeamOnly + " Choose Initial Team Only");
            System.Console.WriteLine("-" + ConsoleOptions.UseSavedInitialTeam + " Use Saved Initial Team");
            System.Console.WriteLine("-" + ConsoleOptions.MaxGameweek + " Max Gameweek");
            System.Console.WriteLine("-" + ConsoleOptions.StrategyName + " Strategy Name");
        }

        private static void RunSeasonSimulation(IUnityContainer container, ILogger logger, SeasonSimulationOptions simulationOptions)
        {
            logger.Log(Tag.Progress, "Starting season simulation");
            var strategy = container.Resolve<IStrategy>();
            var simulator = container.Resolve<ISeasonSimulator>();
            var result = simulator.PerformSimulation(strategy, simulationOptions);

            logger.Log(Tag.Progress, "Ending season simulation");

            if (!simulationOptions.ChooseInitialTeamOnly)
            {
                logger.Log(Tag.Progress, string.Format("Total points: {0}", result.TotalPointsScored));
            }
        }

        private static void CalculatePlayerForm(IUnityContainer container, ILogger logger)
        {
            logger.Log(Tag.Progress, "Calculating player form (Points per game)");

            var playerFormCalculator = container.Resolve<IPlayerFormCalculator>();
            var playerPoolReducer = container.Resolve<IPlayerPoolReducer>();
            var allPlayers = GetAllPlayers(container, 1);

            var playerForms = (from player in allPlayers
                               let playerForm = playerFormCalculator.CalculateCurrentPlayerForm(player, allPlayers)
                               select new Tuple<Player, PlayerForm>(player, playerForm)).ToList();

            var reducedPlayers = playerPoolReducer.ReducePlayerPool(allPlayers);

            logger.Log(Tag.Prediction, string.Format("Displaying {0} players considered for initial team selection", reducedPlayers.Count));

            foreach (var playerForm in playerForms.Where(pf => reducedPlayers.Contains(pf.Item1)).OrderByDescending(kvp => kvp.Item2.NormalisedPointsPerGame))
            {
                LogPlayerForm(playerForm.Item1, playerForm.Item2, logger);
            }

            var ignoredPlayers = allPlayers.Except(reducedPlayers).ToList();

            logger.Log(Tag.Prediction, string.Format("Displaying {0} players NOT considered for initial team selection", ignoredPlayers.Count));
            foreach (var playerForm in playerForms.Where(pf => ignoredPlayers.Contains(pf.Item1)).OrderByDescending(kvp => kvp.Item2.NormalisedPointsPerGame))
            {
                LogPlayerForm(playerForm.Item1, playerForm.Item2, logger);
            }
        }

        private static void LogPlayerForm(Player player, PlayerForm playerForm, ILogger logger)
        {
            logger.Log(Tag.Prediction,
                           string.Format("{0} {1} - Previous Season: {2} mins, {3} pts",
                           playerForm.NormalisedPointsPerGame.ToFormatted(), player.Name, playerForm.PreviousSeasonMinutesPlayed, playerForm.PreviousSeasonTotalPointsScored));
        }
    }
}
