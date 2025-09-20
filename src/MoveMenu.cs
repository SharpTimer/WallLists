using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using Dapper;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SharpTimerWallLists
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        private static Vector ZeroVec() => new Vector(0, 0, 0);
        private static QAngle ZeroAng() => new QAngle(0, 0, 0);

        private sealed class DbListSlotRaw
        {
            public ListType Type { get; init; }
            public int Slot { get; init; }
            public string Loc { get; init; } = ""; // X Y Z
            public string Ang { get; init; } = ""; // P Y R
        }

        private sealed class DbListSlot
        {
            public ListType Type { get; init; }
            public int Slot { get; init; } // E.g. 1-4
            public required Vector Pos { get; set; }
            public required QAngle Ang { get; set; }
            public string Raw { get; init; } = "";
            public string Label => $"{Type} #{Slot}";
        }

        // Called by !walllists
        private void Command_ListsMove(CCSPlayerController? player, CommandInfo _)
        {
            if (player is null || !player.IsValid)
                    return;
            
            if (!AdminManager.PlayerHasPermissions(player, Permission))
            {
                player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}You do not have the correct permission to execute this command.");
                return;
            }

            if (!_hasMenuManager)
            {
                player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}CS2MenuManager API not found! Move menu command is not available.");
                return;
            }

            if (!Config.SaveToDb)
            {
                player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed} This tool currently supports DB-saved lists only. Enable SaveToDb in config.");
                return;
            }

            if (string.IsNullOrEmpty(Server.MapName))
            {
                player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed} Current map is unknown.");
                return;
            }

            ShowListsRootMenu(player);
        }

        private BaseMenu CreateMenu(string title)
        {
            try
            {
                // Options: WasdMenu/ChatMenu/CenterHtmlMenu/ConsoleMenu/PlayerMenu
                return MenuManager.MenuByType(Config.MenuType, title, this);
            }
            catch // Fallback
            {
                return MenuManager.MenuByType(typeof(WasdMenu), title, this);
            }
        }

        // Level 1: Shows all lists for this map
        private void ShowListsRootMenu(CCSPlayerController player)
        {
            var map = Server.MapName;

            _ = Task.Run(async () =>
            {
                List<DbListSlotRaw> raw;
                try
                {
                    raw = await GetAllDbSlotsForMapRaw(map);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[Wall-Lists] Failed to load DB slots for menu.");
                    Server.NextFrame(() => player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}Failed to load lists from DB."));
                    return;
                }

                Server.NextFrame(() =>
                {
                    var slots = new List<DbListSlot>(raw.Count);
                    foreach (var r in raw)
                    {
                        if (!TryParseVector(r.Loc, out var pos)) continue;
                        if (!TryParseAngles(r.Ang, out var ang)) continue;

                        slots.Add(new DbListSlot
                        {
                            Type = r.Type,
                            Slot = r.Slot,
                            Pos  = pos,
                            Ang  = ang
                        });
                    }

                    if (slots.Count == 0)
                    {
                        player.PrintToChat($"{pluginPrefix} {ChatColors.LightRed}No lists found for this map.");
                        return;
                    }

                    var root = CreateMenu($"SharpTimer Wall Lists");
                    foreach (var slot in slots)
                    {
                        var captured = slot;
                        var opt = root.AddItem(captured.Label, (p, opt) =>
                        {
                            ShowEditMenu(p, captured, root);
                        });
                        opt.PostSelectAction = PostSelectAction.Nothing;
                    }
                    root.Display(player, 0);
                });
            });
        }

        // Level 2: Choose "Change Location" or "Change Angle" for selected list
        private void ShowEditMenu(CCSPlayerController player, DbListSlot slot, IMenu prev)
        {
            var edit = CreateMenu($"Edit {slot.Type} #{slot.Slot}");
            edit.PrevMenu = prev;

            var loc = edit.AddItem("Change Location", (p, _) => ShowMoveLocationMenu(p, slot, edit));
            loc.PostSelectAction = PostSelectAction.Nothing;

            var ang = edit.AddItem("Change Angle", (p, _) => ShowMoveAngleMenu(p, slot, edit));
            ang.PostSelectAction = PostSelectAction.Nothing;

            edit.Display(player, 0);
        }

        // Level 3a: Move location in 6 directions (-/+ 5 units)
        private void ShowMoveLocationMenu(CCSPlayerController player, DbListSlot slot, IMenu prev)
        {
            const float step = 5f;

            var m = CreateMenu($"Move {slot.Type} #{slot.Slot}");
            m.PrevMenu = prev;

            AddMove(m, "(Y - 5)", slot, new Vector(0, -step, 0), ZeroAng());
            AddMove(m, "(Y + 5)", slot, new Vector(0, +step, 0), ZeroAng());
            AddMove(m, "(X + 5)", slot, new Vector(+step, 0, 0), ZeroAng());
            AddMove(m, "(X - 5)", slot, new Vector(-step, 0, 0), ZeroAng());
            AddMove(m, "Move Up    (Z + 5)", slot, new Vector(0, 0, +step), ZeroAng());
            AddMove(m, "Move Down  (Z - 5)", slot, new Vector(0, 0, -step), ZeroAng());

            m.Display(player, 0);
        }

        // Level 3b: Change angles in 6 directions (-/+ 5 degrees)
        private void ShowMoveAngleMenu(CCSPlayerController player, DbListSlot slot, IMenu prev)
        {
            const float step = 5f;

            var m = CreateMenu($"Rotate {slot.Type} #{slot.Slot}");
            m.PrevMenu = prev;

            AddMove(m, "Pitch +5", slot, ZeroVec(), new QAngle(+step, 0, 0));
            AddMove(m, "Pitch -5", slot, ZeroVec(), new QAngle(-step, 0, 0));
            AddMove(m, "Yaw +5", slot, ZeroVec(), new QAngle(0, +step, 0));
            AddMove(m, "Yaw -5", slot, ZeroVec(), new QAngle(0, -step, 0));
            AddMove(m, "Roll +5", slot, ZeroVec(), new QAngle(0, 0, +step));
            AddMove(m, "Roll -5", slot, ZeroVec(), new QAngle(0, 0, -step));

            m.Display(player, 0);
        }

        private void AddMove(BaseMenu menu, string text, DbListSlot slot, Vector dPos, QAngle dAng)
        {
            var item = menu.AddItem(text, (p, opt) =>
            {
                // Record the current selection index
                int idx = menu.ItemOptions.IndexOf(opt);

                var newPos = new Vector(slot.Pos.X + dPos.X, slot.Pos.Y + dPos.Y, slot.Pos.Z + dPos.Z);
                var newAng = new QAngle(slot.Ang.X + dAng.X, slot.Ang.Y + dAng.Y, slot.Ang.Z + dAng.Z);

                var mapName = Server.MapName;

                _ = Task.Run(async () =>
                {
                    try { await SaveWorldTextSlotToDb(mapName, slot.Type, slot.Slot, newPos, newAng); }
                    catch (Exception ex) { Logger.LogError(ex, "[Wall-Lists] SaveWorldTextSlotToDb failed."); }
                });

                slot.Pos = newPos;
                slot.Ang = newAng;

                Server.NextWorldUpdate(() => RefreshLists());

                // Re-display at the same selection option
                if (text.StartsWith("Move", StringComparison.OrdinalIgnoreCase))
                    menu.Title = $"Move {slot.Type} #{slot.Slot}";
                else
                    menu.Title = $"Rotate {slot.Type} #{slot.Slot}";

                Server.NextFrame(() => menu.DisplayAt(p, idx, menu.MenuTime));
            });

            item.PostSelectAction = PostSelectAction.Nothing;
        }

        // Build a list of all existing lists across Points/Times/Completions
        private async Task<List<DbListSlotRaw>> GetAllDbSlotsForMapRaw(string map)
        {
            var result = new List<DbListSlotRaw>();

            foreach (var lt in new[] { ListType.Points, ListType.Times, ListType.Completions })
            {
                var rec = await QueryStListsRowAsync(map, lt);
                if (rec is null) continue;

                string?[] cols = [ rec.Location1, rec.Location2, rec.Location3, rec.Location4 ];
                for (int i = 0; i < cols.Length; i++)
                {
                    var raw = cols[i];
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    int comma = raw.IndexOf(',');
                    if (comma <= 0 || comma >= raw.Length - 1) continue;

                    var loc = raw[..comma].Trim();
                    var ang = raw[(comma + 1)..].Trim();

                    result.Add(new DbListSlotRaw
                    {
                        Type = lt,
                        Slot = i + 1,
                        Loc = loc,
                        Ang = ang
                    });
                }
            }

            return result;
        }

        private async Task<StListRecord?> QueryStListsRowAsync(string map, ListType type)
        {
            string table = $"{Config.DatabaseSettings.TablePrefix}st_lists";
            using var connection = CreateDbConnection();
            return await connection.QueryFirstOrDefaultAsync<StListRecord>(
                $"SELECT Location1, Location2, Location3, Location4 FROM {table} WHERE MapName = @m AND ListType = @t;",
                new { m = map, t = type.ToString() }
            );
        }  

        private static bool TryParseVector(string input, out Vector result)
        {
            result = new Vector(0, 0, 0);
            if (string.IsNullOrWhiteSpace(input)) return false;

            var parts = input.Trim()
                            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return false;

            if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                result = new Vector(x, y, z);
                return true;
            }

            return false;
        }

        private static bool TryParseAngles(string input, out QAngle result)
        {
            result = new QAngle(0, 0, 0);
            if (string.IsNullOrWhiteSpace(input)) return false;

            var parts = input.Trim()
                            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return false;

            if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var pitch) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var yaw) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var roll))
            {
                result = new QAngle(pitch, yaw, roll);
                return true;
            }

            return false;
        }
    }
}