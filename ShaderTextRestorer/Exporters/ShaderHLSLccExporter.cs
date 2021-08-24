using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO;
using HLSLccCsharpWrapper;
using System.IO;
using System.Runtime.Versioning;
using ShaderTextRestorer.Exporters.DirectX;
using System;

namespace ShaderTextRestorer.Exporters
{
	public class ShaderHLSLccExporter : ShaderDXExporter
	{
		GLLang m_GLLang;

		public ShaderHLSLccExporter(GPUPlatform graphicApi, GLLang lang) : base(graphicApi)
		{
			m_GLLang = lang;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			if(!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
			{
				base.Export(writer,ref subProgram);
				return;
			}

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
						string shaderText = Imports.ShaderTranslateFromMem(exportData, m_GLLang, ext);
						if (string.IsNullOrEmpty(shaderText))
						{
							base.Export(writer, ref subProgram);
						}
						else
						{
							ExportListing(writer, "//ShaderHLSLccExporter\n" + shaderText);
						}
					}
				}
			}
		}
	}
}
