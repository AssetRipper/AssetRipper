namespace uTinyRipper.Layout
{
	public sealed class SerializedLayoutCategory
	{
		public SerializedLayoutCategory(LayoutInfo info)
		{
			AABB = new AABBLayout(info);
			AABBi = new AABBiLayout(info);
			AnimationCurveTpl = new AnimationCurveTplLayout(info);
			ColorRGBA32 = new ColorRGBA32Layout(info);
			ColorRGBAf = new ColorRGBAfLayout(info);
			Gradient = new GradientLayout(info);
			GUIStyle = new GUIStyleLayout(info);
			LayerMask = new LayerMaskLayout(info);
			Matrix4x4f = new Matrix4x4fLayout(info);
			Quaternionf = new QuaternionfLayout(info);
			Rectf = new RectfLayout(info);
			RectOffset = new RectOffsetLayout(info);
			Vector2f = new Vector2fLayout(info);
			Vector2i = new Vector2iLayout(info);
			Vector3f = new Vector3fLayout(info);
			Vector3i = new Vector3iLayout(info);
			Vector4f = new Vector4fLayout(info);
		}

		public AABBLayout AABB { get; }
		public AABBiLayout AABBi { get; }
		public AnimationCurveTplLayout AnimationCurveTpl { get; }
		public ColorRGBA32Layout ColorRGBA32 { get; }
		public ColorRGBAfLayout ColorRGBAf { get; }
		public GradientLayout Gradient { get; }
		public GUIStyleLayout GUIStyle { get; }
		public LayerMaskLayout LayerMask { get; }
		public Matrix4x4fLayout Matrix4x4f { get; }
		public QuaternionfLayout Quaternionf { get; }
		public RectfLayout Rectf { get; }
		public RectOffsetLayout RectOffset { get; }
		public Vector2fLayout Vector2f { get; }
		public Vector2iLayout Vector2i { get; }
		public Vector3fLayout Vector3f { get; }
		public Vector3iLayout Vector3i { get; }
		public Vector4fLayout Vector4f { get; }
	}
}
