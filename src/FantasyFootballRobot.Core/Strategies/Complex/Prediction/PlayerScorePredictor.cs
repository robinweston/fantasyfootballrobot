using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Caching;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class PlayerScorePredictor : IPlayerScorePredictor, ICacher
    {
        private readonly ITeamStrengthCalculator _teamStrengthCalculator;
        private readonly IPredictorParameters _predictorParameters;
        private readonly IPlayerFormCalculator _playerFormCalculator;

        private IDictionary<int, IDictionary<int, double>> _cachedPlayerGameweekPredictions;

        public PlayerScorePredictor(ITeamStrengthCalculator teamStrengthCalculator, IPredictorParameters predictorParameters, IPlayerFormCalculator playerFormCalculator)
        {
            _teamStrengthCalculator = teamStrengthCalculator;
            _predictorParameters = predictorParameters;
            _playerFormCalculator = playerFormCalculator;

            PrimeCache();
        }

        private void PrimeCache()
        {
            _cachedPlayerGameweekPredictions = new ConcurrentDictionary<int, IDictionary<int, double>>();

            for (int i = 0; i <= 38; i++)
            {
                _cachedPlayerGameweekPredictions.Add(i, new ConcurrentDictionary<int, double>());
            }
        }

        private double? GetCachedValue(int gameweek, int playerId)
        {
            var gameweekCache = _cachedPlayerGameweekPredictions[gameweek];

            double cachedValue;
            if(gameweekCache.TryGetValue(playerId, out cachedValue))
            {
                CacheHits++;
                return cachedValue;
            }

            CacheMisses++;
            return null;
        }

        private void SetCachedValue(int gameweek, int playerId, double valueToCache)
        {
            _cachedPlayerGameweekPredictions[gameweek][playerId] = valueToCache;
        }

        public virtual double PredictPlayerGameweekPoints(Player player, int gameweek, IList<Player> allPlayers)
        {
            if (!VerboseLoggingEnabled)
            {
                var cachedValue = GetCachedValue(gameweek, player.Id);
                if (cachedValue.HasValue)
                {
                    return cachedValue.Value;
                } 
            }

            var fixtures = player.FutureFixtures.Where(x => x.GameWeek == gameweek);
            var playerForm = _playerFormCalculator.CalculateCurrentPlayerForm(player, allPlayers);

            var predictedScore = fixtures.Select(f => CalculatePlayerFixturePredictedScore(f, playerForm, allPlayers)).Sum();

            if (double.IsNaN(predictedScore))
            {
                throw new Exception(string.Format("{0} has predicted score for gameweek {1} of Not A Number", player.Name, gameweek));
            }

            SetCachedValue(gameweek, player.Id, predictedScore);

            return predictedScore;
        }

        private double CalculatePlayerFixturePredictedScore(FutureFixture futureFixture, PlayerForm playerForm, IList<Player> allPlayers)
        {
            var oppositionStrength = _teamStrengthCalculator.CalculateTeamStrength(futureFixture.OppositionClubCode, allPlayers);

            var formRelativeToOppositionStrength = playerForm.NormalisedPointsPerGame / oppositionStrength.TeamStrengthMultiplier;

            var locationRatio = futureFixture.Home
                                    ? _predictorParameters.HomeAdvantageMultiplier
                                    : 1 / _predictorParameters.HomeAdvantageMultiplier;

            return formRelativeToOppositionStrength * locationRatio;
        }

        public bool VerboseLoggingEnabled { get; set; }
        public int CacheHits { get; private set; }
        public int CacheMisses { get; private set; }
    }
}