using AssetRipper.Core;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Converters.Shader;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using UnityObject = AssetRipper.Core.Classes.Object.Object;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Library.Exporters.Shaders
{
	[SupportedOSPlatform("windows")]
	public sealed class ShaderAssetExporter : IAssetExporter
	{
		ShaderExportMode ExportMode { get; set; } = ShaderExportMode.Dummy;

		public ShaderAssetExporter(LibraryConfiguration options)
		{
			ExportMode = options.ShaderExportMode;
		}

		public bool IsHandle(UnityObject asset, CoreConfiguration options)
		{
			return true;
		}

		public static bool IsDX11ExportMode(ShaderExportMode mode)
		{
			switch (mode)
			{
				case ShaderExportMode.GLSL:
				case ShaderExportMode.Metal:
				case ShaderExportMode.DXBytecode:
				case ShaderExportMode.DXBytecodeRestored:
					return true;
				default:
					return false;
			}
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				Shader shader = (Shader)asset;
				if (ExportMode == ShaderExportMode.Dummy)
				{
					DummyShaderTextExporter.ExportShader(shader, container, fileStream, DefaultShaderExporterInstantiator);
				}
				else if (IsDX11ExportMode(ExportMode))
				{
					shader.ExportBinary(container, fileStream, HLSLShaderExporterInstantiator);
				}
				else if (ExportMode == ShaderExportMode.Asm)
				{
					shader.ExportBinary(container, fileStream, AssemblyShaderExporterInstantiator);
				}
				else
				{
					shader.ExportBinary(container, fileStream, DefaultShaderExporterInstantiator);
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

		/*old instantiator
		private static ShaderTextExporter ShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d9:
					return new ShaderDXExporter(graphicApi);

				case GPUPlatform.d3d11_9x:
				case GPUPlatform.d3d11:
					return new ShaderHLSLccExporter(graphicApi);

				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				default:
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}
		*/

		private static ShaderTextExporter DefaultShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();
				default:
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}

		private ShaderTextExporter AssemblyShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d9:
				case GPUPlatform.d3d11_9x:
				case GPUPlatform.d3d11:
					return new ShaderAsmExporter(graphicApi);
				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();
				default:
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}

		private ShaderTextExporter HLSLShaderExporterInstantiator(UnityVersion version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d11:
					if (ExportMode == ShaderExportMode.DXBytecode)
					{
						return new ShaderDxBytecodeExporter(graphicApi, restore: false);
					}
					if (ExportMode == ShaderExportMode.DXBytecodeRestored)
					{
						return new ShaderDxBytecodeExporter(graphicApi, restore: true);
					}
					if (ExportMode == ShaderExportMode.GLSL)
					{
						return new ShaderHLSLccExporter(graphicApi, HLSLccWrapper.WrappedGLLang.LANG_DEFAULT);
					}
					if (ExportMode == ShaderExportMode.Metal)
					{
						return new ShaderHLSLccExporter(graphicApi, HLSLccWrapper.WrappedGLLang.LANG_METAL);
					}
					throw new Exception($"Unexpected shader mode {ExportMode}");
				case GPUPlatform.d3d9:
				case GPUPlatform.d3d11_9x:
					return new ShaderAsmExporter(graphicApi);
				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				default:
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}
	}
}
