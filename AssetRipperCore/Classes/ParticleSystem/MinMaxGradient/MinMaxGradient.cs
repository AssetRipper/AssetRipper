using AssetRipper.Core.Classes.Misc.Serializable.Gradient;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem.MinMaxGradient
{
	public sealed class MinMaxGradient : IAssetReadable, IYAMLExportable
	{
		public MinMaxGradient() { }
		public MinMaxGradient(bool _)
		{
			MinMaxState = MinMaxGradientState.Color;

			MinColor = ColorRGBAf.White;
			MaxColor = ColorRGBAf.White;
			MaxGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
			MinGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
		}

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool IsColor32(UnityVersion version) => version.IsLess(5, 4);

		private static int GetMaxGradientPlacement(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch, 4))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			int maxGradientPlacement = GetMaxGradientPlacement(reader.Version);
			if (maxGradientPlacement == 1)
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

			// Int16 before 5.6.0p4
			MinMaxState = (MinMaxGradientState)reader.ReadUInt16();
			reader.AlignStream();

			if (maxGradientPlacement != 1)
			{
				if (maxGradientPlacement == 2)
				{
					MaxGradient.Read(reader);
					MinGradient.Read(reader);
				}
				MinColor.Read(reader);
				MaxColor.Read(reader);
				if (maxGradientPlacement == 3)
				{
					MaxGradient.Read(reader);
					MinGradient.Read(reader);
				}
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

		public ColorRGBAf MinColor = new();
		public ColorRGBAf MaxColor = new();
		public Gradient MaxGradient = new();
		public Gradient MinGradient = new();
	}
}
