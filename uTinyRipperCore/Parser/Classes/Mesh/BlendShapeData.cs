using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct BlendShapeData : IAsset
	{
		public BlendShapeData(Version version)
		{
			Vertices = new BlendShapeVertex[0];
			Shapes = new BlendShape[0];
			Channels = new BlendShapeChannel[0];
			FullWeights = ArrayExtensions.EmptyFloats;
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2017);

		public string FindShapeNameByCRC(uint crc)
		{
			foreach (BlendShapeChannel blendChannel in Channels)
			{
				if (blendChannel.NameHash == crc)
				{
					return blendChannel.Name;
				}
			}
			return null;
		}

		public BlendShapeData Convert()
		{
			BlendShapeData instance = new BlendShapeData();
			instance.Vertices = Vertices.ToArray();
			instance.Shapes = Shapes.ToArray();
			instance.Channels = Channels.ToArray();
			instance.FullWeights = FullWeights.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			Vertices = reader.ReadAssetArray<BlendShapeVertex>();
			Shapes = reader.ReadAssetArray<BlendShape>();
			Channels = reader.ReadAssetArray<BlendShapeChannel>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			FullWeights = reader.ReadSingleArray();
		}

		public void Write(AssetWriter writer)
		{
			writer.WriteAssetArray(Vertices);
			writer.WriteAssetArray(Shapes);
			writer.WriteAssetArray(Channels);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream(AlignType.Align4);
			}

			writer.WriteArray(FullWeights);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VerticesName, Vertices.ExportYAML(container));
			node.Add(ShapesName, Shapes.ExportYAML(container));
			node.Add(ChannelsName, Channels.ExportYAML(container));
			node.Add(FullWeightsName, FullWeights.ExportYAML());
			return node;
		}

		public BlendShapeVertex[] Vertices { get; set; }
		public BlendShape[] Shapes { get; set; }
		public BlendShapeChannel[] Channels { get; set; }
		public float[] FullWeights { get; set; }

		public const string VerticesName = "vertices";
		public const string ShapesName = "shapes";
		public const string ChannelsName = "channels";
		public const string FullWeightsName = "fullWeights";
	}
}
