using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.CompressedFiles;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> encompassing an entire game.
/// </summary>
public sealed class GameBundle : Bundle
{
	public IResourceProvider? ResourceProvider { get; set; }
	public IReadOnlyList<TemporaryBundle> TemporaryBundles => temporaryBundles;
	private readonly List<TemporaryBundle> temporaryBundles = new();

	public override string Name => nameof(GameBundle);

	protected override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is ProcessedBundle or SerializedBundle;
	}

	protected override bool IsCompatibleCollection(AssetCollection collection)
	{
		return collection is SerializedAssetCollection or ProcessedAssetCollection;
	}

	protected override ResourceFile? ResolveResourceInternal(string originalName, string fixedName)
	{
		ResourceFile? file = base.ResolveResourceInternal(originalName, fixedName);
		if (file is null && ResourceProvider is not null)
		{
			ResourceFile? resourceFile = ResourceProvider.FindResource(originalName);
			if (resourceFile is not null)
			{
				AddResource(resourceFile);
			}
			return resourceFile;
		}
		else
		{
			return file;
		}
	}

	public void ClearTemporaryBundles()
	{
		temporaryBundles.Clear();
	}

	public void AddTemporaryBundle(TemporaryBundle bundle)
	{
		if (bundle.Parent is null)
		{
			temporaryBundles.Add(bundle);
			bundle.Parent = this;
		}
		else if (bundle.Parent == this)
		{
		}
		else
		{
			throw new ArgumentException($"{nameof(bundle)} already has a parent.", nameof(bundle));
		}
	}

	public TemporaryBundle AddNewTemporaryBundle()
	{
		TemporaryBundle bundle = new();
		temporaryBundles.Add(bundle);
		bundle.Parent = this;
		return bundle;
	}

	public void AddMissingDependencies(IDependencyProvider provider, AssetFactoryBase assetFactory)
	{
		HashSet<FileIdentifier> fileIdentifiers = GetUnresolvedDependencies().ToHashSet();
		while(fileIdentifiers.Count > 0)
		{
			FileIdentifier identifier = fileIdentifiers.First();
			File? file = provider.FindDependency(identifier);
			file?.ReadContentsRecursively();
			while (file is CompressedFile compressedFile)
			{
				file = compressedFile.UncompressedFile;
			}
			if (file is SerializedFile serializedFile)
			{
				//Collection is added to this automatically
				SerializedAssetCollection collection = SerializedAssetCollection.FromSerializedFile(this, serializedFile, assetFactory);
				fileIdentifiers.AddRange(collection.GetUnresolvedDependencies());
			}
			else if (file is FileContainer container)
			{
				SerializedBundle bundle = SerializedBundle.FromFileContainer(container, assetFactory);
				AddBundle(bundle);
				fileIdentifiers.AddRange(bundle.GetUnresolvedDependencies());
			}
			fileIdentifiers.Remove(identifier);
		}
	}

	public static GameBundle FromPaths(IEnumerable<string> paths, AssetFactoryBase assetFactory)
	{
		GameBundle gameBundle = new();
		foreach (string path in paths)
		{
			File? file = SchemeReader.LoadFile(path);
			file?.ReadContentsRecursively();
			while (file is CompressedFile compressedFile)
			{
				file = compressedFile.UncompressedFile;
			}
			if (file is SerializedFile serializedFile)
			{
				//Collection is added to this automatically
				SerializedAssetCollection.FromSerializedFile(gameBundle, serializedFile, assetFactory);
			}
			else if (file is FileContainer container)
			{
				SerializedBundle bundle = SerializedBundle.FromFileContainer(container, assetFactory);
				gameBundle.AddBundle(bundle);
			}
			else if (file is ResourceFile resourceFile)
			{
				gameBundle.AddResource(resourceFile);
			}
		}
		return gameBundle;
	}

	public bool HasAnyAssetCollections()
	{
		return FetchAssetCollections().Any();
	}
}
public interface IDependencyProvider
{
	File? FindDependency(FileIdentifier identifier);
}
public interface IResourceProvider
{
	ResourceFile? FindResource(string identifier);
}
internal static class HashSetExtensions
{
	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> values)
	{
		foreach (T value in values)
		{
			set.Add(value);
		}
	}
}
