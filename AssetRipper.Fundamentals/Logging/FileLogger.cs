using AssetRipper.Core.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Core.Logging
{
	public class FileLogger : ILogger
	{
		private readonly string filePath;

		public FileLogger() : this(ExecutingDirectory.Combine("AssetRipper.log")) { }

		/// <param name="filePath">The absolute path to the log file</param>
		public FileLogger(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Invalid path", nameof(filePath));

			this.filePath = filePath;

			File.Create(this.filePath).Close();
		}

		public void Log(LogType type, LogCategory category, string message)
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (category != LogCategory.None)
				stringBuilder.Append($"{category.ToString()} ");
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
			catch (System.IO.IOException)
			{
				//Could not log to file
			}
		}

		public void BlankLine(int numLines)
		{
			try
			{
				File.AppendAllLines(filePath, Enumerable.Repeat("", 5).ToArray());
			}
			catch (System.IO.IOException)
			{
				//Could not log to file
			}
		}
	}
}
