using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Converters.Misc
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
