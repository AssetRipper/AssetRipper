using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class MetaPtr : IYAMLExportable
	{
		public MetaPtr(long fileID)
		{
			FileID = fileID;
			GUID = default;
			AssetType = default;
		}

		public MetaPtr(long fileID, UnityGUID guid, AssetType assetType)
		{
			FileID = fileID;
			GUID = guid;
			AssetType = assetType;
		}

		public MetaPtr(ClassIDType classID, AssetType assetType) :
			this(ExportCollection.GetMainExportID((uint)classID), UnityGUID.MissingReference, assetType)
		{
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(FileIDName, FileID);
			if (!GUID.IsZero)
			{
				node.Add(GuidName, GUID.ExportYAML(container));
				node.Add(TypeName, (int)AssetType);
			}
			return node;
		}

		public static MetaPtr NullPtr { get; } = new MetaPtr(0);

		public long FileID { get; }
		public UnityGUID GUID { get; }
		public AssetType AssetType { get; }

		public const string FileIDName = "fileID";
		public const string GuidName = "guid";
		public const string TypeName = "type";
	}
}
