using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct SnapshotConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			NameHash = stream.ReadUInt32();
			m_values = stream.ReadSingleArray();
			m_transitionTypes = stream.ReadUInt32Array();
			m_transitionIndices = stream.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("nameHash", NameHash);
			node.Add("values", Values.ExportYAML());
			node.Add("transitionTypes", TransitionTypes.ExportYAML(true));
			node.Add("transitionIndices", TransitionIndices.ExportYAML(true));
			return node;
		}

		public uint NameHash { get; private set; }
		public IReadOnlyList<float> Values => m_values;
		public IReadOnlyList<uint> TransitionTypes => m_transitionTypes;
		public IReadOnlyList<uint> TransitionIndices => m_transitionIndices;

		private float[] m_values;
		private uint[] m_transitionTypes;
		private uint[] m_transitionIndices;
	}
}
