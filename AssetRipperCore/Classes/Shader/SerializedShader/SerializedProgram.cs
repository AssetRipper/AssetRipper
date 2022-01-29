using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProgram : IAssetReadable
	{
		/// <summary>
		/// 2020.3.2 and greater
		/// </summary>
		public static bool HasCommonParameters(UnityVersion version) => version.IsGreaterEqual(2020, 3, 2);

		public void Read(AssetReader reader)
		{
			SubPrograms = reader.ReadAssetArray<SerializedSubProgram>();
			if (HasCommonParameters(reader.Version))
			{
				CommonParameters.Read(reader);
			}
		}

		public int GetTierCount()
		{
			int tierCount = 1;
			int tier = SubPrograms[0].ShaderHardwareTier;
			for (int i = 1; i < SubPrograms.Length; i++)
			{
				if (SubPrograms[i].ShaderHardwareTier <= tier)
				{
					break;
				}

				tierCount++;
			}
			return tierCount;
		}

		public SerializedSubProgram[] SubPrograms { get; set; }
		public SerializedProgramParameters CommonParameters { get; set; }
	}
}
