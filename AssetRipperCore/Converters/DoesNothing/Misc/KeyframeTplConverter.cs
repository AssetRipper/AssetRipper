using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Misc
{
	public static class KeyframeTplConverter
	{
		public static KeyframeTpl<T> Convert<T>(IExportContainer container, KeyframeTpl<T> origin) where T : IAsset, new()
		{
			KeyframeTpl<T> instance = origin.Clone();
			instance.TangentMode = origin.GetTangentMode(container.Version).ToTangent(container.ExportVersion);
			return instance;
		}
	}
}
