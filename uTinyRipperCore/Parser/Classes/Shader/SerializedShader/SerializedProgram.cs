namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedProgram : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			SubPrograms = reader.ReadAssetArray<SerializedSubProgram>();
		}

		public void Export(ShaderWriter writer, ShaderType type)
		{
			if (SubPrograms.Length == 0)
			{
				return;
			}

			writer.WriteIndent(3);
			writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
			int tierCount = GetTierCount(SubPrograms);
			for (int i = 0; i < SubPrograms.Length; i++)
			{
				Platform uplatform = writer.Platform;
				GPUPlatform platform = SubPrograms[i].GpuProgramType.ToGPUPlatform(uplatform);
				int index = writer.Shader.Platforms.IndexOf(platform);
				ref ShaderSubProgramBlob blob = ref writer.Shader.SubProgramBlobs[index];
				SubPrograms[i].Export(writer, ref blob, type, tierCount > 1);
			}
			writer.WriteIndent(3);
			writer.Write("}\n");
		}

		private int GetTierCount(SerializedSubProgram[] subPrograms)
		{
			int tierCount = 1;
			int tier = subPrograms[0].ShaderHardwareTier;
			for (int i = 1; i < subPrograms.Length; i++)
			{
				if (subPrograms[i].ShaderHardwareTier <= tier)
				{
					break;
				}

				tierCount++;
			}
			return tierCount;
		}

		public SerializedSubProgram[] SubPrograms { get; set; }
	}
}
