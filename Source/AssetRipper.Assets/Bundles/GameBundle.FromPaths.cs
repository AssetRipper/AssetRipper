using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.CompressedFiles;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

partial class GameBundle
{
	/// <summary>
	/// Create and initialize a <see cref="GameBundle"/> from a set of paths.
	/// </summary>
	/// <param name="paths">The set of paths to load.</param>
	/// <param name="assetFactory">The factory for reading assets.</param>
	/// <param name="dependencyProvider"></param>
	/// <param name="resourceProvider"></param>
	/// <param name="defaultVersion">The default version to use if a file does not have a version, ie the version has been stripped.</param>
	public static GameBundle FromPaths(IEnumerable<string> paths, AssetFactoryBase assetFactory, IGameInitializer? initializer = null)
	{
		GameBundle gameBundle = new();
		initializer?.OnCreated(gameBundle, assetFactory);
		gameBundle.InitializeFromPaths(paths, assetFactory, initializer);
		initializer?.OnPathsLoaded(gameBundle, assetFactory);
		gameBundle.InitializeAllDependencyLists(initializer?.DependencyProvider);
		initializer?.OnDependenciesInitialized(gameBundle, assetFactory);
		return gameBundle;
	}

	private void InitializeFromPaths(IEnumerable<string> paths, AssetFactoryBase assetFactory, IGameInitializer? initializer)
	{
		ResourceProvider = initializer?.ResourceProvider;
		List<FileBase> fileStack = LoadFilesAndDependencies(paths, initializer?.DependencyProvider);
		UnityVersion defaultVersion = initializer is null ? default : initializer.DefaultVersion;

		while (fileStack.Count > 0)
		{
			switch (RemoveLastItem(fileStack))
			{
				case SerializedFile serializedFile:
					SerializedAssetCollection.FromSerializedFile(this, serializedFile, assetFactory, defaultVersion);
					break;
				case FileContainer container:
					SerializedBundle serializedBundle = SerializedBundle.FromFileContainer(container, assetFactory, defaultVersion);
					AddBundle(serializedBundle);
					break;
				case ResourceFile resourceFile:
					AddResource(resourceFile);
					break;
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

	private static List<FileBase> LoadFilesAndDependencies(IEnumerable<string> paths, IDependencyProvider? dependencyProvider)
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

	private static void LoadDependencies(SerializedFile serializedFile, List<FileBase> files, HashSet<string> serializedFileNames, IDependencyProvider? dependencyProvider)
	{
		foreach (FileIdentifier fileIdentifier in serializedFile.Dependencies)
		{
			string name = fileIdentifier.GetFilePath();
			if (serializedFileNames.Add(name) && dependencyProvider?.FindDependency(fileIdentifier) is { } dependency)
			{
				files.Add(dependency);
			}
		}
	}
}
