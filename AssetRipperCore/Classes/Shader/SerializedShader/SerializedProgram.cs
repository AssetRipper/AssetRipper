using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedProgram : IAssetReadable
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

		public void Export(ShaderWriter writer, ShaderType type)
		{
			if (SubPrograms.Length == 0)
			{
				return;
			}

			writer.WriteIndent(3);
			writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
			int tierCount = GetTierCount();
			for (int i = 0; i < SubPrograms.Length; i++)
			{
				SubPrograms[i].Export(writer, type, tierCount > 1);
			}
			writer.WriteIndent(3);
			writer.Write("}\n");
		}

		private int GetTierCount()
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
