using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils;
using Custom_Texture_Importer.Utils.CLI;
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
            WriteLineColored(INFO_COLOR, "Environment does not support interaction.");
            return;
        }

        INFO_COLOR = GetColorFromName(Config.CurrentConfig.InfoColor);
        ERROR_COLOR = GetColorFromName(Config.CurrentConfig.ErrorColor);
        WARNING_COLOR = GetColorFromName(Config.CurrentConfig.WarningColor);
        INPUT_COLOR = GetColorFromName(Config.CurrentConfig.InputColor);
        PROMPT_COLOR = GetColorFromName(Config.CurrentConfig.PromptColor);
        WriteLineColored(INFO_COLOR, "Welcome to the Custom Texture Importer!");
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
        AnsiConsole.MarkupLine($"[{INFO_COLOR.ToMarkup()}]{processingText}...[/]");

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

        var isCommand = false;
        if (input[0] == '#')
        {
            await RunCommand(input[1..]);
            isCommand = true;
        }
        
        return await Task.FromResult((input, isCommand));
    }

    private static async Task RunCommand(string command)
    {
        var parser = new Parser(command);
        if (parser.Diagnostics.Any())
        {
            foreach (var diagnostic in parser.Diagnostics)
            {
                AnsiConsole.MarkupLine($"[{ERROR_COLOR.ToMarkup()}]{diagnostic}[/]");
            }

            return;
        }

        var evaluator = new Evaluator(parser.Parse());
        await evaluator.Evaluate();
    }

    public static void Log(string text)
    {
        AnsiConsole.MarkupLine($"[{LOG_COLOR.ToMarkup()}]{text}[/]");
    }

    public static void WriteLineColored(Color color, string text)
    {
        AnsiConsole.MarkupLine($"[{color}]{text}[/]");
    }

    public static void Clear()
    {
        AnsiConsole.Clear();
    }
}
