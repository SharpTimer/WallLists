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
using Microsoft.Extensions.Logging;

namespace SharpTimerWallLists
{
    [MinimumApiVersion(288)]
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "SharpTimer Wall Lists";
        public override string ModuleAuthor => "Marchand";
        public override string ModuleVersion => "1.0.5";

        public required PluginConfig Config { get; set; } = new PluginConfig();
        public static PluginCapability<IK4WorldTextSharedAPI> Capability_SharedAPI { get; } = new("k4-worldtext:sharedapi");
        private CounterStrikeSharp.API.Modules.Timers.Timer? _updateTimer;
        private List<int> _currentPointsList = new();
        private List<int> _currentTimesList = new();
        private List<int> _currentCompletionsList = new();

        public string pluginPrefix = $" {ChatColors.Magenta}[{ChatColors.LightPurple}Wall-Lists{ChatColors.Magenta}]";

        public void OnConfigParsed(PluginConfig config)
        {
            if (config.Version < Config.Version)
                Logger.LogWarning("Configuration version mismatch (Expected: {0} | Current: {1})", Config.Version, config.Version);

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

            AddTimer(3, () => LoadWorldTextFromFile(Server.MapName));

            AddCommand($"css_{Config.TimesListCommand}", "Sets up the map times list", OnMapListAdd);
            AddCommand($"css_{Config.PointsListCommand}", "Sets up the points list", OnPointsListAdd);
            AddCommand($"css_{Config.CompletionsListCommand}", "Sets up the map completions list", OnCompletionsListAdd);
            AddCommand($"css_{Config.RemoveListCommand}", "Removes the closest list (100 units max)", OnRemoveList);
            AddCommand($"css_{Config.ReloadConfigCommand}", "Reloads the config file", ReloadConfigCommand);
            AddCommand($"css_{Config.UpdateConfigCommand}", "Updates the config to the latest version", UpdateConfigCommand);

            if (Config.TimeBasedUpdate)
            {
                _updateTimer = AddTimer(Config.UpdateInterval, RefreshLists, TimerFlags.REPEAT);
            }

            RegisterEventHandler((EventRoundStart @event, GameEventInfo info) =>
            {
                RefreshLists();
                return HookResult.Continue;
            });

            RegisterListener<Listeners.OnMapStart>((mapName) =>
            {
                AddTimer(1, () => LoadWorldTextFromFile(mapName));
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
                    int topCount = listType switch
                    {
                        ListType.Points => Config.PointsCount,
                        ListType.Times => Config.TimesCount,
                        ListType.Completions => Config.CompletionsCount,
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
                                    SaveWorldTextToFile(location, rotation, listType);
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

        private void RemoveClosestList(CCSPlayerController player, CommandInfo command)
        {
            var checkAPI = Capability_SharedAPI.Get();
            if (checkAPI is null)
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}Failed to get the shared K4-API.");
                return;
            }

            var combinedList = _currentPointsList.Concat(_currentTimesList).Concat(_currentCompletionsList).ToList();

            var target = combinedList
                .SelectMany(id => checkAPI.GetWorldTextLineEntities(id)?.Select(entity => new 
                { 
                    Id = id, 
                    Entity = entity, 
                    IsPointsList = _currentPointsList.Contains(id),
                    IsTimesList = _currentTimesList.Contains(id),
                    IsCompletionsList = _currentCompletionsList.Contains(id)
                }) ?? Enumerable.Empty<dynamic>())
                .Where(x => x.Entity.AbsOrigin != null && player.PlayerPawn.Value?.AbsOrigin != null && DistanceTo(x.Entity.AbsOrigin, player.PlayerPawn.Value!.AbsOrigin) < 100)
                .OrderBy(x => DistanceTo(x.Entity.AbsOrigin, player.PlayerPawn.Value!.AbsOrigin))
                .FirstOrDefault();

            if (target is null)
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}Move closer to the list that you want to remove.");
                return;
            }

            try
            {
                checkAPI.RemoveWorldText(target.Id, false);

                if (target.IsPointsList)
                {
                    _currentPointsList.Remove(target.Id);
                }
                else if (target.IsTimesList)
                {
                    _currentTimesList.Remove(target.Id);
                }
                else if (target.IsCompletionsList)
                {
                    _currentCompletionsList.Remove(target.Id);
                }

                var mapName = Server.MapName;
                var mapsDirectory = Path.Combine(ModuleDirectory, "maps");
                var path = target.IsPointsList
                    ? Path.Combine(mapsDirectory, $"{mapName}_pointslist.json")
                    : target.IsTimesList
                        ? Path.Combine(mapsDirectory, $"{mapName}_timeslist.json")
                        : Path.Combine(mapsDirectory, $"{mapName}_completionslist.json");

                if (File.Exists(path))
                {
                    var data = JsonSerializer.Deserialize<List<WorldTextData>>(File.ReadAllText(path));
                    if (data != null)
                    {
                        Vector entityVector = target.Entity.AbsOrigin;
                        data.RemoveAll(x =>
                        {
                            Vector location = ParseVector(x.Location);
                            return location.X == entityVector.X &&
                                location.Y == entityVector.Y &&
                                x.Rotation == target.Entity.AbsRotation.ToString();
                        });

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };

                        string jsonString = JsonSerializer.Serialize(data, options);
                        File.WriteAllText(path, jsonString);
                    }
                }
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Green}List removed!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error removing list in RemoveClosestList method.");
            }
        }

        private float DistanceTo(Vector a, Vector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
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
                    ListType.Times => $"{mapName}_timeslist.json",
                    ListType.Points => $"{mapName}_pointslist.json",
                    ListType.Completions => $"{mapName}_completionslist.json",
                    _ => throw new ArgumentException("Invalid list type")
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
                            ListType.Points => Config.PointsCount,
                            ListType.Times => Config.TimesCount,
                            ListType.Completions => Config.CompletionsCount,
                            _ => throw new ArgumentException("Invalid list type")
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

        private void LoadWorldTextFromFile(string? passedMapName = null)
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

        private void RefreshLists()
        {
            var mapName = Server.MapName;
            
            Task.Run(async () =>
            {
                try
                {
                    var timesTopList = await GetTopPlayersAsync(Config.TimesCount, ListType.Times, mapName);
                    var pointsTopList = await GetTopPlayersAsync(Config.PointsCount, ListType.Points, mapName);
                    var completionsTopList = await GetTopPlayersAsync(Config.CompletionsCount, ListType.Completions, mapName);

                    var pointsLinesList = GetTopListTextLines(pointsTopList, ListType.Points);
                    var timesLinesList = GetTopListTextLines(timesTopList, ListType.Times);
                    var completionsLinesList = GetTopListTextLines(completionsTopList, ListType.Completions);

                    Server.NextWorldUpdate(() =>
                    {
                        try
                        {
                            var checkAPI = Capability_SharedAPI.Get();
                            if (checkAPI != null)
                            {
                                _currentPointsList.ForEach(id => checkAPI.UpdateWorldText(id, pointsLinesList));
                                _currentTimesList.ForEach(id => checkAPI.UpdateWorldText(id, timesLinesList));
                                _currentCompletionsList.ForEach(id => checkAPI.UpdateWorldText(id, completionsLinesList));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error during NextWorldUpdate in RefreshLists.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error refreshing lists in RefreshLists method.");
                }
            });
        }

        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private List<TextLine> GetTopListTextLines(List<PlayerPlace> topList, ListType listType)
        {
            Color ParseColor(string colorName)
            {
                try
                {
                    var colorProperty = typeof(Color).GetProperty(colorName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (colorProperty == null)
                    {
                        throw new ArgumentException($"Invalid color name: {colorName}");
                    }

                    var colorValue = colorProperty.GetValue(null);
                    if (colorValue == null)
                    {
                        throw new InvalidOperationException($"Color property '{colorName}' has no value.");
                    }

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
                    ListType.Points => Config.PointsTextAlignment.ToLower(),
                    ListType.Times => Config.TimesTextAlignment.ToLower(),
                    ListType.Completions => Config.CompletionsTextAlignment.ToLower(),
                    _ => "center"
                };

                return alignment switch
                {
                    "left" => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT,
                    "center" => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER,
                    _ => PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER
                };
            }

            int maxNameLength = Config.MaxNameLength;
            var linesList = new List<TextLine>();

            if (listType == ListType.Points)
            {
                linesList.Add(new TextLine
                {
                    Text = Config.PointsTitleText,
                    Color = ParseColor(Config.TitleTextColor),
                    FontSize = Config.TitleFontSize,
                    FullBright = true,
                    Scale = Config.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }
            else if (listType == ListType.Times)
            {
                linesList.Add(new TextLine
                {
                    Text = Config.TimesTitleText,
                    Color = ParseColor(Config.TitleTextColor),
                    FontSize = Config.TitleFontSize,
                    FullBright = true,
                    Scale = Config.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }
            else if (listType == ListType.Completions)
            {
                linesList.Add(new TextLine
                {
                    Text = Config.CompletionsTitleText,
                    Color = ParseColor(Config.TitleTextColor),
                    FontSize = Config.TitleFontSize,
                    FullBright = true,
                    Scale = Config.TitleTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }

            for (int i = 0; i < topList.Count; i++)
            {
                var topplayer = topList[i];
                var truncatedName = TruncateString(topplayer.PlayerName, maxNameLength);
                var color = i switch
                {
                    0 => ParseColor(Config.FirstPlaceColor),
                    1 => ParseColor(Config.SecondPlaceColor),
                    2 => ParseColor(Config.ThirdPlaceColor),
                    _ => ParseColor(Config.DefaultColor)
                };

                    var pointsOrTimeOrCompletions = listType switch
                    {
                        ListType.Points => topplayer.GlobalPoints.ToString(),
                        ListType.Times => FormatTime(topplayer.TimerTicks),
                        ListType.Completions => topplayer.Completions.ToString(),
                        _ => string.Empty
                    };
                var lineText = $"{i + 1}. {truncatedName} - {pointsOrTimeOrCompletions}";

                linesList.Add(new TextLine
                {
                    Text = lineText,
                    Color = color,
                    FontSize = Config.ListFontSize,
                    FullBright = true,
                    Scale = Config.ListTextScale,
                    JustifyHorizontal = GetTextAlignment(listType)

                });
            }

            return linesList;
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
}
