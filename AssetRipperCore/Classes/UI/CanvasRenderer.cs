//using AssetRipper.AssetExporters;
//using AssetRipper.YAML;

using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.UI
{
	public sealed class CanvasRenderer : Component
	{
		public CanvasRenderer(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasCullTransparentMesh(UnityVersion version) => version.IsGreaterEqual(2018, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasCullTransparentMesh(reader.Version))
			{
				CullTransparentMesh = reader.ReadBoolean();
			}
		}

		/*protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAMLRoot(container);
			return node;
		}*/

		public bool CullTransparentMesh { get; set; }
	}
}
