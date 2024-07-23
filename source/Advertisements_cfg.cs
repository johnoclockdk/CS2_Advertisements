using CounterStrikeSharp.API.Core;

namespace Advertisements;

public class AdvertisementConfig : BasePluginConfig
{
    public string? ChatPrefix { get; set; } = "{lightblue}Advertisement{defualt} | ";
    public string? DatabaseHost { get; set; } = "ip";
    public int DatabasePort { get; set; } = 3306;
    public string? DatabaseUser { get; set; } = "root";
    public string? DatabasePassword { get; set; } = "passoword";
    public string? DatabaseName { get; set; } = "database";
    public float Timer { get; set; } = 60;
}
