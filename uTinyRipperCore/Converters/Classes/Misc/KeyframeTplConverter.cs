using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Converters.Misc
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
