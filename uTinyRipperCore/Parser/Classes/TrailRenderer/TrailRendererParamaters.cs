using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Parser.Classes.TrailRenderers
{
	public struct TrailRendererParameters : IAssetReadable, IYAMLExportable
	{
		public static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}
		/// <summary>
		/// 2019.2.0 and greater
		/// </summary>
		public static bool IsReadShadowBias(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		public void Read(AssetReader reader)
		{
			WidthMultiplier = reader.ReadSingle();
			WidthCurve.Read(reader);
			ColorGradient.Read(reader);
			NumCornerVertices = reader.ReadInt32();
			NumCapVertices = reader.ReadInt32();
			Alignment = (LineAlignment)reader.ReadInt32();
			TextureMode = (LineTextureMode)reader.ReadInt32();
			if (IsReadShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			GenerateLightingData = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(WidthMultiplierName, WidthMultiplier);
			node.Add(WidthCurveName, WidthCurve.ExportYAML(container));
			node.Add(ColorGradientName, ColorGradient.ExportYAML(container));
			node.Add(NumCornerVerticesName, NumCornerVertices);
			node.Add(NumCapVerticesName, NumCapVertices);
			node.Add(AlignmentName, (int)Alignment);
			node.Add(TextureModeName, (int)TextureMode);
			if (IsReadShadowBias(container.Version))
			{
				node.Add(ShadowBiasName, ShadowBias);
			}
			node.Add(GenerateLightingDataName, GenerateLightingData);
			return node;
		}
		public float WidthMultiplier { get; private set; }
		public int NumCornerVertices { get; private set; }
		public int NumCapVertices { get; private set; }
		public LineAlignment Alignment { get; private set; }
		public LineTextureMode TextureMode { get; private set; }
		public float ShadowBias { get; private set; }
		public bool GenerateLightingData { get; private set; }

		public const string WidthMultiplierName = "widthMultiplier";
		public const string WidthCurveName = "widthCurve";
		public const string ColorGradientName = "colorGradient";
		public const string NumCornerVerticesName = "numCornerVertices";
		public const string NumCapVerticesName = "numCapVertices";
		public const string AlignmentName = "alignment";
		public const string TextureModeName = "textureMode";
		public const string ShadowBiasName = "shadowBias";
		public const string GenerateLightingDataName = "generateLightingData";

		public AnimationCurveTpl<Float> WidthCurve;
		public Gradient ColorGradient;
	}
}
