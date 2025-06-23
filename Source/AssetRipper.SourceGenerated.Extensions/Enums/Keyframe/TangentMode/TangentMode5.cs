namespace AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;

/// <summary>
/// 5.5.0 and greater
/// </summary>
public enum TangentMode5
{
	Free = 0,
	Broken = 1,
	Auto = 2,
	Linear = 4,
	Constant = 6,
	ClampedAuto = 8,

	LFree = Free,
	HFree = Free,
	LAuto = Auto,
	HAuto = Auto << 4,
	LLinear = Linear,
	HLinear = Linear << 4,
	LConstant = Constant,
	HConstant = Constant << 4,
	LClampedAuto = ClampedAuto,
	HClampedAuto = ClampedAuto << 4,
}

public static class TangentMode5Extensions
{
	public static TangentMode ToTangentMode(this TangentMode5 _this)
	{
		return (TangentMode)_this;
	}
}
