using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.ArchiveFiles;
using UtinyRipper.AssetExporters;
using UtinyRipper.BundleFiles;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;
using UtinyRipper.WebFiles;

using MonoManager = UtinyRipper.AssetExporters.Mono.MonoManager;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public class FileCollection : IFileCollection
	{
		public FileCollection():
			this(null)
		{
		}

		public FileCollection(Action<string> requestDependencyCallback):
			this(requestDependencyCallback, null)
		{
		}

		public FileCollection(Action<string> requestDependencyCallback, Action<string> requestAssemblyCallback)
		{
			Exporter = new ProjectExporter(this);
			AssemblyManager = new AssemblyManager(OnRequestAssembly);

			m_requestDependencyCallback = requestDependencyCallback;
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		public void Load(string filePath)
		{
			if (BundleFile.IsBundleFile(filePath))
			{
				LoadAssetBundle(filePath);
			}
			else if(ArchiveFile.IsArchiveFile(filePath))
			{
				LoadArchiveFile(filePath);
			}
			else if (WebFile.IsWebFile(filePath))
			{
				LoadWebFile(filePath);
			}
			else
			{
				LoadSerializedFile(filePath);
			}
		}

		public void Load(IReadOnlyCollection<string> filePathes)
		{
			foreach (string file in filePathes)
			{
				Load(file);
			}
		}

		public void Read(Stream stream, string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			Read(stream, fileName, filePath);
		}

		internal void Read(Stream stream, string fileName, string filePath)
		{
			Read(stream, fileName, filePath, OnRequestDependency);
		}

		internal void Read(Stream stream, string fileName, string filePath, Action<string> requestDependencyCallback)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				ReadAssetBundle(stream, filePath, requestDependencyCallback);
			}
			else if (ArchiveFile.IsArchiveFile(stream))
			{
				ReadArchiveFile(stream, filePath);
			}
			else if (WebFile.IsWebFile(stream))
			{
				ReadWebFile(stream, filePath, requestDependencyCallback);
			}
			else
			{
				ReadSerializedFile(stream, fileName, filePath, requestDependencyCallback);
			}
		}

		public void LoadSerializedFile(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			LoadSerializedFile(fileName, filePath);
		}

		internal void LoadSerializedFile(string fileName, string filePath)
		{
			SerializedFile file = new SerializedFile(this, fileName, filePath);
			file.Load(filePath, OnRequestDependency);
			AddSerializedFile(file);
		}

		public void LoadSerializedFile(string filePath, TransferInstructionFlags flags)
		{
			string fileName = Path.GetFileName(filePath);
			LoadSerializedFile(fileName, filePath, flags);
		}

		internal void LoadSerializedFile(string fileName, string filePath, TransferInstructionFlags flags)
		{
			SerializedFile file = new SerializedFile(this, fileName, filePath, flags);
			file.Load(filePath, OnRequestDependency);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			ReadSerializedFile(stream, fileName, filePath);
		}

		internal void ReadSerializedFile(Stream stream, string fileName, string filePath)
		{
			ReadSerializedFile(stream, fileName, filePath, OnRequestDependency);
		}

		internal void ReadSerializedFile(Stream stream, string fileName, string filePath, Action<string> requestDependencyCallback)
		{
			SerializedFile file = new SerializedFile(this, fileName, filePath);
			file.Read(stream, requestDependencyCallback);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath, TransferInstructionFlags flags)
		{
			string fileName = Path.GetFileName(filePath);
			ReadSerializedFile(stream, fileName, filePath, flags);
		}

		internal void ReadSerializedFile(Stream stream, string fileName, string filePath, TransferInstructionFlags flags)
		{
			SerializedFile file = new SerializedFile(this, fileName, filePath, flags);
			file.Read(stream, OnRequestDependency);
			AddSerializedFile(file);
		}

		public void LoadAssetBundle(string filePath)
		{
			using (BundleFile bundle = new BundleFile(this, filePath, OnRequestDependency))
			{
				bundle.Load(filePath);
			}
		}

		public void ReadAssetBundle(Stream stream, string filePath)
		{
			ReadAssetBundle(stream, filePath, OnRequestDependency);
		}

		internal void ReadAssetBundle(Stream stream, string filePath, Action<string> requestDependencyCallback)
		{
			using (BundleFile bundle = new BundleFile(this, filePath, requestDependencyCallback))
			{
				bundle.Read(stream);
			}
		}

		public void LoadArchiveFile(string filePath)
		{
			using (ArchiveFile archive = new ArchiveFile(this, filePath))
			{
				archive.Load(filePath);
			}
		}

		public void ReadArchiveFile(Stream stream, string filePath)
		{
			using (ArchiveFile archive = new ArchiveFile(this, filePath))
			{
				archive.Read(stream);
			}
		}

		public void LoadWebFile(string filePath)
		{
			using (WebFile web = new WebFile(this, filePath, OnRequestDependency))
			{
				web.Load(filePath);
			}
		}

		public void ReadWebFile(Stream stream, string filePath)
		{
			ReadWebFile(stream, filePath, OnRequestDependency);
		}

		internal void ReadWebFile(Stream stream, string filePath, Action<string> requestDependencyCallback)
		{
			using (WebFile web = new WebFile(this, filePath, requestDependencyCallback))
			{
				web.Read(stream);
			}
		}

		public void LoadAssembly(string filePath)
		{
			AssemblyManager.Load(filePath);
		}

		public void ReadAssembly(Stream stream, string fileName)
		{
			AssemblyManager.Read(stream, fileName);
		}

		public void Unload(string filepath)
		{
			for(int i = 0; i > m_files.Count; i++)
			{
				SerializedFile file = m_files[i];
				if(file.FilePath == filepath)
				{
					m_files.RemoveAt(i);
					i--;
				}
			}
			for (int i = 0; i > m_resources.Count; i++)
			{
				ResourcesFile file = m_resources[i];
				if (file.FilePath.StartsWith(filepath, StringComparison.Ordinal))
				{
					file.Dispose();
					m_resources.RemoveAt(i);
					i--;
				}
			}
		}

		public void UnloadAssembly(string name)
		{
			AssemblyManager.Unload(name);
		}

		public void UnloadAll()
		{
			m_files.Clear();

			foreach (ResourcesFile resource in m_resources)
			{
				resource.Dispose();
			}
			m_resources.Clear();

			AssemblyManager.Dispose();
		}

		public ISerializedFile GetSerializedFile(FileIdentifier fileRef)
		{
			ISerializedFile file = FindSerializedFile(fileRef);
			if (file == null)
			{
				throw new Exception($"{nameof(SerializedFile)} with Name '{fileRef.AssetPath}' and FileName '{fileRef.FilePathOrigin}' was not found");
			}
			return file;
		}

		public ISerializedFile FindSerializedFile(FileIdentifier file)
		{
			return m_files.Find(file.IsFile);
		}

		public ResourcesFile FindResourcesFile(ISerializedFile ifile, string fileName)
		{
			SerializedFile file = (SerializedFile)ifile;

			fileName = FilenameUtils.FixResourcePath(fileName);

			// check asset bundles / web files
			string filePath = file.FilePath;
			foreach (ResourcesFile res in m_resources)
			{
				if(res.FilePath == filePath && res.Name == fileName)
				{
					return res;
				}
			}

			// check manualy loaded resource files 
			string dirPath = Path.GetDirectoryName(filePath) ?? string.Empty;
			string resPath = Path.Combine(dirPath, fileName);
			foreach (ResourcesFile res in m_resources)
			{
				if (res.FilePath == resPath && res.Name == fileName)
				{
					return res;
				}
			}
			
			// lazy loading
			if (FileMultiStream.Exists(resPath))
			{
				Stream stream = FileMultiStream.OpenRead(resPath);
				ResourcesFile resesFile = new ResourcesFile(resPath, fileName, stream);
				m_resources.Add(resesFile);
				return resesFile;
			}
			return null;
		}
		
		public IEnumerable<Object> FetchAssets()
		{
			foreach(SerializedFile file in m_files)
			{
				foreach(Object asset in file.FetchAssets())
				{
					yield return asset;
				}
			}
		}
				
		private void AddSerializedFile(SerializedFile file)
		{
			if(m_files.Any(t => t.Name == file.Name))
			{
				throw new ArgumentException($"Assets file with name '{file.Name}' already presents in collection", nameof(file));
			}

			if(!RTTIClassHierarchyDescriptor.IsReadSignature(file.Header.Generation))
			{
				SetVersion(file);
			}
			
			if (m_files.Any(t => !t.Platform.IsCompatible(file.Platform)))
			{
				throw new ArgumentException($"Assets file '{file.Name}' has incompatible with other assets files platform {file.Platform} ", nameof(file));
			}
			
			m_files.Add(file);
		}

		internal void AddResourceFile(ResourcesFile resource)
		{
			if (m_resources.Any(t => t.Name == resource.Name))
			{
				throw new ArgumentException($"Resource file with name '{resource.Name}' already presents in collection", nameof(resource));
			}
			m_resources.Add(resource);
		}

		private void SetVersion(SerializedFile file)
		{
			if (file.Version.IsSet)
			{
				return;
			}

			foreach (Object asset in file.Assets)
			{
				if(asset.ClassID == ClassIDType.BuildSettings)
				{	
					BuildSettings settings = (BuildSettings)asset;
					file.Version.Parse(settings.BSVersion);
					break;
				}
			}
		}

		private void OnRequestDependency(string dependency)
		{
			foreach(SerializedFile file in Files)
			{
				if (file.Name == dependency)
				{
					return;
				}
			}
			m_requestDependencyCallback?.Invoke(dependency);
		}

		private void OnRequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			foreach (ResourcesFile file in m_resources)
			{
				if (file.Name == assemblyName)
				{
					long position = file.Stream.Position;
					file.Position = 0;
					ReadAssembly(file.Stream, assemblyName);
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
					file.Stream.Position = position;
					return;
				}
			}

			m_requestAssemblyCallback?.Invoke(assembly);
		}
		
		public ProjectExporter Exporter { get; }
		public AssetFactory AssetFactory { get; } = new AssetFactory();
		public IReadOnlyList<ISerializedFile> Files => m_files;
		public IAssemblyManager AssemblyManager { get; }

		private readonly List<SerializedFile> m_files = new List<SerializedFile>();
		private readonly List<ResourcesFile> m_resources = new List<ResourcesFile>();

		private readonly Action<string> m_requestDependencyCallback;
		private readonly Action<string> m_requestAssemblyCallback;
	}
}
