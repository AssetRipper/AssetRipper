namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

/// <summary>
/// <see href="https://docs.unity3d.com/Manual/SL-ZTest.html"/>
/// </summary>
public enum ZTest
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
}

public static class ZTestExtensions
{
	public static bool IsLEqual(this ZTest _this)
	{
		return _this == ZTest.LEqual;
	}
}
