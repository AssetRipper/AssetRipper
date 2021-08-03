using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Textures
{
	[SupportedOSPlatform("windows")]
	public enum TexgenpackTexturetype
	{
		RGTC1,
		RGTC2,
		BPTC_FLOAT,
		BPTC
	}
}
