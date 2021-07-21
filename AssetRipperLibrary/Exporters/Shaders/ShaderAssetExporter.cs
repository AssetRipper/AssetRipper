using AssetRipper;
using AssetRipper.Converters;
using AssetRipper.Converters.Classes.Shader;
using AssetRipper.Converters.Project;
using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Shader;
using AssetRipper.Parser.Classes.Shader.Enums;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;
using AssetRipper.Utils;
using AssetRipperLibrary.Exporters.Shaders.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Object = AssetRipper.Parser.Classes.Object.Object;
using Version = AssetRipper.Parser.Files.File.Version.Version;

namespace AssetRipperLibrary.Exporters.Shaders
{
	[SupportedOSPlatform("windows")]
	public sealed class ShaderAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				Shader shader = (Shader)asset;
				shader.ExportBinary(container, fileStream, ShaderExporterInstantiator);
			}
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset);
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		private static ShaderTextExporter ShaderExporterInstantiator(Version version, GPUPlatform graphicApi)
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

	}
}
