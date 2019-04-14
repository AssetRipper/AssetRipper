using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public struct MinMaxGradient : IAssetReadable, IYAMLExportable
	{
		public MinMaxGradient(bool _)
		{
			MinMaxState = MinMaxGradientState.Color;

			MinColor = ColorRGBAf.White;
			MaxColor = ColorRGBAf.White;
			MaxGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
			MinGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
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
					MinColor.Read32(reader);
					MaxColor.Read32(reader);
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
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("minMaxState", (ushort)MinMaxState);
			node.Add("minColor", MinColor.ExportYAML(container));
			node.Add("maxColor", MaxColor.ExportYAML(container));
			node.Add("maxGradient", MaxGradient.ExportYAML(container));
			node.Add("minGradient", MinGradient.ExportYAML(container));
			return node;
		}

		public MinMaxGradientState MinMaxState { get; private set; }
		
		public ColorRGBAf MinColor;
		public ColorRGBAf MaxColor;
		public Gradient MaxGradient;
		public Gradient MinGradient;
	}
}
