using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.VersionHandling
{
	public abstract class UnityHandlerBase
	{
		public abstract UnityVersion UnityVersion { get; }
		public IAssetFactory AssetFactory { get; protected set; }
		public IAssetImporterFactory ImporterFactory { get; protected set; }
	}
}
