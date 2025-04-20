using AssetRipper.GUI.Web.Dependencies;
using AssetRipper.Import;
using Microsoft.AspNetCore.Routing;

namespace AssetRipper.GUI.Web;

/// <summary>
/// Contains references to online dependencies.
/// </summary>
internal static partial class OnlineDependencies
{
	private static IReadOnlyList<EmbeddedDependency> GetDependencies()
	{
		if (AssetRipperRuntimeInformation.Build.Debug)
		{
			return EmbeddedDependency.GetAllDevelopment();
		}
		else
		{
			return EmbeddedDependency.GetAllProduction();
		}
	}

	public static void MapDependencies(IEndpointRouteBuilder endpoints)
	{
		foreach (EmbeddedDependency dependency in GetDependencies())
		{
			StaticContentLoader.Add(dependency.Path, dependency.Content);
			endpoints.MapStaticFile(dependency.Path, dependency.MimeType);
		}
	}

	// Dependencies are ordered alphabetically in this class.

	/// <summary>
	/// <see href="https://www.babylonjs.com/"/>
	/// </summary>
	internal static partial class Babylon
	{
		public const string PathMain = "/js/babylon.js";
		public const string PathLoader = "/js/babylonjs.loaders.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(PathMain).Close();
			new Script(writer).WithSrc(PathLoader).Close();
		}
	}

	/// <summary>
	/// <see href="https://getbootstrap.com/"/>
	/// </summary>
	internal static class Bootstrap
	{
		public const string PathMain = "/css/bootstrap.min.css";

		internal static void WriteStyleSheetReference(TextWriter writer)
		{
			new Link(writer).WithRel("stylesheet").WithHref(PathMain).Close();
		}

		public const string PathBundle = "/js/bootstrap.bundle.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(PathBundle).Close();
		}
	}

	/// <summary>
	/// <see href="https://jquery.com/"/>
	/// </summary>
	internal static class Popper
	{
		public const string Path = "/js/popper.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(Path).Close();
		}
	}

	/// <summary>
	/// <see href="https://vuejs.org/"/>
	/// </summary>
	internal static class Vue
	{
		public const string Path = "/js/vue.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(Path).Close();
		}
	}
}
