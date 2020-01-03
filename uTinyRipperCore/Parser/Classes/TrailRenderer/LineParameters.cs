using uTinyRipper.YAML;
using uTinyRipper.Converters.TrailRenderers;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.TrailRenderers
{
	public struct LineParameters : IAsset
	{
		public LineParameters(Version version)
		{
			WidthMultiplier = 1.0f;
			WidthCurve = new AnimationCurveTpl<Float>(false);
			WidthCurve.Curve = new KeyframeTpl<Float>[] { new KeyframeTpl<Float>(0.0f, 1.0f, KeyframeTpl<Float>.DefaultFloatWeight), };
			ColorGradient = new Classes.Gradient(ColorRGBAf.White, ColorRGBAf.White);
			NumCornerVertices = 0;
			NumCapVertices = 0;
			Alignment = LineAlignment.View;
			TextureMode = LineTextureMode.Stretch;
			ShadowBias = 0.5f;
			GenerateLightingData = false;
		}

		public static int ToSerializedVersion(Version version)
		{
			// ShadowBias default value has been changed from 0 to 0.5
			if (version.IsGreaterEqual(2018, 3))
			{
				return 3;
			}
			// min version is 2nd 
			return 2;
		}

		/// <summary>
		/// 2018.3.0 and greater
		/// </summary>
		public static bool HasShadowBias(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasGenerateLightingData(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);

		public LineParameters Convert(IExportContainer container)
		{
			return LineParametersConverter.Convert(container, ref this);
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
			if (HasShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			if (HasGenerateLightingData(reader.Version))
			{
				GenerateLightingData = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(WidthMultiplier);
			WidthCurve.Write(writer);
			ColorGradient.Write(writer);
			writer.Write(NumCornerVertices);
			writer.Write(NumCapVertices);
			writer.Write((int)Alignment);
			writer.Write((int)TextureMode);
			if (HasShadowBias(writer.Version))
			{
				writer.Write(ShadowBias);
			}
			if (HasGenerateLightingData(writer.Version))
			{
				writer.Write(GenerateLightingData);
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(WidthMultiplierName, WidthMultiplier);
			node.Add(WidthCurveName, WidthCurve.ExportYAML(container));
			node.Add(ColorGradientName, ColorGradient.ExportYAML(container));
			node.Add(NumCornerVerticesName, NumCornerVertices);
			node.Add(NumCapVerticesName, NumCapVertices);
			node.Add(AlignmentName, (int)Alignment);
			node.Add(TextureModeName, (int)TextureMode);
			if (HasShadowBias(container.ExportVersion))
			{
				node.Add(ShadowBiasName, ShadowBias);
			}
			if (HasGenerateLightingData(container.ExportVersion))
			{
				node.Add(GenerateLightingDataName, GenerateLightingData);
			}
			return node;
		}

		public float WidthMultiplier { get; set; }
		public int NumCornerVertices { get; set; }
		public int NumCapVertices { get; set; }
		public LineAlignment Alignment { get; set; }
		public LineTextureMode TextureMode { get; set; }
		public float ShadowBias { get; set; }
		public bool GenerateLightingData { get; set; }

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
		public Classes.Gradient ColorGradient;
	}
}
