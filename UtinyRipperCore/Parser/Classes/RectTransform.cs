namespace UtinyRipper.Classes
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

		public Vector2f AnchorMin;
		public Vector2f AnchorMax;
		public Vector2f AnchorPosition;
		public Vector2f SizeDelta;
		public Vector2f Pivot;
	}
}
