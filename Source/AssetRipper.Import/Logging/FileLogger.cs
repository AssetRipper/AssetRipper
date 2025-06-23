using System.Text;

namespace AssetRipper.Import.Logging;

public class FileLogger : FileLoggerBase
{
	private readonly StringBuilder stringBuilder = new();

	public FileLogger() : base() { }

	/// <param name="filePath">The absolute path to the log file</param>
	public FileLogger(string filePath) : base(filePath) { }

	public sealed override void Log(LogType type, LogCategory category, string message)
	{
		stringBuilder.Clear();

		if (category != LogCategory.None)
		{
			stringBuilder.Append($"{category.ToString()} ");
		}

		switch (type)
		{
			case LogType.Warning:
			case LogType.Error:
				stringBuilder.Append($"[{type.ToString()}] ");
				break;
		}
		stringBuilder.Append(": ");
		stringBuilder.Append(message);
		stringBuilder.Append(Environment.NewLine);
		try
		{
			File.AppendAllText(filePath, stringBuilder.ToString());
		}
		catch (IOException)
		{
			//Could not log to file
		}
	}
}
