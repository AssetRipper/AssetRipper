using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderRTBlendState : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			SrcBlend.Read(reader);
			DestBlend.Read(reader);
			SrcBlendAlpha.Read(reader);
			DestBlendAlpha.Read(reader);
			BlendOp.Read(reader);
			BlendOpAlpha.Read(reader);
			ColMask.Read(reader);
		}

		public SerializedShaderFloatValue SrcBlend = new();
		public SerializedShaderFloatValue DestBlend = new();
		public SerializedShaderFloatValue SrcBlendAlpha = new();
		public SerializedShaderFloatValue DestBlendAlpha = new();
		public SerializedShaderFloatValue BlendOp = new();
		public SerializedShaderFloatValue BlendOpAlpha = new();
		public SerializedShaderFloatValue ColMask = new();

		public BlendMode SrcBlendValue => (BlendMode)SrcBlend.Val;
		public BlendMode DestBlendValue => (BlendMode)DestBlend.Val;
		public BlendMode SrcBlendAlphaValue => (BlendMode)SrcBlendAlpha.Val;
		public BlendMode DestBlendAlphaValue => (BlendMode)DestBlendAlpha.Val;
		public BlendOp BlendOpValue => (BlendOp)BlendOp.Val;
		public BlendOp BlendOpAlphaValue => (BlendOp)BlendOpAlpha.Val;
		public ColorWriteMask ColMaskValue => (ColorWriteMask)ColMask.Val;
	}
}
