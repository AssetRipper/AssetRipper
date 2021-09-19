using AssetRipper.Core;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Converters.Shader;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using UnityObject = AssetRipper.Core.Classes.Object.Object;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;
using ShaderTextRestorer.Exporters;
using ShaderTextRestorer.Exporters.DirectX;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class ShaderAssetExporter : IAssetExporter
	{
		ShaderExportMode ExportMode { get; set; } = ShaderExportMode.Dummy;

		public ShaderAssetExporter(LibraryConfiguration options)
		{
			ExportMode = options.ShaderExportMode;
		}

		public bool IsHandle(UnityObject asset, CoreConfiguration options) => true;

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
					shader.ExportDummy(container, fileStream, DefaultShaderExporterInstantiator);
				}
				else if (IsDX11ExportMode(ExportMode))
				{
					shader.ExportBinary(container, fileStream, HLSLShaderExporterInstantiator);
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
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}
	}
}
