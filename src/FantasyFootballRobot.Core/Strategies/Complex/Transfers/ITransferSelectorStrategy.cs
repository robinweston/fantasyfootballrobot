using FantasyFootballRobot.Core.Simulators;

namespace FantasyFootballRobot.Core.Strategies.Complex.Transfers
{
    public interface ITransferSelectorStrategy
    {
        TransferActions SelectTransfers(SeasonState seasonState);
    }
}