using System;

public class Class1
{
	public Class1()
	{
        public override IList<TransferActions> GenerateInitialPopulation()
        {
            var population = new List<TransferActions>();
            while (population.Count < GeneticParameters.PopulationSize)
            {
                var randomTransferActions = CreateRandomTransferActions();
                if (AreTransferActionsValid(randomTransferActions))
                {
                    population.Add(randomTransferActions);
                }
            }

            return population;
        }

        private TransferActions CreateRandomTransferActions()
        {
            var transferActions = new TransferActions();

            transferActions = SetRandomWildcardActions(transferActions);

            var transferCount = Random.Next(15);

            for (var i = 0; i < transferCount; i++)
            {
                var randomTransfer = CreateRandomTransfer();
                transferActions.Transfers.Add(randomTransfer);
            }

            return transferActions;
        }

        private Transfer CreateRandomTransfer()
        {
            var playerOutIndex = Random.Next(15);
            var playerOut = CurrentTeam.Players[playerOutIndex];
            var playerIn = SelectRandomPlayerToTransferIn(playerOut.Position);
            return new Transfer { PlayerIn = playerIn, PlayerOut = playerOut };
        }

        public double CalculateFitness(TransferActions transferActions)
        {
            _transferActioner.VerboseLoggingEnabled = false;

            var clonedSeasonState = SeasonState.Clone();

            var transferResult = _transferActioner.ApplyTransfers(
                clonedSeasonState, transferActions);

            var predictedPoints = _teamScorePredictor.PredictTeamPointsForFutureGameweeks(
                transferResult.UpdatedSeasonState.CurrentTeam, 5);

            var fitness = predictedPoints - transferResult.TransferPointsPenalty;

            return fitness;
        }

        public TransferActions Crossover(TransferActions parent1, TransferActions parent2)
	    {
	        var newActions = new TransferActions();
            var combinedTransfers = parent1.Transfers.Union(parent2.Transfers).ToList();

            var maxTransfers = Math.Min(combinedTransfers.Count, 15);
            var newTransferCount = Random.Next(maxTransfers);

            while (newActions.Transfers.Count < newTransferCount)
            {
                var newTransfer = combinedTransfers[Random.Next(combinedTransfers.Count)];
                combinedTransfers.Remove(newTransfer);
                newActions.Transfers.Add(newTransfer);
            }

            return AreTransferActionsValid(newActions) ? newActions : null;
        }

        public TransferActions Mutate(TransferActions parent)
        {
            var newActions = new TransferActions
                                 {
                                     Transfers = parent.Transfers.Select(t => t).ToList(),
                                 };

            var transferToChange = newActions.Transfers[Random.Next(parent.Transfers.Count)];
            newActions.Transfers.Remove(transferToChange);

            var newTransfer = CreateRandomTransfer();
            newActions.Transfers.Add(newTransfer);

            return AreTransferActionsValid(newActions) ? newActions : null;
        }
	}
}
