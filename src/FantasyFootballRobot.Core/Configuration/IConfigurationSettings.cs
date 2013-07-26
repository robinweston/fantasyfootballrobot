namespace FantasyFootballRobot.Core.Configuration
{
    public interface IConfigurationSettings
    {
        int ValidPlayerJsonCacheHours { get;  }
        string DataDirectory { get;  }
        int SeasonStartYear { get; }
        int TransferWindowWildcardGameweekStart { get; }
        int TransferWindowWildcardGameweekEnd { get; }
        bool MakeTransfersAtStartOfNewGameweek { get; }
    }
}