using System;

namespace AssetRipperLibrary.TextureContainers.PVR
{
	[Flags]
	public enum PVRFlag : uint
	{
		NoFlag			= 0,
		PreMultiplied	= 2,
	}
}
