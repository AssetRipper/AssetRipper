namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// Duplicate of RenderSettings FogMode<br/>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum FogMode
{
	Off = 0,
	Linear = 1,
	Exp = 2,
	Exp2 = 3,
	Count,

	Unknown = -1,
}

public static class FogModeExtensions
{
	public static bool IsUnknown(this FogMode _this)
	{
		return _this == FogMode.Unknown;
	}
}
