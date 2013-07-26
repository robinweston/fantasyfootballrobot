using System;
using System.Configuration;

namespace FantasyFootballRobot.Core.Configuration
{
    public class ConfigurationSettings : IConfigurationSettings
    {

        private static string GetConfigurationValue(string key, object defaultValue)
        {
            var configValue = ConfigurationManager.AppSettings[key];
            if (String.IsNullOrEmpty(configValue))
            {
                configValue = defaultValue.ToString();
            }

            return configValue;
        }

        private static int GetIntConfigurationValue(string key, int defaultValue)
        {
            return int.Parse(GetConfigurationValue(key, defaultValue));
        }

        private bool GetBoolConfigurationValue(string key, bool defaultValue)
        {
            return bool.Parse(GetConfigurationValue(key, defaultValue));
        }

        public int ValidPlayerJsonCacheHours
        {
            get { return int.Parse(GetConfigurationValue("ValidPlayerJsonCacheHours", 24)); }
        }

        public string DataDirectory
        {
            get { return GetConfigurationValue("DataDirectory", @"\DataCache"); }
        }

        public int SeasonStartYear
        {
            get { return GetIntConfigurationValue("SeasonStartYear", 2011); }
        }

        public int TransferWindowWildcardGameweekStart
        {
            get
            {
                return GetIntConfigurationValue("TransferWindowWildcardGameweekStart", 21);
            }
        }

        public int TransferWindowWildcardGameweekEnd
        {
            get
            {
                return GetIntConfigurationValue("TransferWindowWildcardGameweekStart", 23);
            }
        }

        public bool MakeTransfersAtStartOfNewGameweek
        {
            get
            {
                return GetBoolConfigurationValue("MakeTransfersAtStartOfNewGameweek", true);
            }
        }
    }
}