using System;
using System.Collections.Generic;
using uTinyRipper.ArchiveFiles;
using uTinyRipper.Assembly;
using uTinyRipper.BundleFiles;
using uTinyRipper.ResourceFiles;
using uTinyRipper.SerializedFiles;
using uTinyRipper.WebFiles;

namespace uTinyRipper
{
	public abstract class FileList
	{
		internal void AddFile(FileScheme scheme, IFileCollection collection, IAssemblyManager manager)
		{
			switch(scheme.SchemeType)
			{
				case FileEntryType.Serialized:
					{
						SerializedFileScheme serializedScheme = (SerializedFileScheme)scheme;
						SerializedFile file = serializedScheme.ReadFile(collection, manager);
						m_serializedFiles.Add(file);
						OnSerializedFileAdded(file);
					}
					break;

				case FileEntryType.Bundle:
					{
						BundleFileScheme bundleScheme = (BundleFileScheme)scheme;
						BundleFile bundle = bundleScheme.ReadFile(collection, manager);
						m_fileLists.Add(bundle);
						OnFileListAdded(bundle);
					}
					break;

				case FileEntryType.Archive:
					{
						ArchiveFileScheme archiveScheme = (ArchiveFileScheme)scheme;
						ArchiveFile archive = archiveScheme.ReadFile(collection, manager);
						m_fileLists.Add(archive);
						OnFileListAdded(archive);
					}
					break;

				case FileEntryType.Web:
					{
						WebFileScheme webScheme = (WebFileScheme)scheme;
						WebFile webFile = webScheme.ReadFile(collection, manager);
						m_fileLists.Add(webFile);
						OnFileListAdded(webFile);
					}
					break;

				case FileEntryType.Resource:
					{
						ResourceFileScheme resourceScheme = (ResourceFileScheme)scheme;
						ResourceFile resource = resourceScheme.ReadFile();
						m_resourceFiles.Add(resource);
						OnResourceFileAdded(resource);
					}
					break;

				default:
					throw new NotSupportedException(scheme.SchemeType.ToString());
			}
		}

		protected virtual void OnSerializedFileAdded(SerializedFile file)
		{
		}

		protected virtual void OnFileListAdded(FileList list)
		{
		}

		protected virtual void OnResourceFileAdded(ResourceFile resource)
		{
		}

		public IReadOnlyList<SerializedFile> SerializedFiles => m_serializedFiles;
		public IReadOnlyList<FileList> FileLists => m_fileLists;
		public IReadOnlyList<ResourceFile> ResourceFiles => m_resourceFiles;

		private readonly List<SerializedFile> m_serializedFiles = new List<SerializedFile>();
		private readonly List<FileList> m_fileLists = new List<FileList>();
		private readonly List<ResourceFile> m_resourceFiles = new List<ResourceFile>();
	}
}
