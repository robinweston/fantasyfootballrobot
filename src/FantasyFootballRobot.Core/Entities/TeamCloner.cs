using System.Linq;
using FantasyFootballRobot.Core.Entities.Utilities.Cloning;

namespace FantasyFootballRobot.Core.Entities
{
    public static class TeamCloner
    {
        public static T DeepClone<T>(T team) where T : Team, new()
        {
            return ExpressionTreeCloner.DeepFieldClone(team);
        }

        public static T ShallowClone<T>(T team) where T : Team, new()
        {
            return new T
                   {
                       Players = team.Players.Select(p => p).ToList(),
                       Captain = team.Captain,
                       ViceCaptain = team.ViceCaptain
                   };
        }
    }
}
