using AssetRipper.Project;
using AssetRipper.Project.Exporters;
using AssetRipper.IO;
using AssetRipper.Classes;
using AssetRipper.Classes.Meta;
using AssetRipper.Classes.Meta.Importers.Asset;
using AssetRipper.Classes.PrefabInstance;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.IO.Asset;
using AssetRipper.Utils;
using AssetRipper.YAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityObject = AssetRipper.Classes.Object.UnityObject;
using AssetRipper.Extensions;

namespace AssetRipper.Structure.Collections
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

		public static long GetMainExportID(UnityObject asset)
		{
			return GetMainExportID((uint)asset.ClassID, 0);
		}

		public static long GetMainExportID(uint classID)
		{
			return GetMainExportID(classID, 0);
		}

		public static long GetMainExportID(UnityObject asset, uint value)
		{
			return GetMainExportID((uint)asset.ClassID, value);
		}

		public static long GetMainExportID(uint classID, uint value)
		{
			if (classID > 100100)
			{
				if (value != 0)
				{
					throw new ArgumentException("Unique asset type with non unique modifier", nameof(value));
				}
				return classID;
			}

#if DEBUG
			int digits = BitConverterExtensions.GetDigitsCount(value);
			if(digits > 5)
			{
				throw new ArgumentException($"Value {value} for main export ID must have no more than 5 digits");
			}
#endif
			return (classID * 100000) + value;
		}

		public abstract bool Export(ProjectAssetContainer container, string dirPath);
		public abstract bool IsContains(UnityObject asset);
		public abstract long GetExportID(UnityObject asset);
		public abstract MetaPtr CreateExportPointer(UnityObject asset, bool isLocal);

		protected void ExportAsset(ProjectAssetContainer container, AssetImporter importer, UnityObject asset, string path, string name)
		{
			if (!DirectoryUtils.Exists(path))
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

		protected string GetUniqueFileName(ISerializedFile file, UnityObject asset, string dirPath)
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
			fileName = FileUtils.FixInvalidNameCharacters(fileName);

			fileName = $"{fileName}.{GetExportExtension(asset)}";
			return GetUniqueFileName(dirPath, fileName);
		}

		protected string GetUniqueFileName(string directoryPath, string fileName)
		{
			return FileUtils.GetUniqueName(directoryPath, fileName, FileUtils.MaxFileNameLength - MetaExtension.Length);
		}

		protected virtual string GetExportExtension(UnityObject asset)
		{
			return asset.ExportExtension;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract ISerializedFile File { get; }
		public virtual TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;
		public abstract IEnumerable<UnityObject> Assets { get; }
		public abstract string Name { get; }

		private const string MetaExtension = ".meta";
	}
}
