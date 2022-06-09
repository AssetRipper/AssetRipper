using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_1113;
using AssetRipper.SourceGenerated.Classes.ClassID_121;
using AssetRipper.SourceGenerated.Classes.ClassID_134;
using AssetRipper.SourceGenerated.Classes.ClassID_1953259897;
using AssetRipper.SourceGenerated.Classes.ClassID_200;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_62;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_84;
using AssetRipper.SourceGenerated.Classes.ClassID_850595691;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.Yaml;
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
			using FileStream fileStream = System.IO.File.Create(metaPath);
			using InvariantStreamWriter streamWriter = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));

			YamlWriter writer = new();
			writer.IsWriteDefaultTag = false;
			writer.IsWriteVersion = false;
			writer.IsFormatKeys = true;
			YamlDocument doc = meta.ExportYamlDocument(container);
			writer.AddDocument(doc);
			writer.Write(streamWriter);
		}

		public abstract bool Export(IProjectAssetContainer container, string dirPath);
		public abstract bool IsContains(IUnityObjectBase asset);
		public abstract long GetExportID(IUnityObjectBase asset);
		public abstract MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal);

		protected void ExportAsset(IProjectAssetContainer container, IUnityObjectBase importer, IUnityObjectBase asset, string path, string name)
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
			string? fileName = asset switch
			{
				IPrefabInstance prefab => prefab.GetName(file),
				IHasNameString hasName => hasName.GetNameNotEmpty(),
				_ => null,
			};
			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = asset.AssetClassName;
			}

			fileName = FileUtils.RemoveCloneSuffixes(fileName);
			fileName = FileUtils.FixInvalidNameCharacters(fileName);

			fileName = $"{fileName}.{GetExportExtension(asset)}";
			return GetUniqueFileName(dirPath, fileName);
		}

		protected static string GetUniqueFileName(string directoryPath, string fileName)
		{
			return FileUtils.GetUniqueName(directoryPath, fileName, FileUtils.MaxFileNameLength - MetaExtension.Length);
		}

		protected virtual string GetExportExtension(IUnityObjectBase asset)
		{
			return asset switch
			{
				IShader => "shader",
				IMaterial => "mat",
				IAnimationClip => "anim",
				IAnimatorController => "controller",
				IAnimatorOverrideController => "overrideController",
				SourceGenerated.Classes.ClassID_319.IAvatarMask or SourceGenerated.Classes.ClassID_1011.IAvatarMask => "mask",
				IShaderVariantCollection => "shadervariants",
				ICubemap => "cubemap",
				IFlare => "flare",
				ILightingSettings => "lighting",
				ILightmapParameters => "giparams",
				IPhysicMaterial => "physicMaterial",
				IPhysicsMaterial2D => "physicsMaterial2D",
				IRenderTexture => "renderTexture",
				ITerrainLayer => "terrainlayer",
				_ => asset.ExportExtension
			};
		}

		protected static IUnityObjectBase Convert(IUnityObjectBase asset, IExportContainer container)
		{
			if (asset is IGameObject gameObject)
			{
				gameObject.ConvertToEditorFormat(container);
			}
			else if (asset is ITransform transform)
			{
				transform.ConvertToEditorFormat();
			}
			return asset;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract ISerializedFile File { get; }
		public virtual TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;
		public abstract IEnumerable<IUnityObjectBase> Assets { get; }
		public abstract string Name { get; }

		private const string MetaExtension = ".meta";
	}
}
