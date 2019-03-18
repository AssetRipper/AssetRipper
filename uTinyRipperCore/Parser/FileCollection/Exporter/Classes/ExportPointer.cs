using System;
using uTinyRipper.Classes;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public sealed class ExportPointer : IYAMLExportable
	{
		public ExportPointer(long fileID)
		{
			FileID = fileID;
			GUID = default;
			AssetType = default;
		}

		public ExportPointer(long fileID, EngineGUID guid, AssetType assetType)
		{
			FileID = fileID;
			GUID = guid;
			AssetType = assetType;
		}

		public ExportPointer(ClassIDType classID, AssetType assetType) :
			this(ExportCollection.GetMainExportID((uint)classID), EngineGUID.MissingReference, assetType)
		{
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("fileID", FileID);
			if(!GUID.IsZero)
			{
				node.Add("guid", GUID.ExportYAML(container));
				node.Add("type", (int)AssetType);
			}
			return node;
		}

		public static ExportPointer EmptyPointer { get; } = new ExportPointer(0);

		public long FileID { get; }
		public EngineGUID GUID { get; }
		public AssetType AssetType { get; }
	}
}
