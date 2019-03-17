//using UtinyRipper.AssetExporters;
//using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CanvasRenderer : Component
	{
		public CanvasRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadCullTransparentMesh(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadCullTransparentMesh(reader.Version))
			{
				CullTransparentMesh = reader.ReadBoolean();
			}
		}

		/*protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAMLRoot(container);
			return node;
		}*/

		public bool CullTransparentMesh { get; private set; }
	}
}
