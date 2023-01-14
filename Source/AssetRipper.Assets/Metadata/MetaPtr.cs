using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.IO.Files;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.Metadata
{
	public readonly record struct MetaPtr(long FileID, UnityGUID GUID, AssetType AssetType) : IYamlExportable
	{
		public MetaPtr(long fileID) : this(fileID, UnityGUID.Zero, AssetType.Serialized)
		{
		}

		public YamlNode ExportYaml(IExportContainer container)
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

		public static MetaPtr NullPtr { get; } = new MetaPtr(0);

		public static MetaPtr CreateMissingReference(int classID, AssetType assetType)
		{
			return new MetaPtr(ExportIdHandler.GetMainExportID((uint)classID), UnityGUID.MissingReference, assetType);
		}

		private const string FileIDName = "fileID";
		private const string GuidName = "guid";
		private const string TypeName = "type";
	}
}
