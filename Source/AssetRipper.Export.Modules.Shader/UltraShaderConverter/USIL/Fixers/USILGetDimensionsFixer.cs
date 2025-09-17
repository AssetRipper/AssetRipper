using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Fixers;

/// <summary>
/// Combines ResourceDimensionInfo and SampleCountInfo into a single GetDimensions call
/// </summary>
public class USILGetDimensionsFixer : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		List<USILInstruction> instructions = shader.instructions;
		for (int i = 0; i < instructions.Count; i++)
		{
			bool matches = USILOptimizerUtil.DoOpcodesMatch(instructions, i,
			[
				USILInstructionType.ResourceDimensionInfo,
				USILInstructionType.SampleCountInfo
			]);

			if (!matches)
			{
				if (instructions[i].instructionType == USILInstructionType.ResourceDimensionInfo)
				{
					// discard unused NumberOfLevels for GetDimensions
					if (!shader.locals.Any(l => l.name == "resinfo_extra"))
					{
						shader.locals.Add(new USILLocal("float", "resinfo_extra", USILLocalType.Scalar));
						changes = true;
					}
				}
				continue;
			}

			USILInstruction resinfoInst = instructions[0];
			USILInstruction sampleinfoInst = instructions[1];

			// needed? (did I even get the right registers?)
			if (resinfoInst.srcOperands[1].registerIndex != sampleinfoInst.srcOperands[0].registerIndex)
			{
				continue;
			}

			Debug.Assert(sampleinfoInst.destOperand != null);

			resinfoInst.srcOperands[5] = sampleinfoInst.destOperand;

			//USILInstruction usilInst = new USILInstruction();
			//USILOperand usilResource = new USILOperand(resinfoInst.srcOperands[0]);
			//USILOperand usilMipLevel = new USILOperand(resinfoInst.srcOperands[1]);
			//USILOperand usilWidth = new USILOperand(resinfoInst.srcOperands[2]);
			//USILOperand usilHeight = new USILOperand(resinfoInst.srcOperands[3]);
			//USILOperand usilDepthOrArraySize = new USILOperand(resinfoInst.srcOperands[4]);
			//USILOperand usilMipCount  = new USILOperand(resinfoInst.srcOperands[5]);
			//USILOperand usilSampleCount = new USILOperand(sampleinfoInst.destOperand);

			//usilInst.instructionType = USILInstructionType.GetDimensions;
			//usilInst.destOperand = null;
			//usilInst.srcOperands = new List<USILOperand>
			//{
			//	usilResource,
			//	usilMipLevel,
			//	usilWidth,
			//	usilHeight,
			//	usilDepthOrArraySize,
			//	usilMipCount,
			//	usilSampleCount
			//};

			//instructions[i] = usilInst;

			instructions.RemoveAt(i + 1); // remove SampleCountInfo
			changes = true;
		}

		return changes; // any changes made?
	}
}
