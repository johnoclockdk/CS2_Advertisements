using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Nexd.MySQL;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Advertisements;
[MinimumApiVersion(240)]
public partial class AdvertisementsCore : BasePlugin, IPluginConfig<AdvertisementConfig>
{
    public override string ModuleName => "Advertisements";
    public override string ModuleVersion => "2.0";
    public override string ModuleAuthor => "Johnoclock, xWidovV";
    public override string ModuleDescription => "Display Advertisements from database";

    private MySqlDb? g_Db = null;
    HashSet<Advertisement> g_AdvertisementsList = new();
    private CounterStrikeSharp.API.Modules.Timers.Timer? timer;
    public required AdvertisementConfig Config { get; set; }

    public void OnConfigParsed(AdvertisementConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventCsWinPanelRound>(EventCsWinPanelRound, HookMode.Pre);

        RegisterListener<OnClientAuthorized>(OnClientAuthorized);
        RegisterListener<OnClientDisconnect>(OnClientDisconnect);

        g_Db = new(Config.DatabaseHost!, Config.DatabaseUser!, Config.DatabasePassword!, Config.DatabaseName!, Config.DatabasePort);
        Console.WriteLine(g_Db.ExecuteNonQueryAsync("CREATE TABLE IF NOT EXISTS `advertisements` (`id` INT NOT NULL AUTO_INCREMENT,`message` VARCHAR(1024) NOT NULL,`location` VARCHAR(128),`server` VARCHAR(512), `flag` VARCHAR(512), `enable` int(12),PRIMARY KEY (`id`));").Result);

        GetAdvertisements();

        Console.WriteLine("Advertisements is loaded");
    }

    public override void Unload(bool hotReload)
    {
        timer!.Kill();
        Console.WriteLine("Advertisements is unloaded");
    }
}