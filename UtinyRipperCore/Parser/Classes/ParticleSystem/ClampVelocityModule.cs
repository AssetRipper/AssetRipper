using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct ClampVelocityModule : IAssetReadable, IYAMLExportable
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
			
			X.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			Magnitude.Read(stream);
			SeparateAxis = stream.ReadBoolean();
			InWorldSpace = stream.ReadBoolean();
			MultiplyDragByParticleSize = stream.ReadBoolean();
			MultiplyDragByParticleVelocity = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Dampen = stream.ReadSingle();
			Drag.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("x", X.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("magnitude", Magnitude.ExportYAML(exporter));
			node.Add("separateAxis", SeparateAxis);
			node.Add("inWorldSpace", InWorldSpace);
			node.Add("multiplyDragByParticleSize", MultiplyDragByParticleSize);
			node.Add("multiplyDragByParticleVelocity", MultiplyDragByParticleVelocity);
			node.Add("dampen", Dampen);
			node.Add("drag", Drag.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public bool SeparateAxis { get; private set; }
		public bool InWorldSpace { get; private set; }
		public bool MultiplyDragByParticleSize { get; private set; }
		public bool MultiplyDragByParticleVelocity { get; private set; }
		public float Dampen { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve Magnitude;
		public MinMaxCurve Drag;
	}
}
