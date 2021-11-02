using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.VersionHandling
{
	public class LegacyHandler : UnityHandlerBase
	{
		public override UnityVersion UnityVersion => throw new NotSupportedException();

		public LegacyHandler()
		{
			this.AssetFactory = new AssetFactory();
			this.ImporterFactory = new LegacyImporterFactory();
		}
	}
}
