using AssetRipper.Layout.Classes.Misc;

namespace AssetRipper.Layout.Categories
{
	public sealed class MiscLayoutCategory
	{
		public MiscLayoutCategory(LayoutInfo info)
		{
			GUID = new GUIDLayout(info);
			KeyframeTpl = new KeyframeTplLayout(info);
		}

		public GUIDLayout GUID { get; }
		public KeyframeTplLayout KeyframeTpl { get; }
	}
}
