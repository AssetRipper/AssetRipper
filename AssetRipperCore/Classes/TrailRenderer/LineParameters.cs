using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Converters.TrailRenderer;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.TrailRenderer
{
	public sealed class LineParameters : IAsset
	{
		public LineParameters() { }
		public LineParameters(UnityVersion version)
		{
			WidthMultiplier = 1.0f;
			WidthCurve = new AnimationCurveTpl<Float>(false);
			WidthCurve.Curve = new KeyframeTpl<Float>[] { new KeyframeTpl<Float>(0.0f, 1.0f, KeyframeTpl<Float>.DefaultFloatWeight), };
			ColorGradient = new Misc.Serializable.Gradient.Gradient(ColorRGBAf.White, ColorRGBAf.White);
			NumCornerVertices = 0;
			NumCapVertices = 0;
			Alignment = LineAlignment.View;
			TextureMode = LineTextureMode.Stretch;
			ShadowBias = 0.5f;
			GenerateLightingData = false;
		}

		public LineParameters Clone()
		{
			LineParameters result = new LineParameters();
			result.WidthMultiplier = WidthMultiplier;
			result.WidthCurve = WidthCurve;
			result.ColorGradient = ColorGradient;
			result.NumCornerVertices = NumCornerVertices;
			result.NumCapVertices = NumCapVertices;
			result.Alignment = Alignment;
			result.TextureMode = TextureMode;
			result.ShadowBias = ShadowBias;
			result.GenerateLightingData = GenerateLightingData;
			return result;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// ShadowBias default value has been changed from 0 to 0.5
			if (HasShadowBias(version))
			{
				return 3;
			}

			if(HasAnimationCurve(version))
			{
				return 2;
			}

			return 1;
		}

		/// <summary>
		/// 5.5.0f3 and greater
		/// </summary>
		public static bool HasAnimationCurve(UnityVersion version) => version.IsGreaterEqual(5, 5, 0, UnityVersionType.Final, 3);
		/// <summary>
		/// 2018.3.0 and greater
		/// </summary>
		public static bool HasShadowBias(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasGenerateLightingData(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);

		public LineParameters Convert(IExportContainer container)
		{
			return LineParametersConverter.Convert(container, this);
		}

		public void Read(AssetReader reader)
		{
			if (HasAnimationCurve(reader.Version))
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
			else
			{
				StartWidth = reader.ReadSingle();
				EndWidth = reader.ReadSingle();
				StartColor.Read(reader);
				EndColor.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			if (HasAnimationCurve(writer.Version))
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
			else
			{
				writer.Write(StartWidth);
				writer.Write(EndWidth);
				StartColor.Write(writer);
				EndColor.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasAnimationCurve(container.ExportVersion))
			{
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
			}
			else
			{
				node.Add("startWidth", StartWidth);
				node.Add("endWidth", EndWidth);
				node.Add("m_StartColor", StartColor.ExportYAML(container));
				node.Add("m_EndColor", EndColor.ExportYAML(container));
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

		public float StartWidth { get; set; }
		public float EndWidth { get; set; }

		public const string WidthMultiplierName = "widthMultiplier";
		public const string WidthCurveName = "widthCurve";
		public const string ColorGradientName = "colorGradient";
		public const string NumCornerVerticesName = "numCornerVertices";
		public const string NumCapVerticesName = "numCapVertices";
		public const string AlignmentName = "alignment";
		public const string TextureModeName = "textureMode";
		public const string ShadowBiasName = "shadowBias";
		public const string GenerateLightingDataName = "generateLightingData";

		public AnimationCurveTpl<Float> WidthCurve = new();
		public Misc.Serializable.Gradient.Gradient ColorGradient = new();

		public ColorRGBA32 StartColor = new();
		public ColorRGBA32 EndColor = new();
	}
}
