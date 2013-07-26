using AutoMapper;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class PlayerStatusResolver : ValueResolver<string, Status>
    {
        protected override Status ResolveCore(string source)
        {
            switch(source.ToLower())
            {
                case "a":
                    return Status.Available;
                case "u":
                    return Status.Unavailable;
            }
            return Status.Unknown;
        }
    }
}