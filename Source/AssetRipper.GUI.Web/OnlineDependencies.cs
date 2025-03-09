using AssetRipper.Import;
using Microsoft.AspNetCore.Routing;

namespace AssetRipper.GUI.Web;

/// <summary>
/// Contains references to online dependencies.
/// </summary>
internal static class OnlineDependencies
{
	// Dependencies are ordered alphabetically in this class.

	/// <summary>
	/// <see href="https://www.babylonjs.com/"/>
	/// </summary>
	internal static class Babylon
	{
		public const string SourceMain = "https://assetripper.com/files/babylon/7.0.0/main";
		public const string SourceLoader = "https://assetripper.com/files/babylon/7.0.0/loader";
		public const string PathMain = "/js/babylon.js";
		public const string PathLoader = "/js/babylonjs.loaders.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(PathMain).Close();
			new Script(writer).WithSrc(PathLoader).Close();
		}

		internal static void Map(IEndpointRouteBuilder endpoints)
		{
			endpoints.MapRemoteFile(PathMain, "application/javascript", SourceMain);
			endpoints.MapRemoteFile(PathLoader, "application/javascript", SourceLoader);
		}
	}

	/// <summary>
	/// <see href="https://getbootstrap.com/"/>
	/// </summary>
	internal static class Bootstrap
	{
		public const string SourceMain = "https://assetripper.com/files/bootstrap/5.3.2/css";
		public const string IntegrityMain = "sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN";
		public const string PathMain = "/css/bootstrap.min.css";

		internal static void WriteStyleSheetReference(TextWriter writer)
		{
			new Link(writer)
			{
				Rel = "stylesheet",
				Href = PathMain,
				Integrity = IntegrityMain,
				CrossOrigin = "anonymous"
			}.Close();
		}

		public const string SourceBundle = "https://assetripper.com/files/bootstrap/5.3.2/js";
		public const string IntegrityBundle = "sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL";
		public const string PathBundle = "/js/bootstrap.bundle.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer)
			{
				Src = PathBundle,
				Integrity = IntegrityBundle,
				CrossOrigin = "anonymous"
			}.Close();
		}

		internal static void Map(IEndpointRouteBuilder endpoints)
		{
			endpoints.MapRemoteFile(PathMain, "text/css", SourceMain, IntegrityMain);
			endpoints.MapRemoteFile(PathBundle, "application/javascript", SourceBundle, IntegrityBundle);
		}
	}

	/// <summary>
	/// <see href="https://jquery.com/"/>
	/// </summary>
	internal static class Popper
	{
		public const string Source = "https://assetripper.com/files/popper/2.11.8/js";
		public const string Integrity = "sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r";
		public const string Path = "/js/popper.min.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer)
			{
				Src = Path,
				Integrity = Integrity,
				CrossOrigin = "anonymous"
			}.Close();
		}

		internal static void Map(IEndpointRouteBuilder endpoints)
		{
			endpoints.MapRemoteFile(Path, "application/javascript", Source, Integrity);
		}
	}

	/// <summary>
	/// <see href="https://vuejs.org/"/>
	/// </summary>
	internal static class Vue
	{
		public const string Development = "https://assetripper.com/files/vue/3.5.13/dev";
		public const string Production = "https://assetripper.com/files/vue/3.5.13/prod";
		public const string Path = "/js/vue.js";

		internal static void WriteScriptReference(TextWriter writer)
		{
			new Script(writer).WithSrc(Path).Close();
		}

		internal static void Map(IEndpointRouteBuilder endpoints)
		{
			if (AssetRipperRuntimeInformation.Build.Debug)
			{
				endpoints.MapRemoteFile(Path, "application/javascript", Development);
			}
			else
			{
				endpoints.MapRemoteFile(Path, "application/javascript", Production);
			}
		}
	}
}
