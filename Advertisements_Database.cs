using System.Collections;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Nexd.MySQL;

namespace AdvertisementsDatabase;
public class AdvertisementsDatabase : BasePlugin
{
    public override string ModuleName => "Advertisements_Database";
    public override string ModuleVersion => "0.1";
    public override string ModuleAuthor => "";
    public override string ModuleDescription => "";

    private MySqlDb? g_Db = null;
    ArrayList g_AdvertisementsList = new ArrayList();

    public override void Load(bool hotReload)
    {
        new Cfg().CheckConfig(ModuleDirectory);
        g_Db = new(Cfg.Config.DatabaseHost!, Cfg.Config.DatabaseUser!, Cfg.Config.DatabasePassword!, Cfg.Config.DatabaseName!, Cfg.Config.DatabasePort);
        Console.WriteLine(g_Db.ExecuteNonQueryAsync("CREATE TABLE IF NOT EXISTS `advertisements` (`id` INT NOT NULL AUTO_INCREMENT,`message` VARCHAR(1024) NOT NULL,PRIMARY KEY (`id`));").Result);

        GetAdvertisements();

        Console.WriteLine("Advertisements_Database is loaded");
    }

    [ConsoleCommand("css_adv", "advertisements command")]
    [ConsoleCommand("css_advertisements", "advertisements command")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null && player!.IsValid && player.Connected != PlayerConnectedState.PlayerConnected && !player.PlayerPawn.IsValid) return;

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
                string message = pair.Value["message"]!.ToString();

                // Print both ID and message to the console
                player.PrintToConsole("ID: " + id + ", Message: " + message + "\n");
            }
        }

        if (command.ArgByIndex(1) == "add")
        {

            if (command.ArgCount >= 3 && command.ArgCount <= 3)
            {
                g_Db!.ExecuteNonQueryAsync($"INSERT INTO `advertisements` (`message`) VALUES ('{command.ArgByIndex(2)}');");

                ReloadAdvertisements();

                player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully added {command.ArgByIndex(2)}");
            }
            else
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements add <message>");
            }
        }

        if (command.ArgByIndex(1) == "edit")
        {
            if (command.ArgCount >= 3 && command.ArgCount <= 4)
            {
                g_Db!.ExecuteNonQueryAsync($"UPDATE `advertisements` SET message = '{command.ArgByIndex(3)}' WHERE id = '{command.ArgByIndex(2)}'");

                ReloadAdvertisements();

                player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully edit {command.ArgByIndex(2)}");
            }
            else
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements edit <id> <message>");
            }
        }

        if (command.ArgByIndex(1) == "remove" && command.ArgByIndex(1) == "delete")
        {
            if (command.ArgCount >= 3 && command.ArgCount <= 3)
            {
                g_Db!.ExecuteNonQueryAsync($"DELETE FROM `advertisements` WHERE `id` = '{command.ArgByIndex(2)}';");

                ReloadAdvertisements();

                player.PrintToChat($"{Cfg.Config.ChatPrefix} Successfully Removed {command.ArgByIndex(2)}");
            }
            else
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: css_advertisements remove <id>");
            }
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

        var results = g_Db!.ExecuteQuery("select * from advertisements");

        foreach (KeyValuePair<int, MySqlFieldValue> pair in results)
        {
            g_AdvertisementsList.Add(pair.Value["message"]);
        }
    }

    private void GetAdvertisements()
    {
        var results = g_Db!.ExecuteQuery("select * from advertisements");

        foreach (KeyValuePair<int, MySqlFieldValue> pair in results)
        {
            g_AdvertisementsList.Add(pair.Value["message"]);
        }

        base.AddTimer(Cfg.Config.Timer, Timer_Advertisements, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
    }

    private int currentAdIndex = 0;

    private void Timer_Advertisements()
    {
        if (g_AdvertisementsList.Count == 0)
        {
            Console.WriteLine("No advertisements to display.");
            return;
        }

        string? CMessage = g_AdvertisementsList[currentAdIndex] as string; // Assuming the list contains strings.
        currentAdIndex = (currentAdIndex + 1) % g_AdvertisementsList.Count; // Move to next ad, reset to 0 at end of list.

        Server.PrintToChatAll($"{Cfg.Config.ChatPrefix} {ReplaceMessageTags(CMessage!)}");
    }

    private string ReplaceMessageTags(string message)
    {
        var replacedMessage = message
            .Replace("{MAP}", NativeAPI.GetMapName())
            .Replace("{TIME}", DateTime.Now.ToString("HH:mm:ss"))
            .Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy"))
            .Replace("{SERVERNAME}", ConVar.Find("hostname")!.StringValue)
            .Replace("{IP}", ConVar.Find("ip")!.StringValue)
            .Replace("{PORT}", ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString());

        replacedMessage = ReplaceColorTags(replacedMessage);

        return replacedMessage;
    }

    private string ReplaceColorTags(string message)
    {
        string[] colorPatterns =
        {
            "{DEFAULT}", "{RED}", "{LIGHTPURPLE}", "{GREEN}", "{LIME}", "{LIGHTGREEN}", "{LIGHTRED}", "{GRAY}",
            "{LIGHTOLIVE}", "{OLIVE}", "{LIGHTBLUE}", "{BLUE}", "{PURPLE}", "{GRAYBLUE}"
        };
        string[] colorReplacements =
        {
            "\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08", "\x09", "\x10", "\x0B", "\x0C", "\x0E",
            "\x0A"
        };

        for (var i = 0; i < colorPatterns.Length; i++)
            message = message.Replace(colorPatterns[i], colorReplacements[i]);

        return message;
    }
}