using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using ShaderTextRestorer.IO;

namespace ShaderTextRestorer.Exporters.DirectX
{
	public class ShaderDXExporter : ShaderTextExporter
	{
		public override string Name => "ShaderDXExporter";

		public ShaderDXExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;

			if (DXShaderTextExtractor.TryDecompileText(exportData, writer.Version, m_graphicApi, out string decompiledText))
			{
				ExportListing(writer, "//ShaderDXExporter_Decompiler\n" + (decompiledText ?? ""));
			}
			else if (DXShaderTextExtractor.TryGetShaderText(exportData, writer.Version, m_graphicApi, out string disassemblyText))
			{
				ExportListing(writer, "//ShaderDXExporter_Disassembler\n" + (disassemblyText ?? ""));
			}
		}

		protected readonly GPUPlatform m_graphicApi;
	}
}
