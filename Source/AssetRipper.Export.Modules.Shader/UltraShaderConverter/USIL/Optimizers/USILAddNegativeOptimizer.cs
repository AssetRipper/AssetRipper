using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Optimizers;

/// <summary>
/// Changes A + -B to A - B
/// </summary>
public class USILAddNegativeOptimizer : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		List<USILInstruction> instructions = shader.instructions;
		foreach (USILInstruction instruction in instructions)
		{
			if (instruction.instructionType == USILInstructionType.Add)
			{
				USILOperand leftOperand = instruction.srcOperands[0];
				USILOperand rightOperand = instruction.srcOperands[1];
				if (IsTrulyNegative(rightOperand))
				{
					instruction.instructionType = USILInstructionType.Subtract;
					NegateOperand(rightOperand);
					changes = true;
				}
				else if (IsTrulyNegative(leftOperand) && !IsTrulyNegative(rightOperand))
				{
					instruction.instructionType = USILInstructionType.Subtract;
					NegateOperand(leftOperand);
					instruction.srcOperands[0] = rightOperand;
					instruction.srcOperands[1] = leftOperand;
					changes = true;
				}
			}
		}
		return changes; // any changes made?
	}

	private static bool IsTrulyNegative(USILOperand operand)
	{
		switch (operand.operandType)
		{
			case USILOperandType.ImmediateInt:
				{
					foreach (int imm in operand.immValueInt)
					{
						// this includes 0 as being ok for negative. hopefully there are no +/- 0 instructions?
						if (imm > 0)
						{
							return false;
						}
					}
					return true;
				}

			case USILOperandType.ImmediateFloat:
				{
					foreach (float imm in operand.immValueFloat)
					{
						if (imm > 0)
						{
							return false;
						}
					}
					return true;
				}

			case USILOperandType.Multiple:
				{
					foreach (USILOperand child in operand.children)
					{
						if (!IsTrulyNegative(child))
						{
							return false;
						}
					}
					return true;
				}

			default:
				return operand.negative;
		}
	}

	private static void NegateOperand(USILOperand operand)
	{
		switch (operand.operandType)
		{
			case USILOperandType.ImmediateInt:
				{
					for (int i = 0; i < operand.immValueInt.Length; i++)
					{
						operand.immValueInt[i] = -operand.immValueInt[i];
					}

					break;
				}

			case USILOperandType.ImmediateFloat:
				{
					for (int i = 0; i < operand.immValueFloat.Length; i++)
					{
						operand.immValueFloat[i] = -operand.immValueFloat[i];
					}

					break;
				}

			case USILOperandType.Multiple:
				{
					foreach (USILOperand child in operand.children)
					{
						NegateOperand(child);
					}

					break;
				}

			default:
				operand.negative = !operand.negative;
				break;
		}
	}
}
