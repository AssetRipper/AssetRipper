using System.IO;

namespace uTinyRipper.Classes.Shaders.Exporters
{
	public class ShaderUnknownExporter : ShaderTextExporter
	{
		public ShaderUnknownExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		protected override void Export(BinaryReader reader, TextWriter writer)
		{
			writer.Write("/*Can't export program data {0} as a text*/", m_graphicApi);
		}

		private GPUPlatform m_graphicApi;
	}
}
