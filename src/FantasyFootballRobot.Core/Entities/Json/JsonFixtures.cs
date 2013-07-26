// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using Newtonsoft.Json.Linq;

namespace FantasyFootballRobot.Core.Entities.Json
{

    public class JsonFixtures
    {

        private readonly JObject _jobject;
        public JsonFixtures(JObject obj)
        {
            _jobject = obj;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string[][] _all;
        public string[][] All
        {
            get {
                return _all ??
                       (_all =
                        (string[][])
                        JsonClassHelper.ReadArray(JsonClassHelper.GetJToken<JArray>(_jobject, "all"),
                                                          JsonClassHelper.ReadString, typeof (string[][])));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[][] _summary;
        public object[][] Summary
        {
            get {
                return _summary ??
                       (_summary =
                        (object[][])
                        JsonClassHelper.ReadArray(JsonClassHelper.GetJToken<JArray>(_jobject, "summary"),
                                                          JsonClassHelper.ReadObject, typeof (object[][])));
            }
        }

    }
}
