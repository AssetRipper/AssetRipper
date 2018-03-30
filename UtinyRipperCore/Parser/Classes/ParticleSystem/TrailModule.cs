using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct TrailModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Mode = stream.ReadInt32();
			Ratio = stream.ReadSingle();
			Lifetime.Read(stream);
			MinVertexDistance = stream.ReadSingle();
			TextureMode = stream.ReadInt32();
			RibbonCount = stream.ReadInt32();
			WorldSpace = stream.ReadBoolean();
			DieWithParticles = stream.ReadBoolean();
			SizeAffectsWidth = stream.ReadBoolean();
			SizeAffectsLifetime = stream.ReadBoolean();
			InheritParticleColor = stream.ReadBoolean();
			GenerateLightingData = stream.ReadBoolean();
			SplitSubEmitterRibbons = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			ColorOverLifetime.Read(stream);
			WidthOverTrail.Read(stream);
			ColorOverTrail.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("mode", Mode);
			node.Add("ratio", Ratio);
			node.Add("lifetime", Lifetime.ExportYAML(exporter));
			node.Add("minVertexDistance", MinVertexDistance);
			node.Add("textureMode", TextureMode);
			node.Add("ribbonCount", RibbonCount);
			node.Add("worldSpace", WorldSpace);
			node.Add("dieWithParticles", DieWithParticles);
			node.Add("sizeAffectsWidth", SizeAffectsWidth);
			node.Add("sizeAffectsLifetime", SizeAffectsLifetime);
			node.Add("inheritParticleColor", InheritParticleColor);
			node.Add("generateLightingData", GenerateLightingData);
			node.Add("splitSubEmitterRibbons", SplitSubEmitterRibbons);
			node.Add("colorOverLifetime", ColorOverLifetime.ExportYAML(exporter));
			node.Add("widthOverTrail", WidthOverTrail.ExportYAML(exporter));
			node.Add("colorOverTrail", ColorOverTrail.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public int Mode { get; private set; }
		public float Ratio { get; private set; }
		public float MinVertexDistance { get; private set; }
		public int TextureMode { get; private set; }
		public int RibbonCount { get; private set; }
		public bool WorldSpace { get; private set; }
		public bool DieWithParticles { get; private set; }
		public bool SizeAffectsWidth { get; private set; }
		public bool SizeAffectsLifetime { get; private set; }
		public bool InheritParticleColor { get; private set; }
		public bool GenerateLightingData { get; private set; }
		public bool SplitSubEmitterRibbons { get; private set; }

		public MinMaxCurve Lifetime;
		public MinMaxGradient ColorOverLifetime;
		public MinMaxCurve WidthOverTrail;
		public MinMaxGradient ColorOverTrail;
	}
}
