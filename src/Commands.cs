using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;


namespace SharpTimerWallLists
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public void OnPointsListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Points);
        }

        public void OnMapListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Times);
        }

        public void OnCompletionsListAdd(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
                return;
            }

            CreateTopList(player, command, ListType.Completions);
        }

        public void OnRemoveList(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || command == null) return;

            if (!AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
                return;
            }

            RemoveClosestList(player, command);
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Server.NextWorldUpdate(() => RemoveClosestList(player, command));
            });
        }

        public void ReloadConfigCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
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
            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.CommandPermission))
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}You do not have the correct permission to execute this command.");
                return;
            }

            try
            {
                Config.Update();
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Lime}Configuration updated successfully!");
            }
            catch (Exception)
            {
                command?.ReplyToCommand($"{pluginPrefix} {ChatColors.Red}Failed to update configuration.");
            }
        } 
    }
}
