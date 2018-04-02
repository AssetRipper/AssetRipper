using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MinMaxGradient : IAssetReadable, IYAMLExportable
	{
		public static bool IsColor32(Version version)
		{
			return version.IsLess(5, 4);
		}

		private static bool IsMaxGradientFirst(Version version)
		{
			return version.IsLess(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			
			if (version.IsGreaterEqual(5, 4))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			if (IsMaxGradientFirst(stream.Version))
			{
				MaxGradient.Read(stream);
				MinGradient.Read(stream);
				if (IsColor32(stream.Version))
				{
					MinColor32.Read(stream);
					MaxColor32.Read(stream);
				}
				else
				{
					MinColor.Read(stream);
					MaxColor.Read(stream);
				}
			}

			MinMaxState = stream.ReadUInt16();
			stream.AlignStream(AlignType.Align4);

			if (!IsMaxGradientFirst(stream.Version))
			{
				MinColor.Read(stream);
				MaxColor.Read(stream);
				MaxGradient.Read(stream);
				MinGradient.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("minMaxState", MinMaxState);
			node.Add("minColor", MinColor.ExportYAML(exporter));
			node.Add("maxColor", MaxColor.ExportYAML(exporter));
			node.Add("maxGradient", MaxGradient.ExportYAML(exporter));
			node.Add("minGradient", MinGradient.ExportYAML(exporter));
			return node;
		}

		public ushort MinMaxState { get; private set; }
		
		public ColorRGBA32 MinColor32;
		public ColorRGBA32 MaxColor32;
		public ColorRGBAf MinColor;
		public ColorRGBAf MaxColor;
		public Gradient MaxGradient;
		public Gradient MinGradient;
	}
}
