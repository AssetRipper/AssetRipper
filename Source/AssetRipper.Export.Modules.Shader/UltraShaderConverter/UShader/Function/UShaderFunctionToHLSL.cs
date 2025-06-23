using AssetRipper.Export.Modules.Shaders.Extensions;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;
using AssetRipper.SourceGenerated.Extensions;
using System.Text;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

public class UShaderFunctionToHLSL
{
	private readonly UShaderProgram _shader;
	private readonly StringBuilder _stringBuilder = new();
	private string _baseIndent = "";
	/// <summary>
	/// Each indent level is 4 spaces.
	/// </summary>
	private const string _indent = "    ";
	private int _indentLevel;

	private delegate void InstHandler(USILInstruction inst);
	private readonly Dictionary<USILInstructionType, InstHandler> _instructionHandlers;

	public UShaderFunctionToHLSL(UShaderProgram shader)
	{
		_shader = shader;

		_instructionHandlers = new()
		{
			{ USILInstructionType.Move, new InstHandler(HandleMove) },
			{ USILInstructionType.MoveConditional, new InstHandler(HandleMoveConditional) },
			{ USILInstructionType.Add, new InstHandler(HandleAdd) },
			{ USILInstructionType.Subtract, new InstHandler(HandleSubtract) },
			{ USILInstructionType.Multiply, new InstHandler(HandleMultiply) },
			{ USILInstructionType.Divide, new InstHandler(HandleDivide) },
			{ USILInstructionType.MultiplyAdd, new InstHandler(HandleMultiplyAdd) },
			{ USILInstructionType.And, new InstHandler(HandleAnd) },
			{ USILInstructionType.Or, new InstHandler(HandleOr) },
			{ USILInstructionType.Not, new InstHandler(HandleNot) },
			{ USILInstructionType.Minimum, new InstHandler(HandleMinimum) },
			{ USILInstructionType.Maximum, new InstHandler(HandleMaximum) },
			{ USILInstructionType.SquareRoot, new InstHandler(HandleSquareRoot) },
			{ USILInstructionType.SquareRootReciprocal, new InstHandler(HandleSquareRootReciprocal) },
			{ USILInstructionType.Logarithm2, new InstHandler(HandleLogarithm2) },
			{ USILInstructionType.Exponential, new InstHandler(HandleExponential) },
			{ USILInstructionType.Reciprocal, new InstHandler(HandleReciprocal) },
			{ USILInstructionType.Fractional, new InstHandler(HandleFractional) },
			{ USILInstructionType.Floor, new InstHandler(HandleFloor) },
			{ USILInstructionType.Ceiling, new InstHandler(HandleCeiling) },
			{ USILInstructionType.Round, new InstHandler(HandleRound) },
			{ USILInstructionType.Truncate, new InstHandler(HandleTruncate) },
			{ USILInstructionType.IntToFloat, new InstHandler(HandleIntToFloat) },
			{ USILInstructionType.FloatToInt, new InstHandler(HandleFloatToInt) },
			{ USILInstructionType.Sine, new InstHandler(HandleSine) },
			{ USILInstructionType.Cosine, new InstHandler(HandleCosine) },
			{ USILInstructionType.ShiftLeft, new InstHandler(HandleShiftLeft) },
			{ USILInstructionType.ShiftRight, new InstHandler(HandleShiftRight) },
			{ USILInstructionType.DotProduct2, new InstHandler(HandleDotProduct) },
			{ USILInstructionType.DotProduct3, new InstHandler(HandleDotProduct) },
			{ USILInstructionType.DotProduct4, new InstHandler(HandleDotProduct) },
			{ USILInstructionType.Sample, new InstHandler(HandleSample) },
			{ USILInstructionType.SampleComparison, new InstHandler(HandleSample) },
			{ USILInstructionType.SampleComparisonLODZero, new InstHandler(HandleSample) },
			{ USILInstructionType.SampleLOD, new InstHandler(HandleSampleLOD) },
			{ USILInstructionType.SampleDerivative, new InstHandler(HandleSampleDerivative) },
			{ USILInstructionType.LoadResource, new InstHandler(HandleLoadResource) },
			{ USILInstructionType.LoadResourceMultisampled, new InstHandler(HandleLoadResource) },
			{ USILInstructionType.LoadResourceStructured, new InstHandler(HandleLoadResourceStructured) },
			{ USILInstructionType.Discard, new InstHandler(HandleDiscard) },
			{ USILInstructionType.ResourceDimensionInfo, new InstHandler(HandleResourceDimensionInfo) },
			{ USILInstructionType.SampleCountInfo, new InstHandler(HandleSampleCountInfo) },
			{ USILInstructionType.GetDimensions, new InstHandler(HandleResourceDimensionInfo) },
			{ USILInstructionType.DerivativeRenderTargetX, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.DerivativeRenderTargetY, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.DerivativeRenderTargetXCoarse, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.DerivativeRenderTargetYCoarse, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.DerivativeRenderTargetXFine, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.DerivativeRenderTargetYFine, new InstHandler(HandleDerivativeRenderTarget) },
			{ USILInstructionType.IfFalse, new InstHandler(HandleIf) },
			{ USILInstructionType.IfTrue, new InstHandler(HandleIf) },
			{ USILInstructionType.Else, new InstHandler(HandleElse) },
			{ USILInstructionType.EndIf, new InstHandler(HandleEndIf) },
			{ USILInstructionType.Loop, new InstHandler(HandleLoop) },
			{ USILInstructionType.EndLoop, new InstHandler(HandleEndLoop) },
			{ USILInstructionType.Break, new InstHandler(HandleBreak) },
			{ USILInstructionType.Continue, new InstHandler(HandleContinue) },
			{ USILInstructionType.ForLoop, new InstHandler(HandleForLoop) },
			{ USILInstructionType.Equal, new InstHandler(HandleEqual) },
			{ USILInstructionType.NotEqual, new InstHandler(HandleNotEqual) },
			{ USILInstructionType.LessThan, new InstHandler(HandleLessThan) },
			{ USILInstructionType.LessThanOrEqual, new InstHandler(HandleLessThanOrEqual) },
			{ USILInstructionType.GreaterThan, new InstHandler(HandleGreaterThan) },
			{ USILInstructionType.GreaterThanOrEqual, new InstHandler(HandleGreaterThanOrEqual) },
			{ USILInstructionType.Return, new InstHandler(HandleReturn) },
			// extra
			{ USILInstructionType.MultiplyMatrixByVector, new InstHandler(MultiplyMatrixByVector) },
			{ USILInstructionType.Comment, new InstHandler(HandleComment) }
		};
	}

