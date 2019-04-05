using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedProgram : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_subPrograms = reader.ReadAssetArray<SerializedSubProgram>();
		}

		public void Export(ShaderWriter writer, ShaderType type)
		{
			if (SubPrograms.Count > 0)
			{
				writer.WriteIndent(3);
				writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
				HashSet<ShaderGpuProgramType> isTierLookup = GetIsTierLookup(SubPrograms);
				foreach (SerializedSubProgram subProgram in SubPrograms)
				{
					Platform uplatform = writer.Platform;
					GPUPlatform platform = subProgram.GpuProgramType.ToGPUPlatform(uplatform);
					int index = writer.Shader.Platforms.IndexOf(platform);
					ShaderSubProgramBlob blob = writer.Shader.SubProgramBlobs[index];
					bool isTier = isTierLookup.Contains(subProgram.GpuProgramType);
					subProgram.Export(writer, blob, type, isTier);
				}
				writer.WriteIndent(3);
				writer.Write("}\n");
			}
		}

		private HashSet<ShaderGpuProgramType> GetIsTierLookup(IReadOnlyList<SerializedSubProgram> subPrograms)
		{
			HashSet<ShaderGpuProgramType> lookup = new HashSet<ShaderGpuProgramType>();
			Dictionary<ShaderGpuProgramType, byte> seen = new Dictionary<ShaderGpuProgramType, byte>();
			foreach (SerializedSubProgram subProgram in subPrograms)
			{
				if (seen.ContainsKey(subProgram.GpuProgramType))
				{
					if (seen[subProgram.GpuProgramType] != subProgram.ShaderHardwareTier)
					{
						lookup.Add(subProgram.GpuProgramType);
					}
				}
				else
				{
					seen[subProgram.GpuProgramType] = subProgram.ShaderHardwareTier;
				}
			}
			return lookup;
		}

		public IReadOnlyList<SerializedSubProgram> SubPrograms => m_subPrograms;

		private SerializedSubProgram[] m_subPrograms;
	}
}
