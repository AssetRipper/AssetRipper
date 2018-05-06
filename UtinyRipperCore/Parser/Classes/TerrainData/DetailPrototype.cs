using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.TerrainDatas
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

		public void Read(AssetStream stream)
		{
			Prototype.Read(stream);
			PrototypeTexture.Read(stream);
			MinWidth = stream.ReadSingle();
			MaxWidth = stream.ReadSingle();
			MinHeight = stream.ReadSingle();
			MaxHeight = stream.ReadSingle();
			NoiseSpread = stream.ReadSingle();
			BendFactor = stream.ReadSingle();
			HealthyColor.Read(stream);
			DryColor.Read(stream);
			if (IsReadGrayscaleLighting(stream.Version))
			{
				GrayscaleLighting = stream.ReadInt32();
			}
			LightmapFactor = stream.ReadSingle();
			RenderMode = stream.ReadInt32();
			UsePrototypeMesh = stream.ReadInt32();
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
