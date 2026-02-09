namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum CullMode
{
	Off = 0,
	Front = 1,
	Back = 2,
	Count,

	Unknown = -1,
}

public static class CullExtensions
{
	public static bool IsOff(this CullMode _this)
	{
		return _this == CullMode.Off;
	}

	public static bool IsFront(this CullMode _this)
	{
		return _this == CullMode.Front;
	}

	public static bool IsBack(this CullMode _this)
	{
		return _this == CullMode.Back;
	}
}
