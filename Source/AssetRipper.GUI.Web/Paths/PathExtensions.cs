using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.GUI.Web.Paths;

public static class PathExtensions
{
	public static BundlePath GetPath(this Bundle bundle)
	{
		int count = 0;
		{
			Bundle? current = bundle;
			while (current.Parent is not null)
			{
				count++;
				current = current.Parent;
			}
		}

		if (count == 0)
		{
			return new();
		}

		Span<int> path = count < 1024 ? stackalloc int[count] : new int[count];//It should never even come close to 1024.
		{
			Bundle current = bundle;
			for (int i = count - 1; ; i--)
			{
				if (current.Parent is not null)
				{
					path[i] = current.Parent.Bundles.IndexOf(current);
					current = current.Parent;
				}
				else
				{
					break;
				}
			}
		}
		return new BundlePath(path);
	}

	public static CollectionPath GetPath(this AssetCollection collection)
	{
		return new CollectionPath(collection.Bundle.GetPath(), collection.Bundle.Collections.IndexOf(collection));
	}

	public static AssetPath GetPath(this IUnityObjectBase asset)
	{
		return new AssetPath(asset.Collection.GetPath(), asset.PathID);
	}

	private static int IndexOf<T>(this IReadOnlyList<T> list, T item)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(list[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public static Bundle? TryGetBundle(this GameBundle gameBundle, BundlePath path)
	{
		Bundle current = gameBundle;
		foreach (int index in (ReadOnlySpan<int>)path)
		{
			if (index < 0 || index >= current.Bundles.Count)
			{
				return null;
			}
			current = current.Bundles[index];
		}
		return current;
	}

	public static bool TryGetBundle(this GameBundle gameBundle, BundlePath path, [NotNullWhen(true)] out Bundle? bundle)
	{
		bundle = gameBundle.TryGetBundle(path);
		return bundle is not null;
	}

	public static AssetCollection? TryGetCollection(this GameBundle gameBundle, CollectionPath path)
	{
		Bundle? bundle = gameBundle.TryGetBundle(path.BundlePath);
		if (bundle is null || path.Index < 0 || path.Index >= bundle.Collections.Count)
		{
			return null;
		}
		return bundle.Collections[path.Index];
	}

	public static bool TryGetCollection(this GameBundle gameBundle, CollectionPath path, [NotNullWhen(true)] out AssetCollection? collection)
	{
		collection = gameBundle.TryGetCollection(path);
		return collection is not null;
	}

	public static ResourceFile? TryGetResource(this GameBundle gameBundle, ResourcePath path)
	{
		Bundle? bundle = gameBundle.TryGetBundle(path.BundlePath);
		if (bundle is null || path.Index < 0 || path.Index >= bundle.Resources.Count)
		{
			return null;
		}
		return bundle.Resources[path.Index];
	}

	public static bool TryGetResource(this GameBundle gameBundle, ResourcePath path, [NotNullWhen(true)] out ResourceFile? resource)
	{
		resource = gameBundle.TryGetResource(path);
		return resource is not null;
	}

	public static FailedFile? TryGetFailedFile(this GameBundle gameBundle, FailedFilePath path)
	{
		Bundle? bundle = gameBundle.TryGetBundle(path.BundlePath);
		if (bundle is null || path.Index < 0 || path.Index >= bundle.FailedFiles.Count)
		{
			return null;
		}
		return bundle.FailedFiles[path.Index];
	}

	public static bool TryGetFailedFile(this GameBundle gameBundle, FailedFilePath path, [NotNullWhen(true)] out FailedFile? failedFile)
	{
		failedFile = gameBundle.TryGetFailedFile(path);
		return failedFile is not null;
	}

	public static IUnityObjectBase? TryGetAsset(this GameBundle gameBundle, AssetPath path)
	{
		AssetCollection? collection = gameBundle.TryGetCollection(path.CollectionPath);
		if (collection is null)
		{
			return null;
		}
		// Can't use TryGetAsset because that returns null for NullObject objects.
		return collection.Assets.TryGetValue(path.PathID, out IUnityObjectBase? asset) ? asset : null;
	}

	public static bool TryGetAsset(this GameBundle gameBundle, AssetPath path, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		asset = gameBundle.TryGetAsset(path);
		return asset is not null;
	}
}
