using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Meta
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
				node.Add(GuidName, GUID.ExportYaml(container));
				node.Add(TypeName, (int)AssetType);
			}
			return node;
		}

		public static MetaPtr NullPtr { get; } = new MetaPtr(0);

		public static MetaPtr CreateMissingReference(ClassIDType classID, AssetType assetType)
		{
			return new MetaPtr(ExportIdHandler.GetMainExportID((uint)classID), UnityGUID.MissingReference, assetType);
		}

		private const string FileIDName = "fileID";
		private const string GuidName = "guid";
		private const string TypeName = "type";
	}
}
