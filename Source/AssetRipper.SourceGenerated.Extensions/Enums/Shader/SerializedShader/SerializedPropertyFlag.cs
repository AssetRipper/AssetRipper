namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Shaders/ShaderProperties.cs"/>
/// </summary>
[Flags]
public enum SerializedPropertyFlag : uint
{
	None = 0,
	HideInInspector = 1 << 0,
	PerRendererData = 1 << 1,
	NoScaleOffset = 1 << 2,
	Normal = 1 << 3,
	HDR = 1 << 4,
	Gamma = 1 << 5,
	NonModifiableTextureData = 1 << 6,
	MainTexture = 1 << 7,
	MainColor = 1 << 8,
}

public static class SerializedPropertyFlagExtensions
{
	public static bool IsHideInInspector(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.HideInInspector) != 0;
	}

	public static bool IsPerRendererData(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.PerRendererData) != 0;
	}

	public static bool IsNoScaleOffset(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.NoScaleOffset) != 0;
	}

	public static bool IsNormal(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.Normal) != 0;
	}

	public static bool IsHDR(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.HDR) != 0;
	}

	public static bool IsGamma(this SerializedPropertyFlag _this)
	{
		return (_this & SerializedPropertyFlag.Gamma) != 0;
	}
}
