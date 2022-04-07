using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.UI;
using System.Diagnostics;

namespace Custom_Texture_Importer.Utils;

public static class CLI
{
    public static Task<bool> IsCommand(string input)
    {
        return Task.Run(async () =>
        {
            if (input[0] == '#')
            {
                input = input[1..(!input.Contains(' ') ? input.Length : input.IndexOf(' '))].ToLower();
            }
            else
            {
                return false;
            }

            if (!await RunCommand(input))
            {
                GUI.WriteLineColored(GUI.ERROR_COLOR, $"Invalid command: {input}");
                return false;
            }

            return true;
        });
    }

    private static async Task<bool> RunCommand(string input)
    {
        switch (input)
        {
            case "exit":
                Environment.Exit(0);
                break;
            case "cls":
                Console.Clear();
                break;
            case "restore":
                await FortniteUtil.RemoveDupedUcas();
                GUI.WriteLineColored(GUI.INFO_COLOR, "Removed duped files!");
                break;
            case "colors":
                // Print each color and it's value from ConsoleColor enum
                foreach (var color in Enum.GetValues(typeof(ConsoleColor)))
                {
                    GUI.WriteLineColored((ConsoleColor)color, $"{color} = {(int)color}");
                }
                break;
            case "config":
                GUI.WriteLineColored(GUI.INFO_COLOR, "Current Config:");
                Console.WriteLine();
                GUI.WriteLineColored(GUI.INFO_COLOR, Config.CurrentConfig.ToString());
                break;
            case "config.open":
                Process.Start("Notepad", Config.CONFIG_PATH);
                break;
            case "pakpath.edit":
                var path = Console.ReadLine();
                Config.CurrentConfig.PakPath = path;
                Config.SaveConfig();
                break;
            case "help":
                GUI.WriteLineColored(GUI.INFO_COLOR, "Commands:");
                GUI.WriteLineColored(GUI.INFO_COLOR, "exit - Exits the program");
                GUI.WriteLineColored(GUI.INFO_COLOR, "cls - Clears the console");
                GUI.WriteLineColored(GUI.INFO_COLOR, "restore - Removes duped files");
                GUI.WriteLineColored(GUI.INFO_COLOR, "colors - Prints all the available colors you can use for UI colors.");
                GUI.WriteLineColored(GUI.INFO_COLOR, "config - Prints the current config");
                GUI.WriteLineColored(GUI.INFO_COLOR, "config.open - Opens the config file in notepad");
                GUI.WriteLineColored(GUI.INFO_COLOR, "pakpath.edit - Edits the pak path");
                GUI.WriteLineColored(GUI.INFO_COLOR, "help - Shows this message");
                break;
            default:
                return false;
        }

        return true;
    }
    
}
