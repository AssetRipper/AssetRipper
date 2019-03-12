using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using uTinyRipper.Classes.Shaders.Exporters;

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
			if(SubPrograms.Count > 0)
			{
				writer.WriteIntent(3);
				writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
				foreach (SerializedSubProgram subProgram in SubPrograms)
				{
					Platform uplatform = writer.Platform;
					GPUPlatform platform = subProgram.GpuProgramType.ToGPUPlatform(uplatform);
					int index = writer.Shader.Platforms.IndexOf(platform);
					ShaderSubProgramBlob blob = writer.Shader.SubProgramBlobs[index];
					int count = SubPrograms.Where(t => t.GpuProgramType == subProgram.GpuProgramType).Select(t => t.ShaderHardwareTier).Distinct().Count();
					subProgram.Export(writer, blob, count > 1);
				}
				writer.WriteIntent(3);
				writer.Write("}\n");
			}
		}

		public IReadOnlyList<SerializedSubProgram> SubPrograms => m_subPrograms;
		
		private SerializedSubProgram[] m_subPrograms;
	}
}
