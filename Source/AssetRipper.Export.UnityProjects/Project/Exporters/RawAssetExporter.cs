using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Import.Logging;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public sealed class RawAssetExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = new RawExportCollection(this, asset);
			return true;
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
			if (asset is IHasNameString hasName)
			{
				sb.AppendLine($"Name: {hasName.NameString}");
			}
			sb.AppendLine($"Asset Type: {asset.GetType().FullName}");
			sb.AppendLine($"Path: {path}");
			sb.AppendLine($"Unity Version: {asset.Collection.Version}");
			sb.AppendLine($"Endianess: {asset.Collection.EndianType}");
			sb.AppendLine($"GUID: {asset.GUID}");
			sb.AppendLine($"File: {asset.Collection.Name}");
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
				using MemoryStream memoryStream = new MemoryStream();
				using AssetWriter writer = new AssetWriter(memoryStream, asset.Collection);
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
