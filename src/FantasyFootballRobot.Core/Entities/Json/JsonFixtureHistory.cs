// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using Newtonsoft.Json.Linq;

namespace FantasyFootballRobot.Core.Entities.Json
{

    public class JsonFixtureHistory
    {

        private readonly JObject _jobject;
        public JsonFixtureHistory(JObject obj)
        {
            _jobject = obj;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[][] _all;
        public object[][] All
        {
            get {
                return _all ??
                       (_all =
                        (object[][])
                        JsonClassHelper.ReadArray(JsonClassHelper.GetJToken<JArray>(_jobject, "all"),
                                                          JsonClassHelper.ReadObject, typeof (object[][])));
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
