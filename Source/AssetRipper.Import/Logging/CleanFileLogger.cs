namespace AssetRipper.Import.Logging;

/// <summary>
/// A file logger that doesn't include log types in the output.
/// </summary>
public class CleanFileLogger : FileLoggerBase
{
	public CleanFileLogger() : base() { }

	/// <param name="filePath">The absolute path to the log file</param>
	public CleanFileLogger(string filePath) : base(filePath) { }

	public sealed override void Log(LogType type, LogCategory category, string message)
	{
		try
		{
			File.AppendAllText(filePath, $"{message}{Environment.NewLine}");
		}
		catch (IOException)
		{
			//Could not log to file
		}
	}
}
