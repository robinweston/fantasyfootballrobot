using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface ITransferValidator
    {
        TransferValidity ValidateTransfers(SeasonState seasonState, TransferActions transferActions);
        bool IsInsideTransferWindow(int gameweek);
    }
}