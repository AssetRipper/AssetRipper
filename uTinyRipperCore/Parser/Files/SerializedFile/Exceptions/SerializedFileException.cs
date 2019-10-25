using System;
using System.Text;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileException : Exception
	{
		public SerializedFileException(string message, Version version, string fileName, string filePath):
			base(message)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			Version = version;
			FileName = fileName;
			FilePath = filePath;
		}

		public SerializedFileException(string message, Exception innerException, Version version, string fileName, string filePath):
			base(message, innerException)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			Version = version;
			FileName = fileName;
			FilePath = filePath;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SerializedFileException:");
			sb.Append(" v:").Append(Version.ToString());
			sb.Append(" n:").Append(FileName);
			sb.Append(" p:").Append(FilePath).AppendLine();
			sb.Append("Message: ").Append(Message).AppendLine();
			if(InnerException != null)
			{
				sb.Append("Inner: ").Append(InnerException.ToString()).AppendLine();
			}
			sb.Append("StackTrace: ").Append(StackTrace);
			return sb.ToString();
		}

		public Version Version { get; }
		public string FileName { get; }
		public string FilePath { get; }
	}
}
