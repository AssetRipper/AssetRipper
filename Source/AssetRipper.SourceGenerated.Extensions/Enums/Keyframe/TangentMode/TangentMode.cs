namespace AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;

public enum TangentMode
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

	ClampedAutoBoth = LClampedAuto | HClampedAuto,
	AutoBoth = LAuto | HAuto,
	FreeSmooth = LFree | HFree,
	FreeFree = Broken | LFree | HFree,
	FreeLinear = Broken | LFree | HLinear,
	FreeConstant = Broken | LFree | HConstant,
	LinearFree = Broken | LLinear | HFree,
	LinearLinear = Broken | LLinear | HLinear,
	LinearConstant = Broken | LLinear | HConstant,
	ConstantFree = Broken | LConstant | HFree,
	ConstantLinear = Broken | LConstant | HLinear,
	ConstantConstant = Broken | LConstant | HConstant,
}

public static class TangentModeExtensions
{
	/// <summary>
	/// 5.5.0 and greater
	/// </summary>
	public static bool TangentMode5Relevant(UnityVersion version) => version.GreaterThanOrEquals(5, 5);

	public static int ToTangent(this TangentMode _this, UnityVersion version)
	{
		if (TangentMode5Relevant(version))
		{
			return (int)_this.ToTangentMode5();
		}
		else
		{
			return (int)_this.ToTangentMode2();
		}
	}

	public static TangentMode2 ToTangentMode2(this TangentMode _this)
	{
		int value = (int)_this;
		int mask = (int)TangentMode.HConstant;
		int mask2 = (int)TangentMode2.HConstant;
		value = value & ~mask2 | (value & mask) >> 2;
		return (TangentMode2)value;
	}

	public static TangentMode5 ToTangentMode5(this TangentMode _this)
	{
		return (TangentMode5)_this;
	}
}
