using AssetRipper.Assets.Generics;
using AssetRipper.Export.Modules.Shaders.Extensions;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;
using AssetRipper.SourceGenerated.Subclasses.SerializedPlayerSubProgram;
using AssetRipper.SourceGenerated.Subclasses.SerializedProgram;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperties;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;
using AssetRipper.SourceGenerated.Subclasses.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderRTBlendState;
using AssetRipper.SourceGenerated.Subclasses.SerializedShaderState;
using AssetRipper.SourceGenerated.Subclasses.SerializedStencilOp;
using AssetRipper.SourceGenerated.Subclasses.SerializedSubProgram;
using AssetRipper.SourceGenerated.Subclasses.SerializedSubShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedTagMap;

namespace AssetRipper.Export.Modules.Shaders.IO;

public static class SerializedExtensions
{
	public static void Export(this ISerializedPass _this, ShaderWriter writer)
	{
		writer.WriteIndent(2);
		writer.Write($"{((SerializedPassType)_this.Type).ToString()} ");

		if (_this.Type == (int)SerializedPassType.UsePass)
		{
			writer.Write($"\"{_this.UseName}\"\n");
		}
		else
		{
			writer.Write("{\n");

			if (_this.Type == (int)SerializedPassType.GrabPass)
			{
				if (_this.TextureName.Data.Length > 0)
				{
					writer.WriteIndent(3);
					writer.Write($"\"{_this.TextureName}\"\n");
				}
			}
			else if (_this.Type == (int)SerializedPassType.Pass)
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

	public static void Export(this ISerializedProgram _this, ShaderWriter writer, ShaderType type)
	{
		if (_this.Has_PlayerSubPrograms())
		{
			if (_this.GetPlayerSubPrograms().Count == 0)
			{
				return;
			}
		}
		else
		{
			if (_this.SubPrograms.Count == 0)
			{
				return;
			}
		}

		writer.WriteIndent(3);
		writer.Write($"Program \"{type.ToProgramTypeString()}\" {{\n");
		if (_this.Has_PlayerSubPrograms())
		{
			IReadOnlyList<ISerializedPlayerSubProgram> subPrograms = _this.GetPlayerSubPrograms();
			for (int i = 0; i < subPrograms.Count; i++)
			{
				subPrograms[i].Export(_this.GetParameterBlobIndices()[i], writer, type);
			}
		}
		else
		{
			int tierCount = _this.GetTierCount();
			for (int i = 0; i < _this.SubPrograms.Count; i++)
			{
				_this.SubPrograms[i].Export(writer, type, tierCount > 1);
			}
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
		foreach (Utf8String attribute in _this.Attributes)
		{
			writer.Write($"[{attribute}] ");
		}
		SerializedPropertyFlag flags = (SerializedPropertyFlag)_this.Flags;
		if (flags.IsHideInInspector())
		{
			writer.Write("[HideInInspector] ");
		}
		if (flags.IsPerRendererData())
		{
			writer.Write("[PerRendererData] ");
		}
		if (flags.IsNoScaleOffset())
		{
			writer.Write("[NoScaleOffset] ");
		}
		if (flags.IsNormal())
		{
			writer.Write("[Normal] ");
		}
		if (flags.IsHDR())
		{
			writer.Write("[HDR] ");
		}
		if (flags.IsGamma())
		{
			writer.Write("[Gamma] ");
		}

		writer.Write($"{_this.Name} (\"{_this.Description}\", ");

		switch (_this.GetType_())
		{
			case SerializedPropertyType.Color:
				writer.Write("Color");
				break;

			case SerializedPropertyType.Vector:
				writer.Write("Vector");
				break;

			case SerializedPropertyType.Float:
				writer.Write("Float");
				break;

			case SerializedPropertyType.Range:
				writer.Write($"{"Range"}({_this.DefValue_1_.ToStringInvariant()}, {_this.DefValue_2_.ToStringInvariant()})");
				break;

			case SerializedPropertyType.Texture:
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
						writer.Write("Cube");
						break;
					case 5:
						writer.Write("2DArray");
						break;
					case 6:
						writer.Write("CubeArray");
						break;
					default:
						throw new NotSupportedException("Texture dimension isn't supported");

				}
				break;

			case SerializedPropertyType.Int:
				writer.Write("Int");
				break;

			default:
				throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
		}
		writer.Write(") = ");

		switch (_this.GetType_())
		{
			case SerializedPropertyType.Color:
			case SerializedPropertyType.Vector:
				writer.Write($"({_this.DefValue_0_.ToStringInvariant()},{_this.DefValue_1_.ToStringInvariant()},{_this.DefValue_2_.ToStringInvariant()},{_this.DefValue_3_.ToStringInvariant()})");
				break;

			case SerializedPropertyType.Float:
			case SerializedPropertyType.Range:
			case SerializedPropertyType.Int:
				writer.Write(_this.DefValue_0_.ToStringInvariant());
				break;

			case SerializedPropertyType.Texture:
				writer.Write($"\"{_this.DefTexture.DefaultName}\" {{}}");
				break;

			default:
				throw new NotSupportedException($"Serialized property type {_this.Type} isn't supported");
		}
		writer.Write('\n');
	}

	public static void Export(this ISerializedShader _this, ShaderWriter writer)
	{
		writer.Write($"Shader \"{_this.Name}\" {{\n");

		_this.PropInfo.Export(writer);

		for (int i = 0; i < _this.SubShaders.Count; i++)
		{
			_this.SubShaders[i].Export(writer);
		}

		if (_this.FallbackName.Data.Length != 0)
		{
			writer.WriteIndent(1);
			writer.Write($"Fallback \"{_this.FallbackName}\"\n");
		}

		if (_this.CustomEditorName.Data.Length != 0)
		{
			writer.WriteIndent(1);
			writer.Write($"CustomEditor \"{_this.CustomEditorName}\"\n");
		}

		writer.Write('}');
	}

	public static void Export(this ISerializedShaderRTBlendState _this, TextWriter writer, int index)
	{
		if (!_this.SrcBlendValue().IsOne() || !_this.DestBlendValue().IsZero() || !_this.SrcBlendAlphaValue().IsOne() || !_this.DestBlendAlphaValue().IsZero())
		{
			writer.WriteIndent(3);
			writer.Write("Blend ");
			if (index != -1)
			{
				writer.Write($"{index} ");
			}
			writer.Write($"{_this.SrcBlendValue()} {_this.DestBlendValue()}");
			if (!_this.SrcBlendValue().IsOne() || !_this.DestBlendAlphaValue().IsZero())
			{
				writer.Write($", {_this.SrcBlendAlphaValue()} {_this.DestBlendAlphaValue()}");
			}
			writer.Write('\n');
		}

		if (!_this.BlendOpValue().IsAdd() || !_this.BlendOpAlphaValue().IsAdd())
		{
			writer.WriteIndent(3);
			writer.Write("BlendOp ");
			if (index != -1)
			{
				writer.Write($"{index} ");
			}
			writer.Write(_this.BlendOpValue().ToString());
			if (!_this.BlendOpAlphaValue().IsAdd())
			{
				writer.Write($", {_this.BlendOpAlphaValue()}");
			}
			writer.Write('\n');
		}

		if (!_this.ColMaskValue().IsRBGA())
		{
			writer.WriteIndent(3);
			writer.Write("ColorMask ");
			if (_this.ColMaskValue().IsNone())
			{
				writer.Write(0);
			}
			else
			{
				if (_this.ColMaskValue().IsRed())
				{
					writer.Write('R');
				}
				if (_this.ColMaskValue().IsGreen())
				{
					writer.Write('G');
				}
				if (_this.ColMaskValue().IsBlue())
				{
					writer.Write('B');
				}
				if (_this.ColMaskValue().IsAlpha())
				{
					writer.Write('A');
				}
			}
			if (index != -1)
			{
				writer.Write($" {index}");
			}
			writer.Write($"\n");
		}
	}

	public static void Export(this ISerializedShaderState _this, TextWriter writer)
	{
		if (_this.Name != string.Empty)
		{
			writer.WriteIndent(3);
			writer.Write($"Name \"{_this.Name}\"\n");
		}
		if (_this.LOD != 0)
		{
			writer.WriteIndent(3);
			writer.Write($"LOD {_this.LOD}\n");
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

		if (_this.AlphaToMaskValue())
		{
			writer.WriteIndent(3);
			writer.Write("AlphaToMask On\n");
		}

		if (!_this.ZClipValue().IsOn())
		{
			writer.WriteIndent(3);
			writer.Write($"ZClip {_this.ZClipValue()}\n");
		}
		if (!_this.ZTestValue().IsLEqual() && !_this.ZTestValue().IsNone())
		{
			writer.WriteIndent(3);
			writer.Write($"ZTest {_this.ZTestValue()}\n");
		}
		if (!_this.ZWriteValue().IsOn())
		{
			writer.WriteIndent(3);
			writer.Write($"ZWrite {_this.ZWriteValue()}\n");
		}
		if (!_this.CullingValue().IsBack())
		{
			writer.WriteIndent(3);
			writer.Write($"Cull {_this.CullingValue()}\n");
		}
		if (!_this.OffsetFactor.IsZero() || !_this.OffsetUnits.IsZero())
		{
			writer.WriteIndent(3);
			writer.Write($"Offset {_this.OffsetFactor.Val}, {_this.OffsetUnits.Val}\n");
		}

		if (!_this.StencilRef.IsZero() || !_this.StencilReadMask.IsMax() || !_this.StencilWriteMask.IsMax() || !_this.StencilOp.IsDefault() || !_this.StencilOpFront.IsDefault() || !_this.StencilOpBack.IsDefault())
		{
			writer.WriteIndent(3);
			writer.Write("Stencil {\n");
			if (!_this.StencilRef.IsZero())
			{
				writer.WriteIndent(4);
				writer.Write($"Ref {_this.StencilRef.Val}\n");
			}
			if (!_this.StencilReadMask.IsMax())
			{
				writer.WriteIndent(4);
				writer.Write($"ReadMask {_this.StencilReadMask.Val}\n");
			}
			if (!_this.StencilWriteMask.IsMax())
			{
				writer.WriteIndent(4);
				writer.Write($"WriteMask {_this.StencilWriteMask.Val}\n");
			}
			if (!_this.StencilOp.IsDefault())
			{
				_this.StencilOp.Export(writer, StencilType.Base);
			}
			if (!_this.StencilOpFront.IsDefault())
			{
				_this.StencilOpFront.Export(writer, StencilType.Front);
			}
			if (!_this.StencilOpBack.IsDefault())
			{
				_this.StencilOpBack.Export(writer, StencilType.Back);
			}
			writer.WriteIndent(3);
			writer.Write("}\n");
		}

		if (!_this.FogModeValue().IsUnknown() || !_this.FogColor.IsZero() || !_this.FogDensity.IsZero() || !_this.FogStart.IsZero() || !_this.FogEnd.IsZero())
		{
			writer.WriteIndent(3);
			writer.Write("Fog {\n");
			if (!_this.FogModeValue().IsUnknown())
			{
				writer.WriteIndent(4);
				writer.Write($"Mode {_this.FogMode}\n");
			}
			if (!_this.FogColor.IsZero())
			{
				writer.WriteIndent(4);
				writer.Write($"Color ({_this.FogColor.X.Val.ToStringInvariant()},{_this.FogColor.Y.Val.ToStringInvariant()},{_this.FogColor.Z.Val.ToStringInvariant()},{_this.FogColor.W.Val.ToStringInvariant()})\n");
			}
			if (!_this.FogDensity.IsZero())
			{
				writer.WriteIndent(4);
				writer.Write($"Density {_this.FogDensity.Val.ToStringInvariant()}\n");
			}
			if (!_this.FogStart.IsZero() || !_this.FogEnd.IsZero())
			{
				writer.WriteIndent(4);
				writer.Write($"Range {_this.FogStart.Val.ToStringInvariant()}, {_this.FogEnd.Val.ToStringInvariant()}\n");
			}
			writer.WriteIndent(3);
			writer.Write("}\n");
		}

		if (_this.Lighting)
		{
			writer.WriteIndent(3);
			writer.Write($"Lighting {_this.LightingValue()}\n");
		}
		writer.WriteIndent(3);
		writer.Write($"GpuProgramID {_this.GpuProgramID}\n");
	}

	public static void Export(this ISerializedStencilOp _this, TextWriter writer, StencilType type)
	{
		writer.WriteIndent(4);
		if (_this.CompValue() == StencilComp.Disabled)
		{
			// When the value is set to 'Disabled', it indicates that this is the default value defined using Properties.
			// https://github.com/AssetRipper/AssetRipper/pull/1337
			writer.Write($"Comp{type.ToSuffixString()} [{_this.CompValue()}]\n");
		}
		else
		{
			writer.Write($"Comp{type.ToSuffixString()} {_this.CompValue()}\n");
		}
		writer.WriteIndent(4);
		writer.Write($"Pass{type.ToSuffixString()} {_this.PassValue()}\n");
		writer.WriteIndent(4);
		writer.Write($"Fail{type.ToSuffixString()} {_this.FailValue()}\n");
		writer.WriteIndent(4);
		writer.Write($"ZFail{type.ToSuffixString()} {_this.ZFailValue()}\n");
	}

	public static void Export(this ISerializedSubProgram _this, ShaderWriter writer, ShaderType type, bool isTier)
	{
		writer.WriteIndent(4);
#warning TODO: convertion (DX to HLSL)
		ShaderGpuProgramType programType = _this.GetProgramType(writer.Version);
		GPUPlatform graphicApi = programType.ToGPUPlatform(writer.Platform);
		writer.Write($"SubProgram \"{graphicApi} ");
		if (isTier)
		{
			writer.Write($"hw_tier{_this.ShaderHardwareTier:00} ");
		}
		writer.Write("\" {\n");
		writer.WriteIndent(5);

		int platformIndex = writer.Shader.Platforms!.IndexOf((uint)graphicApi);//ISerializedSubProgram and Platforms both only exist on 5.5+
		writer.Blobs[platformIndex].GetSubProgram(_this.BlobIndex).Export(writer, type);

		writer.Write('\n');
		writer.WriteIndent(4);
		writer.Write("}\n");
	}

	public static void Export(this ISerializedPlayerSubProgram _this, uint paramBlobIndex, ShaderWriter writer, ShaderType type)
	{
		writer.WriteIndent(4);
#warning TODO: convertion (DX to HLSL)
		ShaderGpuProgramType programType = _this.GetProgramType(writer.Version);
		GPUPlatform graphicApi = programType.ToGPUPlatform(writer.Platform);
		writer.Write($"SubProgram \"{graphicApi} ");
		writer.Write("\" {\n");
		writer.WriteIndent(5);

		int platformIndex = writer.Shader.Platforms!.IndexOf((uint)graphicApi);
		writer.Blobs[platformIndex].GetSubProgram(_this.BlobIndex, paramBlobIndex).Export(writer, type);

		writer.Write('\n');
		writer.WriteIndent(4);
		writer.Write("}\n");
	}

	public static void Export(this ISerializedSubShader _this, ShaderWriter writer)
	{
		writer.WriteIndent(1);
		writer.Write("SubShader {\n");
		if (_this.LOD != 0)
		{
			writer.WriteIndent(2);
			writer.Write("LOD {0}\n", _this.LOD);
		}
		_this.Tags.Export(writer, 2);
		for (int i = 0; i < _this.Passes.Count; i++)
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
			foreach (AssetPair<Utf8String, Utf8String> kvp in _this.Tags)
			{
				writer.Write($"\"{kvp.Key}\" = \"{kvp.Value}\" ");
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
				writer.Write($"\"{keyword}\" ");
			}
			if (ShaderSubProgram.HasLocalKeywords(writer.Version))
			{
				foreach (string keyword in _this.LocalKeywords)
				{
					writer.Write($"\"{keyword}\" ");
				}
			}
			writer.Write("}\n");
			writer.WriteIndent(5);
		}

		ShaderGpuProgramType programType = _this.GetProgramType(writer.Version);

		if (writer.WriteQuotesAroundProgram)
		{
			writer.Write($"\"{programType.ToProgramDataKeyword(writer.Platform, type)}");
		}

		if (_this.ProgramData.Length > 0)
		{
			writer.Write("\n");
			writer.WriteIndent(5);

			writer.WriteShaderData(ref _this);
		}

		if (writer.WriteQuotesAroundProgram)
		{
			writer.Write('"');
		}
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
			_this.GetSubProgram((uint)subIndex).Export(writer, ShaderType.Vertex);
		}
		writer.WriteString(header, j, header.Length - j);
	}
}
