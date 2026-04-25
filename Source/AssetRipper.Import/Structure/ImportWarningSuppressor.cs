using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure;

internal static class ImportWarningSuppressor
{
	public static bool IsIgnorableMissingAssembly(string assemblyName)
	{
		return assemblyName is "UnityEngine.TestRunner"
			or "Oculus.VR.Editor"
			|| assemblyName.EndsWith(".Editor", StringComparison.Ordinal)
			|| assemblyName.Contains(".Editor.", StringComparison.Ordinal);
	}

	public static bool IsIgnorableMissingResource(string resourceName)
	{
		string normalized = resourceName.Replace('\\', '/');
		return normalized is "UnityEngine.TestRunner.dll"
			or "Oculus.VR.Editor.dll"
			|| normalized.EndsWith(".Editor.dll", StringComparison.Ordinal)
			|| normalized.Contains(".Editor.", StringComparison.Ordinal);
	}

	public static bool IsIgnorableInvalidScript(IMonoScript? monoScript)
	{
		if (monoScript is null)
		{
			return false;
		}

		string assemblyName = monoScript.GetAssemblyNameFixed();
		if (IsIgnorableMissingAssembly(assemblyName))
		{
			return true;
		}

		string fullName = monoScript.GetFullName();
		return fullName is "OVRConfig"
			or "Assets.Oculus.VR.Editor.OVRPlatformToolSettings"
			or "UnityEngine.GUISkin";
	}

	public static bool IsIgnorableLossyManagedReferenceFallback(string? fullName)
	{
		return fullName is "Fusion.NetworkProjectConfigAsset";
	}
}
