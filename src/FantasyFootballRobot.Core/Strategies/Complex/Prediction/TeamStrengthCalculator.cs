using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class TeamStrengthCalculator : ITeamStrengthCalculator
    {
        private readonly IConfigurationSettings _configSettings;
        private readonly IPredictorParameters _predictorParameters;

        private readonly IDictionary<string, TeamStrength> _cachedTeamStrengths = new ConcurrentDictionary<string, TeamStrength>();

        public TeamStrengthCalculator(IPredictorParameters predictorParameters, IConfigurationSettings configSettings)
        {
            _predictorParameters = predictorParameters;
            _configSettings = configSettings;
        }

        public TeamStrength CalculateTeamStrength(string clubCode, IList<Player> allPlayers)
        {
            //todo: implement once season has started. Make sure cache is per gameweek
         
            if(_cachedTeamStrengths.ContainsKey(clubCode))
            {
                return _cachedTeamStrengths[clubCode];
            }

            var lastSeasonEndYear = _configSettings.SeasonStartYear;
            var averageForm = CalculateLastSeasonForm(allPlayers, lastSeasonEndYear);

            var clubPlayers = allPlayers.Where(p => p.ClubCode == clubCode).ToList();
            var clubForm = CalculateLastSeasonForm(clubPlayers, lastSeasonEndYear);

            if(clubForm.TotalMinutes < _predictorParameters.MinMinutesPlayedLastSeasonToCalculateClubForm)
            {
                clubForm.ArtificalPointsPerMinuteDueToLowSampleSize = true;
                clubForm.PointsPerMinute = GetLowestClubPointsPerMinute(allPlayers, lastSeasonEndYear);
            }

            clubForm.TeamStrengthMultiplier = clubForm.PointsPerMinute/averageForm.PointsPerMinute;

            _cachedTeamStrengths[clubCode] = clubForm;

            return clubForm;
        }

        private double GetLowestClubPointsPerMinute(IEnumerable<Player> allPlayers, int seasonEndYear)
        {
            var playersByClub = allPlayers.GroupBy(p => p.ClubCode);
            return
                playersByClub.Select(pbc => CalculateLastSeasonForm(pbc.ToList(), seasonEndYear)).
                Where(ts => ts.TotalMinutes > _predictorParameters.MinMinutesPlayedLastSeasonToCalculateClubForm).
                Min(ts => ts.PointsPerMinute);
        }

        public virtual TeamStrength CalculateLastSeasonForm(IList<Player> players, int seasonEndYear)
        {
            var seasonRecords =
                players.SelectMany(p => p.PastSeasons.Where(ps => ps.SeasonEndYear == seasonEndYear)).ToList();
            var totalMinutes = seasonRecords.Sum(s => s.MinutesPlayed);
            var totalPoints = seasonRecords.Sum(s => s.TotalPointsScored);

            return new TeamStrength
                   {
                       SamplePlayers = players.Count,
                       TotalMinutes = totalMinutes,
                       TotalPointsScored = totalPoints,
                       PointsPerMinute = totalPoints/(double)totalMinutes,                    
                   };
        }
    }
}