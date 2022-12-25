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
		Stack<FileBase> fileStack = LoadAndSortFiles(paths, dependencyProvider);

		while (fileStack.Count > 0)
		{
			FileBase file = fileStack.Pop();
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

	private static Stack<FileBase> LoadAndSortFiles(IEnumerable<string> paths, IDependencyProvider dependencyProvider)
	{
		List<FileBase> files = new();
		HashSet<string> serializedFileNames = new();
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
				foreach (SerializedFile serializedFileInContainer in container.AllFiles.OfType<SerializedFile>())
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
				foreach (SerializedFile serializedFileInContainer in container.AllFiles.OfType<SerializedFile>())
				{
					LoadDependencies(serializedFileInContainer, files, serializedFileNames, dependencyProvider);
				}
			}
		}

		Stack<FileBase> fileStack = new();
		foreach (FileBase file in files.OrderByDescending(f => f, FileComparer.Shared))
		{
			fileStack.Push(file);
		}

		return fileStack;
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

	private sealed class SerializedFileComparer : NotNullComparer<SerializedFile>
	{
		public static SerializedFileComparer Shared { get; } = new();

		protected override int CompareNotNull(SerializedFile x, SerializedFile y)
		{
			bool xReferencesY = References(x, y);
			bool xReferencedByY = References(y, x);
			if (xReferencesY && !xReferencedByY)
			{
				return 1;
			}
			else if (xReferencedByY && !xReferencesY)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		private static bool References(SerializedFile referencingFile, SerializedFile referencedFile)
		{
			for (int i = 0; i < referencingFile.Dependencies.Count; i++)
			{
				if (referencingFile.Dependencies[i].GetFilePath() == referencedFile.NameFixed)
				{
					return true;
				}
			}

			return false;
		}
	}

	private sealed class FileComparer : NotNullComparer<FileBase>
	{
		public static FileComparer Shared { get; } = new();

		protected override int CompareNotNull(FileBase x, FileBase y)
		{
			if (x is ResourceFile)
			{
				return y is ResourceFile ? 0 : -1;
			}
			else if (y is ResourceFile)
			{
				return 1;
			}
			else if (x is SerializedFile serializedFileX)
			{
				if (y is SerializedFile serializedFileY)
				{
					return SerializedFileComparer.Shared.Compare(serializedFileX, serializedFileY);
				}
				else if (y is FileContainer containerY)
				{
					return Compare(serializedFileX, containerY);
				}
				else
				{
					return -1;
				}
			}
			else if (x is FileContainer containerX)
			{
				if (y is SerializedFile serializedFileY)
				{
					return Compare(containerX, serializedFileY);
				}
				else if (y is FileContainer containerY)
				{
					return Compare(containerX, containerY);
				}
				else
				{
					return -1;
				}
			}
			else
			{
				return y is SerializedFile or FileContainer ? 1 : 0;
			}
		}

		private static int Compare(FileContainer containerX, FileContainer containerY)
		{
			bool xLessThanY = false;
			bool xGreaterThanY = false;
			foreach (SerializedFile serializedFile in containerX.AllFiles.OfType<SerializedFile>())
			{
				int comparison = Compare(serializedFile, containerY);
				if (comparison < 0)
				{
					xLessThanY = true;
				}
				else if (comparison > 0)
				{
					xGreaterThanY = true;
				}
			}
			return MakeComparisonValue(xLessThanY, xGreaterThanY);
		}

		private static int Compare(FileContainer containerX, SerializedFile serializedFileY)
		{
			return -Compare(serializedFileY, containerX);
		}

		private static int Compare(SerializedFile serializedFileX, FileContainer containerY)
		{
			bool xLessThanY = false;
			bool xGreaterThanY = false;
			foreach (SerializedFile serializedFile in containerY.AllFiles.OfType<SerializedFile>())
			{
				int comparison = SerializedFileComparer.Shared.Compare(serializedFileX, serializedFile);
				if (comparison < 0)
				{
					xLessThanY = true;
				}
				else if (comparison > 0)
				{
					xGreaterThanY = true;
				}
			}
			return MakeComparisonValue(xLessThanY, xGreaterThanY);
		}

		private static int MakeComparisonValue(bool xLessThanY, bool xGreaterThanY)
		{
			if (xLessThanY && !xGreaterThanY)
			{
				return -1;
			}
			else if (xGreaterThanY && !xLessThanY)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
	}

	private abstract class NotNullComparer<T> : IComparer<T> where T : class
	{
		public int Compare(T? x, T? y)
		{
			if (x is null)
			{
				return y is null ? 0 : -1;
			}
			else if (y is null)
			{
				return 1;
			}

			return CompareNotNull(x, y);
		}

		protected abstract int CompareNotNull(T x, T y);
	}
}
