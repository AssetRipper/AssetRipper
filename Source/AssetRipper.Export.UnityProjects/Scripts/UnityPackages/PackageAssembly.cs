namespace AssetRipper.Export.UnityProjects.Scripts.UnityPackages;

/// <summary>
/// An assembly defined by a Unity package, with the guids of its script files.
/// </summary>
public sealed class PackageAssembly(string assemblyName, string packageName, string packageVersion)
{
	private readonly Dictionary<(string Namespace, string Class), UnityGuid> scripts = new();
	private readonly Dictionary<string, UnityGuid?> scriptsByClassName = new();

	public string AssemblyName { get; } = assemblyName;
	public string PackageName { get; } = packageName;
	public string PackageVersion { get; } = packageVersion;
	public int ScriptCount => scripts.Count;

	private readonly HashSet<string> declaredTypes = new();

	/// <summary>
	/// The full names of the types declared in this assembly's source files.
	/// </summary>
	public IReadOnlyCollection<string> DeclaredTypes => declaredTypes;

	internal void AddDeclaredType(string fullName) => declaredTypes.Add(fullName);

	internal void AddScript(string @namespace, string className, UnityGuid guid)
	{
		scripts.TryAdd((@namespace, className), guid);
		// A class name shared by several namespaces is ambiguous, so it can't be used as a fallback.
		scriptsByClassName[className] = scriptsByClassName.ContainsKey(className) ? null : guid;
	}

	/// <summary>
	/// Get the guid of the script file for a type.
	/// </summary>
	/// <remarks>
	/// If no exact match exists, a type with the same name in another namespace is accepted when it is unique.
	/// This covers types that were moved between namespaces in newer versions of the package.
	/// </remarks>
	public bool TryGetScriptGuid(string @namespace, string className, out UnityGuid guid)
	{
		if (scripts.TryGetValue((@namespace, className), out guid))
		{
			return true;
		}
		if (scriptsByClassName.TryGetValue(className, out UnityGuid? fallback) && fallback.HasValue)
		{
			guid = fallback.Value;
			return true;
		}
		guid = default;
		return false;
	}
}
