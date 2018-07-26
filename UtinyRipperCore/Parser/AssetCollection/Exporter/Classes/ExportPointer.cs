using System;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public sealed class ExportPointer : IYAMLExportable
	{
		public ExportPointer(ulong fileID)
		{
			FileID = fileID;
			GUID = default;
			AssetType = default;
		}

		public ExportPointer(ulong fileID, UtinyGUID guid, AssetType assetType):
			this(fileID)
		{
			GUID = guid;
			AssetType = assetType;
		}

		public ExportPointer(ClassIDType classID, AssetType assetType) :
			this(ExportCollection.GetMainExportID((uint)classID), UtinyGUID.MissingReference, assetType)
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

		public static readonly ExportPointer EmptyPointer = new ExportPointer(0);

		public ulong FileID { get; }
		public UtinyGUID GUID { get; }
		public AssetType AssetType { get; }
	}
}
