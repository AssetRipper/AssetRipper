namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum StencilOp
{
	Keep = 0,
	Zero = 1,
	Replace = 2,
	IncrementSaturate = 3,
	DecrementSaturate = 4,
	Invert = 5,
	IncrementWrap = 6,
	DecrementWrap = 7,
	Count,
}

public static class StencilOpExtensions
{
	public static bool IsKeep(this StencilOp _this)
	{
		return _this == StencilOp.Keep;
	}
}
