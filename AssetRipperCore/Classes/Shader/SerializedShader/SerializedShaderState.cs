using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedShaderState : IAssetReadable
	{
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasZClip(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasConservative(UnityVersion version) => version.IsGreaterEqual(2020);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			RtBlend0.Read(reader);
			RtBlend1.Read(reader);
			RtBlend2.Read(reader);
			RtBlend3.Read(reader);
			RtBlend4.Read(reader);
			RtBlend5.Read(reader);
			RtBlend6.Read(reader);
			RtBlend7.Read(reader);
			RtSeparateBlend = reader.ReadBoolean();
			reader.AlignStream();

			if (HasZClip(reader.Version))
			{
				ZClip.Read(reader);
			}
			ZTest.Read(reader);
			ZWrite.Read(reader);
			Culling.Read(reader);
			if (HasConservative(reader.Version))
			{
				Conservative.Read(reader);
			}
			OffsetFactor.Read(reader);
			OffsetUnits.Read(reader);
			AlphaToMask.Read(reader);
			StencilOp.Read(reader);
			StencilOpFront.Read(reader);
			StencilOpBack.Read(reader);
			StencilReadMask.Read(reader);
			StencilWriteMask.Read(reader);
			StencilRef.Read(reader);
			FogStart.Read(reader);
			FogEnd.Read(reader);
			FogDensity.Read(reader);
			FogColor.Read(reader);

			FogMode = (FogMode)reader.ReadInt32();
			GpuProgramID = reader.ReadInt32();
			Tags.Read(reader);
			LOD = reader.ReadInt32();
			Lighting = reader.ReadBoolean();
			reader.AlignStream();
		}

		public string Name { get; set; }
		public bool RtSeparateBlend { get; set; }
		public FogMode FogMode { get; set; }
		public int GpuProgramID { get; set; }
		public int LOD { get; set; }
		public bool Lighting { get; set; }

		public ZClip ZClipValue => (ZClip)ZClip.Val;
		public ZTest ZTestValue => (ZTest)ZTest.Val;
		public ZWrite ZWriteValue => (ZWrite)ZWrite.Val;
		public CullMode CullingValue => (CullMode)Culling.Val;
		public bool AlphaToMaskValue => AlphaToMask.Val > 0;
		public string LightingValue => Lighting ? "On" : "Off";

		public SerializedShaderRTBlendState RtBlend0;
		public SerializedShaderRTBlendState RtBlend1;
		public SerializedShaderRTBlendState RtBlend2;
		public SerializedShaderRTBlendState RtBlend3;
		public SerializedShaderRTBlendState RtBlend4;
		public SerializedShaderRTBlendState RtBlend5;
		public SerializedShaderRTBlendState RtBlend6;
		public SerializedShaderRTBlendState RtBlend7;
		public SerializedShaderFloatValue ZClip;
		public SerializedShaderFloatValue ZTest;
		public SerializedShaderFloatValue ZWrite;
		public SerializedShaderFloatValue Culling;
		public SerializedShaderFloatValue Conservative;
		public SerializedShaderFloatValue OffsetFactor;
		public SerializedShaderFloatValue OffsetUnits;
		public SerializedShaderFloatValue AlphaToMask;
		public SerializedStencilOp StencilOp;
		public SerializedStencilOp StencilOpFront;
		public SerializedStencilOp StencilOpBack;
		public SerializedShaderFloatValue StencilReadMask;
		public SerializedShaderFloatValue StencilWriteMask;
		public SerializedShaderFloatValue StencilRef;
		public SerializedShaderFloatValue FogStart;
		public SerializedShaderFloatValue FogEnd;
		public SerializedShaderFloatValue FogDensity;
		public SerializedShaderVectorValue FogColor;
		public SerializedTagMap Tags;
	}
}
