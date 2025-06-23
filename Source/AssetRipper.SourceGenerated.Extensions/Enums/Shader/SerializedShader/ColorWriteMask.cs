namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
[Flags]
public enum ColorWriteMask
{
	None = 0x0,
	Alpha = 0x1,
	Red = 0x2,
	Green = 0x4,
	Blue = 0x8,

	All = Red | Green | Blue | Alpha, // 15
}

public static class ColorMaskExtensions
{
	public static bool IsNone(this ColorWriteMask _this)
	{
		return _this == ColorWriteMask.None;
	}

	public static bool IsRed(this ColorWriteMask _this)
	{
		return (_this & ColorWriteMask.Red) != 0;
	}

	public static bool IsGreen(this ColorWriteMask _this)
	{
		return (_this & ColorWriteMask.Green) != 0;
	}

	public static bool IsBlue(this ColorWriteMask _this)
	{
		return (_this & ColorWriteMask.Blue) != 0;
	}

	public static bool IsAlpha(this ColorWriteMask _this)
	{
		return (_this & ColorWriteMask.Alpha) != 0;
	}

	public static bool IsRBGA(this ColorWriteMask _this)
	{
		return _this == ColorWriteMask.All;
	}
}
