using AssetRipper.Parser.Classes.Shader;
using AssetRipper.Parser.Classes.Shader.Enums;
using AssetRipper.IO;

namespace AssetRipper.Converters.Classes.Shader
{
	public class ShaderUnknownExporter : ShaderTextExporter
	{
		public ShaderUnknownExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			writer.Write("/*Can't export program data {0} as a text*/", m_graphicApi);
		}

		private readonly GPUPlatform m_graphicApi;
	}
}
