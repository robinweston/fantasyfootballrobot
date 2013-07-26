using System;
using System.Collections.Generic;
using System.Globalization;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Core.Strategies;
using System.Linq;

namespace FantasyFootballRobot.Core.Simulators
{
    public class SeasonSimulator : ISeasonSimulator
    {
        private readonly IMultiplePlayersService _playerService;
        private readonly ITimeAdjustor _timeAdjustor;
        private readonly IGameweekSimulator _gameweekSimulator;
        private readonly IDecisionActioner _decisionActioner;
        private readonly ILogger _logger;

        public SeasonSimulator(IMultiplePlayersService playerService, ITimeAdjustor timeAdjustor, IGameweekSimulator gameweekSimulator, IDecisionActioner decisionActioner, ILogger logger)
        {
            _playerService = playerService;
            _logger = logger;
            _timeAdjustor = timeAdjustor;
            _gameweekSimulator = gameweekSimulator;
            _decisionActioner = decisionActioner;
        }

        private IList<Player> GetPlayersForFirstGameweek()
        {
            _logger.Log(Tag.Simulation, "Retrieving players from service");

            var allUpToDatePlayers = _playerService.GetAllPlayers();

            _logger.Log(Tag.Simulation, string.Concat("Retrieved ", allUpToDatePlayers.Count(), " players from service"));
            foreach (Position position in Enum.GetValues(typeof(Position)))
            {
                Position position1 = position;
                _logger.Log(Tag.Simulation, string.Concat(allUpToDatePlayers.Count(p => p.Position == position1), " with position ", position1));
            }

            _logger.Log(Tag.Simulation, string.Concat("Retrieved ", allUpToDatePlayers.Count(), " players from service"));

            return _timeAdjustor.AdjustPlayersToGameweek(allUpToDatePlayers, 1);
        }

        public SeasonSimulationResult PerformSimulation(IStrategy strategy, SeasonSimulationOptions options)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");

            var allPlayers = GetPlayersForFirstGameweek();

            var maxGameweek = CalculateMaxGameweek(options.MaximumGameweek);

            _logger.Log(Tag.Simulation, string.Concat("Max gameweek is ", maxGameweek));

            var startingTeam = options.UseSavedInitialTeam ? SelectInitialTeamByIds(allPlayers, options.InitialTeamPlayerIds) : strategy.PickStartingTeam(allPlayers);

            _logger.Log(Tag.Simulation, "Starting team picked");
            LogHelper.LogTeam(startingTeam, _logger);

            if (options.ChooseInitialTeamOnly)
            {
                return new SeasonSimulationResult();
            }

            var seasonState = _decisionActioner.ValidateAndApplyStartingTeam(startingTeam, allPlayers);

            _logger.Log(Tag.Simulation, "Applied starting team");

            return SimulateSeason(seasonState, strategy, maxGameweek);
        }

        private Team SelectInitialTeamByIds(IEnumerable<Player> allPlayers, IEnumerable<int> playerIds)
        {
            _logger.Log(Tag.Simulation, "Using previously selected initial team");
            var team = new Team { Players = playerIds.Select(pid => allPlayers.Single(p => p.Id == pid)).ToList() };
            team.Captain = team.Players[0];
            team.ViceCaptain = team.Players[1];
            return team;
        }

        private SeasonSimulationResult SimulateSeason(SeasonState startingSeasonState, IStrategy strategy, int maxGameweek)
        {
            var seasonState = startingSeasonState;

            var result = new SeasonSimulationResult();
            for (int gameweek = 1; gameweek <= maxGameweek; gameweek++)
            {
                _logger.Log(Tag.Simulation, string.Concat("Simulating gameweek ", gameweek), true);
                _logger.Log(Tag.Simulation, string.Concat("Current points total: ", result.TotalPointsScored), true);
                _logger.Log(Tag.Simulation, string.Concat("Available money: ", seasonState.Money.ToMoney()), true);
                _logger.Log(Tag.Simulation, string.Concat("Free transfers: ", seasonState.FreeTransfers), true);

                seasonState = UpdateSeasonStateToGameweek(seasonState, gameweek);

                var strategyDecisionResult = ProcessStrategyDecisions(seasonState, strategy);

                seasonState = strategyDecisionResult.UpdatedSeasonState;

                var playerPerformances = CalculatePlayerGameweekPerformances(seasonState);

                var gameweekPerformance = new TeamGameweekPerformance
                                          {
                                              PlayerPerformances = playerPerformances,
                                              TransferActions = strategyDecisionResult.TransfersMade,
                                              TransferResults = strategyDecisionResult.TransferResults,
                                              Gameweek = gameweek
                                          };

                result.GameweekResults.Add(gameweekPerformance);

                _logger.Log(Tag.Result,
                            string.Concat("Total player points scored in gameweek ", gameweek, ": ",
                                            gameweekPerformance.TotalPlayerPointsScored), true);
                _logger.Log(Tag.Result,
                           string.Concat("Transfer penalty points in gameweek ", gameweek, ": ",
                                           gameweekPerformance.TransferResults.TransferPointsPenalty), true);

                _logger.Log(Tag.Result,
                           string.Concat("Total points for gameweek ", gameweek, ": ",
                                           gameweekPerformance.TotalPointsScored), true);
            }

            OutputResult(result);

            return result;
        }

