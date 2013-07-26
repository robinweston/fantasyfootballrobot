using System;
using FantasyFootballRobot.Core.Validation;

namespace FantasyFootballRobot.Core.Exceptions
{
    public class InvalidTeamException : Exception
    {
        public InvalidTeamException(TeamValidationStatus status) : base(status.ToString())
        {
            
        }
    }
}
