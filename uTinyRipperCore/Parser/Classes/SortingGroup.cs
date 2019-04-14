using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class SortingGroup : Behaviour
	{
		public SortingGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			SortingLayerID = reader.ReadInt32();
			SortingLayer = reader.ReadInt16();
			SortingOrder = reader.ReadInt16();
			reader.AlignStream(AlignType.Align4);
			
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingLayerName, SortingLayer);
			node.Add(SortingOrderName, SortingOrder);
			return node;
		}

		public int SortingLayerID { get; private set; }
		public short SortingLayer { get; private set; }
		public short SortingOrder { get; private set; }

		public const string SortingLayerIDName = "m_SortingLayerID";
		public const string SortingLayerName = "m_SortingLayer";
		public const string SortingOrderName = "m_SortingOrder";
	}
}
