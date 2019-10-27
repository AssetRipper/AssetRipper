using uTinyRipper.Classes.Shaders;

namespace uTinyRipper.Converters.Shaders
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
