using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.BundleFiles;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;
using UtinyRipper.WebFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public class FileCollection : IFileCollection
	{
		public event Action<string> EventRequestDependency;

		public FileCollection()
		{
			Exporter = new ProjectExporter(this);
		}

		public void Load(string filePath)
		{
			if (BundleFile.IsBundleFile(filePath))
			{
				LoadAssetBundle(filePath);
			}
			else if (WebFile.IsWebFile(filePath))
			{
				LoadWebFile(filePath);
			}
			else
			{
				string fileName = Path.GetFileName(filePath);
				LoadSerializedFile(filePath, fileName);
			}
		}

		public void Load(IReadOnlyCollection<string> filePathes)
		{
			foreach (string file in filePathes)
			{
				Load(file);
			}
		}

		public void Read(Stream stream, string filePath, string fileName)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				ReadAssetBundle(stream, filePath);
			}
			else if (WebFile.IsWebFile(stream))
			{
				ReadWebFile(stream, filePath);
			}
			else
			{
				ReadSerializedFile(stream, filePath, fileName);
			}
		}

		public void LoadSerializedFile(string filePath, string fileName)
		{
			SerializedFile file = new SerializedFile(this, filePath, fileName);
			file.Load(filePath, OnRequestDependency);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath, string fileName)
		{
			SerializedFile file = new SerializedFile(this, filePath, fileName);
			file.Read(stream, OnRequestDependency);
			AddSerializedFile(file);
		}

		public void LoadAssetBundle(string filePath)
		{
			using (BundleFile bundle = new BundleFile(this, filePath, OnRequestDependency))
			{
				bundle.Load(filePath);
				AddBundleFile(bundle);
			}
		}

		public void ReadAssetBundle(Stream stream, string filePath)
		{
			using (BundleFile bundle = new BundleFile(this, filePath, OnRequestDependency))
			{
				bundle.Read(stream);
				AddBundleFile(bundle);
			}
		}

		public void LoadWebFile(string filePath)
		{
			using (WebFile web = new WebFile(this, filePath, OnRequestDependency))
			{
				web.Load(filePath);
				AddWebFile(web);
			}
		}

		public void ReadWebFile(Stream stream, string filePath)
		{
			using (WebFile web = new WebFile(this, filePath, OnRequestDependency))
			{
				web.Read(stream);
				AddWebFile(web);
			}
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
		}
		
		public ISerializedFile GetSerializedFile(FileIdentifier fileRef)
		{
			ISerializedFile file = FindSerializedFile(fileRef);
			if (file == null)
			{
				throw new Exception($"{nameof(SerializedFile)} with Name '{fileRef.AssetPath}' and FileName '{fileRef.FilePath}' was not found");
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

			fileName = PathUtils.FixResourcePath(fileName);

			// check assets bundles / web files
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

		private void AddBundleFile(BundleFile bundle)
		{
			foreach(SerializedFile file in bundle.SerializedFiles)
			{
				AddSerializedFile(file);
			}
			foreach (ResourcesFile resource in bundle.ResourceFiles)
			{
				m_resources.Add(resource);
			}
		}

		private void AddWebFile(WebFile web)
		{
			foreach (ResourcesFile resource in web.ResourceFiles)
			{
				m_resources.Add(resource);
			}
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
				if(file.Name == dependency)
				{
					return;
				}
			}

			EventRequestDependency?.Invoke(dependency);
		}

		public ProjectExporter Exporter { get; }
		public AssetFactory AssetFactory { get; } = new AssetFactory();
		public IReadOnlyList<ISerializedFile> Files => m_files;

		private readonly List<SerializedFile> m_files = new List<SerializedFile>();
		private readonly List<ResourcesFile> m_resources = new List<ResourcesFile>();
	}
}
