namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

public enum ZWrite
{
	Off = 0,
	On = 1,
}

public static class ZWriteExtensions
{
	public static bool IsOff(this ZWrite _this)
	{
		return _this == ZWrite.Off;
	}

	public static bool IsOn(this ZWrite _this)
	{
		return _this == ZWrite.On;
	}
}
