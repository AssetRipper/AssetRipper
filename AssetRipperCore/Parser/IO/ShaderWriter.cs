using AssetRipper.Classes;
using AssetRipper.Classes.Shaders;
using AssetRipper.Converters.Shaders;
using System;
using System.IO;
using System.Text;

namespace AssetRipper
{
	public class ShaderWriter : InvariantStreamWriter
	{
		public ShaderWriter(Stream stream, Shader shader, Func<Version, GPUPlatform, ShaderTextExporter> exporterInstantiator) :
			base(stream, new UTF8Encoding(false), 4096, true)
		{
			if (shader == null)
			{
				throw new ArgumentNullException(nameof(shader));
			}
			if (exporterInstantiator == null)
			{
				throw new ArgumentNullException(nameof(exporterInstantiator));
			}

			Shader = shader;
			m_exporterInstantiator = exporterInstantiator;
		}

		public void WriteShaderData(ref ShaderSubProgram subProgram)
		{
			ShaderGpuProgramType programType = subProgram.GetProgramType(Version);
			GPUPlatform graphicApi = programType.ToGPUPlatform(Platform);
			ShaderTextExporter exporter = m_exporterInstantiator.Invoke(Shader.File.Version, graphicApi);
			exporter.Export(this, ref subProgram);
		}

		public Shader Shader { get; }
		public Version Version => Shader.File.Version;
		public Platform Platform => Shader.File.Platform;

		private readonly Func<Version, GPUPlatform, ShaderTextExporter> m_exporterInstantiator;
	}
}
