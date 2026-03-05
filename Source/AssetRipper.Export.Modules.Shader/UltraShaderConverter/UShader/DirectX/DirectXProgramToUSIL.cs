using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler.Blocks;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;
using System.Diagnostics;
using System.Text;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;

public class DirectXProgramToUSIL
{
	private readonly DirectXCompiledShader _dxShader;

	public UShaderProgram shader;

	private List<USILLocal> Locals => shader.locals;
	private List<USILInstruction> Instructions => shader.instructions;
	private List<USILInputOutput> Inputs => shader.inputs;
	private List<USILInputOutput> Outputs => shader.outputs;

	private delegate void InstHandler(SHDRInstruction inst);
	private readonly Dictionary<Opcode, InstHandler> _instructionHandlers;

	private readonly Dictionary<int, ResourceDimension> _resourceToDimension;

	public DirectXProgramToUSIL(DirectXCompiledShader dxShader)
	{
		_dxShader = dxShader;

		shader = new UShaderProgram();

		_instructionHandlers = new()
		{
			{ Opcode.mov, new InstHandler(HandleMov) },
			{ Opcode.movc, new InstHandler(HandleMovc) },
			{ Opcode.add, new InstHandler(HandleAdd) },
			{ Opcode.iadd, new InstHandler(HandleAdd) },
			{ Opcode.mul, new InstHandler(HandleMul) },
			{ Opcode.imul, new InstHandler(HandleIMul) },
			{ Opcode.umul, new InstHandler(HandleIMul) },
			{ Opcode.div, new InstHandler(HandleDiv) },
			{ Opcode.udiv, new InstHandler(HandleDiv) },
			{ Opcode.mad, new InstHandler(HandleMad) },
			{ Opcode.imad, new InstHandler(HandleMad) },
			{ Opcode.and, new InstHandler(HandleAnd) },
			{ Opcode.or, new InstHandler(HandleOr) },
			{ Opcode.not, new InstHandler(HandleNot) },
			{ Opcode.ftoi, new InstHandler(HandleFtoi) },
			{ Opcode.ftou, new InstHandler(HandleFtoi) },
			{ Opcode.itof, new InstHandler(HandleItof) },
			{ Opcode.utof, new InstHandler(HandleItof) },
			{ Opcode.min, new InstHandler(HandleMin) },
			{ Opcode.imin, new InstHandler(HandleMin) },
			{ Opcode.umin, new InstHandler(HandleMin) },
			{ Opcode.max, new InstHandler(HandleMax) },
			{ Opcode.imax, new InstHandler(HandleMax) },
			{ Opcode.umax, new InstHandler(HandleMax) },
			{ Opcode.sqrt, new InstHandler(HandleSqrt) },
			{ Opcode.rsq, new InstHandler(HandleRsq) },
			{ Opcode.log, new InstHandler(HandleLog) },
			{ Opcode.exp, new InstHandler(HandleExp) },
			{ Opcode.rcp, new InstHandler(HandleRcp) },
			{ Opcode.frc, new InstHandler(HandleFrc) },
			{ Opcode.round_ni, new InstHandler(HandleRoundNi) },
			{ Opcode.round_pi, new InstHandler(HandleRoundPi) },
			{ Opcode.round_ne, new InstHandler(HandleRoundNe) },
			{ Opcode.round_z, new InstHandler(HandleRoundZ) },
			{ Opcode.sincos, new InstHandler(HandleSincos) },
			{ Opcode.ishl, new InstHandler(HandleIShl) },
			{ Opcode.ishr, new InstHandler(HandleIShr) },
			{ Opcode.ushr, new InstHandler(HandleIShr) },
			{ Opcode.dp2, new InstHandler(HandleDp2) },
			{ Opcode.dp3, new InstHandler(HandleDp3) },
			{ Opcode.dp4, new InstHandler(HandleDp4) },
			{ Opcode.sample, new InstHandler(HandleSample) },
			{ Opcode.sample_c, new InstHandler(HandleSampleC) },
			{ Opcode.sample_c_lz, new InstHandler(HandleSampleC) },
			{ Opcode.sample_l, new InstHandler(HandleSampleL) },
			{ Opcode.sample_b, new InstHandler(HandleSampleL) },
			{ Opcode.sample_d, new InstHandler(HandleSampleD) },
			{ Opcode.ld, new InstHandler(HandleLd) },
			{ Opcode.ldms, new InstHandler(HandleLd) },
			{ Opcode.ld_structured, new InstHandler(HandleLdStructured) },
			{ Opcode.discard, new InstHandler(HandleDiscard) },
			{ Opcode.resinfo, new InstHandler(HandleResInfo) },
			{ Opcode.sampleinfo, new InstHandler(HandleSampleInfo) },
			{ Opcode.deriv_rtx, new InstHandler(HandleDerivRt) },
			{ Opcode.deriv_rty, new InstHandler(HandleDerivRt) },
			{ Opcode.deriv_rtx_coarse, new InstHandler(HandleDerivRt) },
			{ Opcode.deriv_rty_coarse, new InstHandler(HandleDerivRt) },
			{ Opcode.deriv_rtx_fine, new InstHandler(HandleDerivRt) },
			{ Opcode.deriv_rty_fine, new InstHandler(HandleDerivRt) },
			{ Opcode.@if, new InstHandler(HandleIf) },
			{ Opcode.@else, new InstHandler(HandleElse) },
			{ Opcode.endif, new InstHandler(HandleEndIf) },
			{ Opcode.loop, new InstHandler(HandleLoop) },
			{ Opcode.endloop, new InstHandler(HandleEndLoop) },
			{ Opcode.@break, new InstHandler(HandleBreak) },
			{ Opcode.breakc, new InstHandler(HandleBreakc) },
			{ Opcode.@continue, new InstHandler(HandleContinue) },
			{ Opcode.continuec, new InstHandler(HandleContinuec) },
			{ Opcode.eq, new InstHandler(HandleEq) },
			{ Opcode.ieq, new InstHandler(HandleEq) },
			{ Opcode.ne, new InstHandler(HandleNeq) },
			{ Opcode.ine, new InstHandler(HandleNeq) },
			{ Opcode.lt, new InstHandler(HandleLt) },
			{ Opcode.ilt, new InstHandler(HandleLt) },
			{ Opcode.ult, new InstHandler(HandleLt) },
			{ Opcode.ge, new InstHandler(HandleGe) },
			{ Opcode.ige, new InstHandler(HandleGe) },
			{ Opcode.uge, new InstHandler(HandleGe) },
			{ Opcode.ret, new InstHandler(HandleRet) },
                ////dec
                { Opcode.dcl_temps, new InstHandler(HandleTemps) },
			{ Opcode.dcl_resource, new InstHandler(HandleResource) },
			{ Opcode.customdata, new InstHandler(HandleCustomData) }
		};

		_resourceToDimension = new Dictionary<int, ResourceDimension>();
	}

