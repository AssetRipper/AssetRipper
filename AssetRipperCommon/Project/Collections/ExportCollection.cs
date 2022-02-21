using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Material;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Project.Collections
{
	public abstract class ExportCollection : IExportCollection
	{
		protected static void ExportMeta(IExportContainer container, Meta meta, string filePath)
		{
			string metaPath = $"{filePath}{MetaExtension}";
			using var fileStream = System.IO.File.Create(metaPath);
			using var streamWriter = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));

			YAMLWriter writer = new YAMLWriter();
			writer.IsWriteDefaultTag = false;
			writer.IsWriteVersion = false;
			writer.IsFormatKeys = true;
			YAMLDocument doc = meta.ExportYAMLDocument(container);
			writer.AddDocument(doc);
			writer.Write(streamWriter);
		}

		public abstract bool Export(ProjectAssetContainer container, string dirPath);
		public abstract bool IsContains(IUnityObjectBase asset);
		public abstract long GetExportID(IUnityObjectBase asset);
		public abstract MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal);

		protected void ExportAsset(ProjectAssetContainer container, IAssetImporter importer, IUnityObjectBase asset, string path, string name)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			string fullName = $"{name}.{GetExportExtension(asset)}";
			string uniqueName = FileUtils.GetUniqueName(path, fullName, FileUtils.MaxFileNameLength - MetaExtension.Length);
			string filePath = Path.Combine(path, uniqueName);
			AssetExporter.Export(container, asset, filePath);
			Meta meta = new Meta(asset.GUID, importer);
			ExportMeta(container, meta, filePath);
		}

		protected string GetUniqueFileName(ISerializedFile file, IUnityObjectBase asset, string dirPath)
		{
			string fileName = asset switch
			{
				IPrefabInstance prefab => prefab.GetName(file),
				IMonoBehaviour monoBehaviour => monoBehaviour.Name,
				INamedObject named => named.GetValidName(),
				IHasName hasName => hasName.Name,
				_ => null,
			};
			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = asset.GetType().Name;
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

		protected virtual string GetExportExtension(IUnityObjectBase asset)
		{
			if (asset is IShader)
				return "shader";
			else if (asset is IMaterial)
				return "mat";
			else
				return asset.ExportExtension;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract ISerializedFile File { get; }
		public virtual TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;
		public abstract IEnumerable<IUnityObjectBase> Assets { get; }
		public abstract string Name { get; }

		private const string MetaExtension = ".meta";
	}
}
