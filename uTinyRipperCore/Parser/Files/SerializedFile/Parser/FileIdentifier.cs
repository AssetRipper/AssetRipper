using uTinyRipper.Classes.Misc;

namespace uTinyRipper.SerializedFiles
{
#warning TODO: move to classes
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

		public bool IsFile(ISerializedFile file)
		{
			return file.Name == PathName;
		}
		
		public void Read(SerializedReader reader)
		{
			if (HasAssetPath(reader.Generation))
			{
				AssetPath = reader.ReadStringZeroTerm();
			}
			if (HasHash(reader.Generation))
			{
				Guid.Read(reader);
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
				Guid.Write(writer);
				writer.Write((int)Type);
			}
			writer.WriteStringZeroTerm(PathNameOrigin);
		}

		public string GetFilePath()
		{
			if (Type == AssetType.Meta)
			{
				return Guid.ToString();
			}
			return PathName;
		}

		public override string ToString()
		{
			if (Type == AssetType.Meta)
			{
				return Guid.ToString();
			}
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

		public UnityGUID Guid;
	}
}
