using SevenZip;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeChannel previously
	/// </summary>
	public struct BlendShapeChannel : IAssetReadable, IYAMLExportable
	{
		public BlendShapeChannel(string name, int frameIndex, int frameCount)
		{
			Name = name;
			NameHash = CRC.CalculateDigestUTF8(Name);
			FrameIndex = frameIndex;
			FrameCount = frameCount;
		}
		
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			NameHash = reader.ReadUInt32();
			FrameIndex = reader.ReadInt32();
			FrameCount = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name);
			node.Add(NameHashName, NameHash);
			node.Add(FrameIndexName, FrameIndex);
			node.Add(FrameCountName, FrameCount);
			return node;
		}

		public string Name { get; private set; }
		public uint NameHash { get; private set; }
		public int FrameIndex { get; private set; }
		public int FrameCount { get; private set; }

		public const string NameName = "name";
		public const string NameHashName = "nameHash";
		public const string FrameIndexName = "frameIndex";
		public const string FrameCountName = "frameCount";
	}
}
