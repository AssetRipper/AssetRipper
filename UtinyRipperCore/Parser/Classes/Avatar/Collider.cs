using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct Collider : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			X.Read(stream);
			Type = stream.ReadUInt32();
			XMotionType = stream.ReadUInt32();
			YMotionType = stream.ReadUInt32();
			ZMotionType = stream.ReadUInt32();
			MinLimitX = stream.ReadSingle();
			MaxLimitX = stream.ReadSingle();
			MaxLimitY = stream.ReadSingle();
			MaxLimitZ = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_X", X.ExportYAML(container));
			node.Add("m_Type", Type);
			node.Add("m_XMotionType", XMotionType);
			node.Add("m_YMotionType", YMotionType);
			node.Add("m_ZMotionType", ZMotionType);
			node.Add("m_MinLimitX", MinLimitX);
			node.Add("m_MaxLimitX", MaxLimitX);
			node.Add("m_MaxLimitY", MaxLimitY);
			node.Add("m_MaxLimitZ", MaxLimitZ);
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

		public XForm X;
	}
}
