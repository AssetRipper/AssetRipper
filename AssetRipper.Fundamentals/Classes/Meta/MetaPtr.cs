﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Meta
{
	public sealed class MetaPtr : IYamlExportable
	{
		public MetaPtr(long fileID)
		{
			FileID = fileID;
			GUID = new();
			AssetType = new();
		}

		public MetaPtr(long fileID, UnityGUID guid, AssetType assetType)
		{
			FileID = fileID;
			GUID = guid;
			AssetType = assetType;
		}

		public MetaPtr(ClassIDType classID, AssetType assetType) : this(ExportIdHandler.GetMainExportID((uint)classID), UnityGUID.MissingReference, assetType) { }

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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

		public long FileID { get; }
		public UnityGUID GUID { get; }
		public AssetType AssetType { get; }

		public const string FileIDName = "fileID";
		public const string GuidName = "guid";
		public const string TypeName = "type";
	}
}
