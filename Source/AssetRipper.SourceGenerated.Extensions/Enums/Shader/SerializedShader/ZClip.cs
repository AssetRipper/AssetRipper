namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

public enum ZClip
{
	Off = 0,
	On = 1,
}

public static class ZClipExtensions
{
	public static bool IsOn(this ZClip _this)
	{
		return _this == ZClip.On;
	}

	public static bool IsOff(this ZClip _this)
	{
		return _this == ZClip.Off;
	}
}