	public void Convert()
	{
		ConvertInstructions();
		ConvertInputs();
		ConvertOutputs();
	}

	private void ConvertInstructions()
	{
		SHDR shdr = _dxShader.Shdr;

		AddOutputLocal();

		List<SHDRInstruction> instructions = shdr.shaderInstructions;
		for (int i = 0; i < instructions.Count; i++)
		{
			SHDRInstruction inst = instructions[i];
			if (_instructionHandlers.ContainsKey(inst.opcode))
			{
				_instructionHandlers[inst.opcode](inst);
			}
			else if (!SHDRInstruction.IsDeclaration(inst.opcode))
			{
				Instructions.Add(new USILInstruction
				{
					instructionType = USILInstructionType.Comment,
					destOperand = new USILOperand
					{
						comment = $"unsupported_{inst.opcode}",
						operandType = USILOperandType.Comment
					},
					srcOperands = new()
				});
			}
		}
	}

	private void ConvertInputs()
	{
		ISGN isgn = _dxShader.Isgn;

		ISGN.Input[] dxInputs = isgn.inputs;
		for (int i = 0; i < dxInputs.Length; i++)
		{
			ISGN.Input dxInput = dxInputs[i];

			string inputType = dxInput.name;
			string inputName = DXShaderNamingUtils.GetISGNInputName(dxInput);
			int register = dxInput.register;
			int mask = dxInput.mask;

			USILInputOutput usilInput = new()
			{
				type = inputType,
				name = inputName,
				register = register,
				mask = mask,
				isOutput = false
			};

			Inputs.Add(usilInput);
		}
	}

	private void ConvertOutputs()
	{
		OSGN osgn = _dxShader.Osgn;

		OSGN.Output[] dxOutputs = osgn.outputs;
		for (int i = 0; i < dxOutputs.Length; i++)
		{
			OSGN.Output dxOutput = dxOutputs[i];

			string outputType = dxOutput.name;
			string outputName = DXShaderNamingUtils.GetOSGNOutputName(dxOutput);
			int register = dxOutput.register;
			int mask = dxOutput.mask;

			USILInputOutput usilOutput = new()
			{
				type = outputType,
				name = outputName,
				register = register,
				mask = mask,
				isOutput = true
			};

			Outputs.Add(usilOutput);
		}
	}

	public override string ToString()
	{
		StringBuilder sb = new();

		int depth = 4;
		foreach (USILInstruction instruction in Instructions)
		{
			if (instruction.instructionType is USILInstructionType.EndIf or
				USILInstructionType.EndLoop or
				USILInstructionType.Else)
			{
				depth--;
			}

			sb.Append(new string(' ', depth * 4));
			sb.AppendLine(instruction.ToString());

			if (instruction.instructionType is USILInstructionType.IfFalse or
				USILInstructionType.IfTrue or
				USILInstructionType.Else or
				USILInstructionType.Loop)
			{
				depth++;
			}
		}

		return sb.ToString();
	}

	///////////////////////
	// internal stuff

