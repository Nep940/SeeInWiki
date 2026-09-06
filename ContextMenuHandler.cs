using System;
using Dalamud.Game;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;

namespace SeeInWiki;

public class ContextMenuHandler : IDisposable
{
    private readonly Plugin _plugin;

    public ContextMenuHandler(Plugin plugin)
    {
        _plugin = plugin;
        Plugin.ContextMenu.OnMenuOpened += OnMenuOpened;
    }

    public void Dispose()
    {
        Plugin.ContextMenu.OnMenuOpened -= OnMenuOpened;
    }

    private void OnMenuOpened(IMenuOpenedArgs args)
    {
        uint? baseItemId = null;
        uint? glamourItemId = null;

        // 1. Check if the menu target is an inventory item (Inventory, Armory Chest, Retainer, Chocobo Saddlebag, etc.)
        if (args.MenuType == ContextMenuType.Inventory && args.Target is MenuTargetInventory inventoryTarget)
        {
            if (inventoryTarget.TargetItem.HasValue)
            {
                var targetItem = inventoryTarget.TargetItem.Value;
                baseItemId = targetItem.ItemId;

                if (_plugin.Config.IncludeGlamourOption && targetItem.GlamourId > 0)
                {
                    glamourItemId = targetItem.GlamourId;
                }
            }
        }

        // 2. Fallback: if not from inventory (or no item found yet), check HoveredItem (chat links, vendor shops, recipes, market board, etc.)
        if (!baseItemId.HasValue || baseItemId.Value == 0)
        {
            ulong hovered = Plugin.GameGui.HoveredItem;
            if (hovered > 0)
            {
                baseItemId = (uint)hovered;
            }
        }

        // If no item ID was resolved, there is nothing to show
        if (!baseItemId.HasValue || baseItemId.Value == 0)
            return;

        uint cleanItemId = CorrectItemId(baseItemId.Value);
        if (cleanItemId == 0)
            return;

        // Resolve English item name from game sheets
        string? itemName = ResolveItemName(cleanItemId);
        if (string.IsNullOrWhiteSpace(itemName))
            return;

        // Create main "See in Wiki" menu entry
        string wikiUrl = WikiHelper.GetWikiUrl(itemName);
        var label = !string.IsNullOrWhiteSpace(_plugin.Config.ContextMenuLabel)
            ? _plugin.Config.ContextMenuLabel
            : "See in Wiki";

        var menuItem = new MenuItem
        {
            Name = (SeString)label,
            IsEnabled = true,
            OnClicked = _ =>
            {
                if (_plugin.Config.EchoToChat)
                {
                    Plugin.ChatGui.Print($"[See in Wiki] Opening wiki page for \"{itemName}\"...");
                }
                WikiHelper.OpenBrowser(wikiUrl);
            }
        };

        args.AddMenuItem(menuItem);

        // Optional: add a separate button for the glamoured item if present
        if (glamourItemId.HasValue && glamourItemId.Value > 0)
        {
            uint cleanGlamourId = CorrectItemId(glamourItemId.Value);
            if (cleanGlamourId > 0 && cleanGlamourId != cleanItemId)
            {
                string? glamourName = ResolveItemName(cleanGlamourId);
                if (!string.IsNullOrWhiteSpace(glamourName))
                {
                    string glamourWikiUrl = WikiHelper.GetWikiUrl(glamourName);
                    var glamourMenuItem = new MenuItem
                    {
                        Name = (SeString)$"{label} (Glamour)",
                        IsEnabled = true,
                        OnClicked = _ =>
                        {
                            if (_plugin.Config.EchoToChat)
                            {
                                Plugin.ChatGui.Print($"[See in Wiki] Opening wiki page for glamour \"{glamourName}\"...");
                            }
                            WikiHelper.OpenBrowser(glamourWikiUrl);
                        }
                    };

                    args.AddMenuItem(glamourMenuItem);
                }
            }
        }
    }

    /// <summary>
    /// Corrects the item ID by removing HQ (+1,000,000) or Collectible (+500,000) offsets.
    /// </summary>
    public static uint CorrectItemId(uint itemId)
    {
        if (itemId >= 1000000)
            return itemId - 1000000;
        if (itemId > 500000)
            return itemId - 500000;
        return itemId;
    }

    /// <summary>
    /// Resolves the English item name from game data tables to ensure the wiki URL works properly.
    /// </summary>
    public static string? ResolveItemName(uint itemId)
    {
        try
        {
            var sheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Item>(ClientLanguage.English);
            if (sheet != null && sheet.TryGetRow(itemId, out var item))
            {
                var text = item.Name.ExtractText();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;

                return item.Name.ToString();
            }
        }
        catch (Exception ex)
        {
            Plugin.PluginLog.Debug(ex, $"Failed to resolve item name for ID {itemId}");
        }

        return null;
    }
}
