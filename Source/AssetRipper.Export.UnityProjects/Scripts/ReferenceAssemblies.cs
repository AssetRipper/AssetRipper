using AssetRipper.Import.Structure.Assembly.Managers;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static class ReferenceAssemblies
{
	private static readonly AssemblyDataFile assemblyDataFile = AssemblyDataFile.Load();
	private static UnityGuid UnityEngineGUID => new UnityGuid(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

	public static bool IsPredefinedAssembly(string assemblyName)
	{
		return assemblyName
			is "Assembly-CSharp"
			or "Assembly-CSharp-firstpass"
			or "Assembly-CSharp-Editor"
			or "Assembly-CSharp-Editor-firstpass"
			or "Assembly-UnityScript"
			or "Assembly-UnityScript-firstpass";
	}

	public static Dictionary<string, UnityGuid> GetReferenceAssemblies(IAssemblyManager assemblyManager, UnityVersion version)
	{
		Debug.Assert(assemblyDataFile.Assemblies.Count > 0);

		AssemblyData assemblyData = assemblyDataFile.Get(version);

		Dictionary<string, UnityGuid> referenceAssemblies = [];
		foreach ((string assembly, UnityGuid guid) in assemblyData.UnityExtensions)
		{
			referenceAssemblies.Add(assembly, guid);
		}
		foreach (string assembly in GetMonoAssemblies(assemblyManager, assemblyData))
		{
			referenceAssemblies.TryAdd(assembly, UnityEngineGUID);
		}
		foreach (string assembly in assemblyData.Unity)
		{
			referenceAssemblies.TryAdd(assembly, UnityEngineGUID);
		}

		// Todo: investigate why 5.4.0a0 does not have UnityEngine
		referenceAssemblies.TryAdd("UnityEngine", UnityEngineGUID);

		return referenceAssemblies;

		static IReadOnlyList<string> GetMonoAssemblies(IAssemblyManager assemblyManager, AssemblyData assemblyData)
		{
			if (assemblyManager.HasMscorlib2)
			{
				return assemblyData.Mono2.Count > 0 ? assemblyData.Mono2 : assemblyData.Mono4;
			}
			else
			{
				return assemblyData.Mono4.Count > 0 ? assemblyData.Mono4 : assemblyData.Mono2;
			}
		}
	}
}
