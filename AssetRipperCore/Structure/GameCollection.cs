using AssetRipper.Core.Classes;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Project;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoManager = AssetRipper.Core.Structure.Assembly.Managers.MonoManager;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Structure
{
	public sealed class GameCollection : FileList, IFileCollection, IDisposable
	{
		public sealed class Parameters
		{
			public Parameters(AssetLayout layout)
			{
				Layout = layout;
			}

			public AssetLayout Layout { get; }
			public ScriptingBackend ScriptBackend { get; set; }
			public PlatformGameStructure PlatformStructure { get; set; }
			public Func<string, string> RequestAssemblyCallback { get; set; }
			public Func<string, string> RequestResourceCallback { get; set; }
		}

		public AssetLayout Layout { get; }

		public ProjectExporter Exporter { get; }
		public AssetFactory AssetFactory { get; } = new AssetFactory();
		public IReadOnlyDictionary<string, SerializedFile> GameFiles => m_files;
		public IAssemblyManager AssemblyManager { get; }

		private readonly Dictionary<string, SerializedFile> m_files = new Dictionary<string, SerializedFile>();
		private readonly Dictionary<string, ResourceFile> m_resources = new Dictionary<string, ResourceFile>();
		private readonly Dictionary<LayoutInfo, AssetLayout> m_layouts = new Dictionary<LayoutInfo, AssetLayout>();

		private readonly HashSet<SerializedFile> m_scenes = new HashSet<SerializedFile>();

		private readonly Func<string, string> m_assemblyCallback;
		private readonly Func<string, string> m_resourceCallback;

		private readonly Dictionary<ClassIDType, List<UnityObjectBase>> _cachedAssetsByType = new();

		public GameCollection(Parameters pars, CoreConfiguration configuration) : base(nameof(GameCollection))
		{
			Layout = pars.Layout;
			m_layouts.Add(Layout.Info, Layout);

			switch (pars.ScriptBackend)
			{
				case ScriptingBackend.Mono:
					AssemblyManager = new MonoManager(Layout, OnRequestAssembly);
					break;
				case ScriptingBackend.Il2Cpp:
					AssemblyManager = new Il2CppManager(Layout, OnRequestAssembly);
					break;
				case ScriptingBackend.Unknown:
					AssemblyManager = new BaseManager(Layout, OnRequestAssembly);
					break;
			}

			m_assemblyCallback = pars.RequestAssemblyCallback;
			m_resourceCallback = pars.RequestResourceCallback;

			Logger.SendStatusChange("loading_step_load_assemblies");

			try
			{
				//Loads any Mono or IL2Cpp assemblies
				AssemblyManager.Initialize(pars.PlatformStructure);
			}
			catch(Exception ex)
			{
				Logger.Error(LogCategory.Import, "Could not initialize assembly manager. Switching to the 'Unknown' scripting backend.");
				Logger.Error(ex);
				AssemblyManager = new BaseManager(Layout, OnRequestAssembly);
			}

			Exporter = new ProjectExporter(this, configuration);
		}

		public void Export(CoreConfiguration options) => Exporter.Export(this, options);

		public void LoadAssembly(string filePath) => AssemblyManager.Load(filePath);

		public void ReadAssembly(Stream stream, string fileName) => AssemblyManager.Read(stream, fileName);

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

			string resPath = m_resourceCallback?.Invoke(fixedName);
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

		public T FindAsset<T>(string name) where T : NamedObject
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			foreach (Object asset in FetchAssets())
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

		private void OnRequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			if (m_resources.TryGetValue(assemblyName, out ResourceFile resFile))
			{
				resFile.Stream.Position = 0;
				ReadAssembly(resFile.Stream, assemblyName);
			}
			else
			{
				string path = m_assemblyCallback?.Invoke(assembly);
				if (path == null)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
					return;
				}
				LoadAssembly(path);
			}
			Logger.Info(LogCategory.Import, $"Assembly '{assembly}' has been loaded");
		}

		private void Dispose(bool disposing)
		{
			AssemblyManager.Dispose();
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
