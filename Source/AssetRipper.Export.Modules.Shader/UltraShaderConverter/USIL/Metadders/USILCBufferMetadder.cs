using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Metadders;

public class USILCBufferMetadder : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		List<USILInstruction> instructions = shader.instructions;
		foreach (USILInstruction instruction in instructions)
		{
			if (instruction.destOperand != null)
			{
				UseMetadata(instruction.destOperand, shaderData);
			}

			foreach (USILOperand operand in instruction.srcOperands)
			{
				UseMetadata(operand, shaderData);
			}
		}
		return true; // any changes made?
	}

	private static void UseMetadata(USILOperand operand, ShaderSubProgram shaderData)
	{
		if (operand.operandType == USILOperandType.ConstantBuffer)
		{
			int cbRegIdx = operand.registerIndex;
			int cbArrIdx = operand.arrayIndex;

			List<int> operandMaskAddresses = new();
			foreach (int operandMask in operand.mask)
			{
				operandMaskAddresses.Add(cbArrIdx * 16 + operandMask * 4);
			}

			HashSet<NumericShaderParameter> cbParams = new HashSet<NumericShaderParameter>();
			List<int> cbMasks = new List<int>();

			BufferBinding binding = shaderData.ConstantBufferBindings.First(b => b.Index == cbRegIdx);
			ConstantBuffer constantBuffer = shaderData.ConstantBuffers.First(b => b.Name == binding.Name);

			// Search children fields
			foreach (NumericShaderParameter param in constantBuffer.AllNumericParams)
			{
				int paramCbStart = param.Index;
				int paramCbSize = param.RowCount * param.ColumnCount * 4;
				int paramCbEnd = paramCbStart + paramCbSize;

				foreach (int operandMaskAddress in operandMaskAddresses)
				{
					if (operandMaskAddress >= paramCbStart && operandMaskAddress < paramCbEnd)
					{
						cbParams.Add(param);

						int maskIndex = (operandMaskAddress - paramCbStart) / 4;
						if (param.IsMatrix)
						{
							maskIndex %= 4;
						}
						cbMasks.Add(maskIndex);
					}
				}
			}

			// Search children structs and its fields
			foreach (StructParameter stParam in constantBuffer.StructParams)
			{
				foreach (NumericShaderParameter cbParam in stParam.AllNumericMembers)
				{
					int paramCbStart = cbParam.Index;
					int paramCbSize = cbParam.RowCount * cbParam.ColumnCount * 4;
					int paramCbEnd = paramCbStart + paramCbSize;

					foreach (int operandMaskAddress in operandMaskAddresses)
					{
						if (operandMaskAddress >= paramCbStart && operandMaskAddress < paramCbEnd)
						{
							cbParams.Add(cbParam);

							int maskIndex = (operandMaskAddress - paramCbStart) / 4;
							if (cbParam.IsMatrix)
							{
								maskIndex %= 4;
							}
							cbMasks.Add(maskIndex);
						}
					}
				}
			}

			// Multiple params got opto'd into one operation
			if (cbParams.Count > 1)
			{
				operand.operandType = USILOperandType.Multiple;
				operand.children = new USILOperand[cbParams.Count];

				int i = 0;
				List<string> paramStrs = new List<string>();
				foreach (NumericShaderParameter param in cbParams)
				{
					USILOperand childOperand = new USILOperand();
					childOperand.operandType = USILOperandType.ConstantBuffer;

					childOperand.mask = MatchMaskToConstantBuffer(operand.mask, param.Index, param.RowCount);
					childOperand.metadataName = param.Name;
					childOperand.metadataNameAssigned = true;
					childOperand.arrayRelative = operand.arrayRelative;
					childOperand.arrayIndex -= param.Index / 16;
					childOperand.metadataNameWithArray = operand.arrayRelative != null && !param.IsMatrix;

					operand.children[i++] = childOperand;
				}
			}
			else if (cbParams.Count == 1)
			{
				NumericShaderParameter param = cbParams.First();

				// Matrix
				if (param.IsMatrix)
				{
					//int matrixIdx = cbArrIdx - param.Index / 16;

					operand.operandType = USILOperandType.Matrix;
					//operand.arrayIndex = matrixIdx;
					operand.transposeMatrix = true;
				}
				//else
				//{
				operand.arrayIndex -= param.Index / 16;
				//}

				operand.mask = cbMasks.ToArray();
				operand.metadataName = param.Name;
				operand.metadataNameAssigned = true;
				operand.metadataNameWithArray = operand.arrayRelative != null && !param.IsMatrix;

				if (cbMasks.Count == param.RowCount && !param.IsMatrix)
				{
					operand.displayMask = false;
				}
			}
		}
	}

	private static int[] MatchMaskToConstantBuffer(int[] mask, int pos, int size)
	{
		// Mask is aligned (x, xy, xyz, xyzw)
		// todo: bad opto breaks things lol keep this out
		// if (pos % 16 == 0)
		// {
		//     return mask;
		// }

		int offset = pos / 4 % 4;
		List<int> result = new List<int>();
		for (int i = 0; i < mask.Length; i++)
		{
			if (mask[i] >= offset && mask[i] < offset + size)
			{
				result.Add(mask[i] - offset);
			}
		}
		return result.ToArray();
	}
}
