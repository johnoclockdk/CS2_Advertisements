using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using System.Text.Json;


namespace AdvertisementsDatabase;

internal class Cfg
{
    public static Config Config = new();


    /// <summary>
    /// Checks the configuration file for the module and creates it if it does not exist.
    /// </summary>
    /// <param name="moduleDirectory">The directory where the module is located.</param>
    public void CheckConfig(string moduleDirectory)
    {
        string path = Path.Join(moduleDirectory, "config.json");

        if (!File.Exists(path))
        {
            CreateAndWriteFile(path);
        }

        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new(fs))
        {
            // Deserialize the JSON from the file and load the configuration.
            Config = JsonSerializer.Deserialize<Config>(sr.ReadToEnd());
        }

        foreach (PropertyInfo prop in Config.GetType().GetProperties())
        {
            if (prop.PropertyType != typeof(string))
            {
                continue;
            }

            prop.SetValue(Config, ModifyColorValue(prop.GetValue(Config).ToString()));
        }
    }

    /// <summary>
    /// Creates a new file at the specified path and writes the default configuration settings to it.
    /// </summary>
    /// <param name="path">The path where the file should be created.</param>
    private static void CreateAndWriteFile(string path)
    {

        using (FileStream fs = File.Create(path))
        {
            // File is created, and fs will automatically be disposed when the using block exits.
        }

        Console.WriteLine($"File created: {File.Exists(path)}");

        Config = new Config
        {
            ChatPrefix = "[Advertisements]",
            Timer = float.Parse("60"),
            DatabaseHost = "ip",
            DatabasePort = 3306,
            DatabaseUser = "root",
            DatabasePassword = "myawsomepassword",
            DatabaseName = "Advertisements"
        };

        // Serialize the config object to JSON and write it to the file.
        string jsonConfig = JsonSerializer.Serialize(Config, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(path, jsonConfig);
    }

    // Essential method for replacing chat colors from the config file, the method can be used for other things as well.
    private string ModifyColorValue(string msg)
    {
        if (!msg.Contains('{'))
        {
            return string.IsNullOrEmpty(msg) ? "[Advertisements]" : msg;
        }

        string modifiedValue = msg;

        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";
            if (msg.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null).ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
        return modifiedValue;
    }
}

internal class Config
{
    public string? ChatPrefix { get; set; }
    public string? DatabaseHost { get; set; }
    public int DatabasePort { get; set; }
    public string? DatabaseUser { get; set; }
    public string? DatabasePassword { get; set; }
    public string? DatabaseName { get; set; }
    public float Timer { get; set; }

}
