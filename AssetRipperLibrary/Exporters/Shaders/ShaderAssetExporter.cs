using AssetRipper.Converters.Shader;
using AssetRipper.Project;
using AssetRipper.Project.Exporters;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Shader;
using AssetRipper.Classes.Shader.Enums;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using AssetRipper.Utils;
using AssetRipperLibrary.Exporters.Shaders.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using UnityObject = AssetRipper.Classes.Object.Object;
using Version = AssetRipper.Parser.Files.Version;
using AssetRipper;

namespace AssetRipperLibrary.Exporters.Shaders
{
	[SupportedOSPlatform("windows")]
	public sealed class ShaderAssetExporter : IAssetExporter
	{
		public bool IsHandle(UnityObject asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				Shader shader = (Shader)asset;
				shader.ExportBinary(container, fileStream, ShaderExporterInstantiator);
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
