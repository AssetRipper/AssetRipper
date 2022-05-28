namespace AssetRipper.Core.Classes.Camera
{
	[Flags]
	public enum StereoTargetEyeMask
	{
		None = 0,
		Left = 1,
		Right = 2,
		Both = Left | Right
	}
}
