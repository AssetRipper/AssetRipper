using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;

namespace AssetRipper.GUI.Web.Paths;

internal static class PathLinking
{
	public const string FormKey = "Path";
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
		WriteLink(writer, GetUrl<T>(), path.ToJson(), name, @class);
	}

	private static void WriteLink(TextWriter writer, string url, string json, string name, string? @class)
	{
		using (new Form(writer).WithAction(url).WithMethod("post").End())
		{
			new Input(writer).WithType("hidden").WithName(FormKey).WithValue(json.ToHtml()).Close();
			new Input(writer).WithType("submit").MaybeWithClass(@class).WithValue(name.ToHtml()).Close();
		}
	}

	private static string GetUrl<T>() where T : IPath<T>
	{
		if (typeof(T) == typeof(AssetPath))
		{
			return "/Assets/View";
		}
		else if (typeof(T) == typeof(CollectionPath))
		{
			return "/Collections/View";
		}
		else if (typeof(T) == typeof(ScenePath))
		{
			return "/Scenes/View";
		}
		else if (typeof(T) == typeof(BundlePath))
		{
			return "/Bundles/View";
		}
		else if (typeof(T) == typeof(ResourcePath))
		{
			return "/Resources/View";
		}
		else
		{
			return "";//Exceptions prevent inlining
		}
	}

	private static Input MaybeWithClass(this Input @this, string? @class)
	{
		return @class is null ? @this : @this.WithClass(@class);
	}
}
