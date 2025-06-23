namespace AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;

/// <summary>
/// Less than 5.5.0
/// </summary>
public enum TangentMode2
{
	Free = 0,
	Broken = 1,
	Auto = 2,
	Linear = 4,
	Constant = 6,

	LFree = Free,
	HFree = Free,
	LAuto = Auto,
	HAuto = Auto << 2,
	LLinear = Linear,
	HLinear = Linear << 2,
	LConstant = Constant,
	HConstant = Constant << 2,
}

public static class TangentMode2Extensions
{
	public static TangentMode ToTangentMode(this TangentMode2 _this)
	{
		int value = (int)_this;
		int mask = (int)TangentMode2.HConstant;
		value = value & ~mask | (value & mask) << 2;
		return (TangentMode)value;
	}
}
