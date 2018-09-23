using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct CompressedAnimationCurve : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Path = reader.ReadString();
			Times.Read(reader);
			Values.Read(reader);
			Slopes.Read(reader);
			PreInfinity = reader.ReadInt32();
			PostInfinity = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Path", Path);
			node.Add("m_Times", Times.ExportYAML(container));
			node.Add("m_Values", Values.ExportYAML(container));
			node.Add("m_Slopes", Slopes.ExportYAML(container));
			node.Add("m_PreInfinity", PreInfinity);
			node.Add("m_PostInfinity", PostInfinity);
			return node;
		}

		public string Path { get; private set; }
		public int PreInfinity { get; private set; }
		public int PostInfinity { get; private set; }

		public PackedIntVector Times;
		public PackedQuatVector Values;
		public PackedFloatVector Slopes;
	}
}
