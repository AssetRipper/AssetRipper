using ShaderTextRestorer.ShaderBlob;
using System.Collections.Generic;
using System.Linq;

namespace ShaderLabConvert
{
	public class USILInputOutputMetadder : IUSILOptimizer
	{
		private UShaderProgram _shader;
		private ShaderSubProgram _shaderData;

		public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
		{
			_shader = shader;
			_shaderData = shaderData;

			List<USILInstruction> instructions = shader.instructions;
			foreach (USILInstruction instruction in instructions)
			{
				if (instruction.destOperand != null)
				{
					UseMetadata(instruction.destOperand);
				}
				foreach (USILOperand operand in instruction.srcOperands)
				{
					UseMetadata(operand);
				}
			}
			return true; // any changes made?
		}

		private void UseMetadata(USILOperand operand)
		{
			if (operand.operandType == USILOperandType.InputRegister)
			{
				int searchMask = (operand.mask.Length != 0) ? (1 << operand.mask[0]) : 0;
				USILInputOutput input = _shader.inputs.First(
					i => i.register == operand.registerIndex && ((searchMask & i.mask) == searchMask)
				);

				// correct mask
				operand.mask = MatchMaskToInputOutput(operand.mask, input.mask, true);

				if (_shader.shaderFunctionType == UShaderFunctionType.Fragment && input.type == "SV_IsFrontFace")
				{
					operand.metadataName = input.name;
				}
				else
				{
					operand.metadataName = _shader.shaderFunctionType switch
					{
						UShaderFunctionType.Vertex => $"{USILConstants.VERT_INPUT_NAME}.{input.name}",
						UShaderFunctionType.Fragment => $"{USILConstants.FRAG_INPUT_NAME}.{input.name}",
						_ => $"unk_input.{input.name}",
					};
				}

				operand.metadataNameAssigned = true;
			}
			else if (operand.operandType == USILOperandType.OutputRegister)
			{
				int searchMask = 0;
				for (int i = 0; i < operand.mask.Length; i++)
				{
					searchMask |= 1 << operand.mask[i];
				}

				List<USILInputOutput> outputs = _shader.outputs.Where(
					o => o.register == operand.registerIndex && ((searchMask & o.mask) != 0)
				).ToList();

				foreach (USILInputOutput output in outputs)
				{
					// correct mask
					int[] matchedMask = MatchMaskToInputOutput(operand.mask, output.mask, true);
					int[] realMatchedMask = MatchMaskToInputOutput(operand.mask, output.mask, false);
					operand.mask = matchedMask;

					operand.metadataName = _shader.shaderFunctionType switch
					{
						UShaderFunctionType.Vertex => $"{USILConstants.VERT_OUTPUT_LOCAL_NAME}.{output.name}",
						UShaderFunctionType.Fragment => $"{USILConstants.FRAG_OUTPUT_LOCAL_NAME}.{output.name}",
						_ => $"unk_output.{output.name}",
					};
					operand.metadataNameAssigned = true;
				}
			}
			else if (DXShaderNamingUtils.HasSpecialInputOutputName(operand.operandType))
			{
				string name = DXShaderNamingUtils.GetSpecialInputOutputName(operand.operandType);

				operand.metadataName = _shader.shaderFunctionType switch
				{
					UShaderFunctionType.Vertex => $"{USILConstants.VERT_OUTPUT_LOCAL_NAME}.{name}",
					UShaderFunctionType.Fragment => $"{USILConstants.FRAG_OUTPUT_LOCAL_NAME}.{name}",
					_ => $"unk_special.{name}",
				};
				operand.metadataNameAssigned = true;
			}
		}

		private int[] MatchMaskToInputOutput(int[] mask, int maskTest, bool moveSwizzles)
		{
			// Move swizzles (for example, .zw -> .xy) based on first letter
			int moveCount = 0;
			int i;
			for (i = 0; i < 4; i++)
			{
				if ((maskTest & 1) == 1)
				{
					break;
				}

				moveCount++;
				maskTest >>= 1;
			}

			// Count remaining 1 bits
			int bitCount = 0;
			for (; i < 4; i++)
			{
				if ((maskTest & 1) == 0)
				{
					break;
				}

				bitCount++;
				maskTest >>= 1;
			}

			List<int> result = new List<int>();
			for (int j = 0; j < mask.Length; j++)
			{
				if (mask[j] >= moveCount && mask[j] < bitCount + moveCount)
				{
					if (moveSwizzles)
					{
						result.Add(mask[j] - moveCount);
					}
					else
					{
						result.Add(mask[j]);
					}
				}
			}
			return result.ToArray();
		}
	}
}
