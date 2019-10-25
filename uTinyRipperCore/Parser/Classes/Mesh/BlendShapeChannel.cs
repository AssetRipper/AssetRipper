using SevenZip;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeChannel previously
	/// </summary>
	public struct BlendShapeChannel : IAsset
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

		public void Write(AssetWriter writer)
		{
			writer.Write(Name);
			writer.Write(NameHash);
			writer.Write(FrameIndex);
			writer.Write(FrameCount);
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

		public string Name { get; set; }
		public uint NameHash { get; set; }
		public int FrameIndex { get; set; }
		public int FrameCount { get; set; }

		public const string NameName = "name";
		public const string NameHashName = "nameHash";
		public const string FrameIndexName = "frameIndex";
		public const string FrameCountName = "frameCount";
	}
}
