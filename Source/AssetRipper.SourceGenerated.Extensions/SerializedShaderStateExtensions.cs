using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderState;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderStateExtensions
{
	public static FogMode FogModeValue(this ISerializedShaderState state) => (FogMode)state.FogMode;
	public static ZClip ZClipValue(this ISerializedShaderState state) => (ZClip)(state.ZClip?.Value ?? 0);
	public static ZTest ZTestValue(this ISerializedShaderState state) => (ZTest)state.ZTest.Value;
	public static ZWrite ZWriteValue(this ISerializedShaderState state) => (ZWrite)state.ZWrite.Value;
	public static CullMode CullingValue(this ISerializedShaderState state) => (CullMode)state.Culling.Value;
	public static bool AlphaToMaskValue(this ISerializedShaderState state) => state.AlphaToMask.Value is not 0;
	public static bool ConservativeValue(this ISerializedShaderState state) => state.Conservative?.Value is not null and not 0;
	public static string LightingValue(this ISerializedShaderState state) => state.Lighting ? "On" : "Off";
}
