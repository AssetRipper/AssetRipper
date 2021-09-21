using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using ShaderTextRestorer.IO;

namespace ShaderTextRestorer.Exporters
{
	public class ShaderUnknownExporter : ShaderTextExporter
	{
		public override string Name => "ShaderUnknownExporter";

		public ShaderUnknownExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			writer.Write("/*ShaderUnknownExporter : Can't export program data {0} as a text*/", m_graphicApi);
		}

		private readonly GPUPlatform m_graphicApi;
	}
}
