using System;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public sealed class IHVImageFormatImporter : DefaultImporter
	{
		public IHVImageFormatImporter(Texture2D texture)
		{
			if(texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}
			m_texture = texture;
			m_importSettings = new TextureImportSettings(m_texture.TextureSettings);
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			node.Add("textureSettings", m_importSettings.ExportYAML(container));
			node.Add("isReadable", m_texture.IsReadable);
#warning TODO: imageFormat convertion?
			node.Add("sRGBTexture", true);
		}

		public override string Name => nameof(IHVImageFormatImporter);

		private readonly Texture2D m_texture;
		private readonly TextureImportSettings m_importSettings;
	}
}
