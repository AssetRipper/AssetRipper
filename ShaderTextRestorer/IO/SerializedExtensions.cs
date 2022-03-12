using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Blob;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.Extensions;
using System;
using System.Globalization;
using System.IO;

namespace ShaderTextRestorer.IO
{
	public static class SerializedExtensions
	{
		public static void Export(this SerializedPass _this, ShaderWriter writer)
		{
			writer.WriteIndent(2);
			writer.Write("{0} ", _this.Type.ToString());

			if (_this.Type == SerializedPassType.UsePass)
			{
				writer.Write("\"{0}\"\n", _this.UseName);
			}
			else
			{
				writer.Write("{\n");

				if (_this.Type == SerializedPassType.GrabPass)
				{
					if (_this.TextureName.Length > 0)
					{
						writer.WriteIndent(3);
						writer.Write("\"{0}\"\n", _this.TextureName);
					}
				}
				else if (_this.Type == SerializedPassType.Pass)
				{
					_this.State.Export(writer);

					if ((_this.ProgramMask & ShaderType.Vertex.ToProgramMask()) != 0)
					{
						_this.ProgVertex.Export(writer, ShaderType.Vertex);
					}
					if ((_this.ProgramMask & ShaderType.Fragment.ToProgramMask()) != 0)
					{
						_this.ProgFragment.Export(writer, ShaderType.Fragment);
					}
					if ((_this.ProgramMask & ShaderType.Geometry.ToProgramMask()) != 0)
					{
						_this.ProgGeometry.Export(writer, ShaderType.Geometry);
					}
					if ((_this.ProgramMask & ShaderType.Hull.ToProgramMask()) != 0)
					{
						_this.ProgHull.Export(writer, ShaderType.Hull);
					}
					if ((_this.ProgramMask & ShaderType.Domain.ToProgramMask()) != 0)
					{
						_this.ProgDomain.Export(writer, ShaderType.Domain);
					}
					if ((_this.ProgramMask & ShaderType.RayTracing.ToProgramMask()) != 0)
					{
						_this.ProgDomain.Export(writer, ShaderType.RayTracing);
					}

#warning HasInstancingVariant?
				}
				else
				{
					throw new NotSupportedException($"Unsupported pass type {_this.Type}");
				}

				writer.WriteIndent(2);
				writer.Write("}\n");
			}
		}

		public static void Export(this SerializedProgram _this, ShaderWriter writer, ShaderType type)
		{
			if (_this.SubPrograms.Length == 0)
			{
				return;
			}

			writer.WriteIndent(3);
			writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
			int tierCount = _this.GetTierCount();
			for (int i = 0; i < _this.SubPrograms.Length; i++)
			{
				_this.SubPrograms[i].Export(writer, type, tierCount > 1);
			}
			writer.WriteIndent(3);
			writer.Write("}\n");
		}

