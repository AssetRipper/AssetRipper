using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedShaderRTBlendState : IAssetReadable
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

		public SerializedShaderFloatValue SrcBlend;
		public SerializedShaderFloatValue DestBlend;
		public SerializedShaderFloatValue SrcBlendAlpha;
		public SerializedShaderFloatValue DestBlendAlpha;
		public SerializedShaderFloatValue BlendOp;
		public SerializedShaderFloatValue BlendOpAlpha;
		public SerializedShaderFloatValue ColMask;

		public BlendMode SrcBlendValue => (BlendMode)SrcBlend.Val;
		public BlendMode DestBlendValue => (BlendMode)DestBlend.Val;
		public BlendMode SrcBlendAlphaValue => (BlendMode)SrcBlendAlpha.Val;
		public BlendMode DestBlendAlphaValue => (BlendMode)DestBlendAlpha.Val;
		public BlendOp BlendOpValue => (BlendOp)BlendOp.Val;
		public BlendOp BlendOpAlphaValue => (BlendOp)BlendOpAlpha.Val;
		public ColorWriteMask ColMaskValue => (ColorWriteMask)ColMask.Val;
	}
}
