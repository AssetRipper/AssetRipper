using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.ArchiveFiles;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.BundleFiles;
using uTinyRipper.Classes;
using uTinyRipper.ResourceFiles;
using uTinyRipper.SerializedFiles;
using uTinyRipper.WebFiles;

using MonoManager = uTinyRipper.Assembly.Mono.MonoManager;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
{
	public sealed class FileCollection : FileList, IFileCollection, IDisposable
	{
		public struct Parameters
		{
			public Action<string> RequestAssemblyCallback { get; set; }
			public Func<string, string> RequestResourceCallback { get; set; }
		}

		public FileCollection()
		{
			Exporter = new ProjectExporter(this);
			AssemblyManager = new AssemblyManager(OnRequestAssembly);
		}

		public FileCollection(Parameters pars) :
			this()
		{
			m_assemblyCallback = pars.RequestAssemblyCallback;
			m_resourceCallback = pars.RequestResourceCallback;
		}

		~FileCollection()
		{
			Dispose(false);
		}

		public static FileScheme LoadScheme(string filePath, string fileName)
		{
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, 0, stream.Length, filePath, fileName);
			}
		}

		public static FileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			if (BundleFile.IsBundleFile(stream, offset, size))
			{
				return BundleFile.ReadScheme(stream, offset, size, filePath, fileName);
			}
			if (ArchiveFile.IsArchiveFile(stream, offset, size))
			{
				return ArchiveFile.ReadScheme(stream, offset, size, filePath, fileName);
			}
			if (WebFile.IsWebFile(stream, offset, size))
			{
				return WebFile.ReadScheme(stream, offset, size, filePath, fileName);
			}
			if (ResourceFile.IsDefaultResourceFile(fileName))
			{
				return ResourceFile.ReadScheme(stream, offset, size, filePath, fileName);
			}
			if (SerializedFile.IsSerializedFile(stream, offset, size))
			{
				return SerializedFile.ReadScheme(stream, offset, size, filePath, fileName);
			}
			return ResourceFile.ReadScheme(stream, offset, size, filePath, fileName);
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

		public ISerializedFile FindSerializedFile(FileIdentifier identifier)
		{
			m_files.TryGetValue(identifier.FilePath, out SerializedFile file);
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
				m_resources.Add(fixedName, null);
				return null;
			}

			using (ResourceFileScheme scheme = ResourceFile.LoadScheme(resPath, resName))
			{
				AddFile(scheme, this, AssemblyManager);
			}
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

		protected override void OnSerializedFileAdded(SerializedFile file)
		{
			//SetVersion(file);

			if (m_files.ContainsKey(file.Name))
			{
				throw new ArgumentException($"{nameof(SerializedFile)} with name '{file.Name}' already presents in the collection", nameof(file));
			}
			/*if (file.Platform != Platform)
			{
				throw new ArgumentException($"{nameof(SerializedFile)} '{file.Name}' is incompatible with platform of other asset files {file.Platform} ", nameof(file));
			}
			if (file.Version != Version)
			{
				throw new ArgumentException($"{nameof(SerializedFile)} '{file.Name}' is incompatible with version of other asset files {file.Platform} ", nameof(file));
			}*/

			m_files.Add(file.Name, file);
			if (SerializedFileIsScene(file))
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
			foreach (ResourceFile file in list.ResourceFiles)
			{
				OnResourceFileAdded(file);
			}
			foreach (FileList nestedList in list.FileLists)
			{
				OnFileListAdded(nestedList);
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

		private bool SerializedFileIsScene(ISerializedFile file)
		{
			foreach(Object asset in file.FetchAssets())
			{
				if (asset.ClassID.IsSceneSettings())
				{
					return true;
				}
			}
			return false;
		}

		private void OnRequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			foreach (ResourceFile file in m_resources.Values)
			{
				if (file.Name == assemblyName)
				{
					using (PartialStream stream = new PartialStream(file.Stream, file.Offset, file.Size))
					{
						ReadAssembly(stream, assemblyName);
					}
					Logger.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");

					m_resources.Remove(assemblyName);
					file.Dispose();
					return;
				}
			}

			m_assemblyCallback?.Invoke(assembly);
		}

		//public Version Version { get; }
		//public Platform Platform { get; }
		//public TransferInstructionFlags Flags { get; }

		public ProjectExporter Exporter { get; }
		public AssetFactory AssetFactory { get; } = new AssetFactory();
		public IEnumerable<ISerializedFile> Files => m_files.Values;
		public IAssemblyManager AssemblyManager { get; }

		private readonly Dictionary<string, SerializedFile> m_files = new Dictionary<string, SerializedFile>();
		private readonly Dictionary<string, ResourceFile> m_resources = new Dictionary<string, ResourceFile>();

		private readonly HashSet<SerializedFile> m_scenes = new HashSet<SerializedFile>();

		private readonly Action<string> m_assemblyCallback;
		private readonly Func<string, string> m_resourceCallback;
	}
}
