using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class BlendShapeData : IBlendShapeData
	{
		public IBlendShapeVertex[] Vertices { get; set; } = Array.Empty<IBlendShapeVertex>();
		public IMeshBlendShape[] Shapes { get; set; } = Array.Empty<IMeshBlendShape>();
		public IMeshBlendShapeChannel[] Channels { get; set; } = Array.Empty<IMeshBlendShapeChannel>();
		public float[] FullWeights { get; set; } = Array.Empty<float>();

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2017);

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
			Shapes = reader.ReadAssetArray<MeshBlendShape>();
			Channels = reader.ReadAssetArray<MeshBlendShapeChannel>();
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

		public const string VerticesName = "vertices";
		public const string ShapesName = "shapes";
		public const string ChannelsName = "channels";
		public const string FullWeightsName = "fullWeights";
	}
}
