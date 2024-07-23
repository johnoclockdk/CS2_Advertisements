using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using CounterStrikeSharp.API;
using Nexd.MySQL;
using CounterStrikeSharp.API.Modules.Admin;
using System.Collections.Generic;
using CounterStrikeSharp.API.Modules.Entities;
namespace Advertisements;

public partial class AdvertisementsCore
{
    private void ReloadAdvertisements()
    {
        g_AdvertisementsList.Clear();
        FetchAdvertisements();
    }

    private void GetAdvertisements()
    {
        FetchAdvertisements();
        timer = AddTimer(Config.Timer, Timer_Advertisements, TimerFlags.REPEAT);
    }

    private void FetchAdvertisements()
    {
        string currentServerPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
        var results = g_Db!.ExecuteQuery("select * from `advertisements`");

        foreach (KeyValuePair<int, MySqlFieldValue> pair in results)
        {
            string serverPorts = pair.Value["server"]?.ToString() ?? "";
            string enable = pair.Value["enable"]?.ToString() ?? "";
            if (enable == "0") continue;

            if (string.IsNullOrEmpty(serverPorts) || serverPorts.Split(',').Select(p => p.Trim()).Contains(currentServerPort))
            {
                string message = pair.Value["message"]!.ToString();
                string location = pair.Value["location"]!.ToString();
                string flags = pair.Value["flag"]?.ToString() ?? "";

                g_AdvertisementsList.Add(new Advertisement(message, location, flags));
            }
        }
    }

    Dictionary<ulong, int> currentAdIndex = new();
    Dictionary<ulong, HashSet<Advertisement>> PlayerAdvertisement = new();

    private void Timer_Advertisements()
    {

        if (g_AdvertisementsList == null || g_AdvertisementsList.Count < 1)
        {
            Console.WriteLine("No advertisements to display.");
            return;
        }

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!ValidClient(player)) continue;

            if (!PlayerAdvertisement[player.SteamID].Any())
            {
                continue;
            }

            DisplayAdvertisement(player, PlayerAdvertisement[player.SteamID].ElementAt(currentAdIndex[player.SteamID]));
            // Move to the next advertisement for the next timer tick
            currentAdIndex[player.SteamID] = (currentAdIndex[player.SteamID] + 1) % PlayerAdvertisement[player.SteamID].Count;
        }
    }

    private void OnClientAuthorized(int playerSlot, SteamID steamId)
    {
        if (playerSlot == 65535 || !steamId.IsValid())
        {
            return;
        }

        // Tror ikke dette er nødvendigt
        currentAdIndex[steamId.SteamId64] = 0;
        PlayerAdvertisement[steamId.SteamId64] = g_AdvertisementsList.Where(x => string.IsNullOrEmpty(x.Flag) || AdminManager.PlayerHasPermissions(steamId, x.Flag.Split(','))).ToHashSet();
    }

    private void OnClientDisconnect(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);
        if (!ValidClient(player)) return;
        currentAdIndex.Remove(player.SteamID);
        PlayerAdvertisement.Remove(player.SteamID);
    }


    private void DisplayAdvertisement(CCSPlayerController player, Advertisement advertisement)
    {
        // Ensure both player and advertisement are valid
        if (player == null || advertisement == null)
        {
            Console.WriteLine("Invalid player or advertisement.");
            return;
        }

        // Display the advertisement based on its specified location
        switch (advertisement.Location)
        {
            case "chat":
                // Display the advertisement in chat
                player.PrintToChat($" {ModifyColorValue(Config.ChatPrefix!)} {ReplaceMessageTags(advertisement.Message, player)}");
                break;

            case "center":
                // Display the advertisement in the center of the screen
                player.PrintToCenter($" {ModifyColorValue(Config.ChatPrefix!)} {ReplaceMessageTags(advertisement.Message, player)}");
                break;

            default:
                // Handle unknown locations, perhaps log an error or ignore
                Console.WriteLine($"Unknown advertisement location: {advertisement.Location}");
                break;
        }
    }


    // Essential method for replacing chat colors from the config file, the method can be used for other things as well.
    public static string ModifyColorValue(string msg)
    {
        if (!msg.Contains('{'))
        {
            return string.IsNullOrEmpty(msg) ? "" : msg;
        }

        string modifiedValue = msg;

        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";
            if (msg.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
        return modifiedValue;
    }

    public class Advertisement
    {
        public string Message { get; set; }
        public string Location { get; set; }
        public string Flag { get; set; }

        public Advertisement(string message, string location, string flag)
        {
            Message = message;
            Location = location;
            Flag = flag;
        }
    }

    private bool ValidClient(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.IsHLTV || !player.PlayerPawn.IsValid || player.UserId == -1 || player.IsBot || player.Slot == 65535) return false;
        return true;
    }

    private string ReplaceMessageTags(string message, CCSPlayerController player)
    {
        // Replace various tags with corresponding values
        message = message
            .Replace("{CURRENTMAP}", Server.MapName)
            .Replace("{TIME}", DateTime.Now.ToString("HH:mm:ss"))
            .Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy"))
            .Replace("{SERVERNAME}", ConVar.Find("hostname")!.StringValue)
            .Replace("{NAME}", player.PlayerName)
            .Replace("{STEAMID}", player.SteamID.ToString())
            .Replace("{PLAYERCOUNT}", Utilities.GetPlayers().FindAll(x => ValidClient(x)).Count.ToString())
            .Replace("{MAXPLAYERS}", Server.MaxPlayers.ToString())
            .Replace("{IP}", ConVar.Find("ip")!.StringValue)
            .Replace("{PORT}", ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString())
            .Replace("{NEWLINE}", "\u2029");
        return ModifyColorValue(message);
    }
}
