using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public abstract class ExportCollection : IExportCollection
	{
		static ExportCollection()
		{
			string invalidChars = new string(Path.GetInvalidFileNameChars());
			string escapedChars = Regex.Escape(invalidChars);
			FileNameRegex = new Regex($"[{escapedChars}]");
		}

		public static long GetMainExportID(Object asset)
		{
			return GetMainExportID((uint)asset.ClassID, 0);
		}

		public static long GetMainExportID(uint classID)
		{
			return GetMainExportID(classID, 0);
		}

		public static long GetMainExportID(Object asset, uint value)
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
		public abstract bool IsContains(Object asset);
		public abstract long GetExportID(Object asset);
		public abstract ExportPointer CreateExportPointer(Object asset, bool isLocal);
		
		protected void ExportAsset(ProjectAssetContainer container, IAssetImporter importer, Object asset, string path, string name)
		{
			if (!DirectoryUtils.Exists(path))
			{
				DirectoryUtils.CreateVirtualDirectory(path);
			}

			string fullName = $"{name}.{GetExportExtension(asset)}";
			string uniqueName = FileUtils.GetUniqueName(path, fullName, FileUtils.MaxFileNameLength - MetaExtension.Length);
			string filePath = Path.Combine(path, uniqueName);
			AssetExporter.Export(container, asset, filePath);
			Meta meta = new Meta(importer, asset.GUID);
			ExportMeta(container, meta, filePath);
		}

		protected void ExportMeta(IExportContainer container, Meta meta, string filePath)
		{
			string metaPath = $"{filePath}{MetaExtension}";
			using (Stream fileStream = FileUtils.CreateVirtualFile(metaPath))
			{
				using (StreamWriter streamWriter = new InvariantStreamWriter(fileStream, new UTF8Encoding(false)))
				{
					YAMLWriter writer = new YAMLWriter();
					YAMLDocument doc = meta.ExportYAMLDocument(container);
					writer.IsWriteDefaultTag = false;
					writer.IsWriteVersion = false;
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}
		
		protected string GetUniqueFileName(ISerializedFile file, Object asset, string dirPath)
		{
			string fileName;
			switch (asset)
			{
				case NamedObject named:
					fileName = named.ValidName;
					break;
				case Prefab prefab:
					fileName = prefab.GetName(file);
					break;
				case MonoBehaviour monoBehaviour:
					fileName = monoBehaviour.Name;
					break;

				default:
					fileName = asset.GetType().Name;
					break;
			}
			fileName = FileNameRegex.Replace(fileName, string.Empty);

			fileName = $"{fileName}.{GetExportExtension(asset)}";
			return GetUniqueFileName(dirPath, fileName);
		}

		protected string GetUniqueFileName(string directoryPath, string fileName)
		{
			return FileUtils.GetUniqueName(directoryPath, fileName, FileUtils.MaxFileNameLength - MetaExtension.Length);
		}

		protected virtual string GetExportExtension(Object asset)
		{
			return asset.ExportExtension;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract ISerializedFile File { get; }
		public virtual TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;
		public abstract IEnumerable<Object> Assets { get; }
		public abstract string Name { get; }

		private const string MetaExtension = ".meta";

		private static readonly Regex FileNameRegex;
	}
}
