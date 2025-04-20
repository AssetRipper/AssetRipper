using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderRTBlendState;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SerializedShaderRTBlendStateExtensions
	{
		public static BlendMode SrcBlendValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.SrcBlend.Val;
		public static BlendMode DestBlendValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.DestBlend.Val;
		public static BlendMode SrcBlendAlphaValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.SrcBlendAlpha.Val;
		public static BlendMode DestBlendAlphaValue(this ISerializedShaderRTBlendState state) => (BlendMode)state.DestBlendAlpha.Val;
		public static BlendOp BlendOpValue(this ISerializedShaderRTBlendState state) => (BlendOp)state.BlendOp.Val;
		public static BlendOp BlendOpAlphaValue(this ISerializedShaderRTBlendState state) => (BlendOp)state.BlendOpAlpha.Val;
		public static ColorWriteMask ColMaskValue(this ISerializedShaderRTBlendState state) => (ColorWriteMask)state.ColMask.Val;
	}
}
