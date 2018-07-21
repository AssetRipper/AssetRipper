using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UtinyRipper.Classes.Shaders.Exporters;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedProgram : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_subPrograms = stream.ReadArray<SerializedSubProgram>();
		}

		public void Export(TextWriter writer, Shader shader, ShaderType type, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			if(SubPrograms.Count > 0)
			{
				writer.WriteIntent(3);
				writer.Write("Program \"{0}\" {{\n", type.ToProgramTypeString());
				foreach (SerializedSubProgram subProgram in SubPrograms)
				{
					Platform uplatform = shader.File.Platform;
					GPUPlatform platform = subProgram.GpuProgramType.ToGPUPlatform(uplatform);
					int index = shader.Platforms.IndexOf(platform);
					ShaderSubProgramBlob blob = shader.SubProgramBlobs[index];
					int count = SubPrograms.Where(t => t.GpuProgramType == subProgram.GpuProgramType).Select(t => t.ShaderHardwareTier).Distinct().Count();
					subProgram.Export(writer, blob, uplatform, count > 1, exporterInstantiator);
				}
				writer.WriteIntent(3);
				writer.Write("}\n");
			}
		}

		public IReadOnlyList<SerializedSubProgram> SubPrograms => m_subPrograms;
		
		private SerializedSubProgram[] m_subPrograms;
	}
}
