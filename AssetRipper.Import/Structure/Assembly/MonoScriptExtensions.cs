using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Subclasses.Hash128;
using System.Buffers.Binary;

namespace AssetRipper.Import.Structure.Assembly
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

		public static SerializableType? GetBehaviourType(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = HasNamespace(monoScript.Collection.Version) ?
				assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115.String, monoScript.ClassName_C115.String) :
				assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName_C115.String);
			if (assemblyManager.IsValid(scriptID))
			{
				return assemblyManager.GetSerializableType(scriptID, monoScript.Collection.Version);
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

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, IAssemblyManager assemblyManager, bool includeNamespace)
		{
			bool useNamespace = includeNamespace; //&& monoScript.Namespace_C115 is not null;
			return useNamespace ? assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115.String, monoScript.ClassName_C115.String)
				: assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.ClassName_C115.String);
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			return monoScript.GetScriptID(assemblyManager, false);
		}

		public static TypeDefinition GetTypeDefinition(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(assemblyManager, true);
			return assemblyManager.GetTypeDefinition(scriptID);
		}

		public static bool IsScriptPresents(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(assemblyManager, true);
			return assemblyManager.IsPresent(scriptID);
		}

		public static Hash128 GetPropertiesHash(this IMonoScript monoScript)
		{
			if (monoScript.Has_PropertiesHash_C115_Hash128())
			{
				return monoScript.PropertiesHash_C115_Hash128;
			}
			else
			{
				Span<byte> hash = stackalloc byte[4];
				BinaryPrimitives.WriteUInt32LittleEndian(hash, monoScript.PropertiesHash_C115_UInt32);
				//I have reason to believe that this depends on the endianness of the file containing the MonoScript
				//Might need to have a special case for big endian files, so that the hash matches SerializedType
				return new()
				{
					Bytes__0 = hash[0],
					Bytes__1 = hash[1],
					Bytes__2 = hash[2],
					Bytes__3 = hash[3],
				};
			}
		}
	}
}
