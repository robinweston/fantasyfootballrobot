using System;
using FantasyFootballRobot.Core.Simulators;

namespace FantasyFootballRobot.Core.Exceptions
{
    public class InvalidTransferException : Exception
    {
        public InvalidTransferException(TransferValidity transferValidity) : base(transferValidity.ToString())
        {
        }
    }
}