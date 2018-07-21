using System.IO;

namespace UtinyRipper.Classes.Shaders.Exporters
{
	public class ShaderUnknownExporter : ShaderTextExporter
	{
		public ShaderUnknownExporter(ShaderGpuProgramType programType)
		{
			m_programType = programType;
		}

		protected override void Export(BinaryReader reader, TextWriter writer)
		{
			writer.Write("/*Can't export program data {0} as a text*/", m_programType);
		}

		private ShaderGpuProgramType m_programType;
	}
}
