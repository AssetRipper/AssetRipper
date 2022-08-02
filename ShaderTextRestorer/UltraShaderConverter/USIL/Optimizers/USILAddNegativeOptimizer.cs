using ShaderTextRestorer.ShaderBlob;
using System.Collections.Generic;

namespace ShaderLabConvert
{
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

		private bool IsTrulyNegative(USILOperand operand)
		{
			if (operand.operandType == USILOperandType.ImmediateInt)
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
			else if (operand.operandType == USILOperandType.ImmediateFloat)
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
			else if (operand.operandType == USILOperandType.Multiple)
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
			return operand.negative;
		}

		private void NegateOperand(USILOperand operand)
		{
			if (operand.operandType == USILOperandType.ImmediateInt)
			{
				for (int i = 0; i < operand.immValueInt.Length; i++)
				{
					operand.immValueInt[i] = -operand.immValueInt[i];
				}
			}
			else if (operand.operandType == USILOperandType.ImmediateFloat)
			{
				for (int i = 0; i < operand.immValueFloat.Length; i++)
				{
					operand.immValueFloat[i] = -operand.immValueFloat[i];
				}
			}
			else if (operand.operandType == USILOperandType.Multiple)
			{
				foreach (USILOperand child in operand.children)
				{
					NegateOperand(child);
				}
			}
			else
			{
				operand.negative = !operand.negative;
			}
		}
	}
}
