using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderState : IAssetReadable, IYamlExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			return 2;
			// return 1;
		}

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add("m_Name", Name);
			node.Add("rtBlend0", RtBlend0.ExportYaml(container));
			node.Add("rtBlend1", RtBlend1.ExportYaml(container));
			node.Add("rtBlend2", RtBlend2.ExportYaml(container));
			node.Add("rtBlend3", RtBlend3.ExportYaml(container));
			node.Add("rtBlend4", RtBlend4.ExportYaml(container));
			node.Add("rtBlend5", RtBlend5.ExportYaml(container));
			node.Add("rtBlend6", RtBlend6.ExportYaml(container));
			node.Add("rtBlend7", RtBlend7.ExportYaml(container));
			node.Add("rtSeparateBlend", RtSeparateBlend);
			if (HasZClip(container.ExportVersion))
			{
				node.Add("zClip", ZClip.ExportYaml(container));
			}

			node.Add("zTest", ZTest.ExportYaml(container));
			node.Add("zWrite", ZWrite.ExportYaml(container));
			node.Add("culling", Culling.ExportYaml(container));
			if (HasConservative(container.ExportVersion))
			{
				node.Add("conservative", Conservative.ExportYaml(container));
			}

			node.Add("offsetFactor", OffsetFactor.ExportYaml(container));
			node.Add("offsetUnits", OffsetUnits.ExportYaml(container));
			node.Add("alphaToMask", AlphaToMask.ExportYaml(container));
			node.Add("stencilOp", StencilOp.ExportYaml(container));
			node.Add("stencilOpFront", StencilOpFront.ExportYaml(container));
			node.Add("stencilOpBack", StencilOpBack.ExportYaml(container));
			node.Add("stencilReadMask", StencilReadMask.ExportYaml(container));
			node.Add("stencilWriteMask", StencilWriteMask.ExportYaml(container));
			node.Add("stencilRef", StencilRef.ExportYaml(container));
			node.Add("fogStart", FogStart.ExportYaml(container));
			node.Add("fogEnd", FogEnd.ExportYaml(container));
			node.Add("fogDensity", FogDensity.ExportYaml(container));
			node.Add("fogColor", FogColor.ExportYaml(container));
			node.Add("fogMode", (int)FogMode);
			node.Add("gpuProgramID", GpuProgramID);
			node.Add("m_Tags", Tags.ExportYaml(container));
			node.Add("m_LOD", LOD);
			node.Add("lighting", Lighting);
			return node;
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

		public SerializedShaderRTBlendState RtBlend0 = new();
		public SerializedShaderRTBlendState RtBlend1 = new();
		public SerializedShaderRTBlendState RtBlend2 = new();
		public SerializedShaderRTBlendState RtBlend3 = new();
		public SerializedShaderRTBlendState RtBlend4 = new();
		public SerializedShaderRTBlendState RtBlend5 = new();
		public SerializedShaderRTBlendState RtBlend6 = new();
		public SerializedShaderRTBlendState RtBlend7 = new();
		public SerializedShaderFloatValue ZClip = new();
		public SerializedShaderFloatValue ZTest = new();
		public SerializedShaderFloatValue ZWrite = new();
		public SerializedShaderFloatValue Culling = new();
		public SerializedShaderFloatValue Conservative = new();
		public SerializedShaderFloatValue OffsetFactor = new();
		public SerializedShaderFloatValue OffsetUnits = new();
		public SerializedShaderFloatValue AlphaToMask = new();
		public SerializedStencilOp StencilOp = new();
		public SerializedStencilOp StencilOpFront = new();
		public SerializedStencilOp StencilOpBack = new();
		public SerializedShaderFloatValue StencilReadMask = new();
		public SerializedShaderFloatValue StencilWriteMask = new();
		public SerializedShaderFloatValue StencilRef = new();
		public SerializedShaderFloatValue FogStart = new();
		public SerializedShaderFloatValue FogEnd = new();
		public SerializedShaderFloatValue FogDensity = new();
		public SerializedShaderVectorValue FogColor = new();
		public SerializedTagMap Tags = new();
	}
}
