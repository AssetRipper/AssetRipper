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
	public class AssetCollection : IAssetCollection
	{
		public void Load(string filePath)
		{
			if (BundleFile.IsBundleFile(filePath))
			{
				LoadAssetBundle(filePath);
			}
			else if(WebFile.IsWebFile(filePath))
			{
				LoadWebFile(filePath);
			}
			else
			{
				string fileName = Path.GetFileName(filePath);
				ReadSerializedFile(filePath, fileName);
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

		public void LoadAssetBundle(string filePath)
		{
			using (BundleFile bundle = new BundleFile())
			{
				bundle.Load(filePath);
				ProcessSerializedFileData(bundle.FileData, filePath);
			}
		}

		public void ReadAssetBundle(Stream stream, string filePath)
		{
			using (BundleFile bundle = new BundleFile())
			{
				bundle.Read(stream);
				ProcessSerializedFileData(bundle.FileData, filePath);
			}
		}

		public void LoadWebFile(string filePath)
		{
			using (WebFile web = new WebFile())
			{
				web.Load(filePath);
				ProcessWebFileData(web.FileData, filePath);
			}
		}

		public void ReadWebFile(Stream stream, string filePath)
		{
			using (WebFile web = new WebFile())
			{
				web.Read(stream);
				ProcessWebFileData(web.FileData, filePath);
			}
		}

		public void ReadSerializedFile(string filePath, string fileName)
		{
			SerializedFile file = new SerializedFile(this, filePath, fileName);
			file.Load(filePath);
			AddSerializedFile(file);
		}

		public void ReadSerializedFile(Stream stream, string filePath, string fileName)
		{
			SerializedFile file = new SerializedFile(this, filePath, fileName);
			file.Read(stream);
			AddSerializedFile(file);
		}

		public void Load(IReadOnlyCollection<string> filePathes)
		{
			foreach(string file in filePathes)
			{
				Load(file);
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
			
			const string archivePrefix = "archive:/";
			if (fileName.StartsWith(archivePrefix))
			{
				fileName = Path.GetFileName(fileName);
			}

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
		
		public IEnumerable<Classes.Object> FetchAssets()
		{
			foreach(SerializedFile file in m_files)
			{
				foreach(Classes.Object asset in file.FetchAssets())
				{
					yield return asset;
				}
			}
		}

		private void ProcessSerializedFileData(BundleFileData fileData, string filePath)
		{
			foreach (BundleMetadata metadata in fileData.Metadatas)
			{
				foreach (BundleFileEntry entry in metadata.AssetsEntries)
				{
					SerializedFile file = entry.ReadSerializedFile(this, filePath);
					AddSerializedFile(file);
				}
				foreach (BundleFileEntry entry in metadata.ResourceEntries)
				{
					ResourcesFile resesFile = entry.ReadResourcesFile(filePath);
					m_resources.Add(resesFile);
				}
			}
		}

		private void ProcessWebFileData(WebFileData fileData, string filePath)
		{
			foreach (WebFileEntry entry in fileData.AssetsEntries)
			{
				entry.ReadFile(this, filePath);
			}
			foreach (WebFileEntry entry in fileData.ResourceEntries)
			{
				entry.ReadResourcesFile(filePath);
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
				FillVersion(file);
			}
			
			if (m_files.Any(t => !t.Platform.IsCompatible(file.Platform)))
			{
				throw new ArgumentException($"Assets file '{file.Name}' has incompatible with other assets files platform {file.Platform} ", nameof(file));
			}
			
			m_files.Add(file);
		}

		private void FillVersion(SerializedFile file)
		{
			if (file.Version.IsSet)
			{
				return;
			}

			foreach (Classes.Object asset in file.Assets)
			{
				if(asset.ClassID == ClassIDType.BuildSettings)
				{	
					BuildSettings settings = (BuildSettings)asset;
					file.Version.Parse(settings.BSVersion);
					break;
				}
			}
		}
		
		public AssetsExporter Exporter { get; } = new AssetsExporter();
		public IReadOnlyList<ISerializedFile> Files => m_files;

		private readonly List<SerializedFile> m_files = new List<SerializedFile>();
		private readonly List<ResourcesFile> m_resources = new List<ResourcesFile>();
	}
}
