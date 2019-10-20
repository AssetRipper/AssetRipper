using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DotNetDxc;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Classes.Shaders.Exporters;

using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderHLSLccExporter : ShaderTextExporter
	{
		public ShaderHLSLccExporter(Version version, GPUPlatform graphicApi)
		{
			m_version = version;
			m_graphicApi = graphicApi;
		}

		private static bool IsOffset(GPUPlatform graphicApi)
		{
			return graphicApi != GPUPlatform.d3d9;
		}

		private static bool IsOffset5(Version version)
		{
			return version.IsEqual(5, 3);
		}

		public override void Export(ShaderSubProgram subProgram, TextWriter writer)
		{
			var data = DXShaderExporter.DXShaderObjectExporter.GetObjectData(m_version,
				m_graphicApi, 
				subProgram);

			var ext = new HLSLccWrapper.WrappedGlExtensions();
			ext.ARB_explicit_attrib_location = 1;
			ext.ARB_explicit_uniform_location = 1;
			ext.ARB_shading_language_420pack = 0;
			ext.OVR_multiview = 0;
			ext.EXT_shader_framebuffer_fetch = 0;
			var shader = HLSLccWrapper.Shader.TranslateFromMem(data,
				HLSLccWrapper.WrappedGLLang.LANG_DEFAULT, ext);
			if (shader.OK != 0)
			{
				using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(shader.Text)))
				{
					using (BinaryReader reader = new BinaryReader(memStream))
					{
						Export(reader, writer);
					}
				}
			}
			else
			{
				writer.WriteLine($"//Error {shader.OK}");
			}
		}

		private string GetStringFromBlob(IDxcBlob blob)
		{
			return Marshal.PtrToStringAnsi(blob.GetBufferPointer());
		}

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		private const uint DXBCFourCC = 0x43425844;

		private readonly Version m_version;
		private readonly GPUPlatform m_graphicApi;
	}
}
