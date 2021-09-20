using AssetRipper.Core.Parser.Files.ArchiveFiles;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.WebFiles;
using AssetRipper.Core.Structure.GameStructure;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.Entries
{
	public abstract class FileList
	{
		public FileList(string name)
		{
			Name = name;
		}

		public IEnumerable<SerializedFiles.SerializedFile> FetchSerializedFiles()
		{
			foreach (SerializedFiles.SerializedFile file in SerializedFiles)
			{
				yield return file;
			}
			foreach (FileList list in FileLists)
			{
				foreach (SerializedFiles.SerializedFile file in list.FetchSerializedFiles())
				{
					yield return file;
				}
			}
		}

		public void AddSerializedFile(SerializedFiles.SerializedFile file)
		{
			m_serializedFiles.Add(file);
			OnSerializedFileAdded(file);
		}

		public void AddFileList(FileList list)
		{
			m_fileLists.Add(list);
			OnFileListAdded(list);
		}

		public void AddResourceFile(ResourceFiles.ResourceFile resource)
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
						SerializedFiles.SerializedFile file = serializedScheme.ReadFile(context);
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
						ArchiveFiles.ArchiveFile archive = archiveScheme.ReadFile(context);
						AddFileList(archive);
					}
					break;

				case FileEntryType.Web:
					{
						WebFileScheme webScheme = (WebFileScheme)scheme;
						WebFiles.WebFile webFile = webScheme.ReadFile(context);
						AddFileList(webFile);
					}
					break;

				case FileEntryType.Resource:
					{
						ResourceFileScheme resourceScheme = (ResourceFileScheme)scheme;
						ResourceFiles.ResourceFile resource = resourceScheme.ReadFile();
						AddResourceFile(resource);
					}
					break;

				default:
					throw new NotSupportedException(scheme.SchemeType.ToString());
			}
		}

		protected virtual void OnSerializedFileAdded(SerializedFiles.SerializedFile file) { }

		protected virtual void OnFileListAdded(FileList list) { }

		protected virtual void OnResourceFileAdded(ResourceFiles.ResourceFile resource) { }

		public string Name { get; }

		public IReadOnlyList<SerializedFiles.SerializedFile> SerializedFiles => m_serializedFiles;
		public IReadOnlyList<FileList> FileLists => m_fileLists;
		public IReadOnlyList<ResourceFiles.ResourceFile> ResourceFiles => m_resourceFiles;

		private readonly List<SerializedFiles.SerializedFile> m_serializedFiles = new List<SerializedFiles.SerializedFile>(0);
		private readonly List<FileList> m_fileLists = new List<FileList>(0);
		private readonly List<ResourceFiles.ResourceFile> m_resourceFiles = new List<ResourceFiles.ResourceFile>(0);
	}
}
