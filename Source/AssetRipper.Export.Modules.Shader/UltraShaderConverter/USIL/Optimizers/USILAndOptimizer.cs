using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Optimizers;

/// <summary>
/// Replaces XXX & YYY with XXX ? YYY : 0 if XXX holds a comparison value
/// </summary>
public class USILAndOptimizer : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		HashSet<string> _comparisonResultRegisters = new();

		List<USILInstruction> instructions = shader.instructions;
		foreach (USILInstruction instruction in instructions)
		{
			// track if a register (and its masks) come from comparison results or not
			USILOperand? destOperand = instruction.destOperand;

			if (IsComparisonOrBooleanOp(instruction))
			{
				if (destOperand != null)
				{
					SetRegisterIsComparison(destOperand, true, _comparisonResultRegisters);
				}
			}
			else if (instruction.instructionType == USILInstructionType.And)
			{
				USILOperand leftOperand = instruction.srcOperands[0];
				USILOperand rightOperand = instruction.srcOperands[1];
				if (rightOperand.operandType == USILOperandType.ImmediateFloat &&
					rightOperand.immValueFloat[0] == 1)
				{
					instruction.instructionType = USILInstructionType.MoveConditional;
					instruction.srcOperands = new List<USILOperand>
					{
						leftOperand,
						new USILOperand()
						{
							operandType = USILOperandType.ImmediateFloat,
							immValueFloat = [1f]
						},
						new USILOperand()
						{
							operandType = USILOperandType.ImmediateFloat,
							immValueFloat = [0f]
						}
					};

					if (destOperand != null)
					{
						SetRegisterIsComparison(destOperand, false, _comparisonResultRegisters);
					}
				}
				else
				{
					bool leftIsComparison = IsRegisterComparison(leftOperand, _comparisonResultRegisters);
					bool rightIsComparison = IsRegisterComparison(rightOperand, _comparisonResultRegisters);
					if (leftIsComparison || rightIsComparison)
					{
						USILOperand cmpOperand = leftIsComparison ? leftOperand : rightOperand;
						USILOperand resOperand = leftIsComparison ? rightOperand : leftOperand;

						instruction.instructionType = USILInstructionType.MoveConditional;
						instruction.srcOperands = new List<USILOperand>
						{
							cmpOperand,
							resOperand,
							new USILOperand()
							{
								operandType = USILOperandType.ImmediateFloat,
								immValueFloat = [0f]
							}
						};
					}

					Debug.Assert(destOperand != null);

					// output is comparison if both are comparison (because result is also comparison)
					if (leftIsComparison && rightIsComparison)
					{
						SetRegisterIsComparison(destOperand, true, _comparisonResultRegisters);
					}
					else
					{
						SetRegisterIsComparison(destOperand, false, _comparisonResultRegisters);
					}
				}
			}
		}
		return changes; // any changes made?
	}

	private static void SetRegisterIsComparison(USILOperand operand, bool isComparison, HashSet<string> _comparisonResultRegisters)
	{
		foreach (int maskIdx in operand.mask)
		{
			// I don't know if non temps can do this, so just use ToString without mask
			if (isComparison)
			{
				_comparisonResultRegisters.Add($"{operand.ToString(true)}.{USILConstants.MASK_CHARS[maskIdx]}");
			}
			else
			{
				_comparisonResultRegisters.Remove($"{operand.ToString(true)}.{USILConstants.MASK_CHARS[maskIdx]}");
			}
		}
	}

	// 3dmigoto checked if any match, here we check if they all match
	private static bool IsRegisterComparison(USILOperand operand, HashSet<string> _comparisonResultRegisters)
	{
		foreach (int maskIdx in operand.mask)
		{
			if (!_comparisonResultRegisters.Contains($"{operand.ToString(true)}.{USILConstants.MASK_CHARS[maskIdx]}"))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsComparisonOrBooleanOp(USILInstruction instruction)
	{
		// non-exhaustive list obviously
		return instruction.IsComparisonType() || instruction.instructionType == USILInstructionType.Not;
	}
}
