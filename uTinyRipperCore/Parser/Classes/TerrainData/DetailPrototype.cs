using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

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
			RenderMode = reader.ReadInt32();
			UsePrototypeMesh = reader.ReadInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Prototype.FetchDependency(file, isLog, () => nameof(DetailPrototype), "prototype");
			yield return PrototypeTexture.FetchDependency(file, isLog, () => nameof(DetailPrototype), "prototypeTexture");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("prototype", Prototype.ExportYAML(container));
			node.Add("prototypeTexture", PrototypeTexture.ExportYAML(container));
			node.Add("minWidth", MinWidth);
			node.Add("maxWidth", MaxWidth);
			node.Add("minHeight", MinHeight);
			node.Add("maxHeight", MaxHeight);
			node.Add("noiseSpread", NoiseSpread);
			node.Add("bendFactor", BendFactor);
			node.Add("healthyColor", HealthyColor.ExportYAML(container));
			node.Add("dryColor", DryColor.ExportYAML(container));
			node.Add("lightmapFactor", LightmapFactor);
			node.Add("renderMode", RenderMode);
			node.Add("usePrototypeMesh", UsePrototypeMesh);
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
		public int RenderMode { get; private set; }
		public int UsePrototypeMesh { get; private set; }

		public PPtr<GameObject> Prototype;
		public PPtr<Texture2D> PrototypeTexture;
		public ColorRGBAf HealthyColor;
		public ColorRGBAf DryColor;
	}
}
