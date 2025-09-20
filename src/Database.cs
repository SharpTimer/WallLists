using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using Dapper;
using MySqlConnector;
using System.Data.SQLite;
using Npgsql;
using System.Data;

namespace SharpTimerWallLists
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        private string? _databasePath;
        private string? _connectionString;

        private void InitializeDatabasePathAndConnectionString()
        {
            var dbSettings = Config.DatabaseSettings;
            if (Config.DatabaseType == 1)
            {
                var mySqlSslMode = dbSettings.SslMode.ToLower() switch
                {
                    "none" => MySqlSslMode.None,
                    "preferred" => MySqlSslMode.Preferred,
                    "required" => MySqlSslMode.Required,
                    "verifyca" => MySqlSslMode.VerifyCA,
                    "verifyfull" => MySqlSslMode.VerifyFull,
                    _ => MySqlSslMode.None
                };
                _connectionString = $@"Server={dbSettings.Host};Port={dbSettings.Port};Database={dbSettings.Database};Uid={dbSettings.Username};Pwd={dbSettings.Password};SslMode={mySqlSslMode};AllowPublicKeyRetrieval=True;";
            }
            else if (Config.DatabaseType == 2)
            {
                _databasePath = Path.Combine(Server.GameDirectory, "csgo", "cfg", "SharpTimer", "database.db");
                _connectionString = $"Data Source={_databasePath};Version=3;";
            }
            else if (Config.DatabaseType == 3)
            {
                var npgSqlSslMode = dbSettings.SslMode.ToLower() switch
                {
                    "disable" => SslMode.Disable,
                    "require" => SslMode.Require,
                    "prefer" => SslMode.Prefer,
                    "allow" => SslMode.Allow,
                    "verify-full" => SslMode.VerifyFull,
                    _ => SslMode.Disable
                };
                _connectionString = $"Host={dbSettings.Host};Port={dbSettings.Port};Database={dbSettings.Database};Username={dbSettings.Username};Password={dbSettings.Password};SslMode={npgSqlSslMode};";
            }
        }

        private IDbConnection CreateDbConnection()
        {
            return Config.DatabaseType switch
            {
                1 => new MySqlConnection(_connectionString!),
                2 => new SQLiteConnection(_connectionString!),
                3 => new NpgsqlConnection(_connectionString!),
                _ => throw new ArgumentException("Invalid DatabaseType")
            };
        }

        private async Task<bool> HasEmptyDbSlot(string mapName, ListType listType)
        {
            string table = $"{Config.DatabaseSettings.TablePrefix}st_lists";
            using var conn = CreateDbConnection();

            var rec = await conn.QueryFirstOrDefaultAsync<StListRecord>(
                $@"SELECT Location1,Location2,Location3,Location4
                FROM {table}
                WHERE MapName = @m AND ListType = @t;",
                new { m = mapName, t = listType.ToString() }
            );

            if (rec == null)
                return true;

            if (string.IsNullOrEmpty(rec.Location1)) return true;
            if (string.IsNullOrEmpty(rec.Location2)) return true;
            if (string.IsNullOrEmpty(rec.Location3)) return true;
            if (string.IsNullOrEmpty(rec.Location4)) return true;

            return false;
        }

        public async Task<List<PlayerPlace>> GetTopPlayersAsync(int topCount, ListType listType, string mapName)
        {
            string query;
            string tablePrefix = Config.DatabaseSettings.TablePrefix;
            string RecordStyle = Config.RecordStyle;
            string tableName = $"PlayerStats{(string.IsNullOrEmpty(tablePrefix) ? "" : $"_{tablePrefix}")}";

            // Determine if we should filter by Mode. "" = ignore Mode, anything else = valid 
            bool filterByMode = Config.DefaultMode.Length > 0;

            // SQL calls for Mode depnding on DB type
            string modeSql      = filterByMode ? " AND Mode = @Mode" : string.Empty;
            string modePostgres = filterByMode ? " AND \"Mode\" = @Mode" : string.Empty;

            if (Config.DatabaseType == 1) // MySQL
            {
                query = listType switch
                {
                    ListType.Points => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            GlobalPoints,
                            DENSE_RANK() OVER (ORDER BY GlobalPoints DESC) AS playerPlace
                        FROM {tableName}
                    )
                    SELECT SteamID, PlayerName, GlobalPoints, playerPlace
                    FROM RankedPlayers
                    ORDER BY GlobalPoints DESC
                    LIMIT @TopCount",

                    ListType.Times => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            TimerTicks,
                            DENSE_RANK() OVER (ORDER BY TimerTicks ASC) AS playerPlace
                        FROM PlayerRecords
                        WHERE MapName = @MapName AND Style = {RecordStyle}{modeSql}
                    )
                    SELECT SteamID, PlayerName, TimerTicks, playerPlace
                    FROM RankedPlayers
                    ORDER BY TimerTicks ASC
                    LIMIT @TopCount",

                    ListType.Completions => $@"
                    WITH CompletionCounts AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            COUNT(DISTINCT MapName) AS Completions
                        FROM PlayerRecords
                        WHERE MapName NOT LIKE '%bonus%'
                        GROUP BY SteamID, PlayerName
                    )
                    SELECT SteamID, PlayerName, Completions
                    FROM CompletionCounts
                    ORDER BY Completions DESC
                    LIMIT @TopCount",

                    _ => throw new ArgumentException("Invalid list type")
                };

                try
                {
                    using var connection = new MySqlConnection(_connectionString);
                    object parameters = listType switch
                    {
                        ListType.Points       => new { TopCount = topCount },
                        ListType.Completions  => new { TopCount = topCount },
                        ListType.Times        => filterByMode
                            ? new { TopCount = topCount, MapName = mapName, Mode = Config.DefaultMode }
                            : new { TopCount = topCount, MapName = mapName },
                        _ => throw new ArgumentException("Invalid list type")
                    };

                    return (await connection.QueryAsync<PlayerPlace>(query, parameters)).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to retrieve top players from MySQL for {listType}, please check your database credentials in the config");
                    return new List<PlayerPlace>();
                }
            }

            else if (Config.DatabaseType == 2) // SQLite
            {
                query = listType switch
                {
                    ListType.Points => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            GlobalPoints,
                            DENSE_RANK() OVER (ORDER BY GlobalPoints DESC) AS playerPlace
                        FROM {tableName}
                    )
                    SELECT SteamID, PlayerName, GlobalPoints, playerPlace
                    FROM RankedPlayers
                    ORDER BY GlobalPoints DESC
                    LIMIT @TopCount",

                    ListType.Times => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            TimerTicks,
                            DENSE_RANK() OVER (ORDER BY TimerTicks ASC) AS playerPlace
                        FROM PlayerRecords
                        WHERE MapName = @MapName AND Style = {RecordStyle}{modeSql}
                    )
                    SELECT SteamID, PlayerName, TimerTicks, playerPlace
                    FROM RankedPlayers
                    ORDER BY TimerTicks ASC
                    LIMIT @TopCount",

                    ListType.Completions => $@"
                    WITH CompletionCounts AS (
                        SELECT
                            SteamID,
                            PlayerName,
                            COUNT(DISTINCT MapName) AS Completions
                        FROM PlayerRecords
                        WHERE MapName NOT LIKE '%bonus%'
                        GROUP BY SteamID, PlayerName
                    )
                    SELECT SteamID, PlayerName, Completions
                    FROM CompletionCounts
                    ORDER BY Completions DESC
                    LIMIT @TopCount",

                    _ => throw new ArgumentException("Invalid list type")
                };

                try
                {
                    using var connection = new SQLiteConnection(_connectionString);
                    connection.Open();
                    object parameters = listType switch
                    {
                        ListType.Points       => new { TopCount = topCount },
                        ListType.Completions  => new { TopCount = topCount },
                        ListType.Times        => filterByMode
                            ? new { TopCount = topCount, MapName = mapName, Mode = Config.DefaultMode }
                            : new { TopCount = topCount, MapName = mapName },
                        _ => throw new ArgumentException("Invalid list type")
                    };

                    return (await connection.QueryAsync<PlayerPlace>(query, parameters)).ToList();
                }
                catch (Exception)
                {
                    return new List<PlayerPlace>();
                }
            }

            else if (Config.DatabaseType == 3) // PostgreSQL
            {
                query = listType switch
                {
                    ListType.Points => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            ""SteamID"",
                            ""PlayerName"",
                            ""GlobalPoints"",
                            DENSE_RANK() OVER (ORDER BY ""GlobalPoints"" DESC) AS playerPlace
                        FROM ""{tableName}""
                    )
                    SELECT ""SteamID"", ""PlayerName"", ""GlobalPoints"", playerPlace
                    FROM RankedPlayers
                    ORDER BY ""GlobalPoints"" DESC
                    LIMIT @TopCount",

                    ListType.Times => $@"
                    WITH RankedPlayers AS (
                        SELECT
                            ""SteamID"",
                            ""PlayerName"",
                            ""TimerTicks"",
                            DENSE_RANK() OVER (ORDER BY ""TimerTicks"" ASC) AS playerPlace
                        FROM ""PlayerRecords""
                        WHERE ""MapName"" = @MapName AND ""Style"" = {RecordStyle}{modePostgres}
                    )
                    SELECT ""SteamID"", ""PlayerName"", ""TimerTicks"", playerPlace
                    FROM RankedPlayers
                    ORDER BY ""TimerTicks"" ASC
                    LIMIT @TopCount",

                    ListType.Completions => $@"
                    WITH CompletionCounts AS (
                        SELECT
                            ""SteamID"",
                            ""PlayerName"",
                            COUNT(DISTINCT ""MapName"") AS Completions
                        FROM ""PlayerRecords""
                        WHERE ""MapName"" NOT LIKE '%bonus%'
                        GROUP BY ""SteamID"", ""PlayerName""
                    )
                    SELECT ""SteamID"", ""PlayerName"", Completions
                    FROM CompletionCounts
                    JOIN ""PlayerRecords"" USING (SteamID)
                    ORDER BY Completions DESC
                    LIMIT @TopCount",

                    _ => throw new ArgumentException("Invalid list type")
                };

                try
                {
                    using var connection = new NpgsqlConnection(_connectionString);
                    object parameters = listType switch
                    {
                        ListType.Points       => new { TopCount = topCount },
                        ListType.Completions  => new { TopCount = topCount },
                        ListType.Times        => filterByMode
                            ? new { TopCount = topCount, MapName = mapName, Mode = Config.DefaultMode }
                            : new { TopCount = topCount, MapName = mapName },
                        _ => throw new ArgumentException("Invalid list type")
                    };

                    return (await connection.QueryAsync<PlayerPlace>(query, parameters)).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to retrieve top players from PostgreSQL for {listType}, please check your database credentials in the config");
                    return new List<PlayerPlace>();
                }
            }
            else
            {
                Logger.LogError("Invalid DatabaseType specified in config");
                return new List<PlayerPlace>();
            }
        }
    }
}