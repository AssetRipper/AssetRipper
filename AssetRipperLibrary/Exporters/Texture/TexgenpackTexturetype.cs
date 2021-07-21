using System.Runtime.Versioning;

namespace AssetRipperLibrary.Exporters.Textures
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
