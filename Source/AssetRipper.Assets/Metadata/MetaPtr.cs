using AssetRipper.IO.Files;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.Metadata
{
	public readonly record struct MetaPtr(long FileID, UnityGuid GUID, AssetType AssetType)
	{
		public MetaPtr(long fileID) : this(fileID, UnityGuid.Zero, AssetType.Serialized)
		{
		}

		public YamlNode ExportYaml()
		{
			YamlMappingNode node = new();
			node.Style = MappingStyle.Flow;
			node.Add(FileIDName, FileID);
			if (!GUID.IsZero)
			{
				node.Add(GuidName, GUID.ToString());
				node.Add(TypeName, (int)AssetType);
			}
			return node;
		}

		/// <summary>
		/// Write this pointer to a string.
		/// </summary>
		/// <remarks>
		/// This uses the same format as Yaml.
		/// </remarks>
		/// <returns>This pointer expressed as a string.</returns>
		public override string ToString()
		{
			if (GUID.IsZero)
			{
				return $"{{{FileIDName}: {FileID}}}";
			}
			else
			{
				return $"{{{FileIDName}: {FileID}, {GuidName}: {GUID}, {TypeName}: {(int)AssetType}}}";
			}
		}

		public static MetaPtr NullPtr { get; } = new MetaPtr(0);

		public static MetaPtr CreateMissingReference(int classID, AssetType assetType)
		{
			return new MetaPtr(ExportIdHandler.GetMainExportID(classID), UnityGuid.MissingReference, assetType);
		}

		private const string FileIDName = "fileID";
		private const string GuidName = "guid";
		private const string TypeName = "type";
	}
}
