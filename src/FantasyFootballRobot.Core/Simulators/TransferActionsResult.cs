namespace FantasyFootballRobot.Core.Simulators
{
    public class TransferActionsResult
    {
        private const int PenalisedTransferPointsPenalty = 4;
        public SeasonState UpdatedSeasonState { get; set; }

        public int PenalisedTransfersMade { get; set; }

        public int TransferPointsPenalty
        {
            get
            {
                return PenalisedTransfersMade * PenalisedTransferPointsPenalty;
            }
        }
      
        public int TotalTransfersMade { get; set; }
        public int FreeTransfersMade { get { return TotalTransfersMade - PenalisedTransfersMade; } }
    }
}