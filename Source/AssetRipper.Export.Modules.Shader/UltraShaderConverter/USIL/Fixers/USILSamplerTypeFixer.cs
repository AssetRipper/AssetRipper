using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Fixers;

/// <summary>
/// Corrects the sampler type for built in types
/// </summary>
public class USILSamplerTypeFixer : IUSILOptimizer
{
	// There's most likely a better way to handle this, but I don't care right now.
	public static readonly HashSet<string?> BUILTIN_SAMPLER_TEXTURE_NAMES = new()
	{
		"unity_Lightmap",
		"unity_ShadowMask",
		"unity_DynamicLightmap",
		"unity_SpecCube0",
		"unity_ProbeVolumeSH",
		"_ShadowMapTexture"
	};

	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		List<USILInstruction> instructions = shader.instructions;
		foreach (USILInstruction instruction in instructions)
		{
			if (instruction.IsSampleType())
			{
				USILOperand sampleOperand = instruction.srcOperands[2];

				// USILSamplerMetadder couldn't find sampler metadata, skip
				if (sampleOperand.operandType == USILOperandType.SamplerRegister)
				{
					break;
				}

				// Shouldn't happen, but just in case
				if (!sampleOperand.metadataNameAssigned)
				{
					break;
				}

				if (BUILTIN_SAMPLER_TEXTURE_NAMES.Contains(sampleOperand.metadataName))
				{
					int samplerTypeIdx = GetSamplerTypeIdx(instruction.instructionType);
					if (samplerTypeIdx != -1)
					{
						instruction.srcOperands[samplerTypeIdx] = new USILOperand(1);
						changes = true;
					}
				}
			}
		}
		return changes; // any changes made?
	}

	private static int GetSamplerTypeIdx(USILInstructionType type)
	{
		return type switch
		{
			USILInstructionType.Sample => 3,
			USILInstructionType.SampleLODBias => 4,
			USILInstructionType.SampleComparison => 4,
			USILInstructionType.SampleComparisonLODZero => 4,
			USILInstructionType.SampleLOD => 4,
			_ => -1
		};
	}
}
