namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

public enum SerializedPassType
{
	Pass = 0,
	UsePass = 1,
	GrabPass = 2,
}
public static class SerializedPassTypeExtensions
{
	public static string ToSerializedString(this SerializedPassType @this) => @this switch
	{
		SerializedPassType.Pass => "Pass",
		SerializedPassType.UsePass => "UsePass",
		SerializedPassType.GrabPass => "GrabPass",
		_ => throw new Exception($"Unsupported pass type {@this}"),
	};
}
