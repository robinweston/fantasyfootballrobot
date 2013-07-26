// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using Newtonsoft.Json.Linq;

namespace FantasyFootballRobot.Core.Entities.Json
{

    public class JsonPlayer
    {

        public JsonPlayer(string json)
         : this(JObject.Parse(json))
        {
        }

        private readonly JObject _jobject;
        public JsonPlayer(JObject obj)
        {
            _jobject = obj;
        }

        public int TransfersOut
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "transfers_out"));
            }
        }

        public int Code
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "code"));
            }
        }

        public int EventTotal
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "event_total"));
            }
        }

        public int EventPoints
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "event_points"));
            }
        }

        public int TransfersBalance
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "transfers_balance"));
            }
        }

        public int LastSeasonPoints
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "last_season_points"));
            }
        }

        public int EventCost
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "event_cost"));
            }
        }

        public object NewsAdded
        {
            get
            {
                return JsonClassHelper.ReadObject(JsonClassHelper.GetJToken<JToken>(_jobject, "news_added"));
            }
        }

        public string WebName
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "web_name"));
            }
        }

        public bool InDreamteam
        {
            get
            {
                return JsonClassHelper.ReadBoolean(JsonClassHelper.GetJToken<JValue>(_jobject, "in_dreamteam"));
            }
        }

        public string NextFixture
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "next_fixture"));
            }
        }

        public int Id
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "id"));
            }
        }

        public string ShirtImageUrl
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "shirt_image_url"));
            }
        }

        public string FirstName
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "first_name"));
            }
        }

        public int TransfersOutEvent
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "transfers_out_event"));
            }
        }

        public int ElementTypeId
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "element_type_id"));
            }
        }

        public int MaxCost
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "max_cost"));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[][] _eventExplain;
        public object[][] EventExplain
        {
            get {
                return _eventExplain ??
                       (_eventExplain =
                        (object[][])
                        JsonClassHelper.ReadArray(JsonClassHelper.GetJToken<JArray>(_jobject, "event_explain"),
                                                  JsonClassHelper.ReadObject, typeof (object[][])));
            }
        }

        public int Selected
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "selected"));
            }
        }

        public int MinCost
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "min_cost"));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private JsonFixtures _fixtures;
        public JsonFixtures Fixtures
        {
            get {
                return _fixtures ??
                       (_fixtures =
                        JsonClassHelper.ReadStronglyTypedObject<JsonFixtures>(JsonClassHelper.GetJToken<JObject>(_jobject,
                                                                                                             "fixtures")));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[] _seasonHistory;
        public object[] SeasonHistory
        {
            get {
                return _seasonHistory ??
                       (_seasonHistory =
                        (object[])
                        JsonClassHelper.ReadArray(
                            JsonClassHelper.GetJToken<JArray>(_jobject, "season_history"), JsonClassHelper.ReadObject,
                            typeof (object[])));
            }
        }

        public int TotalPoints
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "total_points"));
            }
        }

        public string TypeName
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "type_name"));
            }
        }

        public int TransfersIn
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "transfers_in"));
            }
        }

        public string TeamName
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "team_name"));
            }
        }

        public string Status
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "status"));
            }
        }

        public string Added
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "added"));
            }
        }

        public double Form
        {
            get
            {
                return JsonClassHelper.ReadFloat(JsonClassHelper.GetJToken<JValue>(_jobject, "form"));
            }
        }

        public string ShirtMobileImageUrl
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "shirt_mobile_image_url"));
            }
        }

        public string CurrentFixture
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "current_fixture"));
            }
        }

        public int NowCost
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "now_cost"));
            }
        }

        public double PointsPerGame
        {
            get
            {
                return JsonClassHelper.ReadFloat(JsonClassHelper.GetJToken<JValue>(_jobject, "points_per_game"));
            }
        }

        public object NewsUpdated
        {
            get
            {
                return JsonClassHelper.ReadObject(JsonClassHelper.GetJToken<JToken>(_jobject, "news_updated"));
            }
        }

        public object SquadNumber
        {
            get
            {
                return JsonClassHelper.ReadObject(JsonClassHelper.GetJToken<JToken>(_jobject, "squad_number"));
            }
        }

        public string News
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "news"));
            }
        }

        public int OriginalCost
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "original_cost"));
            }
        }

        public object NewsReturn
        {
            get
            {
                return JsonClassHelper.ReadObject(JsonClassHelper.GetJToken<JToken>(_jobject, "news_return"));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private JsonFixtureHistory _fixtureHistory;
        public JsonFixtureHistory FixtureHistory
        {
            get {
                return _fixtureHistory ??
                       (_fixtureHistory =
                        JsonClassHelper.ReadStronglyTypedObject<JsonFixtureHistory>(
                            JsonClassHelper.GetJToken<JObject>(_jobject, "fixture_history")));
            }
        }

        public int TeamId
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "team_id"));
            }
        }

        public int TransfersInEvent
        {
            get
            {
                return JsonClassHelper.ReadInteger(JsonClassHelper.GetJToken<JValue>(_jobject, "transfers_in_event"));
            }
        }

        public string SelectedBy
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "selected_by"));
            }
        }

        public string SecondName
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "second_name"));
            }
        }

        public string PhotoMobileUrl
        {
            get
            {
                return JsonClassHelper.ReadString(JsonClassHelper.GetJToken<JValue>(_jobject, "photo_mobile_url"));
            }
        }

    }
}
