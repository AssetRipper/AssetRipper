using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	/// <summary>
	/// First introduced in 5.6.0 as a replacement for such importers as DDSImporter
	/// </summary>
	public sealed class IHVImageFormatImporter : AssetImporter, ISpriteImporter
	{
		public IHVImageFormatImporter(LayoutInfo layout) : base(layout) { }

		public IHVImageFormatImporter(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasImporter(UnityVersion version) => version.IsGreaterEqual(5, 6);

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmaps(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSRGBTexture(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		public override bool IncludesImporter(UnityVersion version)
		{
			return true;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			TextureSettings.Read(reader);
			IsReadable = reader.ReadBoolean();
			if (HasSRGBTexture(reader.Version))
			{
				SRGBTexture = reader.ReadBoolean();
			}
			if (HasStreamingMipmaps(reader.Version))
			{
				StreamingMipmaps = reader.ReadBoolean();
				reader.AlignStream();

				StreamingMipmapsPriority = reader.ReadInt32();
			}
			reader.AlignStream();

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			TextureSettings.Write(writer);
			writer.Write(IsReadable);
			if (HasSRGBTexture(writer.Version))
			{
				writer.Write(SRGBTexture);
			}
			if (HasStreamingMipmaps(writer.Version))
			{
				writer.Write(StreamingMipmaps);
				writer.AlignStream();

				writer.Write(StreamingMipmapsPriority);
			}
			writer.AlignStream();

			PostWrite(writer);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(IsReadableName, IsReadable);
			if (HasSRGBTexture(container.ExportVersion))
			{
				node.Add(SRGBTextureName, SRGBTexture);
			}
			if (HasStreamingMipmaps(container.ExportVersion))
			{
				node.Add(StreamingMipmapsName, StreamingMipmaps);
				node.Add(StreamingMipmapsPriorityName, StreamingMipmapsPriority);
			}
			PostExportYAML(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.IHVImageFormatImporter;

		public bool IsReadable { get; set; }
		public bool SRGBTexture { get; set; }
		public bool StreamingMipmaps { get; set; }
		public int StreamingMipmapsPriority { get; set; }
		public IGLTextureSettings TextureSettings { get; } = new GLTextureSettings();

		protected override bool IncludesIDToName => false;

		public const string TextureSettingsName = "m_TextureSettings";
		public const string IsReadableName = "m_IsReadable";
		public const string SRGBTextureName = "m_sRGBTexture";
		public const string StreamingMipmapsName = "m_StreamingMipmaps";
		public const string StreamingMipmapsPriorityName = "m_StreamingMipmapsPriority";
	}
}
