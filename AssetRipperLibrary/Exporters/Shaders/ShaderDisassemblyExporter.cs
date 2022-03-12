using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using ShaderTextRestorer.Exporters;
using ShaderTextRestorer.Exporters.DirectX;
using ShaderTextRestorer.IO;
using System;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class ShaderDisassemblyExporter : BinaryAssetExporter
	{
		ShaderExportMode ExportMode { get; set; }

		public ShaderDisassemblyExporter(LibraryConfiguration options)
		{
			ExportMode = options.ShaderExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is Shader && ExportMode == ShaderExportMode.Disassembly;
		}

		public static bool IsDX11ExportMode(ShaderExportMode mode) => mode == ShaderExportMode.Disassembly;

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			Shader shader = (Shader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm.Name?.StartsWith("Hidden/Internal") ?? false) 
				return false;

			using Stream fileStream = File.Create(path);
			ExportBinary(shader, container, fileStream, ShaderExporterInstantiator);
			return true;
		}

		private ShaderTextExporter ShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d11_9x:
				case GPUPlatform.d3d11:
				case GPUPlatform.d3d9:
					return new ShaderDXExporter(graphicApi);

				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				case GPUPlatform.openGL:
				case GPUPlatform.gles:
				case GPUPlatform.gles3:
				case GPUPlatform.glcore:
					return new ShaderGLESExporter();

				case GPUPlatform.metal:
					return new ShaderMetalExporter();

				case GPUPlatform.unknown:
					return new ShaderTextExporter();

				default:
					return new ShaderUnknownExporter(graphicApi);
			}
		}

		public void ExportBinary(Shader shader, IExportContainer container, Stream stream, Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (Shader.IsSerialized(container.Version))
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				((SerializedShader)shader.ParsedForm).Export(writer);
			}
			else if (Shader.HasBlob(container.Version))
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				string header = Encoding.UTF8.GetString(shader.Script);
				if (shader.Blobs.Length == 0)
				{
					writer.Write(header);
				}
				else
				{
					shader.Blobs[0].Export(writer, header);
				}
			}
			else
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(shader.Script);
			}
		}
	}
}
