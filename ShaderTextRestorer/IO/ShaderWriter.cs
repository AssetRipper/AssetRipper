using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;
using ShaderTextRestorer.Exporters;
using System;
using System.IO;
using System.Text;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace ShaderTextRestorer.IO
{
	public class ShaderWriter : InvariantStreamWriter
	{
		public ShaderWriter(Stream stream, Shader shader, Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator) : base(stream, new UTF8Encoding(false), 4096, true)
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
			ShaderTextExporter exporter = m_exporterInstantiator.Invoke(Shader.SerializedFile.Version, graphicApi);
			exporter.Export(this, ref subProgram);
		}

		public Shader Shader { get; }
		public UnityVersion Version => Shader.SerializedFile.Version;
		public Platform Platform => Shader.SerializedFile.Platform;

		private readonly Func<UnityVersion, GPUPlatform, ShaderTextExporter> m_exporterInstantiator;
	}
}
