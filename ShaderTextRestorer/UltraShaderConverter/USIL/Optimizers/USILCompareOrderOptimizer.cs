using ShaderTextRestorer.ShaderBlob;
using System.Collections.Generic;

namespace ShaderLabConvert
{
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

		private bool IsOperandFullyConstant(USILOperand operand)
		{
			if (operand.operandType == USILOperandType.ImmediateInt ||
				operand.operandType == USILOperandType.ImmediateFloat)
			{
				return true;
			}
			else if (operand.operandType == USILOperandType.Multiple)
			{
				bool fullyConstant = true;
				foreach (USILOperand child in operand.children)
				{
					fullyConstant &= IsOperandFullyConstant(child);
				}
			}
			return false;
		}

		private USILInstructionType FlipCompareType(USILInstructionType type)
		{
			switch (type)
			{
				case USILInstructionType.LessThan:
					return USILInstructionType.GreaterThan;
				case USILInstructionType.GreaterThan:
					return USILInstructionType.LessThan;
				case USILInstructionType.LessThanOrEqual:
					return USILInstructionType.GreaterThanOrEqual;
				case USILInstructionType.GreaterThanOrEqual:
					return USILInstructionType.LessThanOrEqual;
				default:
					return type;
			}
		}
	}
}
