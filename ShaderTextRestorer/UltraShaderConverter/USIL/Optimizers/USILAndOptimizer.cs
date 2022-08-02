using ShaderTextRestorer.ShaderBlob;
using System.Collections.Generic;

namespace ShaderLabConvert
{
	/// <summary>
	/// Replaces XXX & YYY with XXX ? YYY : 0 if XXX holds a comparison value
	/// </summary>
	public class USILAndOptimizer : IUSILOptimizer
	{
		private HashSet<string> _comparisonResultRegisters;

		public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
		{
			bool changes = false;

			_comparisonResultRegisters = new HashSet<string>();

			List<USILInstruction> instructions = shader.instructions;
			foreach (USILInstruction instruction in instructions)
			{
				// track if a register (and its masks) come from comparison results or not
				USILOperand destOperand = instruction.destOperand;

				if (IsComparisonOrBooleanOp(instruction))
				{
					if (destOperand != null)
					{
						SetRegisterIsComparison(destOperand, true);
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
						instruction.srcOperands = new List<USILOperand>();
						instruction.srcOperands.Add(leftOperand);
						instruction.srcOperands.Add(new USILOperand()
						{
							operandType = USILOperandType.ImmediateFloat,
							immValueFloat = new float[1] { 1f }
						});
						instruction.srcOperands.Add(new USILOperand()
						{
							operandType = USILOperandType.ImmediateFloat,
							immValueFloat = new float[1] { 0f }
						});

						if (destOperand != null)
						{
							SetRegisterIsComparison(destOperand, false);
						}
					}
					else
					{
						bool leftIsComparison = IsRegisterComparison(leftOperand);
						bool rightIsComparison = IsRegisterComparison(rightOperand);
						if (leftIsComparison || rightIsComparison)
						{
							USILOperand cmpOperand = leftIsComparison ? leftOperand : rightOperand;
							USILOperand resOperand = leftIsComparison ? rightOperand : leftOperand;

							instruction.instructionType = USILInstructionType.MoveConditional;
							instruction.srcOperands = new List<USILOperand>();
							instruction.srcOperands.Add(cmpOperand);
							instruction.srcOperands.Add(resOperand);
							instruction.srcOperands.Add(new USILOperand()
							{
								operandType = USILOperandType.ImmediateFloat,
								immValueFloat = new float[1] { 0f }
							});
						}

						// output is comparison if both are comparison (because result is also comparison)
						if (leftIsComparison && rightIsComparison)
						{
							SetRegisterIsComparison(destOperand, true);
						}
						else
						{
							SetRegisterIsComparison(destOperand, false);
						}
					}
				}
			}
			return changes; // any changes made?
		}

		private void SetRegisterIsComparison(USILOperand operand, bool isComparison)
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
		private bool IsRegisterComparison(USILOperand operand)
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

		private bool IsComparisonOrBooleanOp(USILInstruction instruction)
		{
			// non-exhaustive list obviously
			return instruction.IsComparisonType() || instruction.instructionType == USILInstructionType.Not;
		}
	}
}
