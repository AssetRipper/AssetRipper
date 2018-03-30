using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeChannel previously
	/// </summary>
	public struct BlendShapeChannel : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Name = stream.ReadStringAligned();
			NameHash = stream.ReadUInt32();
			FrameIndex = stream.ReadInt32();
			FrameCount = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
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
