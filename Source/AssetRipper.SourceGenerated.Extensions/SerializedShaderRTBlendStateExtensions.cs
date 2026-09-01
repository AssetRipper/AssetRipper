using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderRTBlendState;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderRTBlendStateExtensions
{
	extension(ISerializedShaderRTBlendState state)
	{
		public BlendMode SrcBlendValue => state.SourceBlend.GetValue<BlendMode>();
		public Utf8String SrcBlendName => state.SourceBlend.Name;
		public BlendMode DestBlendValue => state.DestinationBlend.GetValue<BlendMode>();
		public Utf8String DestBlendName => state.DestinationBlend.Name;
		public BlendMode SrcBlendAlphaValue => state.SourceBlendAlpha.GetValue<BlendMode>();
		public Utf8String SrcBlendAlphaName => state.SourceBlendAlpha.Name;
		public BlendMode DestBlendAlphaValue => state.DestinationBlendAlpha.GetValue<BlendMode>();
		public Utf8String DestBlendAlphaName => state.DestinationBlendAlpha.Name;
		public BlendOp BlendOpValue => state.BlendOp.GetValue<BlendOp>();
		public Utf8String BlendOpName => state.BlendOp.Name;
		public BlendOp BlendOpAlphaValue => state.BlendOpAlpha.GetValue<BlendOp>();
		public Utf8String BlendOpAlphaName => state.BlendOpAlpha.Name;
		public ColorWriteMask ColMaskValue => state.ColMask.GetValue<ColorWriteMask>();
		public Utf8String ColMaskName => state.ColMask.Name;
	}
}
