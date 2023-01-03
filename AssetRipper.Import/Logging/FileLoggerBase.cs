using AssetRipper.Core.Utils;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Logging
{
	public abstract class FileLoggerBase : ILogger
	{
		protected readonly string filePath;

		public FileLoggerBase() : this(ExecutingDirectory.Combine("AssetRipper.log")) { }

		/// <param name="filePath">The absolute path to the log file</param>
		public FileLoggerBase(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("Invalid path", nameof(filePath));
			}

			this.filePath = filePath;

			File.Create(this.filePath).Close();
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
}
