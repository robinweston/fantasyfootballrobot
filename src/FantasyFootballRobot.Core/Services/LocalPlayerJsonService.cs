using System;
using System.IO;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Logging;

namespace FantasyFootballRobot.Core.Services
{
    public class LocalPlayerJsonService : IPlayerJsonService
    {
        private static bool _loggedOnce;
        private readonly IPlayerJsonService _childService;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly ILogger _logger;

        public LocalPlayerJsonService(IConfigurationSettings configurationSettings, IPlayerJsonService childService,
                                      ILogger logger)
        {
            _configurationSettings = configurationSettings;
            _logger = logger;
            _childService = childService;
        }

        public string GetPlayerJson(int playerId)
        {
            string filePath = Path.Combine(_configurationSettings.DataDirectory, playerId.ToString());
            filePath = Path.ChangeExtension(filePath, ".json");

            string json = GetLocalJson(filePath);
            if (string.IsNullOrEmpty(json))
            {
                json = _childService.GetPlayerJson(playerId);
                if (!string.IsNullOrEmpty(json))
                {
                    StoreJsonLocally(json, filePath);
                }
            }

            _loggedOnce = true;
            return json;
        }

        private string GetLocalJson(string filePath)
        {
            try
            {
                if (!_loggedOnce)
                {
                    _logger.Log(Tag.PlayerService,
                                string.Concat("Attempting to locate local player JSON at ", filePath));
                }

                DateTime earliestJsonValid = DateTime.Now.AddHours(-_configurationSettings.ValidPlayerJsonCacheHours);
                if (File.Exists(filePath) && File.GetLastWriteTime(filePath) > earliestJsonValid)
                {
                    string json = File.ReadAllText(filePath);

                    if (!_loggedOnce)
                    {
                        _logger.Log(Tag.PlayerService,
                                    "Player json found. Will not log any subsequent JSON service calls");
                    }

                    return json;
                }
            }
            catch (FileNotFoundException)
            {
                if (!_loggedOnce)
                {
                    _logger.Log(Tag.PlayerService, "File not found");
                }
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                if (!_loggedOnce)
                {
                    _logger.Log(Tag.PlayerService, "Directory not found");
                }
                return null;
            }
            if (!_loggedOnce)
            {
                _logger.Log(Tag.PlayerService, "No JSON result");
            }
            return null;
        }

        private static void StoreJsonLocally(string json, string filePath)
        {
            File.WriteAllText(filePath, json);
        }
    }
}