using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Structure.Assembly;
using Mono.Cecil;

namespace AssetRipper.Core.Classes
{
	public interface IMonoScript : IUnityObjectBase
	{
		string ClassName { get; }
		string Namespace { get; }
		/// <summary>
		/// AssemblyIdentifier previously; Currently, it is being fixed in the MonoScript Read method
		/// </summary>
		string AssemblyName { get; }
	}

	public static class IMonoScriptExtensions
	{
		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, bool includeNamespace)
		{
			bool useNamespace = includeNamespace && monoScript.Namespace != null;
			return useNamespace ? monoScript.File.Collection.AssemblyManager.GetScriptID(monoScript.AssemblyName, monoScript.Namespace, monoScript.ClassName)
				: monoScript.File.Collection.AssemblyManager.GetScriptID(monoScript.AssemblyName, monoScript.ClassName);
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript) => monoScript.GetScriptID(false);

		public static TypeDefinition GetTypeDefinition(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(true);
			return monoScript.File.Collection.AssemblyManager.GetTypeDefinition(scriptID);
		}

		public static bool IsScriptPresents(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(true);
			return monoScript.File.Collection.AssemblyManager.IsPresent(scriptID);
		}
	}
}
