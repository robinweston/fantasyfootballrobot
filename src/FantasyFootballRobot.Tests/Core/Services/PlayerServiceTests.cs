using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Services;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Services
{
    public class PlayerServiceTests
    {
        private Mock<ISinglePlayerService> _childPlayerServiceMock;
        private IMultiplePlayersService _service;

        [SetUp]
        public void SetUp()
        {
            _childPlayerServiceMock = new Mock<ISinglePlayerService>();
            _service = new PlayerService(_childPlayerServiceMock.Object);
        }

        [Test]
        public void get_all_players_gets_all_players_until_one_returns_no_result()
        {
            //Arrange
            const int topPlayerId = 6;
            _childPlayerServiceMock.Setup(x => x.GetPlayer(It.Is<int>(i => i < topPlayerId))).Returns(new Player());
            _childPlayerServiceMock.Setup(x => x.GetPlayer(topPlayerId)).Returns((Player)null);

            //Act
            var players = _service.GetAllPlayers();

            //Assert
            _childPlayerServiceMock.VerifyAll();
            Assert.That(players.Count, Is.EqualTo(5));
        }


        [Test]
        public void players_retrieved_from_service_only_once()
        {
            //Arrange
            const int topPlayerId = 6;
            _childPlayerServiceMock.Setup(x => x.GetPlayer(It.Is<int>(i => i < topPlayerId))).Returns(new Player());
            _childPlayerServiceMock.Setup(x => x.GetPlayer(topPlayerId)).Returns((Player)null);

            //Act
            var players = _service.GetAllPlayers();
            var players2 = _service.GetAllPlayers();

            //Assert
            _childPlayerServiceMock.Verify(x => x.GetPlayer(It.IsAny<int>()), Times.Exactly(6));
            Assert.That(players, Is.EqualTo(players2));
        }
    }
}