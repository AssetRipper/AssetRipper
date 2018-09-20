using System;

namespace uTinyRipper.Classes.Lights
{
	/// <summary>
	/// Enum describing what part of a light contribution can be baked.
	/// </summary>
	[Flags]
	public enum LightmapBakeType
	{
		Mixed = 1,
		Baked = 2,
		Realtime = 4,
	}
}