	private void FillUSILOperand(SHDRInstructionOperand dxOperand, USILOperand usilOperand, int[] mask, bool immIsInt)
	{
		usilOperand.mask = mask;

		usilOperand.negative = (dxOperand.extendedData & 0x40) >> 6 == 1;
		usilOperand.absoluteValue = (dxOperand.extendedData & 0x80) >> 7 == 1;

		switch (dxOperand.operand)
		{
			case Operand.Null:
				{
					usilOperand.operandType = USILOperandType.Null;
					break;
				}
			case Operand.ConstantBuffer:
				{
					// figure out cb names in a later pass
					if (dxOperand.subOperands[1] != null)
					{
						// todo combine cb and icb
						int cbSlotIdx = dxOperand.arraySizes[0];
						int cbOff = dxOperand.registerNumber;
						SHDRInstructionOperand cbOperand = dxOperand.subOperands[1];

						usilOperand.operandType = USILOperandType.ConstantBuffer;
						usilOperand.registerIndex = cbSlotIdx;
						usilOperand.arrayRelative = new USILOperand();
						FillUSILOperand(cbOperand, usilOperand.arrayRelative, cbOperand.swizzle, false);
						usilOperand.arrayIndex = cbOff;
					}
					else
					{
						int cbSlotIdx = dxOperand.arraySizes[0];
						int cbArrIdx = dxOperand.arraySizes[1];

						usilOperand.operandType = USILOperandType.ConstantBuffer;
						usilOperand.registerIndex = cbSlotIdx;
						usilOperand.arrayIndex = cbArrIdx;
					}
					break;
				}
			case Operand.IndexableTemp:
				{
					int cbSlotIdx = dxOperand.arraySizes[0];
					int cbArrIdx = dxOperand.arraySizes[1];

					usilOperand.operandType = USILOperandType.ConstantBuffer;
					usilOperand.registerIndex = cbSlotIdx;
					usilOperand.arrayIndex = cbArrIdx;
					break;
				}
			case Operand.Input:
				{
					int inRegIdx = dxOperand.arraySizes[0];

					usilOperand.operandType = USILOperandType.InputRegister;
					usilOperand.registerIndex = inRegIdx;
					break;
				}
			case Operand.Output:
				{
					int outRegIdx = dxOperand.arraySizes[0];

					usilOperand.operandType = USILOperandType.OutputRegister;
					usilOperand.registerIndex = outRegIdx;
					break;
				}
			case Operand.Temp:
				{
					int tmpRegIdx;
					if (dxOperand.arraySizes.Length > 0)
					{
						tmpRegIdx = dxOperand.arraySizes[0];
					}
					else
					{
						tmpRegIdx = 0;
					}

					usilOperand.operandType = USILOperandType.TempRegister;
					usilOperand.registerIndex = tmpRegIdx;
					break;
				}
			case Operand.Resource:
				{
					int rscRegIdx = dxOperand.arraySizes[0];

					usilOperand.operandType = USILOperandType.ResourceRegister;
					usilOperand.registerIndex = rscRegIdx;
					break;
				}
			case Operand.Sampler:
				{
					int smpRegIdx = dxOperand.arraySizes[0];

					usilOperand.operandType = USILOperandType.SamplerRegister;
					usilOperand.registerIndex = smpRegIdx;
					break;
				}

			case Operand.InputCoverageMask:
				usilOperand.operandType = USILOperandType.InputCoverageMask;
				break;
			case Operand.InputThreadGroupID:
				usilOperand.operandType = USILOperandType.InputThreadGroupID;
				break;
			case Operand.InputThreadID:
				usilOperand.operandType = USILOperandType.InputThreadID;
				break;
			case Operand.InputThreadIDInGroup:
				usilOperand.operandType = USILOperandType.InputThreadIDInGroup;
				break;
			case Operand.InputThreadIDInGroupFlattened:
				usilOperand.operandType = USILOperandType.InputThreadIDInGroupFlattened;
				break;
			case Operand.InputPrimitiveID:
				usilOperand.operandType = USILOperandType.InputPrimitiveID;
				break;
			case Operand.InputForkInstanceID:
				usilOperand.operandType = USILOperandType.InputForkInstanceID;
				break;
			case Operand.InputGSInstanceID:
				usilOperand.operandType = USILOperandType.InputGSInstanceID;
				break;
			case Operand.InputDomainPoint:
				usilOperand.operandType = USILOperandType.InputDomainPoint;
				break;
			case Operand.OutputControlPointID:
				usilOperand.operandType = USILOperandType.OutputControlPointID;
				break;
			case Operand.OutputDepth:
				usilOperand.operandType = USILOperandType.OutputDepth;
				break;
			case Operand.OutputCoverageMask:
				usilOperand.operandType = USILOperandType.OutputCoverageMask;
				break;
			case Operand.OutputDepthGreaterEqual:
				usilOperand.operandType = USILOperandType.OutputDepthGreaterEqual;
				break;
			case Operand.OutputDepthLessEqual:
				usilOperand.operandType = USILOperandType.OutputDepthLessEqual;
				break;
			case Operand.StencilRef:
				usilOperand.operandType = USILOperandType.StencilRef;
				break;

			case Operand.Immediate32:
				{
					usilOperand.immIsInt = immIsInt;
					usilOperand.operandType = immIsInt ? USILOperandType.ImmediateInt : USILOperandType.ImmediateFloat;

					if (dxOperand.immValues.Length == 1)
					{
						if (immIsInt)
						{
							usilOperand.immValueInt =
							[
							ConvertFloatToInt((float)dxOperand.immValues[0])
							];
						}
						else
						{
							usilOperand.immValueFloat =
							[
							(float)dxOperand.immValues[0]
							];
						}
					}
					else if (dxOperand.immValues.Length == 4)
					{
						if (immIsInt)
						{
							usilOperand.immValueInt = new int[mask.Length];
							for (int i = 0; i < mask.Length; i++)
							{
								usilOperand.immValueInt[i] = ConvertFloatToInt((float)dxOperand.immValues[mask[i]]);
							}
						}
						else
						{
							usilOperand.immValueFloat = new float[mask.Length];
							for (int i = 0; i < mask.Length; i++)
							{
								usilOperand.immValueFloat[i] = (float)dxOperand.immValues[mask[i]];
							}
						}
					}
					else
					{
						throw new Exception($"{dxOperand.immValues.Length} imm values not possible");
					}
					break;
				}
			case Operand.ImmediateConstantBuffer:
				{
					int icbSlotIdx = dxOperand.arraySizes[0];
					int icbOff = dxOperand.registerNumber;
					SHDRInstructionOperand icbOperand = dxOperand.subOperands[0];

					usilOperand.operandType = USILOperandType.ImmediateConstantBuffer;
					usilOperand.registerIndex = icbSlotIdx;
					usilOperand.arrayRelative = new USILOperand();
					FillUSILOperand(icbOperand, usilOperand.arrayRelative, icbOperand.swizzle, false);
					usilOperand.arrayIndex = icbOff;
					break;
				}
			default:
				{
					throw new NotSupportedException($"operand type {dxOperand.operand} not supported");
				}
		}
	}

	// there is a better way to check this, but this works too
	private static int[] MapMask(int[] destMask, int[] srcMask)
	{
		if (srcMask == null || srcMask.Length == 0)
		{
			// immediates won't have a mask, so match them to destination values
			return destMask;
		}
		else if (destMask == null || destMask.Length == 0 || destMask.Length == 4)
		{
			// 0 shouldn't happen, but with mask 4 (xyzw) don't worry about mapping mask
			// some registers like OutputDepth don't have mask, so skip them too
			return srcMask;
		}
		else if (destMask.Length == 1)
		{
			// with mask size 1, all inputs are also only size 1
			return [srcMask[0]];
		}
		else //dest.mask.Length == (2 || 3)
		{
			// when mask size is 2 or 3, inputs will be size 4. map the masks correctly
			int[] mask = new int[destMask.Length];
			for (int i = 0; i < mask.Length; i++)
			{
				mask[i] = srcMask[destMask[i]];
			}
			return mask;
		}
	}

