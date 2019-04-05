using System.Globalization;
using System.IO;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShaderState : IAssetReadable
	{
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadZClip(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

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
			reader.AlignStream(AlignType.Align4);

			if (IsReadZClip(reader.Version))
			{
				ZClip.Read(reader);
			}
			ZTest.Read(reader);
			ZWrite.Read(reader);
			Culling.Read(reader);
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
			reader.AlignStream(AlignType.Align4);
		}

		public void Export(TextWriter writer)
		{
			if (Name != string.Empty)
			{
				writer.WriteIndent(3);
				writer.Write("Name \"{0}\"\n", Name);
			}
			if (LOD != 0)
			{
				writer.WriteIndent(3);
				writer.Write("LOD {0}\n", LOD);
			}
			Tags.Export(writer, 3);
			
			RtBlend0.Export(writer, RtSeparateBlend ? 0 : -1);
			RtBlend1.Export(writer, 1);
			RtBlend2.Export(writer, 2);
			RtBlend3.Export(writer, 3);
			RtBlend4.Export(writer, 4);
			RtBlend5.Export(writer, 5);
			RtBlend6.Export(writer, 6);
			RtBlend7.Export(writer, 7);

			if (AlphaToMaskValue)
			{
				writer.WriteIndent(3);
				writer.Write("AlphaToMask On\n");
			}

			if (!ZClipValue.IsOn())
			{
				writer.WriteIndent(3);
				writer.Write("ZClip {0}\n", ZClipValue);
			}
			if (!ZTestValue.IsLEqual() && !ZTestValue.IsNone())
			{
				writer.WriteIndent(3);
				writer.Write("ZTest {0}\n", ZTestValue);
			}
			if (!ZWriteValue.IsOn())
			{
				writer.WriteIndent(3);
				writer.Write("ZWrite {0}\n", ZWriteValue);
			}
			if (!CullingValue.IsBack())
			{
				writer.WriteIndent(3);
				writer.Write("Cull {0}\n", CullingValue);
			}
			if (!OffsetFactor.IsZero || !OffsetUnits.IsZero)
			{
				writer.WriteIndent(3);
				writer.Write("Offset {0}, {1}\n", OffsetFactor.Val, OffsetUnits.Val);
			}

			if (!StencilRef.IsZero || !StencilReadMask.IsMax || !StencilWriteMask.IsMax || !StencilOp.IsDefault || !StencilOpFront.IsDefault || !StencilOpBack.IsDefault)
			{
				writer.WriteIndent(3);
				writer.Write("Stencil {\n");
				if(!StencilRef.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Ref {0}\n", StencilRef.Val);
				}
				if(!StencilReadMask.IsMax)
				{
					writer.WriteIndent(4);
					writer.Write("ReadMask {0}\n", StencilReadMask.Val);
				}
				if(!StencilWriteMask.IsMax)
				{
					writer.WriteIndent(4);
					writer.Write("WriteMask {0}\n", StencilWriteMask.Val);
				}
				if(!StencilOp.IsDefault)
				{
					StencilOp.Export(writer, StencilType.Base);
				}
				if(!StencilOpFront.IsDefault)
				{
					StencilOpFront.Export(writer, StencilType.Front);
				}
				if(!StencilOpBack.IsDefault)
				{
					StencilOpBack.Export(writer, StencilType.Back);
				}
				writer.WriteIndent(3);
				writer.Write("}\n");
			}
			
			if(!FogMode.IsUnknown() || !FogColor.IsZero || !FogDensity.IsZero || !FogStart.IsZero || !FogEnd.IsZero)
			{
				writer.WriteIndent(3);
				writer.Write("Fog {\n");
				if(!FogMode.IsUnknown())
				{
					writer.WriteIndent(4);
					writer.Write("Mode {0}\n", FogMode);
				}
				if (!FogColor.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Color ({0},{1},{2},{3})\n",
						FogColor.X.Val.ToString(CultureInfo.InvariantCulture),
						FogColor.Y.Val.ToString(CultureInfo.InvariantCulture),
						FogColor.Z.Val.ToString(CultureInfo.InvariantCulture),
						FogColor.W.Val.ToString(CultureInfo.InvariantCulture));
				}
				if (!FogDensity.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Density {0}\n", FogDensity.Val.ToString(CultureInfo.InvariantCulture));
				}
				if (!FogStart.IsZero ||!FogEnd.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Range {0}, {1}\n",
						FogStart.Val.ToString(CultureInfo.InvariantCulture),
						FogEnd.Val.ToString(CultureInfo.InvariantCulture));
				}
				writer.WriteIndent(3);
				writer.Write("}\n");
			}

			if(Lighting)
			{
				writer.WriteIndent(3);
				writer.Write("Lighting {0}\n", LightingValue);
			}
			writer.WriteIndent(3);
			writer.Write("GpuProgramID {0}\n", GpuProgramID);
		}

		public string Name { get; private set; }
		public bool RtSeparateBlend { get; private set; }
		public FogMode FogMode { get; private set; }
		public int GpuProgramID { get; private set; }
		public int LOD { get; private set; }
		public bool Lighting { get; private set; }

		private ZClip ZClipValue => (ZClip)ZClip.Val;
		private ZTest ZTestValue => (ZTest)ZTest.Val;
		private ZWrite ZWriteValue => (ZWrite)ZWrite.Val;
		private Cull CullingValue => (Cull)Culling.Val;
		private bool AlphaToMaskValue => AlphaToMask.Val > 0;
		private string LightingValue => Lighting ? "On" : "Off";

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
