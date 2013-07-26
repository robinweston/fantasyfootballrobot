using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Constants;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Genetic;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public class GeneticTeamSelector : GeneticAlgorithmBase<FitTeam, IList<Player>>
    {
        private readonly ITeamScorePredictor _teamScorePredictor;
        private readonly IPlayerPoolReducer _playerPoolReducer;
        private readonly IPredictorParameters _predictorParameters;

        private readonly IDictionary<Position, Player[]> _reducedPlayerPoolLookup = new ConcurrentDictionary<Position, Player[]>(); 

        public IList<Player> ReducedPlayerPool { get; private set; } 

        public GeneticTeamSelector(IGeneticParameters geneticParameters, ILogger logger, IRandom random, ITeamScorePredictor teamScorePredictor, IPlayerPoolReducer playerPoolReducer, IPredictorParameters predictorParameters) :
            base(geneticParameters, random, logger)
        {
            _teamScorePredictor = teamScorePredictor;
            _playerPoolReducer = playerPoolReducer;
            _predictorParameters = predictorParameters;
        }

        public override void SetSeedGenes(IList<Player> seedGenes)
        {
            ReducedPlayerPool = _playerPoolReducer.ReducePlayerPool(seedGenes);

            foreach (Position position in Enum.GetValues(typeof (Position)))
            {
                _reducedPlayerPoolLookup[position] = ReducedPlayerPool.Where(x => x.Position == position).ToArray();
            }

            base.SetSeedGenes(seedGenes); 
        }

        public override double CalculateFitness(FitTeam team)
        {
            var previousLoggingValue = _teamScorePredictor.VerboseLoggingEnabled;
            _teamScorePredictor.VerboseLoggingEnabled = VerboseLoggingEnabled;

            const int currentGameweek = 1;
            var prediction = _teamScorePredictor.PredictTeamPointsForFutureGameweeks(team, currentGameweek, _predictorParameters.FutureGameweeksUsedToCalculatePlayerForm, SeedGenes, _predictorParameters.FutureGameweekMultiplier);
            
            _teamScorePredictor.VerboseLoggingEnabled = previousLoggingValue;

            return prediction;
        }

        public override FitTeam Crossover(FitTeam parent1, FitTeam parent2)
        {
            var splicePoint = Random.Next(14) + 1;
           
            var playersFromTeam1 = parent1.Players.Take(splicePoint);
            var playersFromTeam2 = parent2.Players.Skip(splicePoint);

            var childTeam = new FitTeam
                           {
                               Players = playersFromTeam1.Concat(playersFromTeam2).ToList()
                           };

            AssignCaptains(childTeam);

            return IsTeamValid(childTeam) ? childTeam : null;
        }

        public override FitTeam Mutate(FitTeam team)
        {           
            var mutatedTeam = team.ShallowClone();

            var randomPosition = Random.Next(mutatedTeam.Players.Count);
            
            var playerToRemove = mutatedTeam.Players[randomPosition];

            //pick replacement from reduced player pool
            var replacementPlayer = SelectRandomPlayer(playerToRemove.Position);

            mutatedTeam.Players.Remove(playerToRemove);
            mutatedTeam.Players.Insert(randomPosition, replacementPlayer);

            AssignCaptains(mutatedTeam);

            return IsTeamValid(mutatedTeam) ? mutatedTeam : null;
        }

        public override FitTeam CreateRandomChromosome()
        {
            var randomTeam = CreateRandomTeam();
            return IsTeamValid(randomTeam) ? randomTeam : null;
        }

        private static bool IsTeamValid(FitTeam team)
        {
            return team.Validity == TeamValidationStatus.Valid &&
                   team.Players.Sum(p => p.OriginalCost) <= GameConstants.StartingMoney;
        }

        private FitTeam CreateRandomTeam()
        {
            var team = new FitTeam();

            team.Players.Add(SelectRandomPlayer(Position.Goalkeeper));

            team.Players.Add(SelectRandomPlayer(Position.Defender));
            team.Players.Add(SelectRandomPlayer(Position.Defender));
            team.Players.Add(SelectRandomPlayer(Position.Defender));
                                              
            team.Players.Add(SelectRandomPlayer(Position.Midfielder));
            team.Players.Add(SelectRandomPlayer(Position.Midfielder));
            team.Players.Add(SelectRandomPlayer(Position.Midfielder));
            team.Players.Add(SelectRandomPlayer(Position.Midfielder));
                                            
            team.Players.Add(SelectRandomPlayer(Position.Forward));
            team.Players.Add(SelectRandomPlayer(Position.Forward));
            team.Players.Add(SelectRandomPlayer(Position.Forward));
                                      
            team.Players.Add(SelectRandomPlayer(Position.Goalkeeper));
            team.Players.Add(SelectRandomPlayer(Position.Defender));
            team.Players.Add(SelectRandomPlayer(Position.Defender));
            team.Players.Add(SelectRandomPlayer(Position.Midfielder));
                       
            AssignCaptains(team);

            return team;
        }

        private static void AssignCaptains(Team team)
        {
            //captains don't matter here, so just pick first two players
            team.Captain = team.Players.First();
            team.ViceCaptain = team.Players[1];
        }

        private Player SelectRandomPlayer(Position position)
        {
            var playersByPosition = _reducedPlayerPoolLookup[position];
            var randomPosition = Random.Next(playersByPosition.Length);
            return playersByPosition[randomPosition];
        }
    }
}
