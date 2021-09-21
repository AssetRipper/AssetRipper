using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedStencilOp : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Pass.Read(reader);
			Fail.Read(reader);
			ZFail.Read(reader);
			Comp.Read(reader);
		}

		public bool IsDefault => PassValue.IsKeep() && FailValue.IsKeep() && ZFailValue.IsKeep() && CompValue.IsAlways();

		public SerializedShaderFloatValue Pass;
		public SerializedShaderFloatValue Fail;
		public SerializedShaderFloatValue ZFail;
		public SerializedShaderFloatValue Comp;

		public StencilOp PassValue => (StencilOp)Pass.Val;
		public StencilOp FailValue => (StencilOp)Fail.Val;
		public StencilOp ZFailValue => (StencilOp)ZFail.Val;
		public StencilComp CompValue => (StencilComp)Comp.Val;
	}
}
