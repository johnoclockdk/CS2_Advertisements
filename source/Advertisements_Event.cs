using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
namespace Advertisements;

public partial class AdvertisementsCore
{
    private int panelAdIndex = 0; // New index for panel advertisements

    private HookResult EventCsWinPanelRound(EventCsWinPanelRound handle, GameEventInfo info)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!ValidClient(player))
            {
                continue;
            }

            Advertisement advertisement = g_AdvertisementsList.ElementAt(currentAdIndex[player.SteamID]);

            if (!advertisement.Location.Equals("panel", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // Handle advertisements with location "panel"
            handle.FunfactToken = ReplaceMessageTags(advertisement.Message, player);
            handle.TimerTime = 5;
        }
        panelAdIndex = (panelAdIndex + 1) % g_AdvertisementsList.Count; // Update panelAdIndex

        return HookResult.Continue;
    }
}