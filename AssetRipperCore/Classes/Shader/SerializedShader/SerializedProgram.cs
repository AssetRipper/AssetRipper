using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProgram : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_SubPrograms", SubPrograms.ExportYAML(container));
			if (HasCommonParameters(container.ExportVersion))
			{
				node.Add("m_CommonParameters", CommonParameters.ExportYAML(container));
			}

			return node;
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
		public SerializedProgramParameters CommonParameters { get; set; } = new();
	}
}
