using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class PlayerFormCalculator : IPlayerFormCalculator
    {

        private readonly IConfigurationSettings _configSettings;
        private readonly IPredictorParameters _predictorParameters;
        private readonly ITeamStrengthCalculator _teamStrengthCalculator;

        public PlayerFormCalculator(IPredictorParameters predictorParameters, ITeamStrengthCalculator teamStrengthCalculator, IConfigurationSettings configSettings)
        {
            _predictorParameters = predictorParameters;
            _teamStrengthCalculator = teamStrengthCalculator;
            _configSettings = configSettings;
        }

        public PlayerForm CalculateCurrentPlayerForm(Player player, IList<Player> allPlayers)
        {           
            var scores = player.PastFixtures.OrderByDescending(pf => pf.GameWeek).
                Take(_predictorParameters.PastGamesUsedToCalculatePlayerForm).
                Select(pf => CalculatePastFixtureNormalisedScore(pf, allPlayers)).ToList();

            var lastSeason = player.GetPastSeason(_configSettings.SeasonStartYear);
            var lastSeasonForm = CreatePlayerFormFromPastSeason(lastSeason);

            if(scores.Count < _predictorParameters.PastGamesUsedToCalculatePlayerForm && 
                lastSeasonForm.PreviousSeasonMinutesPlayed > _predictorParameters.MinMinutesPlayedLastSeasonToCalculatePlayerForm)
            {
                    scores.AddRange(Enumerable.Repeat(lastSeasonForm.NormalisedPointsPerGame,
                                                      _predictorParameters.PastGamesUsedToCalculatePlayerForm -
                                                      scores.Count));
            }

            return new PlayerForm
                   {
                       NormalisedPointsPerGame = CalculateNormalisedMean(scores),
                       PreviousSeasonMinutesPlayed = lastSeasonForm.PreviousSeasonMinutesPlayed,
                       PreviousSeasonTotalPointsScored = lastSeasonForm.PreviousSeasonTotalPointsScored
                   };
        }

        private double CalculateNormalisedMean(IList<double> scores)
        {
            if(scores.Count == 0)
            {
                return 0;
            }

            //http://en.wikipedia.org/wiki/Weighted_mean
            return
                scores.Select(
                    (s, i) => (s * CalculateScoreWeight(i, _predictorParameters.PreviousGameMultiplier))).Sum()
                              /
                              scores.Select(
                                  (_, i) => CalculateScoreWeight(i, _predictorParameters.PreviousGameMultiplier)).Sum();
        }

        private double CalculateScoreWeight(int scoreIndex, double multiplier)
        {
            return Math.Pow(multiplier, scoreIndex + 1);
        }

        private double CalculatePastFixtureNormalisedScore(PastFixture pastFixture, IList<Player> allPlayers)
        {
            var locationRatio = pastFixture.Home
                                    ? _predictorParameters.HomeAdvantageMultiplier
                                    : _predictorParameters.AwayAdvantageMultiplier;

            var normalisedScore = pastFixture.TotalPointsScored / locationRatio;

            var oppositionStrength = _teamStrengthCalculator.CalculateTeamStrength(pastFixture.OppositionClubCode,
                                                                                   allPlayers);

            return normalisedScore * oppositionStrength.TeamStrengthMultiplier;
        }

        private PlayerForm CreatePlayerFormFromPastSeason(Season season)
        {
            var playerForm = new PlayerForm();

            if (season != null && season.MinutesPlayed > 0)
            {
                playerForm.PreviousSeasonTotalPointsScored = season.TotalPointsScored;
                playerForm.PreviousSeasonMinutesPlayed = season.MinutesPlayed;
                var pointsPerMinute = season.TotalPointsScored / (double)season.MinutesPlayed;
                playerForm.NormalisedPointsPerGame = pointsPerMinute * 90;
            }

            return playerForm;
        }
    }
}