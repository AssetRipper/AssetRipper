namespace uTinyRipper.Converters.TrailRenderers
{
	public static class GradientConverter
	{
		public static Classes.Gradient GenerateGradient(IExportContainer container, ref Classes.TrailRenderers.Gradient origin)
		{
			Classes.Gradient instance = new Classes.Gradient();
			instance.Add(0 * ushort.MaxValue / 4, origin.Color0);
			instance.Add(1 * ushort.MaxValue / 4, origin.Color1);
			instance.Add(2 * ushort.MaxValue / 4, origin.Color2);
			instance.Add(3 * ushort.MaxValue / 4, origin.Color3);
			instance.Add(4 * ushort.MaxValue / 4, origin.Color4);
			return instance;
		}
	}
}
