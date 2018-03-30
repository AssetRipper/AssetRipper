namespace UtinyRipper.Classes
{
	public sealed class RectTransform : Transform
	{
		public RectTransform(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			AnchorMin.Read(stream);
			AnchorMax.Read(stream);
			AnchorPosition.Read(stream);
			SizeDelta.Read(stream);
			Pivot.Read(stream);
		}

		public Vector2f AnchorMin;
		public Vector2f AnchorMax;
		public Vector2f AnchorPosition;
		public Vector2f SizeDelta;
		public Vector2f Pivot;
	}
}
