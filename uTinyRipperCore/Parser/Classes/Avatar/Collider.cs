using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Collider : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(XName, X.ExportYAML(container));
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

		public uint Type { get; private set; }
		public uint XMotionType { get; private set; }
		public uint YMotionType { get; private set; }
		public uint ZMotionType { get; private set; }
		public float MinLimitX { get; private set; }
		public float MaxLimitX { get; private set; }
		public float MaxLimitY { get; private set; }
		public float MaxLimitZ { get; private set; }

		public const string XName = "m_X";
		public const string TypeName = "m_Type";
		public const string XMotionTypeName = "m_XMotionType";
		public const string YMotionTypeName = "m_YMotionType";
		public const string ZMotionTypeName = "m_ZMotionType";
		public const string MinLimitXName = "m_MinLimitX";
		public const string MaxLimitXName = "m_MaxLimitX";
		public const string MaxLimitYName = "m_MaxLimitY";
		public const string MaxLimitZName = "m_MaxLimitZ";

		public XForm X;
	}
}
