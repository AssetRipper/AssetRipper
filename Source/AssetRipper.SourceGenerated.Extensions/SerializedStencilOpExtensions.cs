using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedStencilOp;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedStencilOpExtensions
{
	public static StencilOp PassValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.Pass.Value;
	public static StencilOp FailValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.Fail.Value;
	public static StencilOp ZFailValue(this ISerializedStencilOp stencilOp) => (StencilOp)stencilOp.ZFail.Value;
	public static StencilComp CompValue(this ISerializedStencilOp stencilOp) => (StencilComp)stencilOp.Comp.Value;
	public static bool IsDefault(this ISerializedStencilOp stencilOp)
	{
		return stencilOp.PassValue().IsKeep()
			&& stencilOp.FailValue().IsKeep()
			&& stencilOp.ZFailValue().IsKeep()
			&& stencilOp.CompValue().IsAlways();
	}
}
