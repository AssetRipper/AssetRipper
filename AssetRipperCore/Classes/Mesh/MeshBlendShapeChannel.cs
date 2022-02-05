using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class MeshBlendShapeChannel : IMeshBlendShapeChannel
	{
		public MeshBlendShapeChannel() { }
		public MeshBlendShapeChannel(string name, int frameIndex, int frameCount)
		{
			this.SetValues(name, frameIndex, frameCount);
		}

		public void Read(AssetReader reader)
		{
			Name.Read(reader);
			NameHash = reader.ReadUInt32();
			FrameIndex = reader.ReadInt32();
			FrameCount = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.WriteAsset(Name);
			writer.Write(NameHash);
			writer.Write(FrameIndex);
			writer.Write(FrameCount);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name.String);
			node.Add(NameHashName, NameHash);
			node.Add(FrameIndexName, FrameIndex);
			node.Add(FrameCountName, FrameCount);
			return node;
		}

		public Utf8StringBase Name { get; } = new Utf8StringLegacy();
		public uint NameHash { get; set; }
		public int FrameIndex { get; set; }
		public int FrameCount { get; set; }

		public const string NameName = "name";
		public const string NameHashName = "nameHash";
		public const string FrameIndexName = "frameIndex";
		public const string FrameCountName = "frameCount";
	}
}
