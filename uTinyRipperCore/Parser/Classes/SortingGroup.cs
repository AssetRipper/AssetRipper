using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// Introduced in 5.6.0
	/// </summary>
	public sealed class SortingGroup : Behaviour
	{
		public SortingGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static bool IsSortingLayerIDFirst(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Final);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsSortingLayerIDFirst(reader.Version))
			{
				SortingLayerID = reader.ReadInt32();
			}
			SortingLayer = reader.ReadInt16();
			SortingOrder = reader.ReadInt16();
			if (!IsSortingLayerIDFirst(reader.Version))
			{
				SortingLayerID = reader.ReadInt32();
			}
			reader.AlignStream();
			
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingLayerName, SortingLayer);
			node.Add(SortingOrderName, SortingOrder);
			return node;
		}

		public int SortingLayerID { get; set; }
		public short SortingLayer { get; set; }
		public short SortingOrder { get; set; }

		public const string SortingLayerIDName = "m_SortingLayerID";
		public const string SortingLayerName = "m_SortingLayer";
		public const string SortingOrderName = "m_SortingOrder";
	}
}
