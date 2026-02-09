using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedStencilOp;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedStencilOpExtensions
{
	public static StencilOp PassValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.Pass.Val;
	public static StencilOp FailValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.Fail.Val;
	public static StencilOp ZFailValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.ZFail.Val;
	public static StencilComp CompValue(this ISerializedStencilOp stencilOp) => (StencilComp)stencilOp.Comp.Val;
	public static bool IsDefault(this ISerializedStencilOp stencilOp)
	{
		return stencilOp.PassValue().IsKeep()
			&& stencilOp.FailValue().IsKeep()
			&& stencilOp.ZFailValue().IsKeep()
			&& stencilOp.CompValue().IsAlways();
	}
}