		public static void Export(this ISerializedProperties _this, TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach (ISerializedProperty prop in _this.Props)
			{
				prop.Export(writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		public static void Export(this ISerializedProperty _this, TextWriter writer)
		{
			writer.WriteIndent(2);
			foreach (Utf8StringBase attribute in _this.Attributes)
			{
				writer.Write("[{0}] ", attribute);
			}
			if (_this.Flags.IsHideInInspector())
			{
				writer.Write("[HideInInspector] ");
			}
			if (_this.Flags.IsPerRendererData())
			{
				writer.Write("[PerRendererData] ");
			}
			if (_this.Flags.IsNoScaleOffset())
			{
				writer.Write("[NoScaleOffset] ");
			}
			if (_this.Flags.IsNormal())
			{
				writer.Write("[Normal] ");
			}
			if (_this.Flags.IsHDR())
			{
				writer.Write("[HDR] ");
			}
			if (_this.Flags.IsGamma())
			{
				writer.Write("[Gamma] ");
			}

			writer.Write("{0} (\"{1}\", ", _this.Name, _this.Description);

			switch (_this.Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write(nameof(SerializedPropertyType.Vector));
					break;

				case SerializedPropertyType.Int:
					//case SerializedPropertyType.Float:
					writer.Write(nameof(SerializedPropertyType.Float));
					break;

				case SerializedPropertyType.Range:
					writer.Write("{0}({1}, {2})",
						nameof(SerializedPropertyType.Range),
						_this.DefValue1.ToString(CultureInfo.InvariantCulture),
						_this.DefValue2.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
					//case SerializedPropertyType._3D:
					//case SerializedPropertyType.Cube:
					switch (_this.DefTexture.TexDim)
					{
						case 1:
							writer.Write("any");
							break;
						case 2:
							writer.Write("2D");
							break;
						case 3:
							writer.Write("3D");
							break;
						case 4:
							writer.Write(nameof(SerializedPropertyType.Cube));
							break;
						case 5:
							writer.Write("2DArray");
							break;
						case 6:
							writer.Write(nameof(SerializedPropertyType.CubeArray));
							break;
						default:
							throw new NotSupportedException("Texture dimension isn't supported");

					}
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write(") = ");

			switch (_this.Type)
			{
				case SerializedPropertyType.Color:
				case SerializedPropertyType.Vector:
					writer.Write("({0},{1},{2},{3})",
						_this.DefValue0.ToString(CultureInfo.InvariantCulture),
						_this.DefValue1.ToString(CultureInfo.InvariantCulture),
						_this.DefValue2.ToString(CultureInfo.InvariantCulture),
						_this.DefValue3.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType.Int:
				//case SerializedPropertyType.Float:
				case SerializedPropertyType.Range:
					writer.Write(_this.DefValue0.ToString(CultureInfo.InvariantCulture));
					break;

				case SerializedPropertyType._2D:
					//case SerializedPropertyType._3D:
					//case SerializedPropertyType.Cube:
					writer.Write("\"{0}\" {{}}", _this.DefTexture.DefaultName);
					break;

				default:
					throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
			}
			writer.Write('\n');
		}

		public static void Export(this SerializedShader _this, ShaderWriter writer)
		{
			writer.Write("Shader \"{0}\" {{\n", _this.Name);

			_this.PropInfo.Export(writer);

			for (int i = 0; i < _this.SubShaders.Length; i++)
			{
				_this.SubShaders[i].Export(writer);
			}

			if (_this.FallbackName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("Fallback \"{0}\"\n", _this.FallbackName);
			}

			if (_this.CustomEditorName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("CustomEditor \"{0}\"\n", _this.CustomEditorName);
			}

			writer.Write('}');
		}

		public static void Export(this SerializedShaderRTBlendState _this, TextWriter writer, int index)
		{
			if (!_this.SrcBlendValue.IsOne() || !_this.DestBlendValue.IsZero() || !_this.SrcBlendAlphaValue.IsOne() || !_this.DestBlendAlphaValue.IsZero())
			{
				writer.WriteIndent(3);
				writer.Write("Blend ");
				if (index != -1)
				{
					writer.Write("{0} ", index);
				}
				writer.Write("{0} {1}", _this.SrcBlendValue, _this.DestBlendValue);
				if (!_this.SrcBlendAlphaValue.IsOne() || !_this.DestBlendAlphaValue.IsZero())
				{
					writer.Write(", {0} {1}", _this.SrcBlendAlphaValue, _this.DestBlendAlphaValue);
				}
				writer.Write('\n');
			}

			if (!_this.BlendOpValue.IsAdd() || !_this.BlendOpAlphaValue.IsAdd())
			{
				writer.WriteIndent(3);
				writer.Write("BlendOp ");
				if (index != -1)
				{
					writer.Write("{0} ", index);
				}
				writer.Write(_this.BlendOpValue.ToString());
				if (!_this.BlendOpAlphaValue.IsAdd())
				{
					writer.Write(", {0}", _this.BlendOpAlphaValue);
				}
				writer.Write('\n');
			}

			if (!_this.ColMaskValue.IsRBGA())
			{
				writer.WriteIndent(3);
				writer.Write("ColorMask ");
				if (_this.ColMaskValue.IsNone())
				{
					writer.Write(0);
				}
				else
				{
					if (_this.ColMaskValue.IsRed())
					{
						writer.Write('R');
					}
					if (_this.ColMaskValue.IsGreen())
					{
						writer.Write('G');
					}
					if (_this.ColMaskValue.IsBlue())
					{
						writer.Write('B');
					}
					if (_this.ColMaskValue.IsAlpha())
					{
						writer.Write('A');
					}
				}
				writer.Write(" {0}\n", index);
			}
		}

		public static void Export(this SerializedShaderState _this, TextWriter writer)
		{
			if (_this.Name != string.Empty)
			{
				writer.WriteIndent(3);
				writer.Write("Name \"{0}\"\n", _this.Name);
			}
			if (_this.LOD != 0)
			{
				writer.WriteIndent(3);
				writer.Write("LOD {0}\n", _this.LOD);
			}
			_this.Tags.Export(writer, 3);

			_this.RtBlend0.Export(writer, _this.RtSeparateBlend ? 0 : -1);
			_this.RtBlend1.Export(writer, 1);
			_this.RtBlend2.Export(writer, 2);
			_this.RtBlend3.Export(writer, 3);
			_this.RtBlend4.Export(writer, 4);
			_this.RtBlend5.Export(writer, 5);
			_this.RtBlend6.Export(writer, 6);
			_this.RtBlend7.Export(writer, 7);

			if (_this.AlphaToMaskValue)
			{
				writer.WriteIndent(3);
				writer.Write("AlphaToMask On\n");
			}

			if (!_this.ZClipValue.IsOn())
			{
				writer.WriteIndent(3);
				writer.Write("ZClip {0}\n", _this.ZClipValue);
			}
			if (!_this.ZTestValue.IsLEqual() && !_this.ZTestValue.IsNone())
			{
				writer.WriteIndent(3);
				writer.Write("ZTest {0}\n", _this.ZTestValue);
			}
			if (!_this.ZWriteValue.IsOn())
			{
				writer.WriteIndent(3);
				writer.Write("ZWrite {0}\n", _this.ZWriteValue);
			}
			if (!_this.CullingValue.IsBack())
			{
				writer.WriteIndent(3);
				writer.Write("Cull {0}\n", _this.CullingValue);
			}
			if (!_this.OffsetFactor.IsZero || !_this.OffsetUnits.IsZero)
			{
				writer.WriteIndent(3);
				writer.Write("Offset {0}, {1}\n", _this.OffsetFactor.Val, _this.OffsetUnits.Val);
			}

			if (!_this.StencilRef.IsZero || !_this.StencilReadMask.IsMax || !_this.StencilWriteMask.IsMax || !_this.StencilOp.IsDefault || !_this.StencilOpFront.IsDefault || !_this.StencilOpBack.IsDefault)
			{
				writer.WriteIndent(3);
				writer.Write("Stencil {\n");
				if (!_this.StencilRef.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Ref {0}\n", _this.StencilRef.Val);
				}
				if (!_this.StencilReadMask.IsMax)
				{
					writer.WriteIndent(4);
					writer.Write("ReadMask {0}\n", _this.StencilReadMask.Val);
				}
				if (!_this.StencilWriteMask.IsMax)
				{
					writer.WriteIndent(4);
					writer.Write("WriteMask {0}\n", _this.StencilWriteMask.Val);
				}
				if (!_this.StencilOp.IsDefault)
				{
					_this.StencilOp.Export(writer, StencilType.Base);
				}
				if (!_this.StencilOpFront.IsDefault)
				{
					_this.StencilOpFront.Export(writer, StencilType.Front);
				}
				if (!_this.StencilOpBack.IsDefault)
				{
					_this.StencilOpBack.Export(writer, StencilType.Back);
				}
				writer.WriteIndent(3);
				writer.Write("}\n");
			}

			if (!_this.FogMode.IsUnknown() || !_this.FogColor.IsZero || !_this.FogDensity.IsZero || !_this.FogStart.IsZero || !_this.FogEnd.IsZero)
			{
				writer.WriteIndent(3);
				writer.Write("Fog {\n");
				if (!_this.FogMode.IsUnknown())
				{
					writer.WriteIndent(4);
					writer.Write("Mode {0}\n", _this.FogMode);
				}
				if (!_this.FogColor.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Color ({0},{1},{2},{3})\n",
						_this.FogColor.X.Val.ToString(CultureInfo.InvariantCulture),
						_this.FogColor.Y.Val.ToString(CultureInfo.InvariantCulture),
						_this.FogColor.Z.Val.ToString(CultureInfo.InvariantCulture),
						_this.FogColor.W.Val.ToString(CultureInfo.InvariantCulture));
				}
				if (!_this.FogDensity.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Density {0}\n", _this.FogDensity.Val.ToString(CultureInfo.InvariantCulture));
				}
				if (!_this.FogStart.IsZero || !_this.FogEnd.IsZero)
				{
					writer.WriteIndent(4);
					writer.Write("Range {0}, {1}\n",
						_this.FogStart.Val.ToString(CultureInfo.InvariantCulture),
						_this.FogEnd.Val.ToString(CultureInfo.InvariantCulture));
				}
				writer.WriteIndent(3);
				writer.Write("}\n");
			}

			if (_this.Lighting)
			{
				writer.WriteIndent(3);
				writer.Write("Lighting {0}\n", _this.LightingValue);
			}
			writer.WriteIndent(3);
			writer.Write("GpuProgramID {0}\n", _this.GpuProgramID);
		}

		public static void Export(this SerializedStencilOp _this, TextWriter writer, StencilType type)
		{
			writer.WriteIndent(4);
			writer.Write("Comp{0} {1}\n", type.ToSuffixString(), _this.CompValue);
			writer.WriteIndent(4);
			writer.Write("Pass{0} {1}\n", type.ToSuffixString(), _this.PassValue);
			writer.WriteIndent(4);
			writer.Write("Fail{0} {1}\n", type.ToSuffixString(), _this.FailValue);
			writer.WriteIndent(4);
			writer.Write("ZFail{0} {1}\n", type.ToSuffixString(), _this.ZFailValue);
		}

		public static void Export(this SerializedSubProgram _this, ShaderWriter writer, ShaderType type, bool isTier)
		{
			writer.WriteIndent(4);
#warning TODO: convertion (DX to HLSL)
			ShaderGpuProgramType programType = _this.GetProgramType(writer.Version);
			GPUPlatform graphicApi = programType.ToGPUPlatform(writer.Platform);
			writer.Write("SubProgram \"{0} ", graphicApi);
			if (isTier)
			{
				writer.Write("hw_tier{0} ", _this.ShaderHardwareTier.ToString("00"));
			}
			writer.Write("\" {\n");
			writer.WriteIndent(5);

			int platformIndex = writer.Shader.Platforms.IndexOf(graphicApi);
			writer.Shader.Blobs[platformIndex].SubPrograms[_this.BlobIndex].Export(writer, type);

			writer.Write('\n');
			writer.WriteIndent(4);
			writer.Write("}\n");
		}

		public static void Export(this SerializedSubShader _this, ShaderWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("SubShader {\n");
			if (_this.LOD != 0)
			{
				writer.WriteIndent(2);
				writer.Write("LOD {0}\n", _this.LOD);
			}
			_this.Tags.Export(writer, 2);
			for (int i = 0; i < _this.Passes.Length; i++)
			{
				_this.Passes[i].Export(writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		public static void Export(this SerializedTagMap _this, TextWriter writer, int indent)
		{
			if (_this.Tags.Count != 0)
			{
				writer.WriteIndent(indent);
				writer.Write("Tags { ");
				foreach (var kvp in _this.Tags)
				{
					writer.Write("\"{0}\" = \"{1}\" ", kvp.Key, kvp.Value);
				}
				writer.Write("}\n");
			}
		}

		public static void Export(this ShaderSubProgram _this, ShaderWriter writer, ShaderType type)
		{
			if (_this.GlobalKeywords.Length > 0)
			{
				writer.Write("Keywords { ");
				foreach (string keyword in _this.GlobalKeywords)
				{
					writer.Write("\"{0}\" ", keyword);
				}
				if (ShaderSubProgram.HasLocalKeywords(writer.Version))
				{
					foreach (string keyword in _this.LocalKeywords)
					{
						writer.Write("\"{0}\" ", keyword);
					}
				}
				writer.Write("}\n");
				writer.WriteIndent(5);
			}

#warning TODO: convertion (DX to HLSL)
			ShaderGpuProgramType programType = _this.GetProgramType(writer.Version);
			writer.Write("\"{0}", programType.ToProgramDataKeyword(writer.Platform, type));
			if (_this.ProgramData.Length > 0)
			{
				writer.Write("\n");
				writer.WriteIndent(5);

				writer.WriteShaderData(ref _this);
			}
			writer.Write('"');
		}

		public static void Export(this ShaderSubProgramBlob _this, ShaderWriter writer, string header)
		{
			int j = 0;
			while (true)
			{
				int index = header.IndexOf(ShaderSubProgramBlob.GpuProgramIndexName, j);
				if (index == -1)
				{
					break;
				}

				int length = index - j;
				writer.WriteString(header, j, length);
				j += length + ShaderSubProgramBlob.GpuProgramIndexName.Length + 1;

				int subIndex = -1;
				for (int startIndex = j; j < header.Length; j++)
				{
					if (!char.IsDigit(header[j]))
					{
						string numberStr = header.Substring(startIndex, j - startIndex);
						subIndex = int.Parse(numberStr);
						break;
					}
				}

				// we don't know shader type so pass vertex
				_this.SubPrograms[subIndex].Export(writer, ShaderType.Vertex);
			}
			writer.WriteString(header, j, header.Length - j);
		}
	}
}
