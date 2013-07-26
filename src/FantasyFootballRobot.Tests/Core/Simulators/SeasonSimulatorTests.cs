using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Simulators
{
    [TestFixture]
    public class SeasonSimulatorTests
    {
        [SetUp]
        public void SetUp()
        {
            
            _allPlayers = TeamCreationHelper.CreatePlayersWithPastFixtures(1);
           
            _playerServiceMock = new Mock<IMultiplePlayersService>();
            _playerServiceMock.Setup(x => x.GetAllPlayers()).Returns(_allPlayers);

            _startingTeam = TeamCreationHelper.CreateTestTeam();
            var startingSeasonState = new SeasonState { CurrentTeam = _startingTeam };

            _strategyMock = new Mock<IStrategy>();
            _strategyMock.Setup(x => x.MakeTransfers(It.IsAny<SeasonState>())).Returns((SeasonState seasonState) => new TransferActions());
            _strategyMock.Setup(x => x.PickStartingTeam(It.IsAny<IList<Player>>())).Returns(_startingTeam);
            _strategyMock.Setup(x => x.PickGameweekTeam(It.IsAny<SeasonState>())).Returns(_startingTeam);

            _timeAdjustorMock = new Mock<ITimeAdjustor>();
            _timeAdjustorMock.Setup(x => x.AdjustPlayersToGameweek(It.IsAny<IList<Player>>(), It.IsAny<int>())).Returns(
                (IList<Player> players, int gameweek) => players);
            _timeAdjustorMock.Setup(x => x.AdjustTeamToGameweek(It.IsAny<Team>(), It.IsAny<IList<Player>>(), It.IsAny<int>())).Returns(
                (Team team, IList<Player> players, int gameweek) => team);

            _gameweekSimulatorMock = new Mock<IGameweekSimulator>();

            _gameweekSimulatorMock.Setup(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<IList<Player>>())).
                Returns(new List<PlayerGameweekPerformance>());

            _decisionActionerMock = new Mock<IDecisionActioner>();

            _simulationOptions = new SeasonSimulationOptions();

            _decisionActionerMock.Setup(
                x => x.ValidateAndApplyTransfers(It.IsAny<SeasonState>(), It.IsAny<TransferActions>())).Returns((SeasonState s, TransferActions a) => new TransferActionsResult{UpdatedSeasonState = s});
            _decisionActionerMock.Setup(x => x.ValidateAndApplyStartingTeam(It.IsAny<Team>(), It.IsAny<IList<Player>>())).Returns(startingSeasonState);
            _decisionActionerMock.Setup(x => x.ValidateAndApplyGameweekTeamSelection(It.IsAny<SeasonState>(), It.IsAny<Team>())).Returns(startingSeasonState);

            _seasonSimulator = new SeasonSimulator(_playerServiceMock.Object, _timeAdjustorMock.Object,
                                                   _gameweekSimulatorMock.Object, _decisionActionerMock.Object, new Mock<ILogger>().Object);
        }

        private IList<Player> _allPlayers;

        private ISeasonSimulator _seasonSimulator;
        private Mock<IStrategy> _strategyMock;
        private Mock<IMultiplePlayersService> _playerServiceMock;
        private Mock<ITimeAdjustor> _timeAdjustorMock;
        private Mock<IGameweekSimulator> _gameweekSimulatorMock;
        private Mock<IDecisionActioner> _decisionActionerMock;
        Team _startingTeam;
        private SeasonSimulationOptions _simulationOptions;

        [Test]
        public void for_first_gameweek_performance_is_calculated()
        {
            //Arrange
            var timeAdjustedPlayers = new List<Player>();
            const int gameweek = 1;

            _timeAdjustorMock.Setup(x => x.AdjustPlayersToGameweek(_allPlayers, gameweek)).Returns(timeAdjustedPlayers);
            _gameweekSimulatorMock.Setup(x => x.CalculatePlayerPerformances(It.IsAny<Team>(), gameweek, _allPlayers)).Returns(
                new List<PlayerGameweekPerformance>());

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _gameweekSimulatorMock.Verify(x => x.CalculatePlayerPerformances(It.IsAny<Team>(), gameweek, _allPlayers));
        }

        [Test]
        public void for_first_gameweek_player_list_contains_all_players()
        {
            //Arrange

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _playerServiceMock.VerifyAll();
        }

        [Test]
        public void for_first_gameweek_player_list_is_adjusted_to_first_gameweek()
        {
            //Arrange
            const int gameweek = 1;

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _timeAdjustorMock.Verify(x => x.AdjustPlayersToGameweek(_allPlayers, gameweek));
        }

        [Test]
        public void for_first_gameweek_team_is_adjusted_to_first_gameweek()
        {
            //Arrange

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _timeAdjustorMock.Verify(x => x.AdjustTeamToGameweek(_startingTeam, _allPlayers, 1));
        }

        [Test]
        public void simulation_runs_until_the_most_recent_gameweek()
        {
            //Arrange
            const int maxGameweek = 5;

            var timeAdjustedPlayers = new List<Player>();

            _gameweekSimulatorMock.Setup(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), It.Is<int>(i => i <= maxGameweek), _allPlayers)).Returns(new List<PlayerGameweekPerformance>());

            _timeAdjustorMock.Setup(x => x.AdjustPlayersToGameweek(_allPlayers, It.Is<int>(i => i <= maxGameweek))).
                Returns(timeAdjustedPlayers);

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _timeAdjustorMock.Verify(
                x => x.AdjustPlayersToGameweek(It.IsAny<IList<Player>>(), It.Is<int>(i => i > maxGameweek)),
                Times.Never());

            _gameweekSimulatorMock.Verify(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), It.Is<int>(i => i > maxGameweek), It.IsAny<IList<Player>>()), Times.Never());
        }

        [Test]
        public void strategy_not_asked_to_make_transfer_decision_for_first_gameweek()
        {
            //Arrange
            var decision = new TransferActions();
            _strategyMock.Setup(x => x.MakeTransfers(It.Is<SeasonState>(s => s.Gameweek == 1))).Returns(decision);

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _strategyMock.Verify(x => x.MakeTransfers(It.Is<SeasonState>(s => s.Gameweek == 1)), Times.Never());
        }


        [Test]
        public void season_result_contains_gameweek_results()
        {
            //Arrange
            var players = TeamCreationHelper.CreatePlayersWithPastFixtures(10);
            _playerServiceMock.Setup(x => x.GetAllPlayers()).Returns(players);

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults.Count, Is.EqualTo(10));
        }

        [Test]
        public void season_result_contains_correct_number_of_gameweek_results()
        {
            //Arrange
            var players = TeamCreationHelper.CreatePlayersWithPastFixtures(10);
            _playerServiceMock.Setup(x => x.GetAllPlayers()).Returns(players);

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults.Count, Is.EqualTo(10));
        }

        [Test]
        public void gameweek_results_have_correct_points_totals_if_no_transfer_penalties()
        {
            //Arrange
            _gameweekSimulatorMock.Setup(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<IList<Player>>())).
                Returns(CreatePlayerGameweekPerformances(5));

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults.First().TotalPointsScored, Is.EqualTo(5 * 11));
        }

        [Test]
        public void gameweek_results_have_correct_points_totals_if_transfer_penalties_imposed()
        {
            //Arrange

            //we need 2 gameweeks as no transfers in first gameweek
            var players = TeamCreationHelper.CreatePlayersWithPastFixtures(2);
            _playerServiceMock.Setup(x => x.GetAllPlayers()).Returns(players);

            _decisionActionerMock.Setup(
                x => x.ValidateAndApplyTransfers(It.Is<SeasonState>(ss => ss.Gameweek == 2), It.IsAny<TransferActions>())).Returns((SeasonState seasonState, TransferActions actions) =>
                    new TransferActionsResult { PenalisedTransfersMade = 1, UpdatedSeasonState = seasonState });

            _gameweekSimulatorMock.Setup(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), It.IsAny<int>(), It.IsAny<IList<Player>>())).
                Returns(CreatePlayerGameweekPerformances(5));

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults[1].TotalPointsScored, Is.EqualTo((5 * 11) - 4));
        }

        [Test]
        public void gameweek_performance_is_calculated_based_on_updated_season_state_from_team_selection()
        {
            //Arrange

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _gameweekSimulatorMock.Verify(x => x.CalculatePlayerPerformances(It.IsAny<Team>(), 1, It.IsAny<IList<Player>>()));
        }

        [Test]
        public void transfer_actions_and_results_are_included_in_gameweek_results()
        {
            //Arrange

            //set up 2 gameweeks as first has no transfers
            _allPlayers = TeamCreationHelper.CreatePlayersWithPastFixtures(2);
            _playerServiceMock.Setup(x => x.GetAllPlayers()).Returns(_allPlayers);

            var transfers = new TransferActions{Transfers = new List<Transfer>{new Transfer(), new Transfer()}};
            _strategyMock.Setup(x => x.MakeTransfers(It.IsAny<SeasonState>())).Returns(transfers);

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults[1].TransferActions, Is.EqualTo(transfers));
            Assert.That(result.GameweekResults[1].TransferResults, Is.Not.Null);
        }

        [Test]
        public void for_first_gameweek_starting_team_is_picked()
        {
            //Arrange
            var adustedPlayers = new List<Player>();
            _strategyMock.Setup(x => x.PickStartingTeam(adustedPlayers)).Returns(_startingTeam);
            _timeAdjustorMock.Setup(x => x.AdjustPlayersToGameweek(_allPlayers, 1)).Returns(adustedPlayers);

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _timeAdjustorMock.Verify(x => x.AdjustPlayersToGameweek(_allPlayers, 1));
            _strategyMock.Verify(x => x.PickStartingTeam(adustedPlayers));
            _decisionActionerMock.Verify(x => x.ValidateAndApplyStartingTeam(_startingTeam, adustedPlayers));
        }

        [Test]
        public void strategy_picks_team_for_gameweek()
        {
            //Arrange
            var adustedPlayers = new List<Player>();
            _strategyMock.Setup(x => x.PickStartingTeam(adustedPlayers)).Returns(_startingTeam);
            _timeAdjustorMock.Setup(x => x.AdjustPlayersToGameweek(_allPlayers, 1)).Returns(adustedPlayers);

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _strategyMock.Verify(x => x.PickGameweekTeam(It.Is<SeasonState>(ss => ss.CurrentTeam == _startingTeam)));
        }

        [Test]
        public void decision_actioner_processes_team_picks_each_gameweek()
        {
            //Arrange

            //Act
            _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            _decisionActionerMock.Verify(x => x.ValidateAndApplyGameweekTeamSelection(It.Is<SeasonState>(ss => ss.Gameweek == 1), It.IsAny<Team>() ));
        }


        [Test]
        public void total_gameweek_points_are_added_up_correctly()
        {
            //Arrange
            const int gameweek = 1;

            //remember that gk is captain 
            var playerPerformances = new List<PlayerGameweekPerformance>
                                     {
                                         new PlayerGameweekPerformance {TotalPointsScored = 1},
                                         new PlayerGameweekPerformance {TotalPointsScored = 5},
                                         new PlayerGameweekPerformance {TotalPointsScored = 6},
                                         new PlayerGameweekPerformance {TotalPointsScored = -4}
                                     };

            _gameweekSimulatorMock.Setup(
                x => x.CalculatePlayerPerformances(It.IsAny<Team>(), gameweek, It.IsAny<IList<Player>>())).Returns(
                    playerPerformances);

            //Act
            var result = _seasonSimulator.PerformSimulation(_strategyMock.Object, _simulationOptions);

            //Assert
            Assert.That(result.GameweekResults.First().TotalPointsScored, Is.EqualTo(1 + 5 + 6 -4));
        }

        private static IList<PlayerGameweekPerformance> CreatePlayerGameweekPerformances(int pointsScoredPerPlayer)
        {
            var performances = new List<PlayerGameweekPerformance>();

            for(int i=0;i< 11;i++)
            {
                var performance = new PlayerGameweekPerformance {TotalPointsScored = pointsScoredPerPlayer};
                performances.Add(performance);
            }

            return performances;
        }
        

    }
}