        private void OutputResult(SeasonSimulationResult result)
        {
            _logger.Log(Tag.Result, "Outputting simulation results", true);
            _logger.Log(Tag.Result, string.Concat("Total points: ", result.TotalPointsScored), true);
            _logger.Log(Tag.Result, string.Concat("Total transfers made: ", result.TotalTransfersMade), true);
            _logger.Log(Tag.Result,
                       string.Concat("Total penalised transfers made: ", result.TotalPenalisedTransfersMade), true);

            var standardWildcardGameweek =
                result.GameweekResults.SingleOrDefault(x => x.TransferActions.PlayStandardWildcard);
            _logger.Log(Tag.Result,
                string.Format("Standard wildcard played : {0}", standardWildcardGameweek == null ? "Never!" : standardWildcardGameweek.Gameweek.ToString(CultureInfo.InvariantCulture)), true);

            var transferWindowWildcardGameweek =
                result.GameweekResults.SingleOrDefault(x => x.TransferActions.PlayTransferWindowWildcard);
            _logger.Log(Tag.Result,
                string.Format("Transfer window wildcard played : {0}", transferWindowWildcardGameweek == null ? "Never!" : transferWindowWildcardGameweek.Gameweek.ToString(CultureInfo.InvariantCulture)), true);

        }

        private StrategyDecisionsResult ProcessStrategyDecisions(SeasonState seasonState, IStrategy strategy)
        {
            var updatedSeasonState = seasonState;

            var transferActions = new TransferActions();
            var transferResults = new TransferActionsResult();

            //no transfers in first gameweek
            if (seasonState.Gameweek > 1)
            {
                transferActions = strategy.MakeTransfers(updatedSeasonState);
                transferResults = _decisionActioner.ValidateAndApplyTransfers(updatedSeasonState, transferActions);
                updatedSeasonState = transferResults.UpdatedSeasonState;
            }

            var selectedTeamForGameweek = strategy.PickGameweekTeam(updatedSeasonState);
            updatedSeasonState = _decisionActioner.ValidateAndApplyGameweekTeamSelection(updatedSeasonState, selectedTeamForGameweek);

            return new StrategyDecisionsResult
                   {
                       TransfersMade = transferActions,
                       TransferResults = transferResults,
                       UpdatedSeasonState = updatedSeasonState
                   };
        }

        private int CalculateMaxGameweek(int? manualMaxGameweek)
        {
            var allPlayers = _playerService.GetAllPlayers();

            //get the most recent gameweek we have results for
            var maxGameweekFromFixtures = allPlayers.SelectMany(x => x.PastFixtures).Max(x => x.GameWeek);

            if (manualMaxGameweek.HasValue)
            {
                return Math.Min(maxGameweekFromFixtures, manualMaxGameweek.Value);
            }

            return maxGameweekFromFixtures;
        }

        private IList<PlayerGameweekPerformance> CalculatePlayerGameweekPerformances(SeasonState seasonState)
        {
            var allPlayers = _playerService.GetAllPlayers();
            return _gameweekSimulator.CalculatePlayerPerformances(seasonState.CurrentTeam, seasonState.Gameweek, allPlayers);
        }

        private SeasonState UpdateSeasonStateToGameweek(SeasonState seasonState, int gameweek)
        {
            seasonState.Gameweek = gameweek;

            var allUpToDatePlayers = _playerService.GetAllPlayers();

            seasonState.CurrentTeam = _timeAdjustor.AdjustTeamToGameweek(seasonState.CurrentTeam, allUpToDatePlayers,
                                                                                seasonState.Gameweek);
            seasonState.AllPlayers = _timeAdjustor.AdjustPlayersToGameweek(allUpToDatePlayers, seasonState.Gameweek);

            return seasonState;
        }

    }
}