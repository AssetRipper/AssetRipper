using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO;
using AssetRipper.Library.Exporters.Shaders.DirectX;
using DXShaderRestorer;
using HLSLccWrapper;
using System.IO;
using System.Runtime.Versioning;
using Shader = HLSLccWrapper.Shader;

namespace AssetRipper.Library.Exporters.Shaders
{
	[SupportedOSPlatform("windows")]
	public class ShaderHLSLccExporter : ShaderDXExporter
	{
		WrappedGLLang m_GLLang;

		public ShaderHLSLccExporter(GPUPlatform graphicApi, WrappedGLLang lang) : base(graphicApi) 
		{
			m_GLLang = lang;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			using (MemoryStream stream = new MemoryStream(subProgram.ProgramData))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					DXDataHeader header = new DXDataHeader();
					header.Read(reader, writer.Version);

					// HACK: since we can't restore UAV info and HLSLcc requires it, process such shader with default exporter
					if (header.UAVs > 0)
					{
						base.Export(writer, ref subProgram);
					}
					else
					{
						byte[] exportData = DXShaderProgramRestorer.RestoreProgramData(reader, writer.Version, ref subProgram);
						WrappedGlExtensions ext = new WrappedGlExtensions();
						ext.ARB_explicit_attrib_location = 1;
						ext.ARB_explicit_uniform_location = 1;
						ext.ARB_shading_language_420pack = 0;
						ext.OVR_multiview = 0;
						ext.EXT_shader_framebuffer_fetch = 0;
						Shader shader = Shader.TranslateFromMem(exportData, m_GLLang, ext);
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
		}
	}
}
