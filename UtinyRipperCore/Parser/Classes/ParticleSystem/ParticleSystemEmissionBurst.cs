using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct ParticleSystemEmissionBurst : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			CountCurve.Read(stream);
			CycleCount = stream.ReadInt32();
			RepeatInterval = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("time", Time);
			node.Add("countCurve", CountCurve.ExportYAML(exporter));
			node.Add("cycleCount", CycleCount);
			node.Add("repeatInterval", RepeatInterval);
			return node;
		}

		public float Time { get; private set; }
		public int CycleCount { get; private set; }
		public float RepeatInterval { get; private set; }

		public MinMaxCurve CountCurve;
	}
}
