using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	public struct BlendShapeData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadChannels(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetReader reader)
		{
			if (IsReadChannels(reader.Version))
			{
				m_vertices = reader.ReadArray<BlendShapeVertex>();
				m_shapes = reader.ReadArray<BlendShape>();
				m_channels = reader.ReadArray<BlendShapeChannel>();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				m_fullWeights = reader.ReadSingleArray();
			}
			else
			{
				m_shapes = reader.ReadArray<BlendShape>();
				reader.AlignStream(AlignType.Align4);
				m_vertices = reader.ReadArray<BlendShapeVertex>();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("vertices", (m_vertices == null) ? YAMLSequenceNode.Empty : m_vertices.ExportYAML(container));
			node.Add("shapes", (m_shapes == null) ? YAMLSequenceNode.Empty : m_shapes.ExportYAML(container));
			node.Add("channels", IsReadChannels(container.Version) ? m_channels.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add("fullWeights", IsReadChannels(container.Version) ? m_fullWeights.ExportYAML() : YAMLSequenceNode.Empty);
			return node;
		}

		public IReadOnlyList<BlendShapeVertex> Vertices => m_vertices;
		public IReadOnlyList<BlendShape> Shapes => m_shapes;
		public IReadOnlyList<BlendShapeChannel> Channels => m_channels;
		public IReadOnlyList<float> FullWeights => m_fullWeights;

		private BlendShapeVertex[] m_vertices;
		private BlendShape[] m_shapes;
		private BlendShapeChannel[] m_channels;
		private float[] m_fullWeights;
	}
}
