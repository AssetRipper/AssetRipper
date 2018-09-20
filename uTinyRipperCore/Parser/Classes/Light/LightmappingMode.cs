using System;

namespace uTinyRipper.Classes.Lights
{
	[Flags]
	public enum LightmappingMode
	{
		Mixed = 1,
		Baked = 2,
		Realtime = 4,
	}
}
