using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Optimizers;

/// <summary>
/// Moves constant values to the right in comparison instructions
/// </summary>
public class USILCompareOrderOptimizer : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		List<USILInstruction> instructions = shader.instructions;
		foreach (USILInstruction instruction in instructions)
		{
			if (instruction.IsComparisonType())
			{
				USILOperand leftOperand = instruction.srcOperands[0];
				USILOperand rightOperand = instruction.srcOperands[1];
				bool leftIsConstant = IsOperandFullyConstant(leftOperand);
				bool rightIsConstant = IsOperandFullyConstant(rightOperand);
				if (leftIsConstant && !rightIsConstant)
				{
					instruction.srcOperands[0] = rightOperand;
					instruction.srcOperands[1] = leftOperand;
					instruction.instructionType = FlipCompareType(instruction.instructionType);
					changes = true;
				}
			}
		}
		return changes; // any changes made?
	}

	private static bool IsOperandFullyConstant(USILOperand operand)
	{
		if (operand.operandType is USILOperandType.ImmediateInt or USILOperandType.ImmediateFloat)
		{
			return true;
		}
		else if (operand.operandType is USILOperandType.Multiple)
		{
			bool fullyConstant = true;
			foreach (USILOperand child in operand.children)
			{
				fullyConstant &= IsOperandFullyConstant(child);
			}
		}
		return false;
	}

	private static USILInstructionType FlipCompareType(USILInstructionType type)
	{
		return type switch
		{
			USILInstructionType.LessThan => USILInstructionType.GreaterThan,
			USILInstructionType.GreaterThan => USILInstructionType.LessThan,
			USILInstructionType.LessThanOrEqual => USILInstructionType.GreaterThanOrEqual,
			USILInstructionType.GreaterThanOrEqual => USILInstructionType.LessThanOrEqual,
			_ => type,
		};
	}
}
