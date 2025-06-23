using AssetRipper.SourceGenerated.Subclasses.SerializedPlayerSubProgram;
using AssetRipper.SourceGenerated.Subclasses.SerializedProgram;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedProgramExtensions
{
	public static int GetTierCount(this ISerializedProgram program)
	{
		int tierCount = 1;
		if (program.Has_PlayerSubPrograms())
		{
			// where is tier in PlayerSubProgram?
		}
		else
		{
			int tier = program.SubPrograms[0].ShaderHardwareTier;
			for (int i = 1; i < program.SubPrograms.Count; i++)
			{
				if (program.SubPrograms[i].ShaderHardwareTier <= tier)
				{
					break;
				}

				tierCount++;
			}
		}

		return tierCount;
	}

	public static int GetSubProgramCount(this ISerializedProgram program)
	{
		return program.Has_PlayerSubPrograms() ? program.PlayerSubPrograms.Count : program.SubPrograms.Count;
	}

	public static IReadOnlyList<ISerializedPlayerSubProgram> GetPlayerSubPrograms(this ISerializedProgram program)
	{
		if (program.Has_PlayerSubPrograms())
		{
			for (int i = 0; i < program.PlayerSubPrograms.Count; i++)
			{
				if (program.PlayerSubPrograms[i].Count > 0)
				{
					return program.PlayerSubPrograms[i];
				}
			}
		}
		return [];
	}

	public static IReadOnlyList<uint> GetParameterBlobIndices(this ISerializedProgram program)
	{
		if (program.Has_ParameterBlobIndices())
		{
			for (int i = 0; i < program.ParameterBlobIndices.Count; i++)
			{
				if (program.ParameterBlobIndices[i].Count > 0)
				{
					return program.ParameterBlobIndices[i];
				}
			}
		}
		return [];
	}

}
