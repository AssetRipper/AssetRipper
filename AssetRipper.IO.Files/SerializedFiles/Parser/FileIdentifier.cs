using AssetRipper.IO.Files.SerializedFiles.IO;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.SerializedFiles.Parser
{
	/// <summary>
	/// A serialized file may be linked with other serialized files to create shared dependencies.
	/// </summary>
	public struct FileIdentifier : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasAssetPath(FormatVersion generation) => generation >= FormatVersion.Unknown_6;
		/// <summary>
		/// 1.2.0 and greater
		/// </summary>
		public static bool HasHash(FormatVersion generation) => generation >= FormatVersion.Unknown_5;

		public bool IsFile(SerializedFile? file)
		{
			return file is not null && file.Name == PathName;
		}

		public void Read(SerializedReader reader)
		{
			if (HasAssetPath(reader.Generation))
			{
				AssetPath = reader.ReadStringZeroTerm();
			}
			if (HasHash(reader.Generation))
			{
				//Commented out during creation of AssetRipper.IO.Files
				//Guid.Read(reader);
				Guid = reader.ReadBytes(16);
				Type = (AssetType)reader.ReadInt32();
			}
			PathNameOrigin = reader.ReadStringZeroTerm();
			PathName = FilenameUtils.FixFileIdentifier(PathNameOrigin);
		}

		public void Write(SerializedWriter writer)
		{
			if (HasAssetPath(writer.Generation))
			{
				writer.WriteStringZeroTerm(AssetPath);
			}
			if (HasHash(writer.Generation))
			{
				//Commented out during creation of AssetRipper.IO.Files
				//Guid.Write(writer);
				writer.Write(Guid ?? new byte[16]);
				writer.Write((int)Type);
			}
			writer.WriteStringZeroTerm(PathNameOrigin);
		}

		public string GetFilePath()
		{
			//Commented out during creation of AssetRipper.IO.Files
			//if (Type == AssetType.Meta)
			//{
			//	return Guid.ToString();
			//}
			return PathName;
		}

		public override string? ToString()
		{
			//Commented out during creation of AssetRipper.IO.Files
			//if (Type == AssetType.Meta)
			//{
			//	return Guid.ToString();
			//}
			return PathNameOrigin ?? base.ToString();
		}

		/// <summary>
		/// File path without such prefixes as archive:/directory/fileName
		/// </summary>
		public string PathName { get; set; }

		/// <summary>
		/// Virtual asset path. Used for cached files, otherwise it's empty.
		/// The file with that path usually doesn't exist, so it's probably an alias.
		/// </summary>
		public string AssetPath { get; set; }
		/// <summary>
		/// The type of the file
		/// </summary>
		public AssetType Type { get; set; }
		/// <summary>
		/// Actual file path. This path is relative to the path of the current file.
		/// The folder "library" often needs to be translated to "resources" in order to find the file on the file system.
		/// </summary>
		public string PathNameOrigin { get; set; }

		//Commented out during creation of AssetRipper.IO.Files
		//public UnityGUID Guid;
		public byte[]? Guid { get; set; }
	}
}
