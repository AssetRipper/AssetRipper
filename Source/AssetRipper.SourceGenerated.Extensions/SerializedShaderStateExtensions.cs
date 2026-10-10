using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderState;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderStateExtensions
{
	extension(ISerializedShaderState state)
	{
		public FogMode FogModeValue => (FogMode)state.FogMode;
		public ZClip ZClipValue => state.ZClip?.GetValue<ZClip>() ?? default;
		public Utf8String ZClipName => state.ZClip?.Name ?? Utf8String.Empty;
		public ZTest ZTestValue => state.ZTest.GetValue<ZTest>();
		public Utf8String ZTestName => state.ZTest.Name;
		public ZWrite ZWriteValue => state.ZWrite.GetValue<ZWrite>();
		public Utf8String ZWriteName => state.ZWrite.Name;
		public CullMode CullingValue => state.Culling.GetValue<CullMode>();
		public Utf8String CullingName => state.Culling.Name;
		public bool AlphaToMaskValue => state.AlphaToMask.Value is not 0;
		public bool ConservativeValue => state.Conservative?.Value is not null and not 0;
		public string LightingValue => state.Lighting ? "On" : "Off";

		public bool StencilRefIsDefault => state.StencilRef.IsZeroAndNameless;
		public bool StencilReadMaskIsDefault => state.StencilReadMask.IsMaxAndNameless;
		public bool StencilWriteMaskIsDefault => state.StencilWriteMask.IsMaxAndNameless;
		public bool StencilIsDefault
		{
			get
			{
				return state.StencilRefIsDefault
					&& state.StencilReadMaskIsDefault
					&& state.StencilWriteMaskIsDefault
					&& state.StencilOp.IsDefault
					&& state.StencilOpFront.IsDefault
					&& state.StencilOpBack.IsDefault;
			}
		}
	}
}
