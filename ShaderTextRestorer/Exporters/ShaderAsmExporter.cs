using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Parser.Files;
using ShaderTextRestorer.Exporters.DirectX;
using System.IO;

namespace ShaderTextRestorer.Exporters
{
	public class ShaderAsmExporter : CustomShaderTextExporter
	{
		public ShaderAsmExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}
		public override string Extension => ".asm";
		public static string Disassemble(byte[] exportData, UnityVersion version, GPUPlatform m_graphicApi)
		{
			DXShaderTextExtractor.TryGetShaderText(exportData, version, m_graphicApi, out string disassemblyText);
			return "//ShaderAsmExporter\n" + (disassemblyText ?? "");
		}
		public override void DoExport(string filePath, UnityVersion version, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;
			string disassemblyText = Disassemble(exportData, version, m_graphicApi);
			File.WriteAllText(filePath, disassemblyText);
		}

		protected readonly GPUPlatform m_graphicApi;
	}
}
