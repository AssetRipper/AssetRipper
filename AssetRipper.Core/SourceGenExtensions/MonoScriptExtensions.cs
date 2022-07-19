using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using Mono.Cecil;

namespace AssetRipper.Core.SourceGenExtensions
{
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
			return FilenameUtils.FixAssemblyName(monoScript.AssemblyName_C115.String);
		}

		public static SerializableType? GetBehaviourType(this IMonoScript monoScript)
		{
			ScriptIdentifier scriptID = HasNamespace(monoScript.SerializedFile.Version) ?
				monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115.String, monoScript.ClassName_C115.String) :
				monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName_C115.String);
			if (monoScript.SerializedFile.Collection.AssemblyManager.IsValid(scriptID))
			{
				return monoScript.SerializedFile.Collection.AssemblyManager.GetSerializableType(scriptID);
			}
			return null;
		}

		public static string GetFullName(this IMonoScript monoScript)
		{
			if (string.IsNullOrEmpty(monoScript.Namespace_C115.String))
			{
				return monoScript.ClassName_C115.String;
			}
			else
			{
				return $"{monoScript.Namespace_C115.String}.{monoScript.ClassName_C115.String}";
			}
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, bool includeNamespace)
		{
			bool useNamespace = includeNamespace; //&& monoScript.Namespace_C115 is not null;
			return useNamespace ? monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115.String, monoScript.ClassName_C115.String)
				: monoScript.SerializedFile.Collection.AssemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName_C115.String);
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

		public static Hash128 GetPropertiesHash(this IMonoScript monoScript)
		{
			return monoScript.Has_PropertiesHash_C115_Hash128()
				? (Hash128)monoScript.PropertiesHash_C115_Hash128
				: new Hash128(monoScript.PropertiesHash_C115_UInt32);
		}
	}
}
