using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc.KeyframeTpl;
using AssetRipper.Parser.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.IO.Asset;

namespace AssetRipper.Converters.Classes.Misc
{
	public static class KeyframeTplConverter
	{
		public static KeyframeTpl<T> Convert<T>(IExportContainer container, ref KeyframeTpl<T> origin) where T : struct, IAsset
		{
			KeyframeTpl<T> instance = origin;
			instance.TangentMode = origin.GetTangentMode(container.Version).ToTangent(container.ExportVersion);
			return instance;
		}
	}
}
