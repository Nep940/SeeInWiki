using Dalamud.Configuration;
using System;

namespace SeeInWiki;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    /// <summary>
    /// The label displayed in the context menu. Defaults to "See in Wiki".
    /// </summary>
    public string ContextMenuLabel { get; set; } = "See in Wiki";

    /// <summary>
    /// Whether to also show a separate option for glamour items if present.
    /// </summary>
    public bool IncludeGlamourOption { get; set; } = true;

    /// <summary>
    /// Whether to print a confirmation in chat when opening a wiki link.
    /// </summary>
    public bool EchoToChat { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
