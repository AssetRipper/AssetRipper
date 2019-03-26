using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct BlendShapeData : IAssetReadable, IYAMLExportable
	{
		public BlendShapeData(bool _)
		{
			m_vertices = new BlendShapeVertex[0];
			m_shapes = new BlendShape[0];
			m_channels = new BlendShapeChannel[0];
			m_fullWeights = new float[0];
		}

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

		public string FindShapeNameByCRC(Version version, uint crc)
		{
			if (IsReadChannels(version))
			{
				foreach (BlendShapeChannel blendChannel in Channels)
				{
					if (blendChannel.NameHash == crc)
					{
						return blendChannel.Name;
					}
				}
			}
			else
			{
				foreach (BlendShape blendShape in Shapes)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name;
					}
				}
			}
			return null;
		}

		public void Read(AssetReader reader)
		{
			if (IsReadChannels(reader.Version))
			{
				m_vertices = reader.ReadAssetArray<BlendShapeVertex>();
				m_shapes = reader.ReadAssetArray<BlendShape>();
				m_channels = reader.ReadAssetArray<BlendShapeChannel>();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				m_fullWeights = reader.ReadSingleArray();
			}
			else
			{
				m_shapes = reader.ReadAssetArray<BlendShape>();
				reader.AlignStream(AlignType.Align4);
				m_vertices = reader.ReadAssetArray<BlendShapeVertex>();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VerticesName, m_vertices.ExportYAML(container));
			node.Add(ShapesName, m_shapes.ExportYAML(container));
			node.Add(ChannelsName, GetChannels(container.Version).ExportYAML(container));
			node.Add(FullWeightsName, GetFullWeights(container.Version).ExportYAML());
			return node;
		}

		private IReadOnlyList<BlendShapeChannel> GetChannels(Version version)
		{
			if (IsReadChannels(version))
			{
				return m_channels;
			}
			else
			{
				BlendShapeChannel[] channels = new BlendShapeChannel[m_shapes.Length];
				for (int i = 0; i < m_shapes.Length; i++)
				{
					BlendShape shape = m_shapes[i];
					channels[i] = new BlendShapeChannel(shape.Name, i, 1);
				}
				return channels;
			}
		}

		private IReadOnlyList<float> GetFullWeights(Version version)
		{
			if (IsReadChannels(version))
			{
				return m_fullWeights;
			}
			else
			{
				float[] fullWeights = new float[m_shapes.Length];
				for (int i = 0; i < fullWeights.Length; i++)
				{
					fullWeights[i] = 100.0f;
				}
				return fullWeights;
			}
		}

		public IReadOnlyList<BlendShapeVertex> Vertices => m_vertices;
		public IReadOnlyList<BlendShape> Shapes => m_shapes;
		public IReadOnlyList<BlendShapeChannel> Channels => m_channels;
		public IReadOnlyList<float> FullWeights => m_fullWeights;

		public const string VerticesName = "vertices";
		public const string ShapesName = "shapes";
		public const string ChannelsName = "channels";
		public const string FullWeightsName = "fullWeights";

		private BlendShapeVertex[] m_vertices;
		private BlendShape[] m_shapes;
		private BlendShapeChannel[] m_channels;
		private float[] m_fullWeights;
	}
}
