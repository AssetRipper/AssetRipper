using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct KeyframeTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
#warning TODO: TCB to Engine's in/out slope
		public KeyframeTpl(float time, T value)
		{
			Time = time;
			Value = value;
			InSlope = default;
			OutSlope = default;
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			Value.Read(stream);
			InSlope.Read(stream);
			OutSlope.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("time", Time);
			node.Add("value", Value.ExportYAML(exporter));
			node.Add("inSlope", InSlope.ExportYAML(exporter));
			node.Add("outSlope", OutSlope.ExportYAML(exporter));
			if (GetSerializedVersion(exporter.Version) >= 2)
			{
#warning TODO: value?
				node.Add("tangentMode", 0);
			}
			return node;
		}

		public float Time { get; private set; }

		public T Value;
		public T InSlope;
		public T OutSlope;
	}
}
