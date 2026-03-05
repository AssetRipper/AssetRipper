using AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler.Blocks;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;

public static class DXShaderNamingUtils
{
	// these two are useless now
	public static string GetConstantBufferParamTypeName(VectorParameter param) => GetConstantBufferParamTypeName(param.Dim, 1, param.Type, false);
	public static string GetConstantBufferParamTypeName(MatrixParameter param) => GetConstantBufferParamTypeName(param.RowCount, param.ColumnCount, param.Type, true);

	public static string GetConstantBufferParamTypeName(NumericShaderParameter param) => GetConstantBufferParamTypeName(param.RowCount, param.ColumnCount, param.Type, true);

	public static string GetConstantBufferParamTypeName(int rowCount, int columnCount, ShaderParamType paramType, bool isMatrix)
	{
		string name = $"unknownType";
		string baseName = paramType.ToString().ToLower();

		if (columnCount == 1)
		{
			if (rowCount == 1)
			{
				name = $"{baseName}";
			}

			if (rowCount == 2)
			{
				name = $"{baseName}2";
			}

			if (rowCount == 3)
			{
				name = $"{baseName}3";
			}

			if (rowCount == 4)
			{
				name = $"{baseName}4";
			}
		}
		else if (columnCount == 4)
		{
			if (rowCount == 4 && isMatrix)
			{
				name = $"{baseName}4x4";
			}
		}

		return name;
	}

	public static string GetISGNInputName(ISGN.Input input)
	{
		string type;
		if (input.index > 0)
		{
			type = input.name + input.index;
		}
		else
		{
			type = input.name;
		}

		string name = input.name switch
		{
			"SV_POSITION" => "position",
			"SV_Position" => "position",
			"SV_IsFrontFace" => "facing",
			"POSITION" => "vertex",
			_ => type.ToLower(),
		};
		return name;
	}

	public static string GetOSGNOutputName(OSGN.Output output)
	{
		string type;
		if (output.index > 0)
		{
			type = output.name + output.index;
		}
		else
		{
			type = output.name;
		}

		if (HasSpecialInputOutputName(output.name))
		{
			return GetSpecialInputOutputName(output.name);
		}

		string name = output.name switch
		{
			"SV_POSITION" => "position",
			"POSITION" => "vertex",
			_ => type.ToLower(),
		};

		return name;
	}

	public static bool HasSpecialInputOutputName(string typeName) => GetSpecialInputOutputName(typeName) != string.Empty;
	public static string GetSpecialInputOutputName(string typeName)
	{
		switch (typeName)
		{
			case "SV_Depth":
				{
					return "oDepth";
				}
			case "SV_Coverage":
				{
					return "oMask";
				}
			case "SV_DepthGreaterEqual":
				{
					return "oDepthGE";
				}
			case "SV_DepthLessEqual":
				{
					return "oDepthLE";
				}
			case "SV_StencilRef":
				{
					return "oStencilRef"; // not in 3dmigoto
				}
		}

		return string.Empty;
	}

	public static bool HasSpecialInputOutputName(USILOperandType operandType) => GetSpecialInputOutputName(operandType) != string.Empty;
	public static string GetSpecialInputOutputName(USILOperandType operandType)
	{
		switch (operandType)
		{
			case USILOperandType.InputCoverageMask:
				{
					return "vCoverage";
				}
			case USILOperandType.InputThreadGroupID:
				{
					return "vThreadGroupID";
				}
			case USILOperandType.InputThreadID:
				{
					return "vThreadID";
				}
			case USILOperandType.InputThreadIDInGroup:
				{
					return "vThreadIDInGroup";
				}
			case USILOperandType.InputThreadIDInGroupFlattened:
				{
					return "vThreadIDInGroupFlattened";
				}
			case USILOperandType.InputPrimitiveID:
				{
					return "vPrim";
				}
			case USILOperandType.InputForkInstanceID:
				{
					return "vForkInstanceID";
				}
			case USILOperandType.InputGSInstanceID:
				{
					return "vGSInstanceID";
				}
			case USILOperandType.InputDomainPoint:
				{
					return "vDomain";
				}
			case USILOperandType.OutputControlPointID:
				{
					return "outputControlPointID"; // not in 3dmigoto
				}
			case USILOperandType.OutputDepth:
				{
					return "oDepth";
				}
			case USILOperandType.OutputCoverageMask:
				{
					return "oMask";
				}
			case USILOperandType.OutputDepthGreaterEqual:
				{
					return "oDepthGE";
				}
			case USILOperandType.OutputDepthLessEqual:
				{
					return "oDepthLE";
				}
			case USILOperandType.StencilRef:
				{
					return "oStencilRef"; // not in 3dmigoto
				}
		}

		return string.Empty;
	}

	public static string GetOSGNFormatName(OSGN.Output output)
	{
		int maskSize = GetMaskSize(output.mask);
		return ((FormatType)output.format).ToString() + (maskSize != 1 ? maskSize : "");
	}

	public static int GetMaskSize(byte mask)
	{
		int p = 0;
		for (int i = 0; i < 4; i++)
		{
			if ((mask >> i & 1) == 1)
			{
				p++;
			}
		}
		return p;
	}
}
