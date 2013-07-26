using System.IO;
using System.Reflection;

namespace FantasyFootballRobot.Tests.Helpers
{
    internal class ResourceHelper
    { 
        internal static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetAssembly(typeof (ResourceHelper));
            using (Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
