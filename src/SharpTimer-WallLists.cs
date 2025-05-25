using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Extensions;
using K4WorldTextSharedAPI;
using System.Drawing;
using System.Text.Json;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Dapper;

namespace SharpTimerWallLists
{
    [MinimumApiVersion(288)]
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "SharpTimer Wall Lists";
        public override string ModuleAuthor => "Marchand";
        public override string ModuleVersion => "1.0.6";

        public required PluginConfig Config { get; set; } = new PluginConfig();
        public static PluginCapability<IK4WorldTextSharedAPI> Capability_SharedAPI { get; } = new("k4-worldtext:sharedapi");
        private CounterStrikeSharp.API.Modules.Timers.Timer? _updateTimer;
        private List<int> _currentPointsList = new();
        private List<int> _currentTimesList = new();
        private List<int> _currentCompletionsList = new();

        public string pluginPrefix = $" {ChatColors.Magenta}[{ChatColors.LightPurple}Wall-Lists{ChatColors.Magenta}]";
        private string Permission => Config.Commands.CommandPermission;
        private TextSettings Text => Config.TextSettings;
        private ListSettings List => Config.ListSettings;

        public void OnConfigParsed(PluginConfig config)
        {
            if (config.Version < Config.Version)
                Logger.LogWarning($"Configuration version mismatch (Expected: {0} | Current: {1})", Config.Version, config.Version);

            Config = config;
        }

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            try
            {
                Config.Reload();
            }
            catch (Exception)
            {
                Logger.LogWarning($"Failed to reload config file.");
            }
            
            if (Config.AutoUpdateConfig == true)
            {
                try
                {
                    Config.Update();
                }
                catch (Exception)
                {
                    Logger.LogWarning($"Failed to update config file.");
                }
            }

            InitializeDatabasePathAndConnectionString();

            if (Config.SaveToDb)
            {
                string table = "st_lists";
                using var conn = CreateDbConnection();
                conn.Execute($@"
                    CREATE TABLE IF NOT EXISTS {table} (
                        MapName     VARCHAR(255) NOT NULL,
                        ListType    VARCHAR(50)  NOT NULL,
                        Location1   TEXT,
                        Location2   TEXT,
                        Location3   TEXT,
                        Location4   TEXT,
                        PRIMARY KEY (MapName, ListType)
                    );");
            }

            AddTimer(3, () =>
            {
                if (Config.SaveToDb)
                    LoadWorldTextFromDb(Server.MapName);
                else
                    LoadWorldTextFromJson(Server.MapName);
            });

            AddCommand($"css_{Config.Commands.TimesListCommand}", "Sets up the map times list", OnTimesListAdd);
            AddCommand($"css_{Config.Commands.PointsListCommand}", "Sets up the points list", OnPointsListAdd);
            AddCommand($"css_{Config.Commands.CompletionsListCommand}", "Sets up the map completions list", OnCompletionsListAdd);
            AddCommand($"css_{Config.Commands.RemoveListCommand}", "Removes the closest list (100 units max)", OnRemoveList);
            AddCommand($"css_{Config.Commands.ReloadConfigCommand}", "Reloads the config file", ReloadConfigCommand);
            AddCommand($"css_{Config.Commands.UpdateConfigCommand}", "Updates the config to the latest version", UpdateConfigCommand);
            AddCommand("css_importwalllists", "Imports any existing JSON list locations into the database", OnImportLists);

            if (Config.TimeBasedUpdate)
            {
                _updateTimer = AddTimer(Config.UpdateInterval, UpdateLists, TimerFlags.REPEAT);
            }

            RegisterEventHandler((EventRoundStart @event, GameEventInfo info) =>
            {
                UpdateLists();
                return HookResult.Continue;
            });

            RegisterListener<Listeners.OnMapStart>((mapName) =>
            {
                AddTimer(1, () =>
                {
                    if (Config.SaveToDb)
                        LoadWorldTextFromDb(mapName);
                    else
                        LoadWorldTextFromJson(mapName);
                });
            });

            RegisterListener<Listeners.OnMapEnd>(() =>
            {
                var checkAPI = Capability_SharedAPI.Get();
                if (checkAPI != null)
                {
                    _currentPointsList.ForEach(id => checkAPI.RemoveWorldText(id, false));
                    _currentTimesList.ForEach(id => checkAPI.RemoveWorldText(id, false));
                    _currentCompletionsList.ForEach(id => checkAPI.RemoveWorldText(id, false));
                }
                _currentPointsList.Clear();
                _currentTimesList.Clear();
                _currentCompletionsList.Clear();
            });
        }

