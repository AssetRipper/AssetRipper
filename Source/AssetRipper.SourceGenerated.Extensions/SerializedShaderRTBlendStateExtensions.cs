using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderRTBlendState;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderRTBlendStateExtensions
{
	public static BlendMode SrcBlendValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.SourceBlend.Value;
	public static BlendMode DestBlendValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.DestinationBlend.Value;
	public static BlendMode SrcBlendAlphaValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.SourceBlendAlpha.Value;
	public static BlendMode DestBlendAlphaValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.DestinationBlendAlpha.Value;
	public static BlendOp BlendOpValue(this ISerializedShaderRTBlendState state) => (BlendOp)state.BlendOp.Value;
	public static BlendOp BlendOpAlphaValue(this ISerializedShaderRTBlendState state) => (BlendOp)state.BlendOpAlpha.Value;
	public static ColorWriteMask ColMaskValue(this ISerializedShaderRTBlendState state) => (ColorWriteMask)state.ColMask.Value;
}
