using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.Modules.Shaders.Exporters;
using AssetRipper.Export.Modules.Shaders.Exporters.DirectX;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	public sealed class ShaderDisassemblyExporter : ShaderExporterBase
	{
		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IShader shader = (IShader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm?.Name.String.StartsWith("Hidden/Internal", StringComparison.Ordinal) ?? false)
			{
				return false;
			}

			using Stream fileStream = File.Create(path);
			ExportBinary(shader, fileStream, ShaderExporterInstantiator);
			return true;
		}

		private static ShaderTextExporter ShaderExporterInstantiator(GPUPlatform graphicApi)
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

		private static void ExportBinary(IShader shader, Stream stream, Func<GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (shader.Has_ParsedForm())
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				shader.ParsedForm.Export(writer);
			}
			else if (shader.Has_CompressedBlob())
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				string header = shader.Script.String;
				if (writer.Blobs.Length == 0)
				{
					writer.Write(header);
				}
				else
				{
					writer.Blobs[0].Export(writer, header);
				}
			}
			else
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(shader.Script.Data);
			}
		}
	}
}
