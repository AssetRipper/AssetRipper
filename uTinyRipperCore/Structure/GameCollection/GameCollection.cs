using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Game;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

using MonoManager = uTinyRipper.Game.Assembly.Mono.MonoManager;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
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
			public Func<string, string> RequestAssemblyCallback { get; set; }
			public Func<string, string> RequestResourceCallback { get; set; }
		}

		public GameCollection(Parameters pars):
			base(nameof(GameCollection))
		{
			Layout = pars.Layout;
			m_layouts.Add(Layout.Info, Layout);
			AssemblyManager = new AssemblyManager(pars.ScriptBackend, Layout, OnRequestAssembly);
			m_assemblyCallback = pars.RequestAssemblyCallback;
			m_resourceCallback = pars.RequestResourceCallback;
			Exporter = new ProjectExporter(this);
		}

		~GameCollection()
		{
			Dispose(false);
		}

		public static FileScheme LoadScheme(string filePath, string fileName)
		{
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, filePath, fileName);
			}
		}

		public static FileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, false))
			{
				if (BundleFile.IsBundleFile(stream))
				{
					return BundleFile.ReadScheme(buffer, filePath, fileName);
				}
				if (ArchiveFile.IsArchiveFile(stream))
				{
					return ArchiveFile.ReadScheme(buffer, filePath, fileName);
				}
				if (WebFile.IsWebFile(stream))
				{
					return WebFile.ReadScheme(buffer, filePath);
				}
				if (SerializedFile.IsSerializedFile(stream))
				{
					return SerializedFile.ReadScheme(buffer, filePath, fileName);
				}
				return ResourceFile.ReadScheme(buffer, filePath, fileName);
			}
		}

		public static FileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				return BundleFile.ReadScheme(stream, filePath, fileName);
			}
			if (ArchiveFile.IsArchiveFile(stream))
			{
				return ArchiveFile.ReadScheme(stream, filePath, fileName);
			}
			if (WebFile.IsWebFile(stream))
			{
				return WebFile.ReadScheme(stream, filePath);
			}
			if (SerializedFile.IsSerializedFile(stream))
			{
				return SerializedFile.ReadScheme(stream, filePath, fileName);
			}
			return ResourceFile.ReadScheme(stream, filePath, fileName);
		}

		public void LoadAssembly(string filePath)
		{
			AssemblyManager.Load(filePath);
		}

		public void ReadAssembly(Stream stream, string fileName)
		{
			AssemblyManager.Read(stream, fileName);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			Logger.Log(LogType.Info, LogCategory.Import, $"Resource file '{resName}' has been loaded");
			return m_resources[fixedName];
		}

		public T FindAsset<T>()
			where T : Object
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			foreach (Object asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return (T)asset;
				}
			}
			return null;
		}

		public T FindAsset<T>(string name)
			where T : NamedObject
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

		public IEnumerable<Object> FetchAssets()
		{
			foreach (SerializedFile file in m_files.Values)
			{
				foreach (Object asset in file.FetchAssets())
				{
					yield return asset;
				}
			}
		}

		public bool IsScene(ISerializedFile file)
		{
			return m_scenes.Contains(file);
		}

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
				throw new ArgumentException($"{nameof(SerializedFile)} with name '{file.Name}' already presents in the collection", nameof(file));
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
			{
				throw new ArgumentException($"{nameof(ResourceFile)} with name '{file.Name}' already presents in the collection", nameof(file));
			}
			m_resources.Add(file.Name, file);
		}

		private void Dispose(bool disposing)
		{
			AssemblyManager.Dispose();
			foreach (ResourceFile res in m_resources.Values)
			{
				res?.Dispose();
			}
		}

		private bool IsSceneSerializedFile(SerializedFile file)
		{
			foreach (ObjectInfo entry in file.Metadata.Object)
			{
				if (entry.ClassID.IsSceneSettings())
				{
					return true;
				}
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
			Logger.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
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
	}
}
