using System;

namespace UtinyRipper.Classes.Cameras
{
	[Flags]
	public enum StereoTargetEyeMask
	{
		None			= 0,
		Left			= 1,
		Right			= 2,
		Both = Left | Right
	}
}
