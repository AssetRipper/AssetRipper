using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.CompressedFiles;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

partial class GameBundle
{
	public void InitializeFromPaths(IEnumerable<string> paths, AssetFactoryBase assetFactory, IDependencyProvider dependencyProvider, IResourceProvider resourceProvider)
	{
		ResourceProvider = resourceProvider;
		List<FileBase> fileStack = LoadFilesAndDependencies(paths, dependencyProvider);

		while (fileStack.Count > 0)
		{
			FileBase file = RemoveLastItem(fileStack);
			if (file is SerializedFile serializedFile)
			{
				//Collection is added to this automatically
				SerializedAssetCollection.FromSerializedFile(this, serializedFile, assetFactory);
			}
			else if (file is FileContainer container)
			{
				SerializedBundle bundle = SerializedBundle.FromFileContainer(container, assetFactory);
				AddBundle(bundle);
			}
			else if (file is ResourceFile resourceFile)
			{
				AddResource(resourceFile);
			}
		}
	}

	private static FileBase RemoveLastItem(List<FileBase> list)
	{
		int index = list.Count - 1;
		FileBase file = list[index];
		list.RemoveAt(index);
		return file;
	}

	private static List<FileBase> LoadFilesAndDependencies(IEnumerable<string> paths, IDependencyProvider dependencyProvider)
	{
		List<FileBase> files = new();
		HashSet<string> serializedFileNames = new();//Includes missing dependencies
		foreach (string path in paths)
		{
			FileBase? file = SchemeReader.LoadFile(path);
			file?.ReadContentsRecursively();
			while (file is CompressedFile compressedFile)
			{
				file = compressedFile.UncompressedFile;
			}
			if (file is ResourceFile resourceFile)
			{
				files.Add(file);
			}
			else if (file is SerializedFile serializedFile)
			{
				files.Add(file);
				serializedFileNames.Add(serializedFile.NameFixed);
			}
			else if (file is FileContainer container)
			{
				files.Add(file);
				foreach (SerializedFile serializedFileInContainer in container.FetchSerializedFiles())
				{
					serializedFileNames.Add(serializedFileInContainer.NameFixed);
				}
			}
		}

		for (int i = 0; i < files.Count; i++)
		{
			FileBase file = files[i];
			if (file is SerializedFile serializedFile)
			{
				LoadDependencies(serializedFile, files, serializedFileNames, dependencyProvider);
			}
			else if (file is FileContainer container)
			{
				foreach (SerializedFile serializedFileInContainer in container.FetchSerializedFiles())
				{
					LoadDependencies(serializedFileInContainer, files, serializedFileNames, dependencyProvider);
				}
			}
		}

		return files;
	}

	private static void LoadDependencies(SerializedFile serializedFile, List<FileBase> files, HashSet<string> serializedFileNames, IDependencyProvider dependencyProvider)
	{
		for (int j = 0; j < serializedFile.Dependencies.Count; j++)
		{
			FileIdentifier fileIdentifier = serializedFile.Dependencies[j];
			string name = fileIdentifier.GetFilePath();
			if (serializedFileNames.Add(name))
			{
				FileBase? dependency = dependencyProvider.FindDependency(fileIdentifier);
				if (dependency is not null)
				{
					files.Add(dependency);
				}
			}
		}
	}
}
