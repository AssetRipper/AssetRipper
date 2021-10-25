using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.IO;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if DEBUG
using AssetRipper.Core.Extensions;
#endif

namespace AssetRipper.Core.Project.Collections
{
	public abstract class ExportCollection : IExportCollection
	{
		protected static void ExportMeta(IExportContainer container, Meta meta, string filePath)
		{
			string metaPath = $"{filePath}{MetaExtension}";
			using (var fileStream = FileUtils.CreateVirtualFile(metaPath))
			using (var stream = new BufferedStream(fileStream))
			using (var streamWriter = new InvariantStreamWriter(stream, new UTF8Encoding(false)))
			{
				YAMLWriter writer = new YAMLWriter();
				writer.IsWriteDefaultTag = false;
				writer.IsWriteVersion = false;
				writer.IsFormatKeys = true;
				YAMLDocument doc = meta.ExportYAMLDocument(container);
				writer.AddDocument(doc);
				writer.Write(streamWriter);
			}
		}

		public abstract bool Export(IProjectAssetContainer container, string dirPath);
		public abstract bool IsContains(UnityObjectBase asset);
		public abstract long GetExportID(UnityObjectBase asset);
		public abstract MetaPtr CreateExportPointer(UnityObjectBase asset, bool isLocal);

		protected void ExportAsset(IProjectAssetContainer container, AssetImporter importer, UnityObjectBase asset, string path, string name)
		{
			if (!Directory.Exists(path))
			{
				DirectoryUtils.CreateVirtualDirectory(path);
			}

			string fullName = $"{name}.{GetExportExtension(asset)}";
			string uniqueName = FileUtils.GetUniqueName(path, fullName, FileUtils.MaxFileNameLength - MetaExtension.Length);
			string filePath = Path.Combine(path, uniqueName);
			AssetExporter.Export(container, asset, filePath);
			Meta meta = new Meta(asset.GUID, importer);
			ExportMeta(container, meta, filePath);
		}

		protected string GetUniqueFileName(ISerializedFile file, UnityObjectBase asset, string dirPath)
		{
			string fileName;
			switch (asset)
			{
				case PrefabInstance prefab:
					fileName = prefab.GetName(file);
					break;
				case MonoBehaviour monoBehaviour:
					fileName = monoBehaviour.Name;
					break;
				case NamedObject named:
					fileName = named.ValidName;
					break;

				default:
					fileName = asset.GetType().Name;
					break;
			}
			fileName = FileUtils.RemoveCloneSuffixes(fileName);
			fileName = FileUtils.FixInvalidNameCharacters(fileName);

			fileName = $"{fileName}.{GetExportExtension(asset)}";
			return GetUniqueFileName(dirPath, fileName);
		}

		protected string GetUniqueFileName(string directoryPath, string fileName)
		{
			return FileUtils.GetUniqueName(directoryPath, fileName, FileUtils.MaxFileNameLength - MetaExtension.Length);
		}

		protected virtual string GetExportExtension(UnityObjectBase asset)
		{
			return asset.ExportExtension;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract ISerializedFile File { get; }
		public virtual TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;
		public abstract IEnumerable<UnityObjectBase> Assets { get; }
		public abstract string Name { get; }

		private const string MetaExtension = ".meta";
	}
}
