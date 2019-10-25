using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Project.Classes
{
	public sealed class ExportPointer : IYAMLExportable
	{
		public ExportPointer(long fileID)
		{
			FileID = fileID;
			GUID = default;
			AssetType = default;
		}

		public ExportPointer(long fileID, GUID guid, AssetType assetType)
		{
			FileID = fileID;
			GUID = guid;
			AssetType = assetType;
		}

		public ExportPointer(ClassIDType classID, AssetType assetType) :
			this(ExportCollection.GetMainExportID((uint)classID), GUID.MissingReference, assetType)
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

		public static ExportPointer EmptyPointer { get; } = new ExportPointer(0);

		public long FileID { get; }
		public GUID GUID { get; }
		public AssetType AssetType { get; }

		public const string FileIDName = "fileID";
		public const string GuidName = "guid";
		public const string TypeName = "type";
	}
}
