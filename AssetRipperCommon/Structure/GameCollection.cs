using AssetRipper.Core.Classes;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Structure
{
	public sealed class GameCollection : FileList, IFileCollection, IDisposable
	{
		public AssetLayout Layout { get; }

		public IAssetFactory AssetFactory { get; set; }
		public IReadOnlyDictionary<string, SerializedFile> GameFiles => m_files;
		public IAssemblyManager AssemblyManager { get; set; }

		public List<ResourceFile> GameResourceFiles => m_resources.Values.ToList();

		private readonly Dictionary<string, SerializedFile> m_files = new Dictionary<string, SerializedFile>();
		private readonly Dictionary<string, ResourceFile> m_resources = new Dictionary<string, ResourceFile>();
		private readonly Dictionary<LayoutInfo, AssetLayout> m_layouts = new Dictionary<LayoutInfo, AssetLayout>();

		private readonly HashSet<SerializedFile> m_scenes = new HashSet<SerializedFile>();

		public event Func<string, string> ResourceCallback;

		private readonly Dictionary<ClassIDType, List<UnityObjectBase>> _cachedAssetsByType = new();

		public GameCollection(AssetLayout layout) : base(nameof(GameCollection))
		{
			Layout = layout;
			m_layouts.Add(Layout.Info, Layout);
		}

		public ISerializedFile FindSerializedFile(string fileName)
		{
			m_files.TryGetValue(fileName, out SerializedFile file);
			return file;
		}

		public IResourceFile FindResourceFile(string resName)
		{
			string fixedName = FilenameUtils.FixResourcePath(resName);
			if (m_resources.TryGetValue(fixedName, out ResourceFile file))
			{
				return file;
			}

			string resPath = ResourceCallback?.Invoke(fixedName);
			if (resPath == null)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Resource file '{resName}' hasn't been found");
				m_resources.Add(fixedName, null);
				return null;
			}

			using (ResourceFileScheme scheme = ResourceFile.LoadScheme(resPath, fixedName))
			{
				ResourceFile resourceFile = scheme.ReadFile();
				AddResourceFile(resourceFile);
			}
			Logger.Info(LogCategory.Import, $"Resource file '{resName}' has been loaded");
			return m_resources[fixedName];
		}

		public T FindAsset<T>() where T : UnityObjectBase
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			foreach (UnityObjectBase asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return (T)asset;
				}
			}
			return null;
		}

		public T FindAsset<T>(string name) where T : UnityObjectBase, INamedObject
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			foreach (UnityObjectBase asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					T namedAsset = (T)asset;
					if (namedAsset.ValidName == name)
					{
						return namedAsset;
					}
				}
			}
			return null;
		}

		public IEnumerable<UnityObjectBase> FetchAssets()
		{
			foreach (SerializedFile file in m_files.Values)
			{
				foreach (UnityObjectBase asset in file.FetchAssets())
				{
					yield return asset;
				}
			}
		}

		public IEnumerable<UnityObjectBase> FetchAssetsOfType(ClassIDType type)
		{
			if (_cachedAssetsByType.TryGetValue(type, out List<UnityObjectBase> list))
				return list;

			List<UnityObjectBase> objects = FetchAssets().Where(o => o.ClassID == type).ToList();
			_cachedAssetsByType.TryAdd(type, objects);
			
			return objects;
		}

		public bool IsScene(ISerializedFile file) => m_scenes.Contains(file);

		public AssetLayout GetLayout(LayoutInfo info)
		{
			if (!m_layouts.TryGetValue(info, out AssetLayout value))
			{
				value = new AssetLayout(info);
				m_layouts.Add(info, value);
			}
			return value;
		}

		protected override void OnSerializedFileAdded(SerializedFile file)
		{
			if (m_files.ContainsKey(file.Name))
			{
				var existingFile = m_files[file.Name];
				if (existingFile.FilePath == file.FilePath)
				{
					Logger.Error(LogCategory.Import, $"{nameof(SerializedFile)} with name '{file.Name}' and path '{file.FilePath}' was already added to this collection");
					return;
				}
				else if(FileUtils.GetFileSize(file.FilePath) == FileUtils.GetFileSize(existingFile.FilePath))
				{
					return; //assume identical
				}
				throw new ArgumentException($"{nameof(SerializedFile)} with name '{file.Name}' and path '{file.FilePath}' conflicts with file at '{existingFile.FilePath}'", nameof(file));
			}
			if (file.Platform != Layout.Info.Platform)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"'{file.Name}' is incompatible with platform of the game collection");
			}
			if (file.Version != Layout.Info.Version)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"'{file.Name}' is incompatible with version of the game collection");
			}

			m_files.Add(file.Name, file);
			if (IsSceneSerializedFile(file))
			{
				m_scenes.Add(file);
			}
		}

		protected override void OnFileListAdded(FileList list)
		{
			foreach (SerializedFile file in list.SerializedFiles)
			{
				OnSerializedFileAdded(file);
			}
			foreach (FileList nestedList in list.FileLists)
			{
				OnFileListAdded(nestedList);
			}
			foreach (ResourceFile file in list.ResourceFiles)
			{
				OnResourceFileAdded(file);
			}
		}

		protected override void OnResourceFileAdded(ResourceFile file)
		{
			if (m_resources.ContainsKey(file.Name))
				throw new ArgumentException($"{nameof(ResourceFile)} with name '{file.Name}' already presents in the collection", nameof(file));
			else
				m_resources.Add(file.Name, file);
		}

		private bool IsSceneSerializedFile(SerializedFile file)
		{
			foreach (ObjectInfo entry in file.Metadata.Object)
			{
				if (entry.ClassID.IsSceneSettings()) 
					return true;
			}
			return false;
		}

		private void Dispose(bool disposing)
		{
			AssemblyManager?.Dispose();
			foreach (ResourceFile res in m_resources.Values)
			{
				res?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~GameCollection() => Dispose(false);
	}
}
