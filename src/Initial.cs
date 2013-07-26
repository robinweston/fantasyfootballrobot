using System;

public class Class1
{
	public Class1()
	{
        List<Team> GenerateInitialPopulation()
        {
            var teams = new List<Team>();
            while(teams.Count < GeneticParameters.PopulationSize)
            {
                var randomTeam = CreateRandomTeam(PlayerPool);
                if (IsTeamValid(randomTeam))
                {
                    teams.Add(randomTeam);
                }
            }

            return teams;
        }

        Team CreateRandomTeam(IList<Player> allPlayers)
        {
            var team = new Team();
            team.Players.Add(SelectRandomPlayer(allPlayers, Position.Goalkeeper));
            team.Players.Add(SelectRandomPlayer(allPlayers, Position.Defender));
            team.Players.Add(SelectRandomPlayer(allPlayers, Position.Defender));          
            //...
            return team;
        }

        public double CalculateFitness(Team team)
	    {
	        double predictedPoints = 0;

            foreach (var player in team.Players)
            {
                predictedPoints += player.PointsScoredLastSeason;
            }

	        return predictedPoints;
	    }

        public Team Crossover(Team parent1, Team parent2)
        {
            var splicePoint = Random.Next(14) + 1;
           
            var playersFromTeam1 = parent1.Players.Take(splicePoint);
            var playersFromTeam2 = parent2.Players.Skip(splicePoint);

            var childTeam = new Team
                           {
                               Players = playersFromTeam1.Concat(playersFromTeam2)
                           };
            return IsTeamValid(childTeam) ? childTeam : null;
        }

        public Team Mutate(Team team)
        {           
            var mutatedTeam = team.ShallowClone();

            var randomPosition = Random.Next(mutatedTeam.Players.Count);      
            var playerToRemove = mutatedTeam.Players[randomPosition];
            var replacementPlayer = SelectRandomPlayer(ReducedPlayerPool, 
                playerToRemove.Position);

            mutatedTeam.Players.Remove(playerToRemove);
            mutatedTeam.Players.Insert(randomPosition, replacementPlayer);

            return IsTeamValid(mutatedTeam) ? mutatedTeam : null;
        }
	}
}
