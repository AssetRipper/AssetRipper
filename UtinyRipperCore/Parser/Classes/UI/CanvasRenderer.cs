//using UtinyRipper.AssetExporters;
//using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class CanvasRenderer : Component
	{
		public CanvasRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			CullTransparentMesh = stream.ReadBoolean();
		}

		/*protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAMLRoot(container);
			return node;
		}*/

		public bool CullTransparentMesh { get; private set; }
	}
}
