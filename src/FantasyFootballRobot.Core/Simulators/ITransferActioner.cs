using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface ITransferActioner : ILoggable
    {
        TransferActionsResult ApplyTransfers(SeasonState seasonState, TransferActions transferActions);

    }
}
