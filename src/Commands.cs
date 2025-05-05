using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SharpTimerWallLists
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public void OnPointsListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Points);
        }

        public void OnTimesListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Times);
        }

        public void OnCompletionsListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Completions);
        }

        public void OnRemoveList(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            var pawn   = player.PlayerPawn?.Value;
            var atPos  = pawn?.AbsOrigin ?? player.AbsOrigin!;

            if (Config.SaveToDb)
                _ = RemoveClosestDbList(Server.MapName, atPos, player);
            else
                _ = RemoveClosestJsonList(Server.MapName, atPos, player);
        }

        // Used to import existing json lists into the database
        public void OnImportLists(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;
            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have permission to execute this command.");
                return;
            }

            _ = Task.Run(async () =>
            {
                var mapsDir = Path.Combine(ModuleDirectory, "maps");
                if (!Directory.Exists(mapsDir))
                {
                    Server.NextFrame(() =>
                        command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}No maps folder found at {mapsDir}.")
                    );
                    return;
                }

                var importQueue   = new List<ImportEntry>();
                int filesTouched  = 0;

                foreach (var filePath in Directory.GetFiles(mapsDir, "*.json"))
                {
                    var fileName = Path.GetFileName(filePath);

                    // Check list type
                    if (!fileName.EndsWith("pointslist.json",      StringComparison.OrdinalIgnoreCase) &&
                        !fileName.EndsWith("timeslist.json",       StringComparison.OrdinalIgnoreCase) &&
                        !fileName.EndsWith("completionslist.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    filesTouched++;

                    // Get mapName (strip the suffix)
                    ListType type = fileName.EndsWith("pointslist.json", StringComparison.OrdinalIgnoreCase)
                                ? ListType.Points
                                : fileName.EndsWith("timeslist.json", StringComparison.OrdinalIgnoreCase)
                                    ? ListType.Times
                                    : ListType.Completions;
                    string suffix = type == ListType.Points ? "_pointslist.json"
                                    : type == ListType.Times ? "_timeslist.json"
                                    : "_completionslist.json";
                    string mapName = fileName[..^suffix.Length];

                    // Read & deserialize
                    string json = await File.ReadAllTextAsync(filePath);
                    var data = JsonSerializer.Deserialize<List<WorldTextData>>(json);
                    if (data == null || data.Count == 0) 
                        continue;

                    if (data.Count > 4)
                        Logger.LogWarning($"\"{fileName}\" has more than 4 locations (database limit); some lists weren’t imported.");

                    // Take up to 4, parse them
                    foreach (var entry in data.Take(4))
                    {
                        // Strip grouping commas so TryParse works
                        var locRaw = entry.Location.Replace(",", "");
                        var parts  = locRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (parts.Length != 3) 
                            continue;

                        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var fx) ||
                            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var fy) ||
                            !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var fz))
                            continue;

                        var rotRaw = entry.Rotation.Trim();
                        var rparts = rotRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (rparts.Length != 3) 
                            continue;
                        if (!float.TryParse(rparts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var px) ||
                            !float.TryParse(rparts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var py) ||
                            !float.TryParse(rparts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var pr))
                            continue;

                        importQueue.Add(new ImportEntry { MapName = mapName, Type = type, X = fx, Y = fy, Z = fz, Pitch = px, Yaw = py, Roll = pr });
                    }
                }

                // Construct Vectors/QAngles
                Server.NextFrame(() =>
                {
                    player.PrintToChat($"{pluginPrefix} {ChatColors.Lime}Queued {importQueue.Count} entries from {filesTouched} files for import.");
                    foreach (var e in importQueue)
                    {
                        var loc = new Vector(e.X, e.Y, e.Z);
                        var rot = new QAngle(e.Pitch, e.Yaw, e.Roll);
                        _ = SaveFirstAvailableSlotToDb(e.MapName, e.Type, loc, rot);
                    }

                    // Show the imported lists in the world, and notify of completion
                    Server.NextFrame(() =>
                    {
                        RefreshLists();
                        UpdateLists();
                        player.PrintToChat($"{pluginPrefix} {ChatColors.Lime}Database import completed!");
                    });
                });
            });
        }

        public void ReloadConfigCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (player != null && !AdminManager.PlayerHasPermissions(player, Permission))
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }
            
            try
            {
                Config.Reload();
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Lime}Configuration reloaded successfully!");
            }
            catch (Exception ex)
            {
                command?.ReplyToCommand($"Failed to reload configuration: {ex.Message}");
            }
        }

        public void UpdateConfigCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (player != null && !AdminManager.PlayerHasPermissions(player, Permission))
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            try
            {
                Config.Update();
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Lime}Configuration updated successfully!");
            }
            catch (Exception)
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.LightRed}Failed to update configuration.");
            }
        } 
    }
}