using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Endian;
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
		/// Before 2018.2 or Release or after 2022.2
		/// </summary>
		public static bool HasAssemblyName(UnityVersion version, TransferInstructionFlags flags)
		{
			return flags.IsRelease() || version.IsLess(2018, 2) || version.IsGreaterEqual(2022, 2);
		}

		/// <summary>
		/// Before 2018.2 or Release or after 2022.2
		/// </summary>
		public static bool HasAssemblyName(this IMonoScript monoScript)
		{
			return HasAssemblyName(monoScript.Collection.Version, monoScript.Collection.Flags);
		}

		public static string GetValidAssemblyName(this IMonoScript monoScript)
		{
			string name = FilenameUtils.FixAssemblyName(monoScript.AssemblyName_C115);
			return string.IsNullOrEmpty(name) ? "Assembly-CSharp" : name;
		}

		/// <summary>
		/// Apply <see cref="FilenameUtils.FixAssemblyName(string)"/> to <see cref="IMonoScript.AssemblyName_C115"/>.
		/// </summary>
		/// <param name="monoScript">The relevant MonoScript.</param>
		/// <returns></returns>
		public static string GetAssemblyNameFixed(this IMonoScript monoScript)
		{
			return FilenameUtils.FixAssemblyName(monoScript.AssemblyName_C115);
		}

		public static SerializableType? GetBehaviourType(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115, monoScript.ClassName_C115);
			if (assemblyManager.IsValid(scriptID))
			{
				return assemblyManager.GetSerializableType(scriptID, monoScript.Collection.Version);
			}
			return null;
		}

		public static string GetFullName(this IMonoScript monoScript)
		{
			if (string.IsNullOrEmpty(monoScript.Namespace_C115))
			{
				return monoScript.ClassName_C115;
			}
			else
			{
				return $"{monoScript.Namespace_C115}.{monoScript.ClassName_C115}";
			}
		}

		public static ScriptIdentifier GetScriptID(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			return assemblyManager.GetScriptID(monoScript.GetAssemblyNameFixed(), monoScript.Namespace_C115, monoScript.ClassName_C115);
		}

		public static TypeDefinition GetTypeDefinition(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(assemblyManager);
			return assemblyManager.GetTypeDefinition(scriptID);
		}

		public static bool IsScriptPresents(this IMonoScript monoScript, IAssemblyManager assemblyManager)
		{
			ScriptIdentifier scriptID = monoScript.GetScriptID(assemblyManager);
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
				//I have reason to believe that this depends on the endianness of the file containing the MonoScript,
				//but most platforms are little endian, so I don't really know for sure.
				if (monoScript.Collection.EndianType == EndianType.BigEndian)
				{
					BinaryPrimitives.WriteUInt32BigEndian(hash, monoScript.PropertiesHash_C115_UInt32);
				}
				else
				{
					BinaryPrimitives.WriteUInt32LittleEndian(hash, monoScript.PropertiesHash_C115_UInt32);
				}
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
