using AssetRipper.SourceGenerated.Subclasses.SerializedProgram;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SerializedProgramExtensions
	{
		public static int GetTierCount(this ISerializedProgram program)
		{
			int tierCount = 1;
			int tier = program.SubPrograms[0].ShaderHardwareTier;
			for (int i = 1; i < program.SubPrograms.Count; i++)
			{
				if (program.SubPrograms[i].ShaderHardwareTier <= tier)
				{
					break;
				}

				tierCount++;
			}

			return tierCount;
		}
	}
}
