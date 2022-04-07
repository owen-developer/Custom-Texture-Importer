using Spectre.Console;

namespace UI.Tests;

public static class Program
{
    public static async Task Main()
    {
        int count = 20;
        await GUI.ProgressBarLoop("Testing", "Test", new ForLoop<byte>(count, 0, ctx =>
        {
        }));
    }
}

public sealed class ForLoop<T>
{
    public ForLoop(int count, int startIndex, Action<ForLoop<T>> action)
        : this(new T[count], startIndex, action)
    {
    }

    public ForLoop(T[] array, int startIndex, Action<ForLoop<T>> action)
    {
        Array = array;
        StartIndex = startIndex;
        Action = action;
        Count = array.Length;
    }

    public T[] Array { get; }
    public int StartIndex { get; }
    public Action<ForLoop<T>> Action { get; }
    public int Count { get; }
    public int Index { get; private set; }

    public void Run(Action action = null)
    {
        for (Index = StartIndex; Index < Count; Index++)
        {
            Action.Invoke(this);
            if (action is not null)
            {
                action.Invoke();
            }
        }
    }
}

public static class GUI
{
    public static Color INFO_COLOR = Color.Cyan1;
    public static Color ERROR_COLOR = Color.Red;
    public static Color WARNING_COLOR = Color.Yellow;
    public static Color INPUT_COLOR = Color.Orange1;
    public static Color PROMPT_COLOR = Color.Blue;

    static GUI()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.Markup($"[{ERROR_COLOR}]Environment does not support interaction.[/]");
            return;
        }
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

    public static string Input(string prompt)
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{PROMPT_COLOR.ToMarkup()}]{prompt}[/]")
                .PromptStyle(INPUT_COLOR.ToMarkup()));

        return input;
    }
}
