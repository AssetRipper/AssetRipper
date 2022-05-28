//using AssetRipper.AssetExporters;
//using AssetRipper.Yaml;

using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;

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

		/*protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYamlRoot(container);
			return node;
		}*/

		public bool CullTransparentMesh { get; set; }
	}
}
