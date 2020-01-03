using System;
using System.Text;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileException : Exception
	{
		public SerializedFileException(string message, Version version, Platform platform, ClassIDType assetType, string fileName, string filePath):
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
			Platform = platform;
			AssetType = assetType;
			FileName = fileName;
			FilePath = filePath;
		}

		public SerializedFileException(string message, Exception innerException, Version version, Platform platform, ClassIDType assetType, string fileName, string filePath):
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
			Platform = platform;
			AssetType = assetType;
			FileName = fileName;
			FilePath = filePath;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SerializedFileException:");
			sb.Append(" v:").Append(Version.ToString());
			sb.Append(" p:").Append(Platform.ToString());
			sb.Append(" t:").Append(AssetType.ToString());
			sb.Append(" n:").Append(FileName).AppendLine();
			sb.Append("Path:").Append(FilePath).AppendLine();
			sb.Append("Message: ").Append(Message).AppendLine();
			if(InnerException != null)
			{
				sb.Append("Inner: ").Append(InnerException.ToString()).AppendLine();
			}
			sb.Append("StackTrace: ").Append(StackTrace);
			return sb.ToString();
		}

		public Version Version { get; }
		public Platform Platform { get; }
		public ClassIDType AssetType { get; }
		public string FileName { get; }
		public string FilePath { get; }
	}
}
