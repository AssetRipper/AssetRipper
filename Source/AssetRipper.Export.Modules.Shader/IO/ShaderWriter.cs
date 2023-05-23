using AssetRipper.Export.Modules.Shaders.Exporters;
using AssetRipper.Export.Modules.Shaders.Extensions;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetRipper.VersionUtilities;
using System.Text;


namespace AssetRipper.Export.Modules.Shaders.IO
{
	public class ShaderWriter : InvariantStreamWriter
	{
		public ShaderWriter(Stream stream, IShader shader, Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator) : base(stream, new UTF8Encoding(false), 4096, true)
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
			Blobs = shader.ReadBlobs();
			m_exporterInstantiator = exporterInstantiator;
		}

		public void WriteShaderData(ref ShaderSubProgram subProgram)
		{
			ShaderGpuProgramType programType = subProgram.GetProgramType(Version);
			GPUPlatform graphicApi = programType.ToGPUPlatform(Platform);
			ShaderTextExporter exporter = m_exporterInstantiator.Invoke(Shader.Collection.Version, graphicApi);
			exporter.Export(this, ref subProgram);
		}

		public IShader Shader { get; }
		public ShaderSubProgramBlob[] Blobs { get; }
		public UnityVersion Version => Shader.Collection.Version;
		public BuildTarget Platform => Shader.Collection.Platform;

		public bool WriteQuotesAroundProgram { get; set; } = true;

		private readonly Func<UnityVersion, GPUPlatform, ShaderTextExporter> m_exporterInstantiator;
	}
}
