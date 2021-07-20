using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc.Serializable.Gradient;

namespace AssetRipper.Converters.Classes.TrailRenderer
{
	public static class GradientConverter
	{
		public static Gradient GenerateGradient(IExportContainer container, ref Parser.Classes.TrailRenderer.Gradient origin)
		{
			Gradient instance = new Gradient();
			instance.Add(0 * ushort.MaxValue / 4, origin.Color0);
			instance.Add(1 * ushort.MaxValue / 4, origin.Color1);
			instance.Add(2 * ushort.MaxValue / 4, origin.Color2);
			instance.Add(3 * ushort.MaxValue / 4, origin.Color3);
			instance.Add(4 * ushort.MaxValue / 4, origin.Color4);
			return instance;
		}
	}
}
