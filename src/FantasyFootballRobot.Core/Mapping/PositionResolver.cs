using System;
using AutoMapper;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Mapping
{
    public class PositionResolver : ValueResolver<string, Position>
    {
        protected override Position ResolveCore(string source)
        {
            return (Position)Enum.Parse(typeof (Position), source, true);
        }
    }
}