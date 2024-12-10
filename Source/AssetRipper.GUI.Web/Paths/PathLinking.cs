using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Pages.Assets;
using AssetRipper.GUI.Web.Pages.Bundles;
using AssetRipper.GUI.Web.Pages.Collections;
using AssetRipper.GUI.Web.Pages.FailedFiles;
using AssetRipper.GUI.Web.Pages.Resources;
using AssetRipper.GUI.Web.Pages.Scenes;
using System.Runtime.CompilerServices;

namespace AssetRipper.GUI.Web.Paths;

internal static class PathLinking
{
	private const string DefaultClasses = "btn btn-dark p-0 m-0";

	public static void WriteLink(TextWriter writer, Bundle bundle, string? name = null, string? @class = DefaultClasses)
	{
		WriteLink(writer, bundle.GetPath(), name is not null ? name : bundle.Name, @class);
	}

	public static void WriteLink(TextWriter writer, AssetCollection collection, string? name = null, string? @class = DefaultClasses)
	{
		WriteLink(writer, collection.GetPath(), name is not null ? name : collection.Name, @class);
	}

	public static void WriteLink(TextWriter writer, IUnityObjectBase asset, string? name = null, string? @class = DefaultClasses)
	{
		WriteLink(writer, asset.GetPath(), name is not null ? name : asset.GetBestName(), @class);
	}

	public static void WriteLink<T>(TextWriter writer, T path, string name, string? @class = DefaultClasses) where T : IPath<T>
	{
		new A(writer).WithHref(GetUrl(path)).MaybeWithClass(@class).Close(name);
	}

	private static string GetUrl<T>(T path) where T : IPath<T>
	{
		if (typeof(T) == typeof(AssetPath))
		{
			return AssetAPI.GetViewUrl(Unsafe.As<T, AssetPath>(ref path));
		}
		else if (typeof(T) == typeof(CollectionPath))
		{
			return CollectionAPI.GetViewUrl(Unsafe.As<T, CollectionPath>(ref path));
		}
		else if (typeof(T) == typeof(ScenePath))
		{
			return SceneAPI.GetViewUrl(Unsafe.As<T, ScenePath>(ref path));
		}
		else if (typeof(T) == typeof(BundlePath))
		{
			return BundleAPI.GetViewUrl(Unsafe.As<T, BundlePath>(ref path));
		}
		else if (typeof(T) == typeof(ResourcePath))
		{
			return ResourceAPI.GetViewUrl(Unsafe.As<T, ResourcePath>(ref path));
		}
		else if (typeof(T) == typeof(FailedFilePath))
		{
			return FailedFileAPI.GetViewUrl(Unsafe.As<T, FailedFilePath>(ref path));
		}
		else
		{
			return "";//Exceptions prevent inlining
		}
	}

	private static A MaybeWithClass(this A @this, string? @class)
	{
		return @class is null ? @this : @this.WithClass(@class);
	}
}
