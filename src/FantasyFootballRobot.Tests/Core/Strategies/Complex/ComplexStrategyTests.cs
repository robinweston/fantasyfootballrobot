using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex
{
    [TestFixture]
    class ComplexStrategyTests
    {
        private IStrategy _complexStrategy;
        private Mock<IInitialTeamSelectorStrategy> _intitialTeamSelectorMock;
        private Mock<IPlayerScorePredictor> _playerScorePredictorMock;
        private Mock<ITeamGameweekSelector> _teamGameweekSelectorMock;
        private Mock<ITransferSelectorStrategy> _transferSelectorStrategyMock;
        private Team _team;
        private SeasonState _seasonState;
        private IList<Player> _allPlayers;
        private int _gameweek;

        [SetUp]
        public void SetUp()
        {
            _gameweek = 1;
            _team = TeamCreationHelper.CreateTestTeam();
            _allPlayers = new List<Player>();
            _seasonState = new SeasonState {CurrentTeam = _team, AllPlayers = _allPlayers, Gameweek = _gameweek};

            _intitialTeamSelectorMock = new Mock<IInitialTeamSelectorStrategy>();
            _playerScorePredictorMock = new Mock<IPlayerScorePredictor>();
            _teamGameweekSelectorMock = new Mock<ITeamGameweekSelector>();
            _transferSelectorStrategyMock = new Mock<ITransferSelectorStrategy>();

            _complexStrategy = new ComplexStrategy(new Mock<ILogger>().Object, _intitialTeamSelectorMock.Object, _playerScorePredictorMock.Object, _teamGameweekSelectorMock.Object, _transferSelectorStrategyMock.Object);
        }

        [Test]
        public void initial_team_selection_uses_genetic_algorithm()
        {
            // Arrange
            _intitialTeamSelectorMock.Setup(x => x.SelectTeam(_allPlayers)).Returns(_team);

            // Act
            var selectedTeam = _complexStrategy.PickStartingTeam(_allPlayers);

            // Assert
            Assert.That(selectedTeam, Is.EqualTo(_team));
        }

        [Test]
        public void team_gameweek_selection_calculates_team_gameweek_selection_predicts_scores_for_gameweek()
        {
            // Arrange
            var teamSelection = new TeamSelection { Team = new Team() };
            _teamGameweekSelectorMock.Setup(
                x => x.SelectStartingTeamForGameweek(It.IsAny<IList<PredictedPlayerScore>>())).Returns(teamSelection);

            // Act
            _complexStrategy.PickGameweekTeam(_seasonState);

            // Assert
            foreach(var player in _team.Players)
            {
                Player player1 = player;
                _playerScorePredictorMock.Verify(x => x.PredictPlayerGameweekPoints(player1, _gameweek, _allPlayers));
            }
        }

        [Test]
        public void team_gameweek_selection_uses_team_selector_to_select_team()
        {
            // Arrange
            var teamSelection = new TeamSelection{Team = new Team()};

            _playerScorePredictorMock.Setup(
                x => x.PredictPlayerGameweekPoints(It.IsAny<Player>(), It.IsAny<int>(), It.IsAny<IList<Player>>())).
                Returns((Player p, int gameweek, IList<Player> allPlayers) => p.Id);

            _teamGameweekSelectorMock.Setup(
                x => x.SelectStartingTeamForGameweek(It.Is<IList<PredictedPlayerScore>>(ps => ps.All(p => (int)p.PredictedScore == p.Player.Id)))).Returns(
                    teamSelection);

            // Act
            var selectedTeam = _complexStrategy.PickGameweekTeam(_seasonState);

            // Assert
            Assert.That(teamSelection.Team, Is.EqualTo(selectedTeam));

        }
    }
}
