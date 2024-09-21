using AssetRipper.Import;

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
		internal static void WriteScriptReference(TextWriter writer)
		{
			if (!TryWriteCachedScriptReference(writer, "/js/babylon.js"))
			{
				new Script(writer)
				{
					Src = "https://cdn.babylonjs.com/babylon.js"
				}.Close();
			}
			if (!TryWriteCachedScriptReference(writer, "/js/babylonjs.loaders.min.js"))
			{
				new Script(writer)
				{
					Src = "https://cdn.babylonjs.com/loaders/babylonjs.loaders.min.js"
				}.Close();
			}
		}
	}

	/// <summary>
	/// <see href="https://getbootstrap.com/"/>
	/// </summary>
	internal static class Bootstrap
	{
		internal static void WriteStyleSheetReference(TextWriter writer)
		{
			if (!TryWriteCachedStyleSheetReference(writer, "/css/bootstrap.min.css"))
			{
				new Link(writer)
				{
					Rel = "stylesheet",
					Href = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css",
					Integrity = "sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN",
					CrossOrigin = "anonymous"
				}.Close();
			}
		}

		internal static void WriteScriptReference(TextWriter writer)
		{
			if (!TryWriteCachedScriptReference(writer, "/js/bootstrap.bundle.min.js"))
			{
				new Script(writer)
				{
					Src = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js",
					Integrity = "sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL",
					CrossOrigin = "anonymous"
				}.Close();
			}
		}
	}

	/// <summary>
	/// <see href="https://jquery.com/"/>
	/// </summary>
	internal static class Popper
	{
		internal static void WriteScriptReference(TextWriter writer)
		{
			if (!TryWriteCachedScriptReference(writer, "/js/popper.min.js"))
			{
				new Script(writer)
				{
					Src = "https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.min.js",
					Integrity = "sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r",
					CrossOrigin = "anonymous"
				}.Close();
			}
		}
	}

	/// <summary>
	/// <see href="https://vuejs.org/"/>
	/// </summary>
	internal static class Vue
	{
		internal static void WriteScriptReference(TextWriter writer)
		{
			if (!TryWriteCachedScriptReference(writer, "/js/vue.global.prod.js") && !TryWriteCachedScriptReference(writer, "/js/vue.global.js"))
			{
				if (AssetRipperRuntimeInformation.Build.Debug)
				{
					new Script(writer)
					{
						Src = "https://unpkg.com/vue@3/dist/vue.global.js"
					}.Close();
				}
				else
				{
					new Script(writer)
					{
						Src = "https://unpkg.com/vue@3/dist/vue.global.prod.js"
					}.Close();
				}
			}
		}
	}

	private static bool TryWriteCachedStyleSheetReference(TextWriter writer, string path)
	{
		if (StaticContentLoader.Cache.ContainsKey(path))
		{
			new Link(writer)
			{
				Rel = "stylesheet",
				Href = path
			}.Close();
			return true;
		}
		return false;
	}

	private static bool TryWriteCachedScriptReference(TextWriter writer, string path)
	{
		if (StaticContentLoader.Cache.ContainsKey(path))
		{
			new Script(writer)
			{
				Src = path
			}.Close();
			return true;
		}
		return false;
	}
}
