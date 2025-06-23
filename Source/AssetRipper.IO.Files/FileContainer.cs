using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.IO.Files;

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
			case FailedFile failedFile:
				AddFailedFile(failedFile);
				return;
			default:
				throw new NotSupportedException(file.GetType().ToString());
		}
	}

	public void AddSerializedFile(SerializedFile file)
	{
		if (m_serializedFiles is null)
		{
			m_serializedFiles = [file];
		}
		else
		{
			m_serializedFiles.Add(file);
		}
		OnSerializedFileAdded(file);
	}

	public void AddFileContainer(FileContainer container)
	{
		if (m_fileLists is null)
		{
			m_fileLists = [container];
		}
		else
		{
			m_fileLists.Add(container);
		}
		OnFileContainerAdded(container);
	}

	public void AddResourceFile(ResourceFile resource)
	{
		if (m_resourceFiles is null)
		{
			m_resourceFiles = [resource];
		}
		else
		{
			m_resourceFiles.Add(resource);
		}
		OnResourceFileAdded(resource);
	}

	public void AddFailedFile(FailedFile file)
	{
		if (m_failedFiles is null)
		{
			m_failedFiles = [file];
		}
		else
		{
			m_failedFiles.Add(file);
		}
	}

	protected virtual void OnSerializedFileAdded(SerializedFile file) { }

	protected virtual void OnFileContainerAdded(FileContainer container) { }

	protected virtual void OnResourceFileAdded(ResourceFile resource) { }

	public override void ReadContents()
	{
		if (m_resourceFiles is { Count: > 0 })
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

	public IReadOnlyList<SerializedFile> SerializedFiles => m_serializedFiles ?? [];
	public IReadOnlyList<FileContainer> FileLists => m_fileLists ?? [];
	public IReadOnlyList<ResourceFile> ResourceFiles => m_resourceFiles ?? [];
	public IReadOnlyList<FailedFile> FailedFiles => m_failedFiles ?? [];

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
			foreach (FailedFile file in FailedFiles)
			{
				yield return file;
			}
		}
	}

	private List<SerializedFile>? m_serializedFiles;
	private List<FileContainer>? m_fileLists;
	private List<ResourceFile>? m_resourceFiles;
	private List<FailedFile>? m_failedFiles;
}
