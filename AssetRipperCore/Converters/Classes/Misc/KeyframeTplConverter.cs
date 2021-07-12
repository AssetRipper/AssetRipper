using AssetRipper.Classes.Misc;

namespace AssetRipper.Converters.Misc
{
	public static class KeyframeTplConverter
	{
		public static KeyframeTpl<T> Convert<T>(IExportContainer container, ref KeyframeTpl<T> origin)
			where T : struct, IAsset
		{
			KeyframeTpl<T> instance = origin;
			instance.TangentMode = origin.GetTangentMode(container.Version).ToTangent(container.ExportVersion);
			return instance;
		}
	}
}
