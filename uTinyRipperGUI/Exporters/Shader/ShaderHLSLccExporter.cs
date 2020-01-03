using DXShaderRestorer;
using HLSLccWrapper;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderHLSLccExporter : ShaderDXExporter
	{
		public ShaderHLSLccExporter(GPUPlatform graphicApi):
			base(graphicApi)
		{
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = DXShaderProgramRestorer.RestoreProgramData(writer.Version, m_graphicApi, subProgram);
			WrappedGlExtensions ext = new WrappedGlExtensions();
			ext.ARB_explicit_attrib_location = 1;
			ext.ARB_explicit_uniform_location = 1;
			ext.ARB_shading_language_420pack = 0;
			ext.OVR_multiview = 0;
			ext.EXT_shader_framebuffer_fetch = 0;
			Shader shader = Shader.TranslateFromMem(exportData, WrappedGLLang.LANG_DEFAULT, ext);
			if (shader.OK == 0)
			{
				base.Export(writer, ref subProgram);
			}
			else
			{
				ExportListing(writer, shader.Text);
			}
		}
	}
}
