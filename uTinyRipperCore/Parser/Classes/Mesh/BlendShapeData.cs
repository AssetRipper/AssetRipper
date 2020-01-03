using System;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes.Meshes
{
	public struct BlendShapeData : IAsset
	{
		public BlendShapeData(Version version)
		{
			Vertices = Array.Empty<BlendShapeVertex>();
			Shapes = Array.Empty<BlendShape>();
			Channels = Array.Empty<BlendShapeChannel>();
			FullWeights = Array.Empty<float>();
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
				reader.AlignStream();
			}

			FullWeights = reader.ReadSingleArray();
		}

		public void Write(AssetWriter writer)
		{
			Vertices.Write(writer);
			Shapes.Write(writer);
			Channels.Write(writer);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}

			FullWeights.Write(writer);
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
