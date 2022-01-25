using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Project.Exporters
{
	public class RawAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new RawExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			//return WriteRawToPath(container, asset, path);
			return WriteDumpToPath(container, asset, path);
		}

		private static bool WriteDumpToPath(IExportContainer container, IUnityObjectBase asset, string path)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Raw export currently has issues. Dumping instead...");
			sb.AppendLine();
			sb.AppendLine("Asset Information:");
			if (asset is IHasName hasName)
			{
				sb.AppendLine($"Name: {hasName.Name}");
			}
			sb.AppendLine($"Asset Type: {asset.GetType().FullName}");
			sb.AppendLine($"Path: {path}");
			sb.AppendLine($"Unity Version: {asset.AssetUnityVersion}");
			sb.AppendLine($"Endianess: {asset.EndianType}");
			sb.AppendLine($"GUID: {asset.GUID}");
			sb.AppendLine($"File: {asset.SerializedFile.Name}");
			sb.AppendLine($"Path ID: {asset.PathID}");
			sb.AppendLine();
			sb.AppendLine("Container Information:");
			sb.AppendLine($"Name: {container.Name}");
			sb.AppendLine($"Unity Version: {container.Version}");
			sb.AppendLine($"Export Version: {container.ExportVersion}");
			sb.AppendLine($"Platform: {container.Platform}");
			sb.AppendLine($"Export Platform: {container.ExportPlatform}");
			File.WriteAllText(path, sb.ToString());
			return true;
		}

		private static bool WriteRawToPath(IExportContainer container, IUnityObjectBase asset, string path)
		{
			Logger.Info(LogCategory.Export, $"Writing raw to {path}");
			try
			{
				LayoutInfo layoutInfo = new LayoutInfo(asset.AssetUnityVersion, Core.Parser.Files.Platform.NoTarget, asset.TransferInstructionFlags);
				using MemoryStream memoryStream = new MemoryStream();
				using AssetWriter writer = new AssetWriter(memoryStream, asset.EndianType, layoutInfo);
				asset.Write(writer);
				File.WriteAllBytes(path, memoryStream.ToArray());
				return true;
			}
			catch (Exception ex)
			{
				File.WriteAllText(path, ex.ToString());
				return false;
			}
		}
	}
}
