using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Library.Exporters.Shaders.DirectX;
using DXShaderRestorer;
using System.IO;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class ShaderDxBytecodeExporter : CustomShaderTextExporter
	{
		public ShaderDxBytecodeExporter(GPUPlatform graphicApi, bool restore)
		{
			m_graphicApi = graphicApi;
			m_Restore = restore;
		}
		public override string Extension => ".bin";
		public override void DoExport(string filePath, UnityVersion version, ref ShaderSubProgram subProgram)
		{
			using (MemoryStream stream = new MemoryStream(subProgram.ProgramData))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					DXDataHeader header = new DXDataHeader();
					header.Read(reader, version);
					if (header.UAVs > 0)
					{
						File.WriteAllText(filePath, "Cannot convert HLSL shaders with UAVs to GLSL");
						return;
					}
					byte[] exportData;
					if (m_Restore)
					{
						exportData = DXShaderProgramRestorer.RestoreProgramData(reader, version, ref subProgram);
					}
					else
					{
						exportData = reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position);
					}
					File.WriteAllBytes(filePath, exportData);
				}
			}
		}
		bool m_Restore;
		protected readonly GPUPlatform m_graphicApi;
	}
}
