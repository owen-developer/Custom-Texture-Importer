using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils;
using Spectre.Console;

namespace Custom_Texture_Importer.UI;

public static class GUI
{
    public static Color INFO_COLOR = Color.Cyan1;
    public static Color ERROR_COLOR = Color.Red;
    public static Color WARNING_COLOR = Color.Yellow;
    public static Color INPUT_COLOR = Color.Orange1;
    public static Color PROMPT_COLOR = Color.Blue;
    public static Color LOG_COLOR = Color.Grey;

    static GUI()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.Markup($"[{ERROR_COLOR}]Environment does not support interaction.[/]");
            return;
        }

        INFO_COLOR = GetColorFromName(Config.CurrentConfig.InfoColor);
        ERROR_COLOR = GetColorFromName(Config.CurrentConfig.ErrorColor);
        WARNING_COLOR = GetColorFromName(Config.CurrentConfig.WarningColor);
        INPUT_COLOR = GetColorFromName(Config.CurrentConfig.InputColor);
        PROMPT_COLOR = GetColorFromName(Config.CurrentConfig.PromptColor);
    }

    private static Color GetColorFromName(string name)
    {
        var color = Color.White;
        var properties = typeof(Color).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        foreach (var property in properties)
        {
            if (property.Name.ToLower() == name.ToLower())
            {
                color = (Color)property.GetValue(null);
            }
        }

        return color;
    }

    public static async Task ProgressBarLoop<T>(string processingText, string headerText, ForLoop<T> forLoop)
    {
        AnsiConsole.MarkupLine($"[{INFO_COLOR.ToMarkup()}]{processingText}...[/]");

        await Task.Run(() =>
        {
            AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new ProgressColumn[]
                {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
                })
                .Start(ctx =>
                {
                    var pgtsk = ctx.AddTask(headerText, new()
                    {
                        AutoStart = true,
                        MaxValue = forLoop.Count
                    });

                    forLoop.Run(() =>
                    {
                        pgtsk.Increment((double)forLoop.Count / pgtsk.MaxValue);

                        Thread.Sleep(100);
                    });
                });
        });

        AnsiConsole.MarkupLine($"[{INFO_COLOR.ToMarkup()}]Done![/]");
    }

    public static void ProgressBarAction(string processingText, string headerText, Action action)
    {
        AnsiConsole.MarkupLine($"[{INFO_COLOR.ToMarkup()}]{processingText}[/]...");

        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            })
            .Start(ctx =>
            {
                var pgtsk = ctx.AddTask(headerText, new()
                {
                    AutoStart = true,
                    MaxValue = 100
                });

                action.Invoke();
                pgtsk.Increment(100);
                Thread.Sleep(100);
            });

        AnsiConsole.MarkupLine($"[{INFO_COLOR.ToMarkup()}]Done![/]");
    }

    public static async Task<(string Text, bool IsCommand)> Input(string prompt)
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{PROMPT_COLOR.ToMarkup()}]{prompt}[/]")
                .PromptStyle(INPUT_COLOR.ToMarkup()));

        var isCommand = await CLI.IsCommand(input);
        return await Task.FromResult((input, isCommand));
    }

    public static void Log(string text)
    {
        AnsiConsole.MarkupLine($"[{LOG_COLOR.ToMarkup()}]{text}[/]");
    }

    public static void WriteLineColored(Color color, string text)
    {
        AnsiConsole.MarkupLine($"[{color}]{text}[/]");
    }
}
