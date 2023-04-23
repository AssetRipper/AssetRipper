using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.IO.Files
{
	public abstract class FileContainer : FileBase
	{
		public IEnumerable<SerializedFile> FetchSerializedFiles()
		{
			foreach (SerializedFile file in SerializedFiles)
			{
				yield return file;
			}
			foreach (FileContainer list in FileLists)
			{
				foreach (SerializedFile file in list.FetchSerializedFiles())
				{
					yield return file;
				}
			}
		}

		public void AddFile(FileBase file)
		{
			switch (file)
			{
				case SerializedFile serializedFile:
					AddSerializedFile(serializedFile);
					return;
				case ResourceFile resourceFile:
					AddResourceFile(resourceFile);
					return;
				case FileContainer fileList:
					AddFileContainer(fileList);
					return;
				default:
					throw new NotSupportedException(file.GetType().ToString());
			}
		}

		public void AddSerializedFile(SerializedFile file)
		{
			m_serializedFiles.Add(file);
			OnSerializedFileAdded(file);
		}

		public void AddFileContainer(FileContainer container)
		{
			m_fileLists.Add(container);
			OnFileContainerAdded(container);
		}

		public void AddResourceFile(ResourceFile resource)
		{
			m_resourceFiles.Add(resource);
			OnResourceFileAdded(resource);
		}

		protected virtual void OnSerializedFileAdded(SerializedFile file) { }

		protected virtual void OnFileContainerAdded(FileContainer container) { }

		protected virtual void OnResourceFileAdded(ResourceFile resource) { }

		public override void ReadContents()
		{
			if (m_resourceFiles.Count > 0)
			{
				ResourceFile[] resourceFiles = m_resourceFiles.ToArray();
				m_resourceFiles.Clear();
				for (int i = 0; i < resourceFiles.Length; i++)
				{
					AddFile(SchemeReader.ReadFile(resourceFiles[i]));
				}
			}
		}

		public override void ReadContentsRecursively()
		{
			ReadContents();
			foreach (FileContainer container in FileLists)
			{
				container.ReadContentsRecursively();
			}
		}

		public IReadOnlyList<SerializedFile> SerializedFiles => m_serializedFiles;
		public IReadOnlyList<FileContainer> FileLists => m_fileLists;
		public IReadOnlyList<ResourceFile> ResourceFiles => m_resourceFiles;

		public IEnumerable<FileBase> AllFiles
		{
			get
			{
				foreach (ResourceFile resource in ResourceFiles)
				{
					yield return resource;
				}
				foreach (SerializedFile file in SerializedFiles)
				{
					yield return file;
				}
				foreach (FileContainer container in FileLists)
				{
					yield return container;
				}
			}
		}

		private readonly List<SerializedFile> m_serializedFiles = new List<SerializedFile>(0);
		private readonly List<FileContainer> m_fileLists = new List<FileContainer>(0);
		private readonly List<ResourceFile> m_resourceFiles = new List<ResourceFile>(0);
	}
}
