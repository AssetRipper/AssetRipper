using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderState;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderStateExtensions
{
	public static FogMode FogModeValue(this ISerializedShaderState state) => (FogMode)state.FogMode;
	public static ZClip ZClipValue(this ISerializedShaderState state) => (ZClip)(state.ZClip?.Val ?? 0);
	public static ZTest ZTestValue(this ISerializedShaderState state) => (ZTest)state.ZTest.Val;
	public static ZWrite ZWriteValue(this ISerializedShaderState state) => (ZWrite)state.ZWrite.Val;
	public static CullMode CullingValue(this ISerializedShaderState state) => (CullMode)state.Culling.Val;
	public static bool AlphaToMaskValue(this ISerializedShaderState state) => state.AlphaToMask.Val > 0;
	public static string LightingValue(this ISerializedShaderState state) => state.Lighting ? "On" : "Off";
}
