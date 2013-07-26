using AutoMapper;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class TeamNameResolver : ValueResolver<string, string>
    {
        protected override string ResolveCore(string source)
        {
            return Club.GetCodeFromTeamName(source);
        }
    }
}