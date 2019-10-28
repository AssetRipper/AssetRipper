using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct DetailPrototype : IAsset, IDependent
	{
		public static int ToSerializedVersion(Version version)
		{
			// this is min version
			return 2;
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasGrayscaleLighting(Version version) => version.IsLess(3);

		public void Read(AssetReader reader)
		{
			Prototype.Read(reader);
			PrototypeTexture.Read(reader);
			MinWidth = reader.ReadSingle();
			MaxWidth = reader.ReadSingle();
			MinHeight = reader.ReadSingle();
			MaxHeight = reader.ReadSingle();
			NoiseSpread = reader.ReadSingle();
			BendFactor = reader.ReadSingle();
			HealthyColor.Read(reader);
			DryColor.Read(reader);
			if (HasGrayscaleLighting(reader.Version))
			{
				GrayscaleLighting = reader.ReadInt32();
			}
			LightmapFactor = reader.ReadSingle();
			RenderMode = (DetailRenderMode)reader.ReadInt32();
			UsePrototypeMesh = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			Prototype.Write(writer);
			PrototypeTexture.Write(writer);
			writer.Write(MinWidth);
			writer.Write(MaxWidth);
			writer.Write(MinHeight);
			writer.Write(MaxHeight);
			writer.Write(NoiseSpread);
			writer.Write(BendFactor);
			writer.Write(BendFactor);
			HealthyColor.Write(writer);
			DryColor.Write(writer);
			if (HasGrayscaleLighting(writer.Version))
			{
				writer.Write(GrayscaleLighting);
			}
			writer.Write(LightmapFactor);
			writer.Write((int)RenderMode);
			writer.Write(UsePrototypeMesh);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Prototype, PrototypeName);
			yield return context.FetchDependency(PrototypeTexture, PrototypeTextureName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(PrototypeName, Prototype.ExportYAML(container));
			node.Add(PrototypeTextureName, PrototypeTexture.ExportYAML(container));
			node.Add(MinWidthName, MinWidth);
			node.Add(MaxWidthName, MaxWidth);
			node.Add(MinHeightName, MinHeight);
			node.Add(MaxHeightName, MaxHeight);
			node.Add(NoiseSpreadName, NoiseSpread);
			node.Add(BendFactorName, BendFactor);
			node.Add(HealthyColorName, HealthyColor.ExportYAML(container));
			node.Add(DryColorName, DryColor.ExportYAML(container));
			if (HasGrayscaleLighting(container.ExportVersion))
			{
				node.Add(GrayscaleLightingName, GrayscaleLighting);
			}
			node.Add(LightmapFactorName, LightmapFactor);
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(UsePrototypeMeshName, UsePrototypeMesh);
			return node;
		}

		public float MinWidth { get; set; }
		public float MaxWidth { get; set; }
		public float MinHeight { get; set; }
		public float MaxHeight { get; set; }
		public float NoiseSpread { get; set; }
		public float BendFactor { get; set; }
		public int GrayscaleLighting { get; set; }
		public float LightmapFactor { get; set; }
		public DetailRenderMode RenderMode { get; set; }
		public int UsePrototypeMesh { get; set; }

		public const string PrototypeName = "prototype";
		public const string PrototypeTextureName = "prototypeTexture";
		public const string MinWidthName = "minWidth";
		public const string MaxWidthName = "maxWidth";
		public const string MinHeightName = "minHeight";
		public const string MaxHeightName = "maxHeight";
		public const string NoiseSpreadName = "noiseSpread";
		public const string BendFactorName = "bendFactor";
		public const string HealthyColorName = "healthyColor";
		public const string DryColorName = "dryColor";
		public const string GrayscaleLightingName = "grayscaleLighting";
		public const string LightmapFactorName = "lightmapFactor";
		public const string RenderModeName = "renderMode";
		public const string UsePrototypeMeshName = "usePrototypeMesh";

		public PPtr<GameObject> Prototype;
		public PPtr<Texture2D> PrototypeTexture;
		public ColorRGBAf HealthyColor;
		public ColorRGBAf DryColor;
	}
}
