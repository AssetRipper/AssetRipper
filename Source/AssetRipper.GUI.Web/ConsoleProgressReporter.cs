using System;

namespace AssetRipper.GUI.Web;

public class ConsoleProgressReporter : IProgressReporter
{
    public void ReportProgress(int progress)
    {
        Console.WriteLine($"Progress: {progress}%");
    }
}
