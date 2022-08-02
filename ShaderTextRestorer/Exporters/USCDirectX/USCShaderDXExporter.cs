using AssetRipper.Core.Classes.Shader.Enums;
using ShaderLabConvert;
using ShaderTextRestorer.IO;
using ShaderTextRestorer.ShaderBlob;

namespace ShaderTextRestorer.Exporters.USCDirectX
{
	public class USCShaderDXExporter : ShaderTextExporter
	{
		public override string Name => "ShaderUSCDXExporter";
		public UShaderProgram uShaderProgram;

		public USCShaderDXExporter(GPUPlatform graphicApi)
		{
			uShaderProgram = null;
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;

			if (USCDXShaderTextExtractor.TryDecompileText(exportData, writer.Version, m_graphicApi, subProgram, out string decompiledText, out uShaderProgram))
			{
				ExportListing(writer, "// Exported with USC Decompiler\n" + (decompiledText ?? ""));
			}
			else if (USCDXShaderTextExtractor.TryGetShaderText(exportData, writer.Version, m_graphicApi, out string disassemblyText))
			{
				ExportListing(writer, "// ShaderDXExporter_Disassembler\n" + (disassemblyText ?? ""));
			}
		}

		protected readonly GPUPlatform m_graphicApi;
	}
}
