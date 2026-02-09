namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

public enum StencilComp
{
	Disabled = 0,
	Never = 1,
	Less = 2,
	Equal = 3,
	LEqual = 4,
	Greater = 5,
	NotEqual = 6,
	GEqual = 7,
	Always = 8,
	Count,

	Unknown = -1,
}

public static class StencilCompExtensions
{
	public static bool IsAlways(this StencilComp _this)
	{
		return _this == StencilComp.Always;
	}
}
