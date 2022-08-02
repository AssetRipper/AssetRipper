using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using ShaderTextRestorer.Exporters;
using ShaderTextRestorer.Exporters.DirectX;
using ShaderTextRestorer.IO;
using System.IO;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class ShaderDisassemblyExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IShader;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IShader shader = (IShader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm_C48?.NameString?.StartsWith("Hidden/Internal") ?? false)
			{
				return false;
			}

			using Stream fileStream = File.Create(path);
			ExportBinary(shader, fileStream, ShaderExporterInstantiator);
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

		private void ExportBinary(IShader shader, Stream stream, Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (shader.Has_ParsedForm_C48())
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				shader.ParsedForm_C48.Export(writer);
			}
			else if (shader.Has_SubProgramBlob_C48())
			{
				using ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator);
				string? header = shader.Script_C48?.String;
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
				writer.Write(shader.Script_C48?.Data ?? Array.Empty<byte>());
			}
		}
	}
}
