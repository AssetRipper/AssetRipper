using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedStencilOp;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedStencilOpExtensions
{
	extension(ISerializedStencilOp stencilOp)
	{
		public StencilOp PassValue => stencilOp.Pass.GetValue<StencilOp>();
		public Utf8String PassName => stencilOp.Pass.Name;
		public StencilOp FailValue => stencilOp.Fail.GetValue<StencilOp>();
		public Utf8String FailName => stencilOp.Fail.Name;
		public StencilOp ZFailValue => stencilOp.ZFail.GetValue<StencilOp>();
		public Utf8String ZFailName => stencilOp.ZFail.Name;
		public StencilComp CompValue => stencilOp.Comp.GetValue<StencilComp>();
		public Utf8String CompName => stencilOp.Comp.Name;
		public bool IsDefault
		{
			get
			{
				return stencilOp.PassValue.IsKeep()
					&& stencilOp.FailValue.IsKeep()
					&& stencilOp.ZFailValue.IsKeep()
					&& stencilOp.CompValue.IsAlways();
			}
		}
	}
}
