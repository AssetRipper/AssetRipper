using AssetRipper.Import.Structure.Assembly;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Export.UnityProjects.Scripts;

internal readonly record struct MonoScriptInfo(string Class, string Namespace, string Assembly)
{
	public static MonoScriptInfo From(IMonoScript monoScript)
	{
		return new MonoScriptInfo(monoScript.ClassName_R.String, monoScript.Namespace.String, monoScript.GetAssemblyNameFixed());
	}

	public bool IsInjected()
	{
		return Class is { Length: 0 } && Namespace is { Length: 0 } && Assembly is { Length: 0 };
	}
}
