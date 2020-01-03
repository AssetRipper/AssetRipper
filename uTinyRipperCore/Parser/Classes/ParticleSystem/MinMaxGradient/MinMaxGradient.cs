using uTinyRipper.Converters;
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

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.4.0
		/// </summary>
		public static bool IsColor32(Version version) => version.IsLess(5, 4);

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		private static bool IsMaxGradientFirst(Version version) => version.IsLess(5, 6);

		public void Read(AssetReader reader)
		{
			if (IsMaxGradientFirst(reader.Version))
			{
				MaxGradient.Read(reader);
				MinGradient.Read(reader);
				if (IsColor32(reader.Version))
				{
					MinColor32 = reader.ReadAsset<ColorRGBA32>();
					MaxColor32 = reader.ReadAsset<ColorRGBA32>();
				}
				else
				{
					MinColor.Read(reader);
					MaxColor.Read(reader);
				}
			}

			MinMaxState = (MinMaxGradientState)reader.ReadUInt16();
			reader.AlignStream();

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
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(MinMaxStateName, (ushort)MinMaxState);
			node.Add(MinColorName, MinColor.ExportYAML(container));
			node.Add(MaxColorName, MaxColor.ExportYAML(container));
			node.Add(MaxGradientName, MaxGradient.ExportYAML(container));
			node.Add(MinGradientName, MinGradient.ExportYAML(container));
			return node;
		}

		public ColorRGBA32 MinColor32
		{
			get => (ColorRGBA32)MinColor;
			set => MinColor = (ColorRGBAf)value;
		}
		public ColorRGBA32 MaxColor32
		{
			get => (ColorRGBA32)MaxColor;
			set => MaxColor = (ColorRGBAf)value;
		}
		public MinMaxGradientState MinMaxState { get; set; }

		public const string MinMaxStateName = "minMaxState";
		public const string MinColorName = "minColor";
		public const string MaxColorName = "maxColor";
		public const string MaxGradientName = "maxGradient";
		public const string MinGradientName = "minGradient";

		public ColorRGBAf MinColor;
		public ColorRGBAf MaxColor;
		public Gradient MaxGradient;
		public Gradient MinGradient;
	}
}
