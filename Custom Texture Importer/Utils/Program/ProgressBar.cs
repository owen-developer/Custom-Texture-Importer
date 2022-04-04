using System.Text;

namespace Custom_Texture_Importer.Utils.Program;

public class ProgressBar : IDisposable, IProgress<double>
{
    private const int blockCount = 10;
    private const string animation = @"|/-\";
    private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);

    private readonly Timer timer;
    private int animationIndex = 0;

    private double currentProgress;
    private string currentText = string.Empty;
    private bool disposed;

    public ProgressBar()
    {
        timer = new Timer(TimerHandler);

        // A progress bar is only for temporary display in a console window.
        // If the console output is redirected to a file, draw nothing.
        // Otherwise, we'll end up with a lot of garbage in the target file.
        if (!Console.IsOutputRedirected) ResetTimer();
    }

    public void Dispose()
    {
        lock (timer)
        {
            disposed = true;
        }
    }

    public void Report(double value, int sleepTime = 100)
    {
        // Make sure value is in [0..1] range
        value = Math.Max(0, Math.Min(1, value));
        Interlocked.Exchange(ref currentProgress, value);
        Thread.Sleep(sleepTime);
    }

    private void TimerHandler(object state)
    {
        lock (timer)
        {
            if (disposed) return;

            Console.ForegroundColor = Custom_Texture_Importer.Program.PROGRESS_BAR_COLOR;
            var progressBlockCount = (int)(currentProgress * blockCount);
            var percent = (int)(currentProgress * 100);
            var text = string.Format("[{0}{1}] {2,3}% {3}",
                new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount), percent, "");
            UpdateText(text);
            Console.ResetColor();

            ResetTimer();
        }
    }

    private void UpdateText(string text)
    {
        // Get length of common portion
        var commonPrefixLength = 0;
        var commonLength = Math.Min(currentText.Length, text.Length);
        while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            commonPrefixLength++;

        // Backtrack to the first differing character
        var outputBuilder = new StringBuilder();
        outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

        // Output new suffix
        _ = outputBuilder.Append(text.AsSpan(commonPrefixLength));

        // If the new text is shorter than the old one: delete overlapping characters
        var overlapCount = currentText.Length - text.Length;
        if (overlapCount > 0)
        {
            outputBuilder.Append(' ', overlapCount);
            outputBuilder.Append('\b', overlapCount);
        }

        Console.Write(outputBuilder);
        currentText = text;
    }

    private void ResetTimer()
    {
        timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
    }
}