using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	public sealed class SourceTextureInformation : IAsset
	{
		public SourceTextureInformation() { }

		public SourceTextureInformation(LayoutInfo layout) : this()
		{
#warning TODO: default values
		}

		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasDoesTextureContainColorRGBAf(UnityVersion version) => version.IsLess(5) && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasSourceWasHDR(UnityVersion version) => version.IsGreaterEqual(5);

		public void Read(AssetReader reader)
		{
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			DoesTextureContainAlpha = reader.ReadBoolean();
			if (HasDoesTextureContainColorRGBAf(reader.Version))
			{
				DoesTextureContainColor = reader.ReadBoolean();
			}
			if (HasSourceWasHDR(reader.Version))
			{
				SourceWasHDR = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Width);
			writer.Write(Height);
			writer.Write(DoesTextureContainAlpha);
			if (HasDoesTextureContainColorRGBAf(writer.Version))
			{
				writer.Write(DoesTextureContainColor);
			}
			if (HasSourceWasHDR(writer.Version))
			{
				writer.Write(SourceWasHDR);
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(DoesTextureContainAlphaName, DoesTextureContainAlpha);
			if (HasDoesTextureContainColorRGBAf(container.ExportVersion))
			{
				node.Add(DoesTextureContainColorName, DoesTextureContainColor);
			}
			if (HasSourceWasHDR(container.ExportVersion))
			{
				node.Add(SourceWasHDRName, SourceWasHDR);
			}
			return node;
		}

		public int Width { get; set; }
		public int Height { get; set; }
		public bool DoesTextureContainAlpha { get; set; }
		public bool DoesTextureContainColor { get; set; }
		public bool SourceWasHDR { get; set; }

		public const string WidthName = "width";
		public const string HeightName = "height";
		public const string DoesTextureContainAlphaName = "doesTextureContainAlpha";
		public const string DoesTextureContainColorName = "doesTextureContainColor";
		public const string SourceWasHDRName = "sourceWasHDR";
	}
}
