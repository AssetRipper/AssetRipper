﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Collider : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			Type = reader.ReadUInt32();
			XMotionType = reader.ReadUInt32();
			YMotionType = reader.ReadUInt32();
			ZMotionType = reader.ReadUInt32();
			MinLimitX = reader.ReadSingle();
			MaxLimitX = reader.ReadSingle();
			MaxLimitY = reader.ReadSingle();
			MaxLimitZ = reader.ReadSingle();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(XName, X.ExportYaml(container));
			node.Add(TypeName, Type);
			node.Add(XMotionTypeName, XMotionType);
			node.Add(YMotionTypeName, YMotionType);
			node.Add(ZMotionTypeName, ZMotionType);
			node.Add(MinLimitXName, MinLimitX);
			node.Add(MaxLimitXName, MaxLimitX);
			node.Add(MaxLimitYName, MaxLimitY);
			node.Add(MaxLimitZName, MaxLimitZ);
			return node;
		}

		public uint Type { get; set; }
		public uint XMotionType { get; set; }
		public uint YMotionType { get; set; }
		public uint ZMotionType { get; set; }
		public float MinLimitX { get; set; }
		public float MaxLimitX { get; set; }
		public float MaxLimitY { get; set; }
		public float MaxLimitZ { get; set; }

		public const string XName = "m_X";
		public const string TypeName = "m_Type";
		public const string XMotionTypeName = "m_XMotionType";
		public const string YMotionTypeName = "m_YMotionType";
		public const string ZMotionTypeName = "m_ZMotionType";
		public const string MinLimitXName = "m_MinLimitX";
		public const string MaxLimitXName = "m_MaxLimitX";
		public const string MaxLimitYName = "m_MaxLimitY";
		public const string MaxLimitZName = "m_MaxLimitZ";

		public XForm X = new();
	}
}
