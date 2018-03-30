using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct InitialModule : IAssetReadable, IYAMLExportable
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
			
			StartLifetime.Read(stream);
			StartSpeed.Read(stream);
			StartColor.Read(stream);
			StartSize.Read(stream);
			StartSizeY.Read(stream);
			StartSizeZ.Read(stream);
			StartRotationX.Read(stream);
			StartRotationY.Read(stream);
			StartRotation.Read(stream);
			RandomizeRotationDirection = stream.ReadSingle();
			MaxNumParticles = stream.ReadInt32();
			Size3D = stream.ReadBoolean();
			Rotation3D = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			GravityModifier.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("startLifetime", StartLifetime.ExportYAML(exporter));
			node.Add("startSpeed", StartSpeed.ExportYAML(exporter));
			node.Add("startColor", StartColor.ExportYAML(exporter));
			node.Add("startSize", StartSize.ExportYAML(exporter));
			node.Add("startSizeY", StartSizeY.ExportYAML(exporter));
			node.Add("startSizeZ", StartSizeZ.ExportYAML(exporter));
			node.Add("startRotationX", StartRotationX.ExportYAML(exporter));
			node.Add("startRotationY", StartRotationY.ExportYAML(exporter));
			node.Add("startRotation", StartRotation.ExportYAML(exporter));
			node.Add("randomizeRotationDirection", RandomizeRotationDirection);
			node.Add("maxNumParticles", MaxNumParticles);
			node.Add("size3D", Size3D);
			node.Add("rotation3D", Rotation3D);
			node.Add("gravityModifier", GravityModifier.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public float RandomizeRotationDirection { get; private set; }
		public int MaxNumParticles { get; private set; }
		public bool Size3D { get; private set; }
		public bool Rotation3D { get; private set; }

		public MinMaxCurve StartLifetime;
		public MinMaxCurve StartSpeed;
		public MinMaxGradient StartColor;
		public MinMaxCurve StartSize;
		public MinMaxCurve StartSizeY;
		public MinMaxCurve StartSizeZ;
		public MinMaxCurve StartRotationX;
		public MinMaxCurve StartRotationY;
		public MinMaxCurve StartRotation;
		public MinMaxCurve GravityModifier;
	}
}
