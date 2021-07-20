using AssetRipper.Parser.Files.ArchiveFile;
using AssetRipper.Parser.Files.BundleFile;
using AssetRipper.Parser.Files.ResourceFile;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Parser.Files.WebFile;
using AssetRipper.Structure.GameStructure;
using System;
using System.Collections.Generic;

namespace AssetRipper.Parser.Files.File.Parser
{
	public abstract class FileList
	{
		public FileList(string name)
		{
			Name = name;
		}

		public IEnumerable<SerializedFile.SerializedFile> FetchSerializedFiles()
		{
			foreach (SerializedFile.SerializedFile file in SerializedFiles)
			{
				yield return file;
			}
			foreach (FileList list in FileLists)
			{
				foreach (SerializedFile.SerializedFile file in list.FetchSerializedFiles())
				{
					yield return file;
				}
			}
		}

		public void AddSerializedFile(SerializedFile.SerializedFile file)
		{
			m_serializedFiles.Add(file);
			OnSerializedFileAdded(file);
		}

		public void AddFileList(FileList list)
		{
			m_fileLists.Add(list);
			OnFileListAdded(list);
		}

		public void AddResourceFile(ResourceFile.ResourceFile resource)
		{
			m_resourceFiles.Add(resource);
			OnResourceFileAdded(resource);
		}

		internal void AddFile(GameProcessorContext context, FileScheme scheme)
		{
			switch (scheme.SchemeType)
			{
				case FileEntryType.Serialized:
					{
						SerializedFileScheme serializedScheme = (SerializedFileScheme)scheme;
						SerializedFile.SerializedFile file = serializedScheme.ReadFile(context);
						AddSerializedFile(file);
					}
					break;

				case FileEntryType.Bundle:
					{
						BundleFileScheme bundleScheme = (BundleFileScheme)scheme;
						BundleFile.BundleFile bundle = bundleScheme.ReadFile(context);
						AddFileList(bundle);
					}
					break;

				case FileEntryType.Archive:
					{
						ArchiveFileScheme archiveScheme = (ArchiveFileScheme)scheme;
						ArchiveFile.ArchiveFile archive = archiveScheme.ReadFile(context);
						AddFileList(archive);
					}
					break;

				case FileEntryType.Web:
					{
						WebFileScheme webScheme = (WebFileScheme)scheme;
						WebFile.WebFile webFile = webScheme.ReadFile(context);
						AddFileList(webFile);
					}
					break;

				case FileEntryType.Resource:
					{
						ResourceFileScheme resourceScheme = (ResourceFileScheme)scheme;
						ResourceFile.ResourceFile resource = resourceScheme.ReadFile();
						AddResourceFile(resource);
					}
					break;

				default:
					throw new NotSupportedException(scheme.SchemeType.ToString());
			}
		}

		protected virtual void OnSerializedFileAdded(SerializedFile.SerializedFile file)
		{
		}

		protected virtual void OnFileListAdded(FileList list)
		{
		}

		protected virtual void OnResourceFileAdded(ResourceFile.ResourceFile resource)
		{
		}

		public string Name { get; }

		public IReadOnlyList<SerializedFile.SerializedFile> SerializedFiles => m_serializedFiles;
		public IReadOnlyList<FileList> FileLists => m_fileLists;
		public IReadOnlyList<ResourceFile.ResourceFile> ResourceFiles => m_resourceFiles;

		private readonly List<SerializedFile.SerializedFile> m_serializedFiles = new List<SerializedFile.SerializedFile>(0);
		private readonly List<FileList> m_fileLists = new List<FileList>(0);
		private readonly List<ResourceFile.ResourceFile> m_resourceFiles = new List<ResourceFile.ResourceFile>(0);
	}
}