	private void HandleMov(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0];
		SHDRInstructionOperand src0 = inst.operands[1];

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Move,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 },
			saturate = inst.saturated
		};

		Instructions.Add(usilInst);
	}

	private void HandleMovc(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation. If src0, then dest = src1 else dest = src2
		SHDRInstructionOperand src0 = inst.operands[1]; // The components on which to test the condition.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components to move. 
		SHDRInstructionOperand src2 = inst.operands[3]; // The components to move. 

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();
		USILOperand usilSrc2 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), false);
		FillUSILOperand(src2, usilSrc2, MapMask(dest.swizzle, src2.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.MoveConditional,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1,
				usilSrc2,
			},
			saturate = inst.saturated,
		};

		Instructions.Add(usilInst);
	}

	private void HandleAdd(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The (vector/number) to add to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The (vector/number) to add to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode == Opcode.iadd;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Add,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated,
			isIntVariant = isInt
		};

		Instructions.Add(usilInst);
	}

	private void HandleMul(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = src0 * src1
		SHDRInstructionOperand src0 = inst.operands[1]; // The multiplicand.
		SHDRInstructionOperand src1 = inst.operands[2]; // The multiplier.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode is Opcode.imul or Opcode.umul;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Multiply,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated,
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.umul
		};

		Instructions.Add(usilInst);
	}

	private void HandleIMul(SHDRInstruction inst)
	{
		SHDRInstructionOperand destHI = inst.operands[0]; // The address of the high 32 bits of the result.
		SHDRInstructionOperand destLO = inst.operands[1]; // The address of the high 32 bits of the result.
		SHDRInstructionOperand src0 = inst.operands[2]; // The value to multiply with src1.
		SHDRInstructionOperand src1 = inst.operands[3]; // The value to multiply with src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		// todo, hi isn't handled
		FillUSILOperand(destLO, usilDest, destLO.swizzle, true);
		FillUSILOperand(src0, usilSrc0, MapMask(destLO.swizzle, src0.swizzle), true);
		FillUSILOperand(src1, usilSrc1, MapMask(destLO.swizzle, src1.swizzle), true);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Multiply,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0, usilSrc1 },
			saturate = inst.saturated,
			isIntVariant = true,
			isIntUnsigned = inst.opcode == Opcode.umul
		};

		Instructions.Add(usilInst);
	}

	private void HandleDiv(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The dividend.
		SHDRInstructionOperand src1 = inst.operands[2]; // The divisor.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode == Opcode.udiv;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Divide,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated,
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.udiv,
		};


		Instructions.Add(usilInst);
	}

	private void HandleMad(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = src0 * src1 + src2
		SHDRInstructionOperand src0 = inst.operands[1]; // The multiplicand.
		SHDRInstructionOperand src1 = inst.operands[2]; // The multiplier.
		SHDRInstructionOperand src2 = inst.operands[3]; // The addend.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();
		USILOperand usilSrc2 = new();

		bool isInt = inst.opcode is Opcode.imad or Opcode.umad;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);
		FillUSILOperand(src2, usilSrc2, MapMask(dest.swizzle, src2.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.MultiplyAdd,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1,
				usilSrc2
			},
			saturate = inst.saturated,
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.umad
		};

		Instructions.Add(usilInst);
	}

	private void HandleAnd(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The 32-bit value to AND with src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The 32-bit value to AND with src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.And,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1,
			},
		};

		Instructions.Add(usilInst);
	}

	private void HandleOr(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components to OR with src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components to OR with src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Or,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1,
			},
		};

		Instructions.Add(usilInst);
	}

	private void HandleNot(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The original components.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Not,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0
			},
		};

		Instructions.Add(usilInst);
	}

	private void HandleFtoi(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation. dest = round_z(src0)
		SHDRInstructionOperand src0 = inst.operands[1]; // The component to convert.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.ftoi => USILInstructionType.FloatToInt,
				Opcode.ftou => USILInstructionType.FloatToUInt,
				_ => USILInstructionType.FloatToUInt,//Throw exception?
			},
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0
			},
		};

		Instructions.Add(usilInst);
	}

	private void HandleItof(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // Contains the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // Contains the value to convert.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.itof => USILInstructionType.IntToFloat,
				Opcode.utof => USILInstructionType.UIntToFloat,
				_ => USILInstructionType.UIntToFloat,//Throw exception?
			},
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0
			},
		};

		Instructions.Add(usilInst);
	}

	private void HandleMin(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = src0 < src1 ? src0 : src1
		SHDRInstructionOperand src0 = inst.operands[1]; // The components to compare to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components to compare to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode is Opcode.imin or Opcode.umin;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Minimum,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.umin
		};

		Instructions.Add(usilInst);
	}

	private void HandleMax(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = src0 >= src1 ? src0 : src1
		SHDRInstructionOperand src0 = inst.operands[1]; // The components to compare to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components to compare to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode is Opcode.imax or Opcode.umax;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Maximum,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.umax
		};

		Instructions.Add(usilInst);
	}

	private void HandleSqrt(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = sqrt(src0)
		SHDRInstructionOperand src0 = inst.operands[1]; // The components for which to take the square root.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.SquareRoot,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRsq(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // Contains the results of the operation. dest = 1.0f / sqrt(src0)
		SHDRInstructionOperand src0 = inst.operands[1]; // The components for the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.SquareRootReciprocal,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleLog(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation. dest = log2(src0)
		SHDRInstructionOperand src0 = inst.operands[1]; // The value for the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Logarithm2,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleExp(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation. dest = 2src0
		SHDRInstructionOperand src0 = inst.operands[1]; // The exponent.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Exponential,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRcp(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results dest = 1.0f / src0.
		SHDRInstructionOperand src0 = inst.operands[1]; // The number to take the reciprocal of.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Reciprocal,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleFrc(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation. dest = src0 - round_ni(src0)
		SHDRInstructionOperand src0 = inst.operands[1]; // The component in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Fractional,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRoundNi(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Floor,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRoundPi(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Ceiling,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRoundNe(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Round,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleRoundZ(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Truncate,
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleSincos(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest0 = inst.operands[0]; // The address of sin(src0), computed per component.
		SHDRInstructionOperand dest1 = inst.operands[1]; // The address of cos(src0), computed per component.
		SHDRInstructionOperand src = inst.operands[2]; // The components for which to compute sin and cos.

		// there is a sincos function in hlsl but we'll just skip that and use individual instructions

		if (dest0.operand != Operand.Null)
		{
			USILOperand usilSinDest = new();
			USILOperand usilSinSrc = new();

			FillUSILOperand(dest0, usilSinDest, dest0.swizzle, false);
			FillUSILOperand(src, usilSinSrc, MapMask(dest0.swizzle, src.swizzle), false);

			USILInstruction usilSinInst = new()
			{
				instructionType = USILInstructionType.Sine,
				destOperand = usilSinDest,
				srcOperands = new() { usilSinSrc }
			};

			Instructions.Add(usilSinInst);
		}

		if (dest1.operand != Operand.Null)
		{
			USILOperand usilCosDest = new();
			USILOperand usilCosSrc = new();

			FillUSILOperand(dest1, usilCosDest, dest1.swizzle, false);
			FillUSILOperand(src, usilCosSrc, MapMask(dest1.swizzle, src.swizzle), false);

			USILInstruction usilCosInst = new()
			{
				instructionType = USILInstructionType.Cosine,
				destOperand = usilCosDest,
				srcOperands = new() { usilCosSrc }
			};

			Instructions.Add(usilCosInst);
		}
	}

	private void HandleIShl(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // Contains the values to be shifted.
		SHDRInstructionOperand src1 = inst.operands[2]; // Contains the shift amount.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, true);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), true);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), true);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.ShiftLeft,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = true
		};

		Instructions.Add(usilInst);
	}

	private void HandleIShr(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // Contains the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // Contains the value to be shifted.
		SHDRInstructionOperand src1 = inst.operands[2]; // Contains the shift amount.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, true);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), true);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), true);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.ShiftRight,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = true
		};

		Instructions.Add(usilInst);
	}

	private void HandleDp2(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation. dest = src0.r * src1.r + src0.g * src1.g
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		int[] mask = [0, 1];
		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, mask, false);
		FillUSILOperand(src1, usilSrc1, mask, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.DotProduct2,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated
		};

		Instructions.Add(usilInst);
	}

	private void HandleDp3(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		int[] mask = [0, 1, 2];
		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, mask, false);
		FillUSILOperand(src1, usilSrc1, mask, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.DotProduct3,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated
		};

		Instructions.Add(usilInst);
	}

	private void HandleDp4(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components in the operation.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		int[] mask = [0, 1, 2, 3];
		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, mask, false);
		FillUSILOperand(src1, usilSrc1, mask, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.DotProduct4,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			saturate = inst.saturated
		};

		Instructions.Add(usilInst);
	}

	private void HandleSample(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // A set of texture coordinates. For more information, see the Remarks section.
		SHDRInstructionOperand srcResource = inst.operands[2]; // A texture register. For more information, see the Remarks section.
		SHDRInstructionOperand srcSampler = inst.operands[3]; // A sampler register. For more information, see the Remarks section.

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcResource = new();
		USILOperand usilSrcSampler = new();
		USILOperand usilSamplerType = new(0); // i.e. unity_ProbeVolumeSH (handled by USILSamplerTypeFixer)

		int[] mask = [0, 1, 2, 3];

		ResourceDimension dimension = _resourceToDimension[srcResource.arraySizes[0]];
		int[] uvMask = GetSampleLocationMask(dimension);

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, MapMask(uvMask, srcAddress.swizzle), false);
		FillUSILOperand(srcResource, usilSrcResource, MapMask(dest.swizzle, srcResource.swizzle), false);
		FillUSILOperand(srcSampler, usilSrcSampler, mask, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Sample,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrcAddress,
				usilSrcResource,
				usilSrcSampler,
				usilSamplerType
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleSampleC(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // A set of texture coordinates. For more information see the sample instruction.
		SHDRInstructionOperand srcResource = inst.operands[2]; // A texture register. For more information see the sample instruction.
		SHDRInstructionOperand srcSampler = inst.operands[3]; // A sampler register. For more information see the sample instruction.
		SHDRInstructionOperand srcReferenceValue = inst.operands[4]; // A register with a single component selected, which is used in the comparison.

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcResource = new();
		USILOperand usilSrcSampler = new();
		USILOperand usilSrcReferenceValue = new();
		USILOperand usilSamplerType = new(0); // i.e. unity_ProbeVolumeSH (handled by USILSamplerTypeFixer)

		int[] mask = [0, 1, 2, 3];

		ResourceDimension dimension = _resourceToDimension[srcResource.arraySizes[0]];
		int[] uvMask = GetSampleLocationMask(dimension);

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, MapMask(uvMask, srcAddress.swizzle), false);
		FillUSILOperand(srcResource, usilSrcResource, MapMask(dest.swizzle, srcResource.swizzle), false);
		FillUSILOperand(srcSampler, usilSrcSampler, mask, false);
		FillUSILOperand(srcReferenceValue, usilSrcReferenceValue, mask, false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.sample_c => USILInstructionType.SampleComparison,
				Opcode.sample_c_lz => USILInstructionType.SampleComparisonLODZero,
				_ => USILInstructionType.SampleComparisonLODZero,//Throw exception?
			},
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrcAddress,
				usilSrcResource,
				usilSrcSampler,
				usilSrcReferenceValue,
				usilSamplerType
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleSampleL(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // A set of texture coordinates. For more information, see the Remarks section.
		SHDRInstructionOperand srcResource = inst.operands[2]; // A texture register. For more information, see the Remarks section.
		SHDRInstructionOperand srcSampler = inst.operands[3]; // A sampler register. For more information, see the Remarks section.
		SHDRInstructionOperand srcLOD = inst.operands[4]; // The LOD.

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcResource = new();
		USILOperand usilSrcSampler = new();
		USILOperand usilSrcLOD = new();
		USILOperand usilSamplerType = new(0); // i.e. unity_ProbeVolumeSH (handled by USILSamplerTypeFixer)

		int[] mask = [0, 1, 2, 3];

		ResourceDimension dimension = _resourceToDimension[srcResource.arraySizes[0]];
		int[] uvMask = GetSampleLocationMask(dimension);

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, MapMask(uvMask, srcAddress.swizzle), false);
		FillUSILOperand(srcResource, usilSrcResource, MapMask(dest.swizzle, srcResource.swizzle), false);
		FillUSILOperand(srcSampler, usilSrcSampler, mask, false);
		FillUSILOperand(srcLOD, usilSrcLOD, srcLOD.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.sample_b => USILInstructionType.SampleLODBias,
				Opcode.sample_l => USILInstructionType.SampleLOD,
				_ => USILInstructionType.SampleLOD, //Throw exception?
			},
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrcAddress,
				usilSrcResource,
				usilSrcSampler,
				usilSrcLOD,
				usilSamplerType
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleSampleD(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // A set of texture coordinates. For more information see the sample instruction.
		SHDRInstructionOperand srcResource = inst.operands[2]; // A texture register. For more information see the sample instruction.
		SHDRInstructionOperand srcSampler = inst.operands[3]; // A sampler register. For more information see the sample instruction.
		SHDRInstructionOperand srcXDerivatives = inst.operands[4]; // The derivatives for the source address in the x direction. For more information, see the Remarks section.
		SHDRInstructionOperand srcYDerivatives = inst.operands[5]; // The derivatives for the source address in the y direction. For more information, see the Remarks section.

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcResource = new();
		USILOperand usilSrcSampler = new();
		USILOperand usilSrcXDerivatives = new();
		USILOperand usilSrcYDerivatives = new();

		int[] mask = [0, 1, 2, 3];

		ResourceDimension dimension = _resourceToDimension[srcResource.arraySizes[0]];
		int[] uvMask = GetSampleLocationMask(dimension);

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, MapMask(uvMask, srcAddress.swizzle), false);
		FillUSILOperand(srcResource, usilSrcResource, MapMask(dest.swizzle, srcResource.swizzle), false);
		FillUSILOperand(srcSampler, usilSrcSampler, mask, false);
		FillUSILOperand(srcXDerivatives, usilSrcXDerivatives, srcXDerivatives.swizzle, false);
		FillUSILOperand(srcYDerivatives, usilSrcYDerivatives, srcYDerivatives.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.SampleDerivative,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrcAddress,
				usilSrcResource,
				usilSrcSampler,
				usilSrcXDerivatives,
				usilSrcYDerivatives
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleLd(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // The texture coordinates needed to perform the sample.
		SHDRInstructionOperand srcResource = inst.operands[2]; // A texture register (t#) which must have been declared identifying which Texture or Buffer to fetch from.

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcResource = new();

		ResourceDimension dimension = _resourceToDimension[srcResource.arraySizes[0]];
		int[] uvMask = GetLoadLocationMask(dimension);

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, MapMask(uvMask, srcAddress.swizzle), false);
		FillUSILOperand(srcResource, usilSrcResource, MapMask(dest.swizzle, srcResource.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.ld => USILInstructionType.LoadResource,
				Opcode.ldms => USILInstructionType.LoadResourceMultisampled,
				_ => USILInstructionType.LoadResourceMultisampled,//Throw exception?
			},
			destOperand = usilDest,
			srcOperands = new() { usilSrcAddress, usilSrcResource },
		};

		Instructions.Add(usilInst);
	}

	private void HandleLdStructured(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand srcAddress = inst.operands[1]; // Specifies the index of the structure to read.
		SHDRInstructionOperand srcByteOffset = inst.operands[2]; // Specifies the byte offset in the structure to start reading from. 
		SHDRInstructionOperand src0 = inst.operands[3]; // The buffer to read from. This parameter must be a SRV (t#), UAV (u#). In the compute shader it can also be thread group shared memory (g#). 

		USILOperand usilDest = new();
		USILOperand usilSrcAddress = new();
		USILOperand usilSrcByteOffset = new();
		USILOperand usilSrc0 = new();

		if (src0.operand == Operand.UnorderedAccessView)
		{
			throw new NotSupportedException("uav on ld_structured not supported");
		}
		else if (src0.operand == Operand.ThreadGroupSharedMemory)
		{
			throw new NotSupportedException("gsm on ld_structured not supported");
		}

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(srcAddress, usilSrcAddress, srcAddress.swizzle, true);
		FillUSILOperand(srcByteOffset, usilSrcByteOffset, srcByteOffset.swizzle, true);
		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.LoadResourceStructured,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrcAddress,
				usilSrcByteOffset,
				usilSrc0
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleDiscard(SHDRInstruction inst)
	{
		SHDRInstructionOperand src0 = inst.operands[0]; // The value that determines whether to discard the current pixel being processed.

		USILOperand usilSrc0 = new();

		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Discard,
			destOperand = null,
			srcOperands = new()
			{
				usilSrc0
			}
		};

		// if works the same way, so we can pretend the discard is an if
		HandleIf(inst);
		Instructions.Add(usilInst);
		HandleEndIf(inst);
	}

	private void HandleResInfo(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The mip level.
		SHDRInstructionOperand src1 = inst.operands[2]; // A t# or u# input texture for which the dimensions are being queried. 

		USILOperand usilResource = new();
		USILOperand usilMipLevel = new();
		USILOperand usilWidth = new();
		USILOperand usilHeight = new();
		USILOperand usilDepthOrArraySize = new();
		USILOperand usilMipCount = new();

		bool hasWidth = false;
		bool hasHeight = false;
		bool hasDepth = false;
		bool hasMipCount = false;
		// resinfo_indexable*** r0.xy, l(0), t0.xwyz ->
		// r0.xy means two outputs, t0.xw means hasWidth and hasMipCount
		for (int i = 0; i < dest.swizzle.Length; i++)
		{
			int index = src1.swizzle[i];
			switch (index)
			{
				case 0: hasWidth = true; break;
				case 1: hasHeight = true; break;
				case 2: hasDepth = true; break;
				case 3: hasMipCount = true; break;
			}
		}

		FillUSILOperand(src1, usilResource, [0, 1, 2, 3], false);
		FillUSILOperand(src0, usilMipLevel, src0.swizzle, false);

		if (hasWidth)
		{
			FillUSILOperand(dest, usilWidth, [0], false);
		}
		else
		{
			usilWidth.operandType = USILOperandType.Null;
		}

		if (hasHeight)
		{
			FillUSILOperand(dest, usilHeight, [1], false);
		}
		else
		{
			usilHeight.operandType = USILOperandType.Null;
		}

		if (hasDepth)
		{
			FillUSILOperand(dest, usilDepthOrArraySize, [2], false);
		}
		else
		{
			usilDepthOrArraySize.operandType = USILOperandType.Null;
		}

		if (hasMipCount)
		{
			FillUSILOperand(dest, usilMipCount, [3], false);
		}
		else
		{
			usilMipCount.operandType = USILOperandType.Null;
		}

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.ResourceDimensionInfo,
			destOperand = null,
			srcOperands = new()
			{
				usilResource,
				usilMipLevel,
				usilWidth,
				usilHeight,
				usilDepthOrArraySize,
				usilMipCount
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleSampleInfo(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the results of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The shader resource.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.SampleCountInfo,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleDerivRt(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The component in the operation.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();

		FillUSILOperand(dest, usilDest, dest.swizzle, false);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), false);

		USILInstruction usilInst = new()
		{
			instructionType = inst.opcode switch
			{
				Opcode.deriv_rtx => USILInstructionType.DerivativeRenderTargetX,
				Opcode.deriv_rty => USILInstructionType.DerivativeRenderTargetY,
				Opcode.deriv_rtx_coarse => USILInstructionType.DerivativeRenderTargetXCoarse,
				Opcode.deriv_rty_coarse => USILInstructionType.DerivativeRenderTargetYCoarse,
				Opcode.deriv_rtx_fine => USILInstructionType.DerivativeRenderTargetXFine,
				Opcode.deriv_rty_fine => USILInstructionType.DerivativeRenderTargetYFine,
				_ => USILInstructionType.DerivativeRenderTargetX
			},
			destOperand = usilDest,
			srcOperands = new() { usilSrc0 }
		};

		Instructions.Add(usilInst);
	}

	private void HandleIf(SHDRInstruction inst)
	{
		SHDRInstructionOperand src0 = inst.operands[0]; // The value that determines whether to discard the current pixel being processed.

		USILOperand usilSrc0 = new();

		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = (inst.instData & 0x40000) != 0 ? USILInstructionType.IfTrue : USILInstructionType.IfFalse,
			destOperand = null,
			srcOperands = new()
			{
				usilSrc0
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleElse(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Else,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleEndIf(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.EndIf,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleLoop(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Loop,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleEndLoop(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.EndLoop,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleBreak(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Break,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleBreakc(SHDRInstruction inst)
	{
		SHDRInstructionOperand src0 = inst.operands[0];

		USILOperand usilSrc0 = new();

		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Break,
			destOperand = null,
			srcOperands = new() { usilSrc0 }
		};

		// if works the same way, so we can pretend the discard is an if
		HandleIf(inst);
		Instructions.Add(usilInst);
		HandleEndIf(inst);
	}


	private void HandleContinue(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Continue,
			destOperand = null,
			srcOperands = new()
		};

		Instructions.Add(usilInst);
	}

	private void HandleContinuec(SHDRInstruction inst)
	{
		SHDRInstructionOperand src0 = inst.operands[0];

		USILOperand usilSrc0 = new();
		FillUSILOperand(src0, usilSrc0, src0.swizzle, false);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Continue,
			destOperand = null,
			srcOperands = new() { usilSrc0 }
		};

		// if works the same way, so we can pretend the discard is an if
		HandleIf(inst);
		Instructions.Add(usilInst);
		HandleEndIf(inst);
	}

	private void HandleEq(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The component to comapre to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The component to comapre to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode == Opcode.ieq;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Equal,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = isInt
		};

		Instructions.Add(usilInst);
	}

	private void HandleNeq(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The components to compare to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The components to compare to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode == Opcode.ine;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.NotEqual,
			destOperand = usilDest,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			},
			isIntVariant = isInt
		};

		Instructions.Add(usilInst);
	}

	private void HandleLt(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The value to compare to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The value to compare to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode is Opcode.ilt or Opcode.ult;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.LessThan,
			destOperand = usilDest,
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.ult,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			}
		};

		Instructions.Add(usilInst);
	}

	private void HandleGe(SHDRInstruction inst)
	{
		SHDRInstructionOperand dest = inst.operands[0]; // The address of the result of the operation.
		SHDRInstructionOperand src0 = inst.operands[1]; // The value to compare to src1.
		SHDRInstructionOperand src1 = inst.operands[2]; // The value to compare to src0.

		USILOperand usilDest = new();
		USILOperand usilSrc0 = new();
		USILOperand usilSrc1 = new();

		bool isInt = inst.opcode is Opcode.ige or Opcode.uge;

		FillUSILOperand(dest, usilDest, dest.swizzle, isInt);
		FillUSILOperand(src0, usilSrc0, MapMask(dest.swizzle, src0.swizzle), isInt);
		FillUSILOperand(src1, usilSrc1, MapMask(dest.swizzle, src1.swizzle), isInt);

		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.GreaterThanOrEqual,
			destOperand = usilDest,
			isIntVariant = isInt,
			isIntUnsigned = inst.opcode == Opcode.uge,
			srcOperands = new()
			{
				usilSrc0,
				usilSrc1
			}
		};

		Instructions.Add(usilInst);
	}


	private void HandleRet(SHDRInstruction inst)
	{
		USILInstruction usilInst = new()
		{
			instructionType = USILInstructionType.Return,
			srcOperands = new()
		};
		Instructions.Add(usilInst);
	}


	private void HandleTemps(SHDRInstruction inst)
	{
		Debug.Assert(inst.declData is not null);
		int tempCount = inst.declData.numTemps;
		for (int i = 0; i < tempCount; i++)
		{
			string tmpString = USILOperand.GetTypeShortForm(USILOperandType.TempRegister) + i;
			USILLocal local = new("float4", tmpString, USILLocalType.Vector4);
			Locals.Add(local);
		}
	}

	private void HandleResource(SHDRInstruction inst)
	{
		Debug.Assert(inst.declData is not null);
		ResourceDimension dimension = inst.declData.resourceDimension;
		int regIndex = inst.declData.operands[0].arraySizes[0]; // todo: not always tX
		_resourceToDimension[regIndex] = dimension;
	}

	private void HandleCustomData(SHDRInstruction inst)
	{
		USILLocal local = new("const float4", "icb", USILLocalType.Vector4, true);
		Debug.Assert(inst.declData is not null);
		foreach (float[] vector in inst.declData.customDataArray)
		{
			USILOperand vectorOperand = new();
			vectorOperand.operandType = USILOperandType.ImmediateFloat;
			vectorOperand.immValueFloat = vector;
			vectorOperand.mask = USILConstants.XYZW_MASK;
			local.defaultValues.Add(vectorOperand);
		}
		Locals.Add(local);
	}

	private void AddOutputLocal()
	{
		string outputName = _dxShader.Shdr.shaderType switch
		{
			DirectXDisassembler.Type.Vertex => USILConstants.VERT_OUTPUT_LOCAL_NAME,
			DirectXDisassembler.Type.Pixel => USILConstants.FRAG_OUTPUT_LOCAL_NAME,
			_ => "o" // ?
		};
		string outputStructName = _dxShader.Shdr.shaderType switch
		{
			DirectXDisassembler.Type.Vertex => USILConstants.VERT_TO_FRAG_STRUCT_NAME,
			DirectXDisassembler.Type.Pixel => USILConstants.FRAG_OUTPUT_STRUCT_NAME,
			_ => "o" // ?
		};

		Locals.Add(new USILLocal(outputStructName, outputName, USILLocalType.Vector4));
	}

	private static int[] GetSampleLocationMask(ResourceDimension dimension)
	{
		switch (dimension)
		{
			case ResourceDimension.texture1d:
				return GetMaskOfSize(1);
			case ResourceDimension.texture1darray:
			case ResourceDimension.texture2d:
				return GetMaskOfSize(2);
			case ResourceDimension.texture2darray:
			case ResourceDimension.texture3d:
			case ResourceDimension.texturecube:
				return GetMaskOfSize(3);
			case ResourceDimension.texturecubearray:
				return GetMaskOfSize(4);
			default:
				return GetMaskOfSize(2); // not handled yet
		}
	}

	private static int[] GetSampleOffsetMask(ResourceDimension dimension)
	{
		switch (dimension)
		{
			case ResourceDimension.texture1d:
			case ResourceDimension.texture1darray:
				return GetMaskOfSize(1);
			case ResourceDimension.texture2d:
			case ResourceDimension.texture2darray:
				return GetMaskOfSize(2);
			case ResourceDimension.texture3d:
				return GetMaskOfSize(3);
			case ResourceDimension.texturecube:
			case ResourceDimension.texturecubearray:
				return GetMaskOfSize(4);
			default:
				return GetMaskOfSize(2); // not handled yet
		}
	}

	private static int[] GetLoadLocationMask(ResourceDimension dimension)
	{
		switch (dimension)
		{
			case ResourceDimension.buffer:
				return GetMaskOfSize(1);
			case ResourceDimension.texture1d:
			case ResourceDimension.texture2dms:
				return GetMaskOfSize(2);
			case ResourceDimension.texture1darray:
			case ResourceDimension.texture2d:
			case ResourceDimension.texture2dmsarray:
				return GetMaskOfSize(3);
			case ResourceDimension.texture2darray:
			case ResourceDimension.texture3d:
				return GetMaskOfSize(4);
			default:
				return GetMaskOfSize(2); // not handled yet
		}
	}

	private static int[] GetLoadSampleIndexMask(ResourceDimension dimension)
	{
		switch (dimension)
		{
			case ResourceDimension.texture2dms:
			case ResourceDimension.texture2dmsarray:
				return GetMaskOfSize(2);
			default:
				return GetMaskOfSize(2); // not handled yet
		}
	}

	private static int[] GetLoadOffsetMask(ResourceDimension dimension)
	{
		switch (dimension)
		{
			case ResourceDimension.texture1d:
			case ResourceDimension.texture1darray:
				return GetMaskOfSize(1);
			case ResourceDimension.texture2d:
			case ResourceDimension.texture2darray:
			case ResourceDimension.texture2dms:
			case ResourceDimension.texture2dmsarray:
				return GetMaskOfSize(2);
			case ResourceDimension.texture3d:
				return GetMaskOfSize(3);
			default:
				return GetMaskOfSize(2); // not handled yet
		}
	}

	public static int[] GetMaskOfSize(int size) => size switch
	{
		0 => Array.Empty<int>(),
		1 => [0],
		2 => [0, 1],
		3 => [0, 1, 2],
		4 => [0, 1, 2, 3],
		_ => throw new ArgumentOutOfRangeException(nameof(size)),
	};

	///////////////////////
	// stuff to be moved to other classes

	private static int ConvertFloatToInt(float f)
	{
		return BitConverter.SingleToInt32Bits(f);
	}
}
