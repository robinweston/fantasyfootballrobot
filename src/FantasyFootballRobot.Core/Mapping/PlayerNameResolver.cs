using System;
using AutoMapper;
using FantasyFootballRobot.Core.Entities.Json;

namespace FantasyFootballRobot.Core.Mapping
{
    public class PlayerNameResolver : ValueResolver<JsonPlayer, string>
    {
        protected override string ResolveCore(JsonPlayer source)
        {
            return String.Format("{0} {1}", source.FirstName, source.SecondName);
        }
    }
}