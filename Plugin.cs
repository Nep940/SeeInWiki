using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SeeInWiki;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "See in Wiki";
    private const string CommandName = "/seeinwiki";

    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;

    public Configuration Config { get; init; }
    private readonly ContextMenuHandler _contextMenuHandler;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        pluginInterface.Inject(this);

        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _contextMenuHandler = new ContextMenuHandler(this);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle chat notifications or test the wiki lookup. Usage: /seeinwiki [itemName]"
        });
    }

    private void OnCommand(string command, string args)
    {
        string input = args.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Config.EchoToChat = !Config.EchoToChat;
            Config.Save();
            ChatGui.Print($"[See in Wiki] Chat notification is now {(Config.EchoToChat ? "ENABLED" : "DISABLED")}.");
            return;
        }

        // Direct search test via slash command
        string url = WikiHelper.GetWikiUrl(input);
        ChatGui.Print($"[See in Wiki] Opening wiki page for \"{input}\"...");
        WikiHelper.OpenBrowser(url);
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);
        _contextMenuHandler.Dispose();
    }
}
