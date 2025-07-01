using AssetRipper.Import.Utils;

namespace AssetRipper.Import.Logging;

public abstract class FileLoggerBase : ILogger
{
	protected readonly string filePath;

	public FileLoggerBase() : this(ExecutingDirectory.Combine($"AssetRipper_{DateTime.Now:yyyyMMdd_HHmmss}.log")) { }

	/// <param name="filePath">The absolute path to the log file</param>
	public FileLoggerBase(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Invalid path", nameof(filePath));
		}

		this.filePath = filePath;

		RotateLogs(this.filePath);

		using FileStream stream = File.Create(this.filePath);
	}

	private static void RotateLogs(string path)
	{
		const int MaxLogFiles = 5;
		string? directory = Path.GetDirectoryName(path);
		if (directory is null)
		{
			return;
		}
		string prefix = Path.GetFileNameWithoutExtension(path).Split('_')[0];
		string extension = Path.GetExtension(path);

		var logFiles = new DirectoryInfo(directory)
			.GetFiles($"{prefix}_*{extension}")
			.OrderBy(f => f.Name)
			.ToArray();

		if (logFiles.Length >= MaxLogFiles)
		{
			for (int i = 0; i <= logFiles.Length - MaxLogFiles; i++)
			{
				logFiles[i].Delete();
			}
		}
	}

	public abstract void Log(LogType type, LogCategory category, string message);

	public void BlankLine(int numLines)
	{
		try
		{
			File.AppendAllLines(filePath, Enumerable.Repeat("", numLines));
		}
		catch (IOException)
		{
			//Could not log to file
		}
	}
}
