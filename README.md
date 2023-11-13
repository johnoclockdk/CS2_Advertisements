# Advertisements Database Plugin

## Description
The Advertisements Database plugin is designed for Counter-Strike: Source (CSS) servers. It allows you to manage and display advertisements stored in a database at specified intervals.

## Requirements
To use this plugin, you need to have the following requirements installed:
- [CounterStrikeSharp](https://docs.cssharp.dev/guides/getting-started/)

## Installation
Follow these steps to install the plugin on your CSS server:

1. **Download the Plugin Files:**
   - Obtain the plugin files from the [GitHub repository](https://github.com/johnoclockdk/cs2_Advertisements_Database) or the source of your choice.
   - Ensure you have the latest version of the plugin.

2. **Extract the Downloaded Files:**
   - Extract the downloaded plugin files to a convenient location on your computer.

3. **Copy Files to Server Directory:**
   - Copy the extracted plugin files to your CSS server's `csgo/` directory.
   - This is typically located in your server's root folder.

4. **Restart the Server:**
   - Restart your CSS server or change the map to apply the changes.
   - The plugin should now be installed and ready for use.

## Configuration
The plugin automatically generates a configuration file in the same location as the plugin DLL. You can edit this configuration file to customize the plugin settings according to your server's needs.

Example Configuration (`config.json`):
```json
{
  "ChatPrefix": "[Advertisements]",
  "DatabaseHost": "ip",
  "DatabasePort": 3306,
  "DatabaseUser": "root",
  "DatabasePassword": "myawsomepassword",
  "DatabaseName": "Advertisements",
  "Timer": "60"
}
```

## Credits
- **[partiusfabaa](https://github.com/partiusfabaa):** For their valuable contribution to the code related to replacing tags and colors.
