using System;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
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
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLInner(container, node);

			TextureImportSettings importSettings = new TextureImportSettings(m_texture.TextureSettings);
			node.Add("textureSettings", importSettings.ExportYAML(container));
			node.Add("isReadable", m_texture.IsReadable);
			node.Add("sRGBTexture", m_texture.ColorSpace == ColorSpace.Gamma ? true : false);
		}

		public override string Name => nameof(IHVImageFormatImporter);

		private readonly Texture2D m_texture;
	}
}
