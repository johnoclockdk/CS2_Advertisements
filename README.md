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

## Markdown File for Plugin Usage

This document details the usage of color tags and message tags for a specific plugin.

# Color Tags

Color tags can be used to customize text colors in messages. Below is the list of available color tags:

- `{DEFAULT}`: Default text color
- `{RED}`: Red text
- `{LIGHTPURPLE}`: Light purple text
- `{GREEN}`: Green text
- `{LIME}`: Lime text
- `{LIGHTGREEN}`: Light green text
- `{LIGHTRED}`: Light red text
- `{GRAY}`: Gray text
- `{LIGHTOLIVE}`: Light olive text
- `{OLIVE}`: Olive text
- `{LIGHTBLUE}`: Light blue text
- `{BLUE}`: Blue text
- `{PURPLE}`: Purple text
- `{GRAYBLUE}`: Gray blue text

# Message Tags

Message tags are used to dynamically insert specific information into messages. The following tags are available:

- `{MAP}`: Inserts the current map name.
- `{TIME}`: Displays the current time in `HH:mm:ss` format.
- `{DATE}`: Shows the current date in `dd.MM.yyyy` format.
- `{SERVERNAME}`: Includes the server's hostname.
- `{NAME}`: Inserts the player's name.
- `{STEAMID}`: Adds the player's Steam ID.
- `{IP}`: Shows the server's IP address.
- `{PORT}`: Includes the server's port number.


## Usage Instructions

To utilize these tags, include them in your message strings as needed. The plugin will automatically replace these tags with the appropriate color codes or information when displaying the messages.


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
- **[hxppy](https://github.com/Skippydingledoo):** For their help with original sourcemod plugin.
