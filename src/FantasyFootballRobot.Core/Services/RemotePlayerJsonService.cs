using System;
using System.IO;
using System.Net;
using System.Text;

namespace FantasyFootballRobot.Core.Services
{
    public class RemotePlayerJsonService : IPlayerJsonService
    {
        const string PlayerRequestTemplate = "http://fantasy.premierleague.com/web/api/elements/{0}/";

        public string GetPlayerJson(int playerId)
        {
            var url = String.Format(PlayerRequestTemplate, playerId);
            var request = WebRequest.Create(url);

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var receiveStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            catch (WebException)
            {
                return null;
            }
        }
    }
}