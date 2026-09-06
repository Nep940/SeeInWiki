using System;
using System.Diagnostics;

namespace SeeInWiki;

public static class WikiHelper
{
    private const string WikiBaseUrl = "https://ffxiv.consolegameswiki.com/wiki/";

    /// <summary>
    /// Constructs the direct wiki URL for the item using its English in-game name.
    /// MediaWiki standard conventions replace spaces with underscores.
    /// </summary>
    /// <param name="itemName">The item's name (e.g. "Bronze Ingot", "Curtana").</param>
    /// <returns>The complete URL to the item page on consolegameswiki.</returns>
    public static string GetWikiUrl(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return "https://ffxiv.consolegameswiki.com/wiki/FF14_Wiki";

        // Remove any leading/trailing whitespace
        string trimmed = itemName.Trim();

        // In MediaWiki, article page names format spaces as underscores
        string wikiPageName = trimmed.Replace(' ', '_');

        // Escape URL special characters (e.g., ?, #, &, etc.)
        string escapedName = Uri.EscapeDataString(wikiPageName);

        return $"{WikiBaseUrl}{escapedName}";
    }

    /// <summary>
    /// Opens the specified URL in the player's default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Windows fallback using cmd start in case default protocol association fails
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{url}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch
            {
                // Silently handle if system shell execution is blocked
            }
        }
    }
}
