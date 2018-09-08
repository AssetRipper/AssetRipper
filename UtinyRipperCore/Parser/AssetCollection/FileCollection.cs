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
	public sealed class FileCollection : IFileCollection, IDisposable
	{
		public struct Parameters
		{
			public Action<string> RequestDependencyCallback { get; set; }
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
			m_dependencyCallback = pars.RequestDependencyCallback;
			m_assemblyCallback = pars.RequestAssemblyCallback;
			m_resourceCallback = pars.RequestResourceCallback;
		}

		~FileCollection()
		{
			Dispose(false);
		}

		public void Load(string filePath)
		{
			if (BundleFile.IsBundleFile(filePath))
			{
				LoadBundleFile(filePath);
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

		public void Read(SmartStream stream, string filePath)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				ReadBundleFile(stream, filePath);
			}
			else if (ArchiveFile.IsArchiveFile(stream))
			{
				ReadArchiveFile(stream, filePath);
			}
			else if (WebFile.IsWebFile(stream))
			{
				ReadWebFile(stream, filePath);
			}
			else
			{
				string fileName = Path.GetFileName(filePath);
				ReadSerializedFile(stream, filePath, fileName, OnRequestDependency);
			}
		}

		internal void ReadResourceFile(SmartStream stream, string filePath, string fileName, long offset, long size)
		{
			ResourcesFile resource = new ResourcesFile(stream, filePath, fileName, offset, size);
			AddResourceFile(resource);
		}

		public void LoadSerializedFile(string filePath)
		{
			SerializedFile.Parameters pars = new SerializedFile.Parameters()
			{
				FileCollection = this,
				AssemblyManager = AssemblyManager,
				FilePath = filePath,
				Name = Path.GetFileName(filePath),
				DependencyCallback = OnRequestDependency,
			};
			SerializedFile file = SerializedFile.Load(pars);
			AddSerializedFile(file);
		}

		public void LoadSerializedFile(string filePath, TransferInstructionFlags flags)
		{
			SerializedFile.Parameters pars = new SerializedFile.Parameters()
			{
				FileCollection = this,
				AssemblyManager = AssemblyManager,
				FilePath = filePath,
				Name = Path.GetFileName(filePath),
				DependencyCallback = OnRequestDependency,
				Flags = flags,
			};
			SerializedFile file = SerializedFile.Load(pars);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			ReadSerializedFile(stream, filePath, fileName, OnRequestDependency);
		}

		internal void ReadSerializedFile(Stream stream, string filePath, string fileName, Action<string> dependencyCallback)
		{
			SerializedFile.Parameters pars = new SerializedFile.Parameters()
			{
				FileCollection = this,
				AssemblyManager = AssemblyManager,
				FilePath = filePath,
				Name = fileName,
				DependencyCallback = dependencyCallback,
			};
			SerializedFile file = SerializedFile.Read(stream, pars);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath, TransferInstructionFlags flags)
		{
			string fileName = Path.GetFileName(filePath);
			ReadSerializedFile(stream, fileName, filePath, flags);
		}

		internal void ReadSerializedFile(Stream stream, string filePath, string fileName, TransferInstructionFlags flags)
		{
			SerializedFile.Parameters pars = new SerializedFile.Parameters()
			{
				FileCollection = this,
				AssemblyManager = AssemblyManager,
				FilePath = filePath,
				Name = fileName,
				DependencyCallback = OnRequestDependency,
				Flags = flags,
			};
			SerializedFile file = SerializedFile.Load(pars);
			AddSerializedFile(file);
		}

		public void LoadBundleFile(string bundlePath)
		{
			using (BundleFile bundle = BundleFile.Load(bundlePath))
			{
				AddBundleFile(bundle);
			}
		}

		public void ReadBundleFile(SmartStream stream, string bundlePath)
		{
			using (BundleFile bundle = BundleFile.Read(stream, bundlePath))
			{
				AddBundleFile(bundle);
			}
		}

		public void LoadArchiveFile(string filePath)
		{
			using (ArchiveFile archive = ArchiveFile.Load(filePath))
			{
				AddArchiveFile(archive);
			}
		}

		public void ReadArchiveFile(SmartStream stream, string archivePath)
		{
			using (ArchiveFile archive = ArchiveFile.Read(stream, archivePath))
			{
				AddArchiveFile(archive);
			}
		}

		public void LoadWebFile(string webPath)
		{
			using (WebFile web = WebFile.Load(webPath))
			{
				AddWebFile(web);
			}
		}

		public void ReadWebFile(SmartStream stream, string webPath)
		{
			using (WebFile web = WebFile.Read(stream, webPath))
			{
				AddWebFile(web);
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

		internal void AddResourceFile(ResourcesFile resource)
		{
			if (m_resources.Any(t => t.Name == resource.Name))
			{
				throw new ArgumentException($"Resource file with name '{resource.Name}' already presents in collection", nameof(resource));
			}
			m_resources.Add(resource);
		}

		internal void AddBundleFile(BundleFile bundle)
		{
			DependencyCollection depCollection = new DependencyCollection(this, bundle.Metadata.Entries, OnRequestDependency);
			depCollection.ReadFiles();
		}

		internal void AddArchiveFile(ArchiveFile archive)
		{
			if (archive.Metadata.Entries.Count > 1)
			{
				throw new NotSupportedException("More than one file for archive isn't supported");
			}

			foreach (ArchiveFileEntry entry in archive.Metadata.Entries)
			{
				// for now archive contains only one file so we shouldn't concern about dependencies
				switch (entry.EntryType)
				{
					case FileEntryType.Serialized:
						{
							entry.ReadSerializedFile(this);
						}
						break;
					case FileEntryType.Bundle:
						{
							entry.ReadBundleFile(this);
						}
						break;
					case FileEntryType.Web:
						{
							entry.ReadWebFile(this);
						}
						break;

					default:
						throw new Exception($"Unsupported file '{entry.Name}' inside archive '{entry.FilePath}'");
				}
			}
		}

		internal void AddWebFile(WebFile web)
		{
			DependencyCollection depCollection = new DependencyCollection(this, web.Metadata.Entries, OnRequestDependency);
			depCollection.ReadFiles();
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

		public ResourcesFile FindResourcesFile(ISerializedFile ifile, string resName)
		{
			SerializedFile file = (SerializedFile)ifile;
			resName = FilenameUtils.FixResourcePath(resName);

			// check asset bundles / web files
			string filePath = file.FilePath;
			foreach (ResourcesFile res in m_resources)
			{
				if(res.FilePath == filePath && res.Name == resName)
				{
					return res.CreateReference();
				}
			}

			string resPath = m_resourceCallback?.Invoke(resName);
			if(resPath == null)
			{
				return null;
			}
			using (SmartStream stream = SmartStream.OpenRead(resPath))
			{
				return new ResourcesFile(stream, resPath, resName, 0, stream.Length);
			}
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
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			AssemblyManager.Dispose();
			foreach (ResourcesFile res in m_resources)
			{
				res.Dispose();
			}
		}

		internal void AddSerializedFile(SerializedFile file)
		{
#if DEBUG
			if(m_files.Any(t => t.Name == file.Name))
			{
				throw new ArgumentException($"Assets file with name '{file.Name}' already presents in collection", nameof(file));
			}
			if (m_files.Any(t => !t.Platform.IsCompatible(file.Platform)))
			{
				throw new ArgumentException($"Assets file '{file.Name}' has incompatible with other assets files platform {file.Platform} ", nameof(file));
			}
#endif

			if (!RTTIClassHierarchyDescriptor.IsReadSignature(file.Header.Generation))
			{
				SetVersion(file);
			}

			m_files.Add(file);
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

		public void OnRequestDependency(string dependency)
		{
			foreach(SerializedFile file in Files)
			{
				if (file.Name == dependency)
				{
					return;
				}
			}
			m_dependencyCallback?.Invoke(dependency);
		}

		private void OnRequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			foreach (ResourcesFile file in m_resources)
			{
				if (file.Name == assemblyName)
				{
					using (PartialStream stream = new PartialStream(file.Stream, file.Offset, file.Size))
					{
						ReadAssembly(stream, assemblyName);
					}
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
					return;
				}
			}

			m_assemblyCallback?.Invoke(assembly);
		}
		
		public ProjectExporter Exporter { get; }
		public AssetFactory AssetFactory { get; } = new AssetFactory();
		public IReadOnlyList<ISerializedFile> Files => m_files;
		public IAssemblyManager AssemblyManager { get; }

		private readonly List<SerializedFile> m_files = new List<SerializedFile>();
		private readonly List<ResourcesFile> m_resources = new List<ResourcesFile>();

		private readonly Action<string> m_dependencyCallback;
		private readonly Action<string> m_assemblyCallback;
		private readonly Func<string, string> m_resourceCallback;
	}
}
