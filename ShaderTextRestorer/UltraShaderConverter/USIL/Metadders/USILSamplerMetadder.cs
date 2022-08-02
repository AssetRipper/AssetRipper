using AssetRipper.Core.Logging;
using ShaderTextRestorer.ShaderBlob;
using ShaderTextRestorer.ShaderBlob.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace ShaderLabConvert
{
	public class USILSamplerMetadder : IUSILOptimizer
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
				foreach (USILOperand operand in instruction.srcOperands)
				{
					if (operand.operandType == USILOperandType.SamplerRegister)
					{
						TextureParameter texParam = _shaderData.TextureParameters.FirstOrDefault(
							p => p.SamplerIndex == operand.registerIndex
						);

						if (texParam == null)
						{
							operand.operandType = USILOperandType.Sampler2D;
							Logger.Warning($"Could not find texture parameter for sampler {operand}");
							continue;
						}

						int dimension = texParam.Dim;
						switch (dimension)
						{
							case 2:
								operand.operandType = USILOperandType.Sampler2D;
								break;
							case 3:
								operand.operandType = USILOperandType.Sampler3D;
								break;
							case 4:
								operand.operandType = USILOperandType.SamplerCube;
								break;
							case 5:
								operand.operandType = USILOperandType.Sampler2DArray;
								break;
							case 6:
								operand.operandType = USILOperandType.SamplerCubeArray;
								break;
						}

						if (texParam != null)
						{
							operand.metadataName = texParam.Name;
							operand.metadataNameAssigned = true;
						}
					}
					else if (operand.operandType == USILOperandType.ResourceRegister)
					{
						TextureParameter texParam = _shaderData.TextureParameters.FirstOrDefault(
							p => p.Index == operand.registerIndex
						);

						if (texParam == null)
						{
							Logger.Warning($"Could not find texture parameter for resource {operand}");
							continue;
						}

						if (texParam != null)
						{
							operand.metadataName = texParam.Name;
							operand.metadataNameAssigned = true;
						}
					}
				}
			}
			return true; // any changes made?
		}
	}
}
