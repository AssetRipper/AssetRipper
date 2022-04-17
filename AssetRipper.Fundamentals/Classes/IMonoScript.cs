using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Serializable;
using Mono.Cecil;

namespace AssetRipper.Core.Classes
{
	public interface IMonoScript : IUnityObjectBase, IHasName
	{
		string ClassName { get; set; }
		string Namespace { get; set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		string AssemblyName { get; set; }
		int ExecutionOrder { get; set; }
		Hash128 PropertiesHash { get; }
	}

	public static class MonoScriptExtensions
	{
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasNamespace(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Less than 2018.1.2 or Release
		/// </summary>
		public static bool HasAssemblyName(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(2018, 1, 2);
		/*
		public static bool HasAssemblyName(this IMonoScript monoScript) => !string.IsNullOrEmpty(monoScript.AssemblyName);

		public static bool HasNamespace(this IMonoScript monoScript) => !string.IsNullOrEmpty(monoScript.Namespace);

		public static string GetValidAssemblyName(this IMonoScript monoScript)
		{
			string name = FilenameUtils.FixAssemblyName(monoScript.AssemblyName);
			return string.IsNullOrEmpty(name) ? "Assembly-CSharp" : name;
		}
		*/

		public static string GetAssemblyNameFixed(this IMonoScript monoScript)
		{
			return FilenameUtils.FixAssemblyName(monoScript.AssemblyName);
		}

		public static SerializableType GetBehaviourType(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = HasNamespace(monoScript.SerializedFile.Version) ?
				monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace, monoScript.ClassName) :
				monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName);
			if (monoScript.SerializedFile.Collection.AssemblyManager.IsValid(scriptID))
			{
				return monoScript.SerializedFile.Collection.AssemblyManager.GetSerializableType(scriptID) as SerializableType;
			}
			return null;
		}

		public static string GetFullName(this IMonoScript monoScript)
		{
			if (string.IsNullOrEmpty(monoScript.Namespace))
				return new string(monoScript.ClassName);
			else
				return $"{monoScript.Namespace}.{monoScript.ClassName}";
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, bool includeNamespace)
		{
			bool useNamespace = includeNamespace && monoScript.Namespace != null;
			return useNamespace ? monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace, monoScript.ClassName)
				: monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName);
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript) => monoScript.GetScriptID(false);

		public static TypeDefinition GetTypeDefinition(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(true);
			return monoScript.SerializedFile.Collection.AssemblyManager.GetTypeDefinition(scriptID);
		}

		public static bool IsScriptPresents(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(true);
			return monoScript.SerializedFile.Collection.AssemblyManager.IsPresent(scriptID);
		}
	}
}