	public string Convert(int indentDepth)
	{
		_baseIndent = new string(' ', indentDepth * _indent.Length);

		_stringBuilder.Clear();

		WriteLocals();

		foreach (USILInstruction inst in _shader.instructions)
		{
			if (_instructionHandlers.ContainsKey(inst.instructionType))
			{
				_instructionHandlers[inst.instructionType](inst);
			}
		}

		return _stringBuilder.ToString();
	}

	private void WriteLocals()
	{
		foreach (USILLocal local in _shader.locals)
		{
			if (local.defaultValues.Count > 0 && local.isArray)
			{
				AppendLine($"{local.type} {local.name}[{local.defaultValues.Count}] = {{");
				if (local.defaultValues.Count > 0)
				{
					_indentLevel++;
					for (int i = 0; i < local.defaultValues.Count; i++)
					{
						USILOperand operand = local.defaultValues[i];
						string comma = i != local.defaultValues.Count - 1 ? "," : "";
						AppendLine($"{operand}{comma}");
					}
					_indentLevel--;
				}
				AppendLine("};");
			}
			else
			{
				AppendLine($"{local.type} {local.name};");
			}
		}
	}

	private void HandleMove(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleMoveConditional(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} ? {srcOps[1]} : {srcOps[2]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleAdd(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} + {srcOps[1]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSubtract(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} - {srcOps[1]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleMultiply(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} * {srcOps[1]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleDivide(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} / {srcOps[1]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleMultiplyAdd(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"{srcOps[0]} * {srcOps[1]} + {srcOps[2]}");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleAnd(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		int op0UintSize = srcOps[0].GetValueCount();
		int op1UintSize = srcOps[1].GetValueCount();
		string value = $"uint{op0UintSize}({srcOps[0]}) & uint{op1UintSize}({srcOps[1]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleOr(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		int op0UintSize = srcOps[0].GetValueCount();
		int op1UintSize = srcOps[1].GetValueCount();
		string value = $"uint{op0UintSize}({srcOps[0]}) | uint{op1UintSize}({srcOps[1]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleNot(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		int op0UintSize = srcOps[0].GetValueCount();
		string value = $"~uint{op0UintSize}({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleMinimum(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"min({srcOps[0]}, {srcOps[1]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleMaximum(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"max({srcOps[0]}, {srcOps[1]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSquareRoot(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"sqrt({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSquareRootReciprocal(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"rsqrt({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleLogarithm2(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"log({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleExponential(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"exp({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleReciprocal(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"rcp({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleFractional(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"frac({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleFloor(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"floor({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleCeiling(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"ceil({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleRound(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"round({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleTruncate(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"trunc({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleIntToFloat(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"floor({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleFloatToInt(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"asint({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSine(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"sin({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleCosine(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"cos({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleShiftLeft(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		USILOperand srcOp0 = srcOps[0];
		USILOperand srcOp1 = srcOps[1];

		// temp fix to prevent compile errors, still innacurate
		int op0IntSize = srcOp0.GetValueCount();
		int op1IntSize = srcOp1.GetValueCount();

		string op0Text, op1Text;

		if (srcOp0.operandType == USILOperandType.ImmediateInt)
		{
			op0Text = $"{srcOp0}";
		}
		else
		{
			op0Text = $"int{op0IntSize}({srcOp0})";
		}

		if (srcOp1.operandType == USILOperandType.ImmediateInt)
		{
			op1Text = $"{srcOp1}";
		}
		else
		{
			op1Text = $"int{op1IntSize}({srcOp1})";
		}

		string value = $"float{op0IntSize}({op0Text} << {op1Text})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleShiftRight(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		USILOperand srcOp0 = srcOps[0];
		USILOperand srcOp1 = srcOps[1];

		// temp fix to prevent compile errors, still innacurate
		int op0IntSize = srcOp0.GetValueCount();
		int op1IntSize = srcOp1.GetValueCount();

		string op0Text, op1Text;

		if (srcOp0.operandType == USILOperandType.ImmediateInt)
		{
			op0Text = $"{srcOp0}";
		}
		else
		{
			op0Text = $"int{op0IntSize}({srcOp0})";
		}

		if (srcOp1.operandType == USILOperandType.ImmediateInt)
		{
			op1Text = $"{srcOp1}";
		}
		else
		{
			op1Text = $"int{op1IntSize}({srcOp1})";
		}

		string value = $"float{op0IntSize}({op0Text} >> {op1Text})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleDotProduct(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = WrapSaturate(inst, $"dot({srcOps[0]}, {srcOps[1]})");
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSample(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		USILOperand textureOperand = srcOps[2];
		int samplerTypeIdx = inst.instructionType == USILInstructionType.Sample ? 3 : 4;
		bool samplerType = srcOps[samplerTypeIdx].immValueInt[0] == 1;
		string args = $"{srcOps[2]}, {srcOps[0]}";
		string value;
		if (!samplerType)
		{
			value = textureOperand.operandType switch
			{
				USILOperandType.Sampler2D => $"tex2D({args})",
				USILOperandType.Sampler3D => $"tex3D({args})",
				USILOperandType.SamplerCube => $"texCUBE({args})",
				USILOperandType.Sampler2DArray => $"UNITY_SAMPLE_TEX2DARRAY({args})",
				USILOperandType.SamplerCubeArray => $"UNITY_SAMPLE_TEXCUBEARRAY({args})",
				_ => $"texND({args})" // unknown real type
			};
		}
		else
		{
			args = $"{srcOps[2]}, {args}";
			value = textureOperand.operandType switch
			{
				USILOperandType.Sampler2D => $"UNITY_SAMPLE_TEX2D_SAMPLER({args})",
				USILOperandType.Sampler3D => $"UNITY_SAMPLE_TEX3D_SAMPLER({args})",
				USILOperandType.SamplerCube => $"UNITY_SAMPLE_TEXCUBE_SAMPLER({args})",
				USILOperandType.Sampler2DArray => $"UNITY_SAMPLE_TEX2DARRAY_SAMPLER({args})",
				USILOperandType.SamplerCubeArray => $"UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER({args})",
				_ => $"texND({args})" // unknown real type
			};
		}
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSampleLOD(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		USILOperand textureOperand = srcOps[2];
		bool samplerType = srcOps[4].immValueInt[0] == 1;
		string args;
		if (srcOps[0].mask.Length == 2) // texture2d
		{
			args = $"{srcOps[2]}, float4({srcOps[0]}, 0, {srcOps[3]})";
		}
		else
		{
			args = $"{srcOps[2]}, float4({srcOps[0]}, {srcOps[3]})";
		}

		string value;
		if (!samplerType)
		{
			value = textureOperand.operandType switch
			{
				USILOperandType.Sampler2D => $"tex2Dlod({args})",
				USILOperandType.Sampler3D => $"tex3Dlod({args})",
				USILOperandType.SamplerCube => $"texCUBElod({args})",
				USILOperandType.Sampler2DArray => $"UNITY_SAMPLE_TEX2DARRAY_LOD({args})",
				USILOperandType.SamplerCubeArray => $"UNITY_SAMPLE_TEXCUBEARRAY_LOD({args})",
				_ => $"texNDlod({args})" // unknown real type
			};
		}
		else
		{
			args = $"{srcOps[2]}, {args}";
			value = textureOperand.operandType switch
			{
				USILOperandType.Sampler2D => $"UNITY_SAMPLE_TEX2D_SAMPLER({args})",
				USILOperandType.Sampler3D => $"UNITY_SAMPLE_TEX3D_SAMPLER({args})",
				USILOperandType.SamplerCube => $"UNITY_SAMPLE_TEXCUBE_SAMPLER({args})",
				USILOperandType.Sampler2DArray => $"UNITY_SAMPLE_TEX2DARRAY_SAMPLER({args})",
				USILOperandType.SamplerCubeArray => $"UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER({args})",
				_ => $"texND({args})" // unknown real type
			};
		}
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleSampleDerivative(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		USILOperand textureOperand = srcOps[2];
		string value;
		string args = $"{srcOps[2]}, {srcOps[0]}, {srcOps[3]}, {srcOps[4]}";
		value = textureOperand.operandType switch
		{
			USILOperandType.Sampler2D => $"tex2Dgrad({args})",
			USILOperandType.Sampler3D => $"tex3Dgrad({args})",
			USILOperandType.SamplerCube => $"texCUBEgrad({args})",
			_ => $"texNDgrad({args})" // unknown real type
		};
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleLoadResource(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string args = $"{srcOps[1]}, {srcOps[0]}";
		string value = $"Load({args})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleLoadResourceStructured(USILInstruction inst)
	{
		// todo (won't work because struct doesn't exist)
		// DXDecompiler: ((float4[arraySize])_Buffer.Load(srcAddress))[srcByteOffset / 16];
		// 3DMigoto: _Buffer[srcAddress].val[srcByteOffset/4]; (with /4 literally part of the output lmao)
		// yo idk
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"((float4[1]){srcOps[2]}.Load({srcOps[0]}))[{srcOps[1].immValueInt[0] / 16}]";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleDiscard(USILInstruction inst)
	{
		string comment = CommentString(inst);
		AppendLine($"{comment}discard;");
	}

	private void HandleResourceDimensionInfo(USILInstruction inst)
	{
		// assumes resinfo_extra exists
		List<USILOperand> srcOps = inst.srcOperands;

		USILOperand usilResource = srcOps[0];
		USILOperand usilMipLevel = srcOps[1];
		USILOperand usilWidth = srcOps[2];
		USILOperand usilHeight = srcOps[3];
		USILOperand usilDepthOrArraySize = srcOps[4];
		USILOperand usilMipCount = srcOps[5];

		List<string> args = new List<string>();

		if (usilMipLevel.immValueFloat[0] == 0 && usilMipCount.operandType == USILOperandType.Null)
		{
			// shorter version (not checking the compiler did this correctly!)
			args.Add(usilWidth.ToString());

			if (usilHeight.operandType != USILOperandType.Null)
			{
				args.Add(usilHeight.ToString());
			}

			if (usilDepthOrArraySize.operandType != USILOperandType.Null)
			{
				args.Add(usilDepthOrArraySize.ToString());
			}
		}
		else
		{
			args.Add(usilMipLevel.ToString());
			args.Add(usilWidth.ToString());

			if (usilHeight.operandType != USILOperandType.Null)
			{
				args.Add(usilHeight.ToString());
			}

			if (usilDepthOrArraySize.operandType != USILOperandType.Null)
			{
				args.Add(usilDepthOrArraySize.ToString());
			}

			if (usilMipCount.operandType != USILOperandType.Null)
			{
				args.Add(usilMipCount.ToString());
			}
			else
			{
				args.Add("resinfo_extra");
			}
		}

		string call = $"GetDimensions({string.Join(", ", args)})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{usilResource}.{call};");
	}

	private void HandleSampleCountInfo(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} = GetRenderTargetSampleCount()";
		string comment = CommentString(inst);
		AppendLine($"{comment}{value};");
	}

	private void HandleDerivativeRenderTarget(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string fun = inst.instructionType switch
		{
			USILInstructionType.DerivativeRenderTargetX => "ddx",
			USILInstructionType.DerivativeRenderTargetY => "ddy",
			USILInstructionType.DerivativeRenderTargetXCoarse => "ddx_coarse",
			USILInstructionType.DerivativeRenderTargetYCoarse => "ddy_coarse",
			USILInstructionType.DerivativeRenderTargetXFine => "ddx_fine",
			USILInstructionType.DerivativeRenderTargetYFine => "ddy_fine",
			_ => "dd?"
		};
		string value = $"{fun}({srcOps[0]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleIf(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string comment = CommentString(inst);
		if (inst.instructionType == USILInstructionType.IfTrue)
		{
			AppendLine($"{comment}if ({srcOps[0]}) {{");
		}
		else
		{
			AppendLine($"{comment}if (!({srcOps[0]})) {{");
		}

		_indentLevel++;
	}

	private void HandleElse(USILInstruction inst)
	{
		_indentLevel--;
		string comment = CommentString(inst);
		AppendLine($"{comment}}} else {{");
		_indentLevel++;
	}

	private void HandleEndIf(USILInstruction inst)
	{
		_indentLevel--;
		string comment = CommentString(inst);
		AppendLine($"{comment}}}");
	}

	private void HandleLoop(USILInstruction inst)
	{
		// this can create bad optos and should be
		// replaced with USILXXXLoopOptimizer if possible.
		string comment = CommentString(inst);
		AppendLine($"{comment}while (true) {{");
		_indentLevel++;
	}

	private void HandleEndLoop(USILInstruction inst)
	{
		_indentLevel--;
		string comment = CommentString(inst);
		AppendLine($"{comment}}}");
	}

	private void HandleBreak(USILInstruction inst)
	{
		string comment = CommentString(inst);
		AppendLine($"{comment}break;");
	}

	private void HandleContinue(USILInstruction inst)
	{
		string comment = CommentString(inst);
		AppendLine($"{comment}continue;");
	}

	private void HandleForLoop(USILInstruction inst)
	{
		string comment = CommentString(inst);

		USILOperand iterRegOp = inst.srcOperands[0];
		USILOperand compOp = inst.srcOperands[1];
		USILInstructionType compType = (USILInstructionType)inst.srcOperands[2].immValueInt[0];
		USILNumberType numberType = (USILNumberType)inst.srcOperands[3].immValueInt[0];
		float addCount = inst.srcOperands[4].immValueFloat[0]; // todo use an int instead of float when int incremented?
		int depth = inst.srcOperands[5].immValueInt[0];

		string numberTypeName = numberType switch
		{
			USILNumberType.Float => "float",
			USILNumberType.Int => "int",
			USILNumberType.UnsignedInt => "unsigned int",
			_ => "?"
		};
		string iterName = USILConstants.ITER_CHARS[depth].ToString(); // better hope someone's not crazy enough to go over
		string compText = compType switch
		{
			USILInstructionType.Equal => "==",
			USILInstructionType.NotEqual => "!=",
			USILInstructionType.GreaterThan => ">",
			USILInstructionType.GreaterThanOrEqual => ">=",
			USILInstructionType.LessThan => "<",
			USILInstructionType.LessThanOrEqual => "<=",
			_ => "?"
		};

		AppendLine(
			$"{comment}for ({numberTypeName} {iterName} = {iterRegOp}; " +
			$"{iterName} {compText} {compOp}; " +
			$"{iterName} += {addCount.ToStringInvariant()}) {{"
		);

		_indentLevel++;
	}

	private void HandleEqual(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} == {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleNotEqual(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} != {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleLessThan(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} < {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleLessThanOrEqual(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} <= {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleGreaterThan(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} > {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleGreaterThanOrEqual(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"{srcOps[0]} >= {srcOps[1]}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleReturn(USILInstruction inst)
	{
		string outputName = _shader.shaderFunctionType switch
		{
			UShaderFunctionType.Vertex => USILConstants.VERT_OUTPUT_LOCAL_NAME,
			UShaderFunctionType.Fragment => USILConstants.FRAG_OUTPUT_LOCAL_NAME,
			_ => "o" // ?
		};

		string value = $"return {outputName}";
		string comment = CommentString(inst);
		AppendLine($"{comment}{value};");
	}

	private void MultiplyMatrixByVector(USILInstruction inst)
	{
		List<USILOperand> srcOps = inst.srcOperands;
		string value = $"mul({srcOps[0]}, {srcOps[1]})";
		string comment = CommentString(inst);
		AppendLine($"{comment}{inst.destOperand} = {value};");
	}

	private void HandleComment(USILInstruction inst)
	{
		AppendLine($"//{inst.destOperand?.comment};");
	}

	private static string WrapSaturate(USILInstruction inst, string str)
	{
		if (inst.saturate)
		{
			str = $"saturate({str})";
		}
		return str;
	}

	private void AppendLine(string line)
	{
		_stringBuilder.Append(_baseIndent);

		for (int i = 0; i < _indentLevel; i++)
		{
			_stringBuilder.Append(_indent);
		}

		_stringBuilder.AppendLine(line);
	}

	// this is awful
	private static string CommentString(USILInstruction inst)
	{
		return inst.commented ? "//" : "";
	}
}
