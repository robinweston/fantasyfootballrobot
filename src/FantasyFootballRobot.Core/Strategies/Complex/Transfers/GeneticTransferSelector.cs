using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyFootballRobot.Core.Caching;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Genetic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Transfers
{

    public class GeneticTransferSelector : GeneticAlgorithmBase<FitTransferActions, SeasonState>
    {
        private readonly ITeamScorePredictor _teamScorePredictor;
        private readonly IPredictorParameters _predictorParameters;
        private readonly ITransferValidator _transferValidator;
        private readonly ITransferActioner _transferActioner;
        private readonly IPlayerPoolReducer _playerPoolReducer;

        public IList<Player> ReducedPlayerPool { get; private set; }

        public GeneticTransferSelector(IGeneticParameters geneticParameters, IRandom random, ILogger logger, ITransferValidator transferValidator, ITeamScorePredictor teamScorePredictor, IPredictorParameters predictorParameters, ITransferActioner transferActioner, IPlayerPoolReducer playerPoolReducer)
            : base(geneticParameters, random, logger)
        {
            _transferValidator = transferValidator;
            _teamScorePredictor = teamScorePredictor;
            _predictorParameters = predictorParameters;
            _transferActioner = transferActioner;
            _playerPoolReducer = playerPoolReducer;
        }

        public override void SetSeedGenes(SeasonState seedGenes)
        {
            ReducedPlayerPool = _playerPoolReducer.ReducePlayerPool(seedGenes.AllPlayers);

            _cachedFitnessValues = new ConcurrentDictionary<string, double>();

            base.SetSeedGenes(seedGenes);
        }

        private string CreateTransfersCacheKey(FitTransferActions actions)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}_{1}_", actions.PlayStandardWildcard, actions.PlayTransferWindowWildcard);
            foreach (var transfer in actions.Transfers.OrderBy(t => t.PlayerIn.Id))
            {
                sb.Append(transfer.PlayerIn.Id);
                sb.Append("_");
                sb.Append(transfer.PlayerOut.Id);
                sb.Append("_");
            }

            return sb.ToString();
        }

        private ConcurrentDictionary<string, double> _cachedFitnessValues;

        public override double CalculateFitness(FitTransferActions chromosome)
        {
            var cacheKey = CreateTransfersCacheKey(chromosome);

            if (!VerboseLoggingEnabled && _cachedFitnessValues.ContainsKey(cacheKey))
            {
                CacheHits++;
                return _cachedFitnessValues[cacheKey];
            }
            CacheMisses++;

            //fitness is total points for nexts N gameweeks

            var clonedSeasonState = SeedGenes.ShallowClone();

            _transferActioner.VerboseLoggingEnabled = false;
            var transferResult = _transferActioner.ApplyTransfers(clonedSeasonState, chromosome);

            var previousLoggingValue = _teamScorePredictor.VerboseLoggingEnabled;
            _teamScorePredictor.VerboseLoggingEnabled = VerboseLoggingEnabled;

            var predictedPoints = _teamScorePredictor.PredictTeamPointsForFutureGameweeks(transferResult.UpdatedSeasonState.CurrentTeam, SeedGenes.Gameweek, _predictorParameters.FutureGameweeksUsedToCalculatePlayerForm,
                                                                                      transferResult.UpdatedSeasonState.AllPlayers, _predictorParameters.FutureGameweekMultiplier);
            _teamScorePredictor.VerboseLoggingEnabled = previousLoggingValue;

            var fitness = predictedPoints - transferResult.TransferPointsPenalty;

            if (!VerboseLoggingEnabled)
            {
                _cachedFitnessValues.TryAdd(cacheKey, fitness);
            }

            return fitness;
        }

        public override FitTransferActions Crossover(FitTransferActions parent1, FitTransferActions parent2)
        {
            var newActions = new FitTransferActions
                             {
                                 PlayStandardWildcard = parent1.PlayStandardWildcard && parent2.PlayStandardWildcard,
                                 PlayTransferWindowWildcard = parent1.PlayTransferWindowWildcard && parent2.PlayTransferWindowWildcard,
                             };

            var combinedTransfers = parent1.Transfers.Union(parent2.Transfers).ToList();

            if (!combinedTransfers.Any())
            {
                return newActions;
            }

            var maxTransfers = Math.Min(combinedTransfers.Count, 15);
            var newTransferCount = Random.Next(maxTransfers) + 1;

            while (newActions.Transfers.Count < newTransferCount)
            {
                var newTransfer = combinedTransfers[Random.Next(combinedTransfers.Count)];
                combinedTransfers.Remove(newTransfer);
                newActions.Transfers.Add(newTransfer);
            }

            return AreTransferActionsValid(newActions) ? newActions : null;
        }

        public override FitTransferActions Mutate(FitTransferActions parent)
        {
            if (parent.Transfers.Count == 0)
            {
                return parent;
            }

            //todo: mutate wildcards?
            var newActions = new FitTransferActions
                                 {
                                     Transfers = parent.Transfers.Select(t => t).ToList(),
                                     PlayStandardWildcard = parent.PlayStandardWildcard,
                                     PlayTransferWindowWildcard = parent.PlayTransferWindowWildcard
                                 };

            var transferToChange = newActions.Transfers[Random.Next(parent.Transfers.Count)];
            newActions.Transfers.Remove(transferToChange);

            var newTransfer = CreateRandomTransfer();
            newActions.Transfers.Add(newTransfer);

            return AreTransferActionsValid(newActions) ? newActions : null;
        }

        public override FitTransferActions CreateRandomChromosome()
        {
            var randomTransferActions = CreateRandomTransferActions();
            return AreTransferActionsValid(randomTransferActions) ? randomTransferActions : null;
        }

        private bool AreTransferActionsValid(FitTransferActions transferActions)
        {
            var transferValidity = _transferValidator.ValidateTransfers(SeedGenes, transferActions);
            return transferValidity == TransferValidity.Valid;
        }

        private FitTransferActions CreateRandomTransferActions()
        {
            var transferActions = new FitTransferActions();

            transferActions = SetWildcardActions(transferActions);

            var transferCount = Random.Next(15);

            for (var i = 0; i < transferCount; i++)
            {
                var randomTransfer = CreateRandomTransfer();
                transferActions.Transfers.Add(randomTransfer);
            }

            return transferActions;
        }

        private FitTransferActions SetWildcardActions(FitTransferActions transferActions)
        {
            var attemptWildcard = Random.Next(1) == 0;
            if (attemptWildcard)
            {           
                if (!SeedGenes.TransferWindowWildcardPlayed && _transferValidator.IsInsideTransferWindow(SeedGenes.Gameweek))
                {
                    transferActions.PlayTransferWindowWildcard = true;
                }
                else if (!SeedGenes.StandardWildCardPlayed)
                {
                    transferActions.PlayStandardWildcard = true;
                }
            }

            return transferActions;
        }

        private Transfer CreateRandomTransfer()
        {
            var playerOutIndex = Random.Next(15);
            var playerOut = SeedGenes.CurrentTeam.Players[playerOutIndex];
            var playerIn = SelectRandomPlayerToTransferIn(playerOut.Position);
            var newTransfer = new Transfer { PlayerIn = playerIn, PlayerOut = playerOut };
            return newTransfer;
        }

        private Player SelectRandomPlayerToTransferIn(Position position)
        {
            var playersInPosition = ReducedPlayerPool.Where(p => p.Position == position).ToList();
            var playerInIndex = Random.Next(playersInPosition.Count);
            return ReducedPlayerPool[playerInIndex];
        }
    }
}