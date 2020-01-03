using uTinyRipper.Classes.Misc;

namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// A serialized file may be linked with other serialized files to create shared dependencies.
	/// </summary>
	public struct FileIdentifier : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasAssetName(FileGeneration generation) => generation >= FileGeneration.FG_210_261;
		/// <summary>
		/// 1.2.0 and greater
		/// </summary>
		public static bool HasHash(FileGeneration generation) => generation >= FileGeneration.FG_120_200;

		public bool IsFile(ISerializedFile file)
		{
			return file.Name == FilePath;
		}
		
		public void Read(SerializedReader reader)
		{
			if (HasAssetName(reader.Generation))
			{
				AssetPath = reader.ReadStringZeroTerm();
			}
			if (HasHash(reader.Generation))
			{
				Hash.Read(reader);
				Type = (AssetType)reader.ReadInt32();
			}
			FilePathOrigin = reader.ReadStringZeroTerm();
			FilePath = FilenameUtils.FixFileIdentifier(FilePathOrigin);
		}

		public void Write(SerializedWriter writer)
		{
			if (HasAssetName(writer.Generation))
			{
				writer.WriteStringZeroTerm(AssetPath);
			}
			if (HasHash(writer.Generation))
			{
				Hash.Write(writer);
				writer.Write((int)Type);
			}
			writer.WriteStringZeroTerm(FilePathOrigin);
		}

		public string GetFilePath()
		{
			if (Type == AssetType.Meta)
			{
				return Hash.ToString();
			}
			return FilePath;
		}

		public override string ToString()
		{
			if (Type == AssetType.Meta)
			{
				return Hash.ToString();
			}
			return FilePathOrigin ?? base.ToString();
		}

		/// <summary>
		/// File path without such prefixes as archive:/directory/fileName
		/// </summary>
		public string FilePath { get; set; }

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
		public string FilePathOrigin { get; set; }

		/// <summary>
		/// Globally unique identifier of the file (or Hash?), 16 bytes long.
		/// Engine apparently always uses the big endian format and when converted to text,
		/// the GUID is a simple 32 character hex string with swapped characters for each byte.
		/// </summary>
		public Hash128 Hash;
	}
}
