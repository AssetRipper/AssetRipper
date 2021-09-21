using AssetRipper.Core;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using ShaderTextRestorer.Exporters;
using ShaderTextRestorer.Exporters.DirectX;
using ShaderTextRestorer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityObject = AssetRipper.Core.Classes.Object.Object;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class ShaderAssetExporter : IAssetExporter
	{
		ShaderExportMode ExportMode { get; set; }

		public ShaderAssetExporter(LibraryConfiguration options)
		{
			ExportMode = options.ShaderExportMode;
		}

		public bool IsHandle(UnityObject asset) => true;

		public static bool IsDX11ExportMode(ShaderExportMode mode) => mode == ShaderExportMode.Disassembly;

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			Shader shader = (Shader)asset;

			//Importing Hidden/Internal shaders causes the unity editor screen to turn black
			if (shader.ParsedForm.Name?.StartsWith("Hidden/") ?? false) 
				return false;

			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				if (ExportMode == ShaderExportMode.Dummy)
				{
					DummyShaderTextExporter.ExportShader(shader, container, fileStream, DefaultShaderExporterInstantiator);
				}
				else if (IsDX11ExportMode(ExportMode))
				{
					ExportBinary(shader, container, fileStream, HLSLShaderExporterInstantiator);
				}
				else
				{
					ExportBinary(shader, container, fileStream, DefaultShaderExporterInstantiator);
				}
			}
			return true;
		}

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
		{
			return new AssetExportCollection(this, asset);
		}

		public AssetType ToExportType(UnityObject asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		public static ShaderTextExporter DefaultShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.unknown:
					return new ShaderTextExporter();

				case GPUPlatform.openGL:
				case GPUPlatform.gles:
				case GPUPlatform.gles3:
				case GPUPlatform.glcore:
					return new ShaderGLESExporter();

				case GPUPlatform.metal:
					return new ShaderMetalExporter();

				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				default:
					return new ShaderUnknownExporter(graphicApi);
			}
		}

		private ShaderTextExporter HLSLShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d11_9x:
				case GPUPlatform.d3d11:
				case GPUPlatform.d3d9:
					return new ShaderDXExporter(graphicApi);
				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				default:
					return DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}

		public void ExportBinary(Shader shader, IExportContainer container, Stream stream) => ExportBinary(shader, container, stream, DefaultShaderExporterInstantiator);
		public void ExportBinary(Shader shader, IExportContainer container, Stream stream, Func<UnityVersion, GPUPlatform, ShaderTextExporter> exporterInstantiator)
		{
			if (Shader.IsSerialized(container.Version))
			{
				using (ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator))
				{
					shader.ParsedForm.Export(writer);
				}
			}
			else if (Shader.HasBlob(container.Version))
			{
				using (ShaderWriter writer = new ShaderWriter(stream, shader, exporterInstantiator))
				{
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
			}
			else
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(shader.Script);
				}
			}
		}
	}
}
