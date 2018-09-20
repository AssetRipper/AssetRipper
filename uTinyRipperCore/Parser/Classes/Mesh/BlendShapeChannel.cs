using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeChannel previously
	/// </summary>
	public struct BlendShapeChannel : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadStringAligned();
			NameHash = reader.ReadUInt32();
			FrameIndex = reader.ReadInt32();
			FrameCount = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("nameHash", NameHash);
			node.Add("frameIndex", FrameIndex);
			node.Add("frameCount", FrameCount);
			return node;
		}

		public string Name { get; private set; }
		public uint NameHash { get; private set; }
		public int FrameIndex { get; private set; }
		public int FrameCount { get; private set; }
	}
}
