using System;

namespace FantasyFootballRobot.Core.Exceptions
{
    public class TeamNotFoundException : Exception
    {
        public TeamNotFoundException(string message) : base(message)
        {
            
        }
    }
}