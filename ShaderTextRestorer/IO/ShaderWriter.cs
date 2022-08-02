using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.VersionUtilities;
using ShaderTextRestorer.Exporters;
using ShaderTextRestorer.Extensions;
using ShaderTextRestorer.ShaderBlob;
using System;
using System.IO;
using System.Text;


namespace ShaderTextRestorer.IO
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
			ShaderTextExporter exporter = m_exporterInstantiator.Invoke(Shader.SerializedFile.Version, graphicApi);
			exporter.Export(this, ref subProgram);
		}

		public IShader Shader { get; }
		public ShaderSubProgramBlob[] Blobs { get; }
		public UnityVersion Version => Shader.SerializedFile.Version;
		public BuildTarget Platform => Shader.SerializedFile.Platform;

		public bool WriteQuotesAroundProgram { get; set; } = true;

		private readonly Func<UnityVersion, GPUPlatform, ShaderTextExporter> m_exporterInstantiator;
	}
}
