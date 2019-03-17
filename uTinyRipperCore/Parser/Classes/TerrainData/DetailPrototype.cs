using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct DetailPrototype : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadGrayscaleLighting(Version version)
		{
			return version.IsLess(3);
		}

		private static int GetSerializedVersion(Version version)
		{
			// this is min version
			return 2;
		}

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
			if (IsReadGrayscaleLighting(reader.Version))
			{
				GrayscaleLighting = reader.ReadInt32();
			}
			LightmapFactor = reader.ReadSingle();
			RenderMode = (DetailRenderMode)reader.ReadInt32();
			UsePrototypeMesh = reader.ReadInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Prototype.FetchDependency(file, isLog, () => nameof(DetailPrototype), "prototype");
			yield return PrototypeTexture.FetchDependency(file, isLog, () => nameof(DetailPrototype), "prototypeTexture");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			node.Add(LightmapFactorName, LightmapFactor);
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(UsePrototypeMeshName, UsePrototypeMesh);
			return node;
		}

		public float MinWidth { get; private set; }
		public float MaxWidth { get; private set; }
		public float MinHeight { get; private set; }
		public float MaxHeight { get; private set; }
		public float NoiseSpread { get; private set; }
		public float BendFactor { get; private set; }
		public int GrayscaleLighting { get; private set; }
		public float LightmapFactor { get; private set; }
		public DetailRenderMode RenderMode { get; private set; }
		public int UsePrototypeMesh { get; private set; }

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
		public const string LightmapFactorName = "lightmapFactor";
		public const string RenderModeName = "renderMode";
		public const string UsePrototypeMeshName = "usePrototypeMesh";

		public PPtr<GameObject> Prototype;
		public PPtr<Texture2D> PrototypeTexture;
		public ColorRGBAf HealthyColor;
		public ColorRGBAf DryColor;
	}
}
