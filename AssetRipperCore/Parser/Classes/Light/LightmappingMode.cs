using System;

namespace AssetRipper.Parser.Classes.Light
{
	[Flags]
	public enum LightmappingMode
	{
		Mixed = 1,
		Baked = 2,
		Realtime = 4,
	}
}
