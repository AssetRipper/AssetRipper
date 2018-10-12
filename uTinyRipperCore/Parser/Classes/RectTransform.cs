using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes
{
	public sealed class RectTransform : Transform
	{
		public RectTransform(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AnchorMin.Read(reader);
			AnchorMax.Read(reader);
			AnchorPosition.Read(reader);
			SizeDelta.Read(reader);
			Pivot.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_AnchorMin", AnchorMin.ExportYAML(container));
			node.Add("m_AnchorMax", AnchorMax.ExportYAML(container));
			node.Add("m_AnchoredPosition", AnchorPosition.ExportYAML(container));
			node.Add("m_SizeDelta", SizeDelta.ExportYAML(container));
			node.Add("m_Pivot", Pivot.ExportYAML(container));
			return node;
		}

		public Vector2f AnchorMin;
		public Vector2f AnchorMax;
		public Vector2f AnchorPosition;
		public Vector2f SizeDelta;
		public Vector2f Pivot;
	}
}
