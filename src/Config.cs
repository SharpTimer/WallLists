using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace SharpTimerWallLists
{
    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("DatabaseType")]
        public int DatabaseType { get; set; } = 1; // 1 = MySQL, 2 = SQLite. 3 = Postgres
        [JsonPropertyName("DatabaseSettings")]
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();

        /////////////////////////////////////////////////////////////////////////////////

        [JsonPropertyName("TimesTitleText")]
        public string TimesTitleText { get; set; } = "|---- Map Times ----|";

        [JsonPropertyName("TimesTextAlignment")]
        public string TimesTextAlignment { get; set; } = "center";

        [JsonPropertyName("TimesCount")]
        public int TimesCount { get; set; } = 5;


        [JsonPropertyName("PointsTitleText")]
        public string PointsTitleText { get; set; } = "|--- Points Leaders ---|";

        [JsonPropertyName("PointsTextAlignment")]
        public string PointsTextAlignment { get; set; } = "center";
        [JsonPropertyName("PointsCount")]
        public int PointsCount { get; set; } = 5;


        [JsonPropertyName("CompletionsTitleText")]
        public string CompletionsTitleText { get; set; } = "|--- Maps Completed ---|";
        
        [JsonPropertyName("CompletionsTextAlignment")]
        public string CompletionsTextAlignment { get; set; } = "center";

        [JsonPropertyName("CompletionsCount")]
        public int CompletionsCount { get; set; } = 5;

        /////////////////////////////////////////////////////////////////////////////////

        [JsonPropertyName("TitleFontSize")]
        public int TitleFontSize { get; set; } = 26;

        [JsonPropertyName("TitleTextScale")]
        public float TitleTextScale { get; set; } = 0.45f;

        [JsonPropertyName("ListFontSize")]
        public int ListFontSize { get; set; } = 24;

        [JsonPropertyName("ListTextScale")]
        public float ListTextScale { get; set; } = 0.35f;

        [JsonPropertyName("MaxNameLength")]
        public int MaxNameLength { get; set; } = 32; // Default value, 32 is max Steam name length

        [JsonPropertyName("TitleTextColor")]
        public string TitleTextColor { get; set; } = "Magenta";

        [JsonPropertyName("FirstPlaceColor")]
        public string FirstPlaceColor { get; set; } = "Lime";

        [JsonPropertyName("SecondPlaceColor")]
        public string SecondPlaceColor { get; set; } = "Coral";

        [JsonPropertyName("ThirdPlaceColor")]
        public string ThirdPlaceColor { get; set; } = "Cyan";

        [JsonPropertyName("DefaultColor")]
        public string DefaultColor { get; set; } = "White";

        /////////////////////////////////////////////////////////////////////////////////

        [JsonPropertyName("TimesListCommand")]
        public string TimesListCommand { get; set; } = "timeslist";
       
        [JsonPropertyName("PointsListCommand")]
        public string PointsListCommand { get; set; } = "pointslist";

        [JsonPropertyName("CompletionsListCommand")]
        public string CompletionsListCommand { get; set; } = "completionslist";

        [JsonPropertyName("RemoveListCommand")]
        public string RemoveListCommand { get; set; } = "removelist";

        [JsonPropertyName("ReloadConfigCommand")]
        public string ReloadConfigCommand { get; set; } = "reloadlistcfg";

        [JsonPropertyName("UpdateConfigCommand")]
        public string UpdateConfigCommand { get; set; } = "updatelistcfg";

        [JsonPropertyName("CommandPermission")]
        public string CommandPermission { get; set; } = "@css/root";

        /////////////////////////////////////////////////////////////////////////////////

        [JsonPropertyName("TimeBasedUpdate")]
        public bool TimeBasedUpdate { get; set; } = false;

        [JsonPropertyName("UpdateInterval")]
        public int UpdateInterval { get; set; } = 60;

        [JsonPropertyName("RecordStyle")]
        public string RecordStyle { get; set; } = "0";

        [JsonPropertyName("AutoUpdateConfig")]
        public bool AutoUpdateConfig { get; set; } = false;

        [JsonPropertyName("ConfigVersion")]
        public override int Version { get; set; } = 5;
    }

    public sealed class DatabaseSettings
    {
        [JsonPropertyName("host")]
        public string Host { get; set; } = "localhost";
        
        [JsonPropertyName("database")]
        public string Database { get; set; } = "database";

        [JsonPropertyName("username")]
        public string Username { get; set; } = "user";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "password";

        [JsonPropertyName("port")]
        public int Port { get; set; } = 3306;

        [JsonPropertyName("sslmode")]
        public string Sslmode { get; set; } = "none";

        [JsonPropertyName("table-prefix")]
        public string TablePrefix { get; set; } = "";
    }
}
