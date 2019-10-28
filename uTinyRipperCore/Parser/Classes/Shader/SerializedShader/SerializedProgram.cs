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
			writer.WriteIndent(3);
			writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
			int tierCount = GetTierCount(SubPrograms);
			for (int i = 0; i < SubPrograms.Length; i++)
			{
				SerializedSubProgram subProgram = SubPrograms[i];
				Platform uplatform = writer.Platform;
				GPUPlatform platform = subProgram.GpuProgramType.ToGPUPlatform(uplatform);
				int index = writer.Shader.Platforms.IndexOf(platform);
				ref ShaderSubProgramBlob blob = ref writer.Shader.SubProgramBlobs[index];
				subProgram.Export(writer, ref blob, type, tierCount > 1);
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
				ref SerializedSubProgram subProgram = ref subPrograms[i];
				if (subProgram.ShaderHardwareTier <= tier)
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
