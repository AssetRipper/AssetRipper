using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AssetRipper.Core.Structure
{
	public sealed class GameCollection : FileList, IFileCollection, IDisposable
	{
		public LayoutInfo Layout { get; }

		public IReadOnlyDictionary<string, SerializedFile> GameFiles => m_files;
		public IAssemblyManager AssemblyManager { get; set; }

		public List<ResourceFile?> GameResourceFiles => m_resources.Values.ToList();
		public List<SerializedFile> GameSerializedFiles => m_files.Values.ToList();

		private readonly Dictionary<string, SerializedFile> m_files = new();
		private readonly Dictionary<string, ResourceFile?> m_resources = new();

		private readonly HashSet<SerializedFile> m_scenes = new HashSet<SerializedFile>();

		public Func<string, string?>? ResourceCallback;

		private readonly Dictionary<Type, List<IUnityObjectBase>> _cachedAssetsByType = new();

		public GameCollection(LayoutInfo layout) : base(nameof(GameCollection))
		{
			Layout = layout;
		}

		public ISerializedFile? FindSerializedFile(string fileName)
		{
			m_files.TryGetValue(fileName, out SerializedFile? file);
			return file;
		}

		public bool TryGetResourceFile(string resourceName, [NotNullWhen(true)] out ResourceFile? file)
		{
			return m_resources.TryGetValue(resourceName, out file);
		}

		public IResourceFile? FindResourceFile(string resName)
		{
			string fixedName = FilenameUtils.FixResourcePath(resName);
			if (m_resources.TryGetValue(fixedName, out ResourceFile? file))
			{
				return file;
			}

			string? resPath = ResourceCallback?.Invoke(fixedName);
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

		public T? FindAsset<T>() where T : IUnityObjectBase
		{
			foreach (IUnityObjectBase asset in FetchAssets())
			{
				if (asset is T castedAsset)
				{
					return castedAsset;
				}
			}
			return default;
		}

		public T? FindAsset<T>(string name) where T : IUnityObjectBase, IHasNameString
		{
			foreach (IUnityObjectBase asset in FetchAssets())
			{
				if (asset is T namedAsset)
				{
					if (namedAsset.NameString == name)
					{
						return namedAsset;
					}
				}
			}
			return default;
		}

		public IEnumerable<IUnityObjectBase> FetchAssets()
		{
			foreach (SerializedFile file in m_files.Values)
			{
				foreach (IUnityObjectBase asset in file.FetchAssets())
				{
					yield return asset;
				}
			}
		}

		public IEnumerable<IUnityObjectBase> FetchAssetsOfType<T>() where T : IUnityObjectBase
		{
			if (_cachedAssetsByType.TryGetValue(typeof(T), out List<IUnityObjectBase>? list))
			{
				return list;
			}

			List<IUnityObjectBase> objects = FetchAssets().Where(o => o is T).ToList();
			_cachedAssetsByType.TryAdd(typeof(T), objects);

			return objects;
		}

		public bool IsScene(ISerializedFile file) => m_scenes.Contains(file);

		protected override void OnSerializedFileAdded(SerializedFile file)
		{
			if (m_files.ContainsKey(file.Name))
			{
				SerializedFile existingFile = m_files[file.Name];
				if (existingFile.FilePath == file.FilePath)
				{
					Logger.Error(LogCategory.Import, $"{nameof(SerializedFile)} with name '{file.Name}' and path '{file.FilePath}' was already added to this collection");
					return;
				}
				else if (FileUtils.GetFileSize(file.FilePath) == FileUtils.GetFileSize(existingFile.FilePath))
				{
					return; //assume identical
				}
				else
				{
#if DEBUG
					throw new ArgumentException($"{nameof(SerializedFile)} with name '{file.Name}' and path '{file.FilePath}' conflicts with file at '{existingFile.FilePath}'", nameof(file));
#else
					Logger.Warning(LogCategory.Import, $"{nameof(SerializedFile)} with name '{file.Name}' and path '{file.FilePath}' conflicts with file at '{existingFile.FilePath}'");
					return;
#endif
				}
			}
			if (file.Platform != Layout.Platform)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"'{file.Name}' is incompatible with platform of the game collection");
			}
			if (file.Version != Layout.Version)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"'{file.Name}' is incompatible with version of the game collection");
			}

			m_files[file.Name] = file;
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
			{
#if DEBUG
				throw new ArgumentException($"{nameof(ResourceFile)} with name '{file.Name}' already presents in the collection", nameof(file));
#else
				Logger.Warning(LogCategory.Import, $"{nameof(ResourceFile)} with name '{file.Name}' already presents in the collection");
#endif
			}
			else
			{
				m_resources.Add(file.Name, file);
			}
		}

		private static bool IsSceneSerializedFile(SerializedFile file)
		{
			return file.Metadata.Object.Any(entry => entry.ClassID.IsSceneSettings());
		}

		private void Dispose(bool disposing)
		{
			AssemblyManager?.Dispose();
			foreach (ResourceFile? res in m_resources.Values)
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
