using System;
using System.IO;
using System.Text;

namespace AssetRipper.Logging
{
	public class FileLogger : ILogger
	{
		private readonly string filePath;

		public FileLogger() : this("AssetRipper.log") { }

		public FileLogger(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Invalid path", nameof(filePath));

			this.filePath = filePath;

			File.Create(this.filePath).Close();
		}

		public void Log(LogType type, LogCategory category, string message)
		{
#if !DEBUG
			if (type == LogType.Debug)
			{
				return;
			}
#endif
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(category.ToString());
			switch (type)
			{
				case LogType.Warning:
				case LogType.Error:
					stringBuilder.Append(" [");
					stringBuilder.Append(type.ToString());
					stringBuilder.Append("]");
					break;
			}
			stringBuilder.Append(": ");
			stringBuilder.Append(message);
			stringBuilder.Append(Environment.NewLine);
			try
			{
				File.AppendAllText(filePath, stringBuilder.ToString());
			}
			catch(System.IO.IOException)
			{
				//Could not log to file
			}
		}
	}
}
