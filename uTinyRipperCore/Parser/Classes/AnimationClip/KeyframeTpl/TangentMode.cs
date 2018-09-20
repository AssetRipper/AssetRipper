using System;

namespace uTinyRipper.Classes.AnimationClips
{
	[Flags]
	public enum TangentMode
	{
		Free			= 0,
		Broken			= 1,
		Auto			= 2,
		Linear			= 4,
		Constant		= 6,
		ClampedAuto		= 8,

		ClampedAutoBoth = ClampedAuto | ClampedAuto << 4,
		AutoBoth		= Auto | Auto << 4,
		FreeSmooth		= Free | Free << 4,
		FreeFree		= Broken | Free | Free << 4,
		FreeLinear		= Broken | Free | Linear << 4,
		FreeConstant	= Broken | Free | Constant << 4,
		LinearFree		= Broken | Linear | Free << 4,
		LinearLinear	= Broken | Linear | Linear << 4,
		LinearConstant	= Broken | Linear | Constant << 4,
		ConstantFree	= Broken | Constant | Free << 4,
		ConstantLinear	= Broken | Constant | Linear << 4,
		ConstantConstant= Broken | Constant | Constant << 4,
	}
}