        public override void Unload(bool hotReload)
        {
            var checkAPI = Capability_SharedAPI.Get();
            if (checkAPI != null)
            {
                _currentPointsList.ForEach(id => checkAPI.RemoveWorldText(id, false));
                _currentTimesList.ForEach(id => checkAPI.RemoveWorldText(id, false));
                _currentCompletionsList.ForEach(id => checkAPI.RemoveWorldText(id, false));
            }
            _currentPointsList.Clear();
            _currentTimesList.Clear();
            _currentCompletionsList.Clear();
            _updateTimer?.Kill();
        }

        private void CreateTopList(CCSPlayerController player, CommandInfo command, ListType listType)
        {
            var checkAPI = Capability_SharedAPI.Get();
            if (checkAPI is null)
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}Failed to get the shared API.");
                return;
            }

            var mapName = Server.MapName;

            Task.Run(async () =>
            {
                try
                {
                    // If using DB, check for an empty slot before drawing the list in the world
                    if (Config.SaveToDb)
                    {
                        bool hasSlot = await HasEmptyDbSlot(mapName, listType);
                        if (!hasSlot)
                        {
                            Server.NextFrame(() =>
                            {
                                player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}All location spots in the database for this type of list have been used up! Please remove one if you want to add a new one.");
                            });
                            return;
                        }
                    }
                    
                    int topCount = listType switch
                    {
                        ListType.Points => List.PointsCount,
                        ListType.Times => List.TimesCount,
                        ListType.Completions => List.CompletionsCount,
                        _ => throw new ArgumentException("Invalid list type")
                    };

                    var topList = await GetTopPlayersAsync(topCount, listType, mapName);
                    var linesList = GetTopListTextLines(topList, listType);

                    Server.NextWorldUpdate(() =>
                    {
                        try
                        {
                            int messageID = checkAPI.AddWorldTextAtPlayer(player, TextPlacement.Wall, linesList);
                            if (listType == ListType.Points)
                                _currentPointsList.Add(messageID);
                            if (listType == ListType.Times)
                                _currentTimesList.Add(messageID);
                            if (listType == ListType.Completions)
                                _currentCompletionsList.Add(messageID);

                            var lineList = checkAPI.GetWorldTextLineEntities(messageID);
                            if (lineList?.Count > 0)
                            {
                                var location = lineList[0]?.AbsOrigin;
                                var rotation = lineList[0]?.AbsRotation;

                                if (location != null && rotation != null)
                                {
                                    if (Config.SaveToDb)
                                    {
                                        _ = SaveFirstAvailableSlotToDb(Server.MapName, listType, location, rotation);
                                    }
                                    else
                                    {
                                        SaveWorldTextToFile(location, rotation, listType);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error during NextWorldUpdate for CreateTopList.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error creating wall list in CreateTopList method.");
                }
            });
        }

        private void SaveWorldTextToFile(Vector location, QAngle rotation, ListType listType)
        {
            try
            {
                var mapName = Server.MapName;
                var mapsDirectory = Path.Combine(ModuleDirectory, "maps");
                if (!Directory.Exists(mapsDirectory))
                {
                    Directory.CreateDirectory(mapsDirectory);
                }

                var filename = listType switch
                {
                    ListType.Times          => $"{mapName}_timeslist.json",
                    ListType.Points         => $"{mapName}_pointslist.json",
                    ListType.Completions    => $"{mapName}_completionslist.json",
                    _                       => throw new ArgumentException("Invalid list type")
                };
                var path = Path.Combine(mapsDirectory, filename);

                var worldTextData = new WorldTextData
                {
                    Location = location.ToString(),
                    Rotation = rotation.ToString()
                };

                List<WorldTextData> data;
                if (File.Exists(path))
                {
                    data = JsonSerializer.Deserialize<List<WorldTextData>>(File.ReadAllText(path)) ?? new List<WorldTextData>();
                }
                else
                {
                    data = new List<WorldTextData>();
                }

                data.Add(worldTextData);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(data, options);
                File.WriteAllText(path, jsonString);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving world text to file in SaveWorldTextToFile method.");
            }
        }

        private async Task SaveWorldTextSlotToDb(string mapName, ListType listType, int slotIndex, Vector? location, QAngle? rotation)
        {
            if (slotIndex < 1 || slotIndex > 4)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot must be 1–4.");

            try
            {
                string table = "st_lists";
                string column = $"Location{slotIndex}";
                using var conn = CreateDbConnection();

                string? combined;
                if (location != null && rotation != null)
                {
                    string locStr = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0:F2} {1:F2} {2:F2}",
                        location.X, location.Y, location.Z
                    );

                    string rotStr = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0:F2} {1:F2} {2:F2}",
                        rotation.X, rotation.Y, rotation.Z
                    );

                    combined = $"{locStr},{rotStr}";
                }
                else
                {
                    combined = null;
                }

                var p = new { m = mapName, t = listType.ToString(), c = combined };

                string sql = Config.DatabaseType switch
                {
                    // MySQL
                    1 => $@"
                        INSERT INTO {table} (MapName, ListType, {column})
                            VALUES (@m,@t,@c)
                        ON DUPLICATE KEY UPDATE
                            {column} = @c;",

                    // SQLite
                    2 => $@"
                        INSERT INTO {table} (MapName, ListType, {column})
                            VALUES (@m,@t,@c)
                        ON CONFLICT (MapName,ListType) DO UPDATE SET
                            {column} = excluded.{column};",

                    // PostgreSQL
                    3 => $@"
                        INSERT INTO {table} (MapName, ListType, {column})
                            VALUES (@m,@t,@c)
                        ON CONFLICT (MapName,ListType) DO UPDATE SET
                            {column} = EXCLUDED.{column};",

                    _ => throw new ArgumentException("Invalid DatabaseType")
                };

                await conn.ExecuteAsync(sql, p);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving single list slot to database.");
            }
        }

        private async Task<bool> SaveFirstAvailableSlotToDb(string mapName, ListType listType, Vector location, QAngle rotation)
        {
            string table = "st_lists";
            using var conn = CreateDbConnection();

            // Load the existing row (if any)
            var rec = await conn.QueryFirstOrDefaultAsync<StListRecord>(
                $@"SELECT Location1,Location2,Location3,Location4
                FROM {table}
                WHERE MapName = @m AND ListType = @t;",
                new { m = mapName, t = listType.ToString() }
            );

            // Find first location empty slot
            for (int i = 1; i <= 4; i++)
            {
                var val = rec == null
                    ? null
                    : i == 1 ? rec.Location1
                    : i == 2 ? rec.Location2
                    : i == 3 ? rec.Location3
                    :          rec.Location4;

                if (string.IsNullOrEmpty(val))
                {
                    await SaveWorldTextSlotToDb(mapName, listType, i, location, rotation);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> RemoveClosestDbList(string mapName, Vector atPosition, CCSPlayerController player)
        {
            try
            {
                string table = "st_lists";
                var listTypes = new[] { ListType.Points, ListType.Times, ListType.Completions };

                float     bestDist = float.MaxValue;
                ListType? bestType = null;
                int       bestSlot = 0;

                using var conn = CreateDbConnection();

                // Find the closest filled db slot 
                foreach (var type in listTypes)
                {
                    var rec = await conn.QueryFirstOrDefaultAsync<StListRecord>(
                        $"SELECT Location1,Location2,Location3,Location4 FROM {table} WHERE MapName=@m AND ListType=@t;",
                        new { m = mapName, t = type.ToString() }
                    );
                    if (rec == null) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        var raw = i switch {
                            1 => rec.Location1,
                            2 => rec.Location2,
                            3 => rec.Location3,
                            _ => rec.Location4
                        };
                        if (string.IsNullOrWhiteSpace(raw)) 
                            continue;

                        // Split into "X Y Z","P Y R"
                        var parts = raw.Split(',', 2, StringSplitOptions.TrimEntries);
                        if (parts.Length < 1) 
                            continue;

                        // Parse X,Y,Z
                        var coords = parts[0]
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (coords.Length != 3) 
                            continue;
                        if (!float.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var fx) ||
                            !float.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var fy) ||
                            !float.TryParse(coords[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var fz))
                        {
                            continue;
                        }

                        float dist = CheckDistance(fx, fy, fz, atPosition);

                        if (dist < bestDist && dist <= 200f)
                        {
                            bestDist = dist;
                            bestType = type;
                            bestSlot = i;
                        }
                    }
                }

                // Nothing found
                if (bestType == null)
                {
                    Server.NextFrame(() =>
                        player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}No list within 200 units to remove.")
                    );
                    return false;
                }

                // Clear that column in the DB
                await SaveWorldTextSlotToDb(mapName, bestType.Value, bestSlot, null, null);

                // Remove the world text, then refresh all lists
                Server.NextFrame(() =>
                {
                    player.PrintToChat($"{pluginPrefix}{ChatColors.Lime} Removed {bestType} slot #{bestSlot}/4 on {mapName}.");

                    var api = Capability_SharedAPI.Get();
                    if (api != null)
                    {
                        var list = bestType.Value switch
                        {
                            ListType.Points      => _currentPointsList,
                            ListType.Times       => _currentTimesList,
                            ListType.Completions => _currentCompletionsList,
                            _                    => throw new InvalidOperationException()
                        };
                        if (list.Count >= bestSlot)
                        {
                            int msgID = list[bestSlot - 1];
                            api.RemoveWorldText(msgID);
                            list.RemoveAt(bestSlot - 1);
                        }
                    }

                    RefreshLists();
                });

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RemoveNearestListFromDb failed");
                return false;
            }
        }

        private async Task<bool> RemoveClosestJsonList( string mapName, Vector atPosition, CCSPlayerController player)
        {
            try
            {
                var mapsDirectory = Path.Combine(ModuleDirectory, "maps");

                var listTypes = new[] {
                    ListType.Points,
                    ListType.Times,
                    ListType.Completions
                };

                float     bestDist = float.MaxValue;
                ListType? bestType = null;
                int       bestIndex = -1;
                List<WorldTextData>? bestData = null;

                // Check each list type’s JSON for the closest spot
                foreach (var type in listTypes)
                {
                    // Build filename
                    var filename = type switch
                    {
                        ListType.Points      => $"{mapName}_pointslist.json",
                        ListType.Times       => $"{mapName}_timeslist.json",
                        ListType.Completions => $"{mapName}_completionslist.json",
                        _                    => throw new ArgumentException("Invalid list type")
                    };
                    var path = Path.Combine(mapsDirectory, filename);

                    if (!File.Exists(path)) 
                        continue;

                    var fileText = await File.ReadAllTextAsync(path);
                    var data = JsonSerializer.Deserialize<List<WorldTextData>>(fileText);
                    if (data == null || data.Count == 0) 
                        continue;

                    for (int i = 0; i < data.Count; i++)
                    {
                        var raw = data[i].Location;
                        if (string.IsNullOrWhiteSpace(raw)) 
                            continue;

                        // Parse "X Y Z" from the stored string
                        var comps = raw
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (comps.Length != 3) 
                            continue;

                        if (!float.TryParse(comps[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var fx)
                        || !float.TryParse(comps[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var fy)
                        || !float.TryParse(comps[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var fz))
                            continue;

                        float dist = CheckDistance(fx, fy, fz, atPosition);

                        if (dist < bestDist && dist <= 200f)
                        {
                            bestDist  = dist;
                            bestType  = type;
                            bestIndex = i;
                            bestData  = data;
                        }
                    }
                }

                // Nothing close enough
                if (bestType == null || bestData == null)
                {
                    Server.NextFrame(() =>
                        player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}No list within 200 units to remove.")
                    );
                    return false;
                }

                // Remove that entry and rewrite the JSON
                var removeFilename = bestType switch
                {
                    ListType.Points      => $"{mapName}_pointslist.json",
                    ListType.Times       => $"{mapName}_timeslist.json",
                    ListType.Completions => $"{mapName}_completionslist.json",
                    _                    => throw new InvalidOperationException()
                };

                var removePath = Path.Combine(Path.Combine(ModuleDirectory, "maps"), removeFilename);

                bestData.RemoveAt(bestIndex);
                string newJson = JsonSerializer.Serialize(bestData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(removePath, newJson);

                Server.NextFrame(() =>
                {
                    player.PrintToChat(
                        $"{pluginPrefix} Removed {bestType.Value} slot #{bestIndex+1} on {mapName}."
                    );

                    var api = Capability_SharedAPI.Get();
                    if (api != null)
                    {
                        var memList = bestType.Value switch
                        {
                            ListType.Points      => _currentPointsList,
                            ListType.Times       => _currentTimesList,
                            ListType.Completions => _currentCompletionsList,
                            _                    => throw new InvalidOperationException()
                        };

                        // Remove the corresponding text entity
                        if (memList.Count > bestIndex)
                        {
                            int msgID = memList[bestIndex];
                            api.RemoveWorldText(msgID);
                            memList.RemoveAt(bestIndex);
                        }
                    }

                    RefreshLists();
                });

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error removing list in RemoveNearestListFromFile.");
                return false;
            }
        }

        private void LoadWorldTextFromFile(string path, ListType listType, string mapName)
        {
            if (File.Exists(path))
            {
                var data = JsonSerializer.Deserialize<List<WorldTextData>>(File.ReadAllText(path));
                if (data == null) return;

                Task.Run(async () =>
                {
                    try
                    {
                        int topCount = listType switch
                        {
                            ListType.Points         => List.PointsCount,
                            ListType.Times          => List.TimesCount,
                            ListType.Completions    => List.CompletionsCount,
                            _                       => throw new ArgumentException("Invalid list type")
                        };

                        var topList = await GetTopPlayersAsync(topCount, listType, mapName);
                        var linesList = GetTopListTextLines(topList, listType);

                        Server.NextWorldUpdate(() =>
                        {
                            try
                            {
                                var checkAPI = Capability_SharedAPI.Get();
                                if (checkAPI is null) return;

                                foreach (var worldTextData in data)
                                {
                                    if (!string.IsNullOrEmpty(worldTextData.Location) && !string.IsNullOrEmpty(worldTextData.Rotation))
                                    {
                                        var messageID = checkAPI.AddWorldText(TextPlacement.Wall, linesList, ParseVector(worldTextData.Location), ParseQAngle(worldTextData.Rotation));
                                        if (listType == ListType.Points)
                                            _currentPointsList.Add(messageID);
                                        else if (listType == ListType.Times)
                                            _currentTimesList.Add(messageID);
                                        else if (listType == ListType.Completions)
                                            _currentCompletionsList.Add(messageID);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "Error during NextWorldUpdate in LoadWorldTextFromFile.");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error loading world text from file in LoadWorldTextFromFile method.");
                    }
                });
            }
        }

        private void LoadWorldTextFromDb(string mapName, ListType listType)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Determine how many entries we need
                    int topCount = listType switch
                    {
                        ListType.Points       => List.PointsCount,
                        ListType.Times        => List.TimesCount,
                        ListType.Completions  => List.CompletionsCount,
                        _                     => throw new ArgumentException("Invalid list type")
                    };

                    // Fetch the data
                    var topList   = await GetTopPlayersAsync(topCount, listType, mapName);
                    var linesList = GetTopListTextLines(topList, listType);

                    // Pull our four slots from the DB
                    string table = "st_lists";
                    using var conn = CreateDbConnection();
                    var rec = await conn.QueryFirstOrDefaultAsync<StListRecord>(
                        $"SELECT Location1, Location2, Location3, Location4 " +
                        $"FROM {table} " +
                        $"WHERE MapName = @m AND ListType = @t;",
                        new { m = mapName, t = listType.ToString() }
                    );

                    if (rec == null) 
                        return;

                    // Spawn them on the next tick
                    Server.NextWorldUpdate(() =>
                    {
                        try
                        {
                            var api = Capability_SharedAPI.Get();
                            if (api is null) return;

                            // Iterate slots 1–4
                            for (int i = 1; i <= 4; i++)
                            {
                                var raw = i switch
                                {
                                    1 => rec.Location1,
                                    2 => rec.Location2,
                                    3 => rec.Location3,
                                    _ => rec.Location4
                                };
                                if (string.IsNullOrEmpty(raw))
                                    continue;

                                var parts = raw.Split(',');
                                var loc   = ParseVector(parts[0]);
                                var rot   = ParseQAngle(parts[1]);

                                var messageID = api.AddWorldText(TextPlacement.Wall, linesList, loc, rot);
                                if (listType == ListType.Points)      _currentPointsList.Add(messageID);
                                else if (listType == ListType.Times)  _currentTimesList.Add(messageID);
                                else if (listType == ListType.Completions) _currentCompletionsList.Add(messageID);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"Error during NextWorldUpdate in LoadWorldTextFromDb for {listType}.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error loading world text from database in LoadWorldTextFromDb for {listType}.");
                }
            });
        }

        private List<TextLine> GetTopListTextLines(List<PlayerPlace> topList, ListType listType)
        {
            Color ParseColor(string colorName)
            {
                try
                {
                    var colorProperty = typeof(Color).GetProperty(colorName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) ?? throw new ArgumentException($"Invalid color name: {colorName}");
                    var colorValue = colorProperty.GetValue(null) ?? throw new InvalidOperationException($"Color property '{colorName}' has no value.");
                    return (Color)colorValue;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Invalid color name: {colorName}. Falling back to White.");
                    return Color.White;
                }
            }

            PointWorldTextJustifyHorizontal_t GetTextAlignment(ListType listType)
            {
                string alignment = listType switch
                {
                    ListType.Points         => List.PointsTextAlignment.ToLower(),
                    ListType.Times          => List.TimesTextAlignment.ToLower(),
                    ListType.Completions    => List.CompletionsTextAlignment.ToLower(),
                    _                       => "center"
                };

                return alignment switch
                {
                    "left"      => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT,
                    "center"    => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER,
                    _           => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER
                };
            }

            int maxNameLength = Text.MaxNameLength;
            var linesList = new List<TextLine>();

            if (listType == ListType.Points)
            {
                linesList.Add(new TextLine
                {
                    Text = List.PointsTitleText,
                    Color = ParseColor(Text.TitleTextColor),
                    FontSize = Text.TitleFontSize,
                    FullBright = true,
                    Scale = Text.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }
            else if (listType == ListType.Times)
            {
                linesList.Add(new TextLine
                {
                    Text = List.TimesTitleText,
                    Color = ParseColor(Text.TitleTextColor),
                    FontSize = Text.TitleFontSize,
                    FullBright = true,
                    Scale = Text.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }
            else if (listType == ListType.Completions)
            {
                linesList.Add(new TextLine
                {
                    Text = List.CompletionsTitleText,
                    Color = ParseColor(Text.TitleTextColor),
                    FontSize = Text.TitleFontSize,
                    FullBright = true,
                    Scale = Text.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }

            for (int i = 0; i < topList.Count; i++)
            {
                var topplayer = topList[i];
                var truncatedName = TruncateString(topplayer.PlayerName, maxNameLength);
                var color = i switch
                {
                    0 => ParseColor(Text.FirstPlaceColor),
                    1 => ParseColor(Text.SecondPlaceColor),
                    2 => ParseColor(Text.ThirdPlaceColor),
                    _ => ParseColor(Text.DefaultColor)
                };

                    var pointsOrTimeOrCompletions = listType switch
                    {
                        ListType.Points         => topplayer.GlobalPoints.ToString(),
                        ListType.Times          => FormatTime(topplayer.TimerTicks),
                        ListType.Completions    => topplayer.Completions.ToString(),
                        _                       => string.Empty
                    };
                var lineText = $"{i + 1}. {truncatedName} - {pointsOrTimeOrCompletions}";

                linesList.Add(new TextLine
                {
                    Text = lineText,
                    Color = color,
                    FontSize = Text.ListFontSize,
                    FullBright = true,
                    Scale = Text.ListTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }

            return linesList;
        }

        private void RefreshLists()
        {
            var mapName = Server.MapName;

            Server.NextWorldUpdate(() =>
            {
                var api = Capability_SharedAPI.Get();
                if (api == null) return;

                // Remove every existing world-text
                foreach (var id in _currentPointsList)      api.RemoveWorldText(id);
                foreach (var id in _currentTimesList)       api.RemoveWorldText(id);
                foreach (var id in _currentCompletionsList) api.RemoveWorldText(id);

                // Clear trackers
                _currentPointsList.Clear();
                _currentTimesList.Clear();
                _currentCompletionsList.Clear();

                // Reload based on save location
                if (Config.SaveToDb)
                    LoadWorldTextFromDb(mapName);
                else
                    LoadWorldTextFromJson(mapName);
            });
        }

        private void UpdateLists()
        {
            var mapName = Server.MapName;
            
            Task.Run(async () =>
            {
                try
                {
                    var timesTopList       = await GetTopPlayersAsync(List.TimesCount,       ListType.Times,       mapName);
                    var pointsTopList      = await GetTopPlayersAsync(List.PointsCount,      ListType.Points,      mapName);
                    var completionsTopList = await GetTopPlayersAsync(List.CompletionsCount, ListType.Completions, mapName);

                    var pointsLinesList      = GetTopListTextLines(pointsTopList,      ListType.Points);
                    var timesLinesList       = GetTopListTextLines(timesTopList,       ListType.Times);
                    var completionsLinesList = GetTopListTextLines(completionsTopList, ListType.Completions);

                    Server.NextWorldUpdate(() =>
                    {
                        try
                        {
                            var api = Capability_SharedAPI.Get();
                            if (api is null) return;

                            _currentPointsList     .ForEach(id => api.UpdateWorldText(id, pointsLinesList));
                            _currentTimesList      .ForEach(id => api.UpdateWorldText(id, timesLinesList));
                            _currentCompletionsList.ForEach(id => api.UpdateWorldText(id, completionsLinesList));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error during NextWorldUpdate in UpdateLists.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error refreshing lists in UpdateLists method.");
                }
            });
        }

        private void LoadWorldTextFromDb(string? passedMapName = null)
        {
            var mapName = passedMapName ?? Server.MapName;
            LoadWorldTextFromDb(mapName, ListType.Points);
            LoadWorldTextFromDb(mapName, ListType.Times);
            LoadWorldTextFromDb(mapName, ListType.Completions);
        }

        private void LoadWorldTextFromJson(string? passedMapName = null)
        {
            var mapName = passedMapName ?? Server.MapName;
            var mapsDirectory = Path.Combine(ModuleDirectory, "maps");

            var pointsPath = Path.Combine(mapsDirectory, $"{mapName}_pointslist.json");
            var timesPath = Path.Combine(mapsDirectory, $"{mapName}_timeslist.json");
            var completionsPath = Path.Combine(mapsDirectory, $"{mapName}_completionslist.json");

            LoadWorldTextFromFile(pointsPath, ListType.Points, mapName);
            LoadWorldTextFromFile(timesPath, ListType.Times, mapName);
            LoadWorldTextFromFile(completionsPath, ListType.Completions, mapName);
        }

        public static Vector ParseVector(string vectorString)
        {
            string[] components = vectorString.Split(' ');
            if (components.Length == 3 &&
                float.TryParse(components[0], out float x) &&
                float.TryParse(components[1], out float y) &&
                float.TryParse(components[2], out float z))
            {
                return new Vector(x, y, z);
            }
            throw new ArgumentException("Invalid vector string format.");
        }

        public static QAngle ParseQAngle(string qangleString)
        {
            string[] components = qangleString.Split(' ');
            if (components.Length == 3 &&
                float.TryParse(components[0], out float x) &&
                float.TryParse(components[1], out float y) &&
                float.TryParse(components[2], out float z))
            {
                return new QAngle(x, y, z);
            }
            throw new ArgumentException("Invalid QAngle string format.");
        }

        private static float CheckDistance(float x, float y, float z, Vector to)
        {
            float dx = x - to.X;
            float dy = y - to.Y;
            float dz = z - to.Z;
            return MathF.Sqrt(dx*dx + dy*dy + dz*dz);
        }

        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }

        public static string FormatTime(int ticks)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(ticks / 64.0);

            string milliseconds = $"{ticks % 64 * (1000.0 / 64.0):000}";

            int totalMinutes = (int)timeSpan.TotalMinutes;
            if (totalMinutes >= 60)
            {
                return $"{totalMinutes / 60:D1}:{totalMinutes % 60:D2}:{timeSpan.Seconds:D2}.{milliseconds}";
            }

            return $"{totalMinutes:D1}:{timeSpan.Seconds:D2}.{milliseconds}";
        }
    }

    public enum ListType
    {
        Points,
        Times,
        Completions
    }

    public class PlayerPlace
    {
        public required string PlayerName { get; set; }
        public int GlobalPoints { get; set; }
        public int TimerTicks { get; set; }
        public int Completions { get; set; }
    }

    public class WorldTextData
    {
        public required string Location { get; set; }
        public required string Rotation { get; set; }
    }

    public class StListRecord
    {
        public string? Location1 { get; set; }
        public string? Location2 { get; set; }
        public string? Location3 { get; set; }
        public string? Location4 { get; set; }
    }

    struct ImportEntry
    {
        public string MapName;
        public ListType Type;
        public float X, Y, Z;
        public float Pitch, Yaw, Roll;
    }
}