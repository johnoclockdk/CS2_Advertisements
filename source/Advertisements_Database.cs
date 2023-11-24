using System.Collections;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Nexd.MySQL;

namespace AdvertisementsDatabase;
public class AdvertisementsDatabase : BasePlugin
{
    public override string ModuleName => "Advertisements_Database";
    public override string ModuleVersion => "1.5";
    public override string ModuleAuthor => "johnoclock";
    public override string ModuleDescription => "Display Advertisements from database";

    private MySqlDb? g_Db = null;
    ArrayList g_AdvertisementsList = new ArrayList();

    public override void Load(bool hotReload)
    {
        new Cfg().CheckConfig(ModuleDirectory);
        g_Db = new(Cfg.Config.DatabaseHost!, Cfg.Config.DatabaseUser!, Cfg.Config.DatabasePassword!, Cfg.Config.DatabaseName!, Cfg.Config.DatabasePort);
        Console.WriteLine(g_Db.ExecuteNonQueryAsync("CREATE TABLE IF NOT EXISTS `advertisements` (`id` INT NOT NULL AUTO_INCREMENT,`message` VARCHAR(1024) NOT NULL,`location` VARCHAR(128),`server` VARCHAR(512),PRIMARY KEY (`id`));").Result);

        GetAdvertisements();

        Console.WriteLine("Advertisements_Database is loaded");
    }

    [ConsoleCommand("css_adv", "advertisements command")]
    [ConsoleCommand("css_advertisements", "advertisements command")]
    [RequiresPermissionsOr("@css/slay", "@adv/adv")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!ValidClient(player)) return;

        if (command.ArgCount == 1)
        {
            player.PrintToChat($"{Cfg.Config.ChatPrefix} Please provide one argment");
            player.PrintToChat("- reload");
            player.PrintToChat("- add");
            player.PrintToChat("- edit");
            player.PrintToChat("- remove or delete");
            player.PrintToChat("- list");
        }

        if (command.ArgByIndex(1) == "list")
        {
            var results = g_Db!.ExecuteQuery("select * from advertisements");

            foreach (KeyValuePair<int, MySqlFieldValue> pair in results)
            {
                // Assuming 'id' is the column name for the ID in your database
                string id = pair.Value["id"]!.ToString();
                string location = pair.Value["location"]!.ToString();
                string message = pair.Value["message"]!.ToString();

                // Print both ID and message to the console
                player.PrintToConsole("ID: " + id + ", Message: " + message + "Location" + location + "\n");
            }
            return;
        }

        if (command.ArgByIndex(1) == "add")
        {
            if (command.ArgCount < 3 || command.ArgCount > 5)
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements add <message> <location> [<port>]");
                return;
            }

            g_Db!.ExecuteNonQueryAsync($"INSERT INTO `advertisements` (`message`, `location`, server) VALUES ('{command.ArgByIndex(2)}', '{command.ArgByIndex(3)}', '{command.ArgByIndex(4)}');");

            ReloadAdvertisements();

            player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully added {command.ArgByIndex(2)}");
            return;
        }

        if (command.ArgByIndex(1) == "edit")
        {
            if (command.ArgCount < 3 || command.ArgCount > 4)
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements edit <id> <message>");
                return;
            }

            g_Db!.ExecuteNonQueryAsync($"UPDATE `advertisements` SET message = '{command.ArgByIndex(3)}' WHERE id = '{command.ArgByIndex(2)}'");

            ReloadAdvertisements();

            player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully edit {command.ArgByIndex(2)}");
            return;
        }

        if (command.ArgByIndex(1) == "remove" && command.ArgByIndex(1) == "delete")
        {
            if (command.ArgCount != 3)
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements remove <id>");
                return;
            }

            g_Db!.ExecuteNonQueryAsync($"DELETE FROM `advertisements` WHERE `id` = '{command.ArgByIndex(2)}';");

            ReloadAdvertisements();

            player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully Removed {command.ArgByIndex(2)}");
            return;
        }

        if (command.ArgByIndex(1) == "reload")
        {
            ReloadAdvertisements();

            player.PrintToChat("advertisements reloaded");
        }
    }

    private void ReloadAdvertisements()
    {
        g_AdvertisementsList.Clear();
        FetchAdvertisements();
    }

    private void GetAdvertisements()
    {
        FetchAdvertisements();
        base.AddTimer(Cfg.Config.Timer, Timer_Advertisements, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
    }

    private void FetchAdvertisements()
    {
        string currentServerPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
        var results = g_Db!.ExecuteQuery("select * from `advertisements`");

        foreach (KeyValuePair<int, MySqlFieldValue> pair in results)
        {
            string serverPorts = pair.Value["server"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(serverPorts) || serverPorts.Split(',').Select(p => p.Trim()).Contains(currentServerPort))
            {
                string message = pair.Value["message"]!.ToString();
                string location = pair.Value["location"]!.ToString();

                g_AdvertisementsList.Add(new Advertisement(message, location));
            }
        }
    }

    private int currentAdIndex = 0;

    private void Timer_Advertisements()
    {
        if (g_AdvertisementsList == null || g_AdvertisementsList.Count < 1)
        {
            Console.WriteLine("No advertisements to display.");
            return;
        }

        List<CCSPlayerController> players = Utilities.GetPlayers();

        foreach (CCSPlayerController player in players)
        {
            if (!ValidClient(player)) continue;

            Advertisement advertisement = (Advertisement)g_AdvertisementsList[currentAdIndex]!;

            if (advertisement.Location == "chat")
            {
                // Handle advertisements with location "chat"
                player.PrintToChat($"{Cfg.Config.ChatPrefix} {ReplaceMessageTags(advertisement.Message, player)}");
            }
            else if (advertisement.Location == "center")
            {
                player.PrintToCenter($"{Cfg.Config.ChatPrefix} {ReplaceMessageTags(advertisement.Message, player)}");
            }
            else if (advertisement.Location == "panel")
            {
                // Handle advertisements with location "panel" (if needed)
            }

            currentAdIndex = (currentAdIndex + 1) % g_AdvertisementsList.Count; // Move to the next ad, reset to 0 at the end of the list.
        }
    }

    private bool ValidClient(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.IsHLTV || !player.PlayerPawn.IsValid || player.UserId == -1 || player.IsBot) return false;
        return true;
    }

    private string ReplaceMessageTags(string message, CCSPlayerController player)
    {
        // Replace various tags with corresponding values
        message = message
            .Replace("{CURRENTMAP}", NativeAPI.GetMapName())
            .Replace("{TIME}", DateTime.Now.ToString("HH:mm:ss"))
            .Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy"))
            .Replace("{SERVERNAME}", ConVar.Find("hostname")!.StringValue)
            .Replace("{NAME}", player.PlayerName)
            .Replace("{STEAMID}", player.SteamID.ToString())
            .Replace("{PLAYERCOUNT}", Utilities.GetPlayers().Count.ToString())
            .Replace("{MAXPLAYERS}", Server.MaxPlayers.ToString())
            .Replace("{IP}", ConVar.Find("ip")!.StringValue)
            .Replace("{PORT}", ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString());

        foreach (var pair in colorReplacements)
        {
            message = message.Replace(pair.Key, pair.Value);
        }
        return message;
    }

    static readonly Dictionary<string, string> colorReplacements = new Dictionary<string, string>
    {
        { "{DEFAULT}", "\x01" },
        { "{RED}", "\x02" },
        { "{LIGHTPURPLE}", "\x03" },
        { "{GREEN}", "\x04" },
        { "{LIME}", "\x05" },
        { "{LIGHTGREEN}", "\x06" },
        { "{LIGHTRED}", "\x07" },
        { "{GRAY}", "\x08" },
        { "{LIGHTOLIVE}", "\x09" },
        { "{OLIVE}", "\x10" },
        { "{LIGHTBLUE}", "\x0B" },
        { "{BLUE}", "\x0C" },
        { "{PURPLE}", "\x0E" },
        { "{GRAYBLUE}", "\x0A" }
    };
}

public class Advertisement
{
    public string Message { get; set; }
    public string Location { get; set; }

    public Advertisement(string message, string location)
    {
        Message = message;
        Location = location;
    }
}