using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Serializable;
using Mono.Cecil;

namespace AssetRipper.Core.Classes
{
	public interface IMonoScript : IUnityObjectBase, IHasName
	{
		string ClassName { get; }
		string Namespace { get; }
		/// <summary>
		/// AssemblyIdentifier previously; Currently, it is being fixed in the MonoScript Read method
		/// </summary>
		string AssemblyName { get; }
		int ExecutionOrder { get; }
		Hash128 PropertiesHash { get; }
	}

	public static class MonoScriptExtensions
	{
		/*
		public static bool HasAssemblyName(this IMonoScript monoScript) => !string.IsNullOrEmpty(monoScript.AssemblyName);

		public static bool HasNamespace(this IMonoScript monoScript) => !string.IsNullOrEmpty(monoScript.Namespace);

		public static string GetValidAssemblyName(this IMonoScript monoScript)
		{
			string name = FilenameUtils.FixAssemblyName(monoScript.AssemblyName);
			return string.IsNullOrEmpty(name) ? "Assembly-CSharp" : name;
		}
		*/

		public static SerializableType GetBehaviourType(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = HasNamespace(monoScript.File.Version) ?
				monoScript.File.Collection.AssemblyManager.GetScriptID(monoScript.AssemblyName, monoScript.Namespace, monoScript.ClassName) :
				monoScript.File.Collection.AssemblyManager.GetScriptID(monoScript.AssemblyName, monoScript.ClassName);
			if (monoScript.File.Collection.AssemblyManager.IsValid(scriptID))
			{
				return monoScript.File.Collection.AssemblyManager.GetSerializableType(scriptID) as SerializableType;
			}
			return null;
		}

		public static string GetFullName(this IMonoScript monoScript)
		{
			if(string.IsNullOrEmpty(monoScript.Namespace))
				return new string(monoScript.ClassName);
			else
				return $"{monoScript.Namespace}.{monoScript.ClassName}";
		}

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

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool HasNamespace(UnityVersion version) => version.IsGreaterEqual(3);
	}
}
