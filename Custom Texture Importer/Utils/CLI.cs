using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.UI;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;

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
                var colors = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var color in colors)
                {
                    if (color.PropertyType == typeof(Color))
                    {
                        var c = (Color)color.GetValue(null);
                        GUI.WriteLineColored(GUI.INFO_COLOR, c.ToMarkup());
                    }
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
    
    private static void EvaluateCommand(string input)
    {
        var parts = ParseCommand(input);
        var config = parts[0] == "config";
    }

    private static void EvaluateConfigCommand(string[] parts)
    {
        var edit = parts[1] == "edit";
        var open = parts[1] == "open";
        var save = parts[1] == "save";
        if (edit)
        {
            
        }
    }

    private static void EditConfig(string[] parts)
    {
        var property = parts[2];
        
    }

    private static unsafe string[] ParseCommand(string input)
    {
        int position = 0;
        var parts = new List<string>();
        string part;
        do
        {
            part = Parse(&position, input);
            if (part != null)
            {
                parts.Add(part);
            }
        }
        while (part != null);

        return parts.ToArray();
    }

    private static unsafe string Parse(int* position, string input)
    {
        char Peek(int offset)
        {
            var index = *position + offset;
            if (index >= input.Length)
            {
                return '\0';
            }

            return input[index];
        }

        if (Peek(0) == '\0')
        {
            return null;
        }

        var start = *position;
        while (Peek(0) != '.' &&
               Peek(0) != '\0')
        {
            *position += 1;
        }

        return input[start..*position];
    }
}
