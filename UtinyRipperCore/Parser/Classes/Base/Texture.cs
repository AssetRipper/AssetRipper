using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public abstract class Texture : NamedObject
	{
		protected Texture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}
	}
}
