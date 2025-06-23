namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum BlendMode
{
	Zero = 0,
	One = 1,
	DstColor = 2,
	SrcColor = 3,
	OneMinusDstColor = 4,
	SrcAlpha = 5,
	OneMinusSrcColor = 6,
	DstAlpha = 7,
	OneMinusDstAlpha = 8,
	SrcAlphaSaturate = 9,
	OneMinusSrcAlpha = 10,
	Count,
}

public static class BlendFactorExtensions
{
	public static bool IsZero(this BlendMode _this)
	{
		return _this == BlendMode.Zero;
	}

	public static bool IsOne(this BlendMode _this)
	{
		return _this == BlendMode.One;
	}
}
