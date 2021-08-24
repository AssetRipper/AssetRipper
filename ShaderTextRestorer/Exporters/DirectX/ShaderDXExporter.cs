using AssetRipper.Core.Converters.Shader;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO;

namespace ShaderTextRestorer.Exporters.DirectX
{
	public class ShaderDXExporter : ShaderTextExporter
	{
		public ShaderDXExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;
			
			if(DXShaderTextExtractor.TryGetShaderText(exportData, writer.Version, m_graphicApi, out string disassemblyText))
			{
				ExportListing(writer, "//ShaderDXExporter\n" + (disassemblyText ?? ""));
			}
		}

		protected readonly GPUPlatform m_graphicApi;
	}
}
