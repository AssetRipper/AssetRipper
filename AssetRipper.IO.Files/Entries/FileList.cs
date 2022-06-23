using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using System.Collections.Generic;

namespace AssetRipper.IO.Files.Entries
{
	public abstract class FileList
	{
		public FileList(string name)
		{
			Name = name;
		}

		public IEnumerable<SerializedFile> FetchSerializedFiles()
		{
			foreach (SerializedFile file in SerializedFiles)
			{
				yield return file;
			}
			foreach (FileList list in FileLists)
			{
				foreach (SerializedFile file in list.FetchSerializedFiles())
				{
					yield return file;
				}
			}
		}

		public void AddSerializedFile(SerializedFile file)
		{
			m_serializedFiles.Add(file);
			OnSerializedFileAdded(file);
		}

		public void AddFileList(FileList list)
		{
			m_fileLists.Add(list);
			OnFileListAdded(list);
		}

		public void AddResourceFile(ResourceFile resource)
		{
			m_resourceFiles.Add(resource);
			OnResourceFileAdded(resource);
		}

		protected virtual void OnSerializedFileAdded(SerializedFile file) { }

		protected virtual void OnFileListAdded(FileList list) { }

		protected virtual void OnResourceFileAdded(ResourceFile resource) { }

		public string Name { get; }

		public IReadOnlyList<SerializedFile> SerializedFiles => m_serializedFiles;
		public IReadOnlyList<FileList> FileLists => m_fileLists;
		public IReadOnlyList<ResourceFile> ResourceFiles => m_resourceFiles;

		private readonly List<SerializedFile> m_serializedFiles = new List<SerializedFile>(0);
		private readonly List<FileList> m_fileLists = new List<FileList>(0);
		private readonly List<ResourceFile> m_resourceFiles = new List<ResourceFile>(0);
	}
}
