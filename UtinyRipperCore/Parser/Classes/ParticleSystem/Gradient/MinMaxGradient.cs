using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MinMaxGradient : IAssetReadable, IYAMLExportable
	{
		public MinMaxGradient(bool _)
		{
			MinMaxState = MinMaxGradientState.Color;

			MinColor = new ColorRGBAf(1.0f, 1.0f, 1.0f, 1.0f);
			MinColor32 = new ColorRGBA32(255, 255, 255, 255);
			MaxColor = new ColorRGBAf(1.0f, 1.0f, 1.0f, 1.0f);
			MaxColor32 = new ColorRGBA32(255, 255, 255, 255);

			MaxGradient = default;
			MaxGradient.AddColor(0, 1.0f, 1.0f, 1.0f);
			MaxGradient.AddColor(ushort.MaxValue, 1.0f, 1.0f, 1.0f);
			MaxGradient.AddAlpha(0, 1.0f);
			MaxGradient.AddAlpha(ushort.MaxValue, 1.0f);

			MinGradient = default;
			MinGradient.AddColor(0, 1.0f, 1.0f, 1.0f);
			MinGradient.AddColor(ushort.MaxValue, 1.0f, 1.0f, 1.0f);
			MinGradient.AddAlpha(0, 1.0f);
			MinGradient.AddAlpha(ushort.MaxValue, 1.0f);
		}

		/// <summary>
		/// Less than 5.4.0
		/// </summary>
		public static bool IsColor32(Version version)
		{
			return version.IsLess(5, 4);
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
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

		public void Read(AssetReader reader)
		{
			if (IsMaxGradientFirst(reader.Version))
			{
				MaxGradient.Read(reader);
				MinGradient.Read(reader);
				if (IsColor32(reader.Version))
				{
					MinColor32.Read(reader);
					MaxColor32.Read(reader);
				}
				else
				{
					MinColor.Read(reader);
					MaxColor.Read(reader);
				}
			}

			MinMaxState = (MinMaxGradientState)reader.ReadUInt16();
			reader.AlignStream(AlignType.Align4);

			if (!IsMaxGradientFirst(reader.Version))
			{
				MinColor.Read(reader);
				MaxColor.Read(reader);
				MaxGradient.Read(reader);
				MinGradient.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("minMaxState", (ushort)MinMaxState);
			node.Add("minColor", GetMinColor(container.Version).ExportYAML(container));
			node.Add("maxColor", GetMaxColor(container.Version).ExportYAML(container));
			node.Add("maxGradient", MaxGradient.ExportYAML(container));
			node.Add("minGradient", MinGradient.ExportYAML(container));
			return node;
		}

		private ColorRGBAf GetMinColor(Version version)
		{
			return IsColor32(version) ? new ColorRGBAf(MinColor32) : MinColor;
		}
		private ColorRGBAf GetMaxColor(Version version)
		{
			return IsColor32(version) ? new ColorRGBAf(MaxColor32) : MaxColor;
		}

		public MinMaxGradientState MinMaxState { get; private set; }
		
		public ColorRGBA32 MinColor32;
		public ColorRGBA32 MaxColor32;
		public ColorRGBAf MinColor;
		public ColorRGBAf MaxColor;
		public Gradient MaxGradient;
		public Gradient MinGradient;
	}
}
