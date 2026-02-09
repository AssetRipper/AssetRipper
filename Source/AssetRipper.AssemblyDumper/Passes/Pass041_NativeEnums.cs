using AssetRipper.AssemblyDumper.Documentation;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.DocExtraction.MetaData;
using System.Text;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass041_NativeEnums
{
	private static readonly Dictionary<string, string> fullNamePrefixToNamespace = new()
	{
		{ "fbxsdk", "Fbx" },
		{ "std", "Std" },
		{ "crn", "Images.Crunch" },
		{ "D2D", "Rendering.DirectX" },
		{ "D3D", "Rendering.DirectX" },
		{ "DXGI", "Rendering.DirectX" },
		{ "cuda", "Rendering.Cuda" },
		{ "FMOD", "Fmod" },
		{ "FLAC", "Flac" },
		{ "Umbra", "Umbra" },
		{ "asio", "Asio" },
		{ "curl", "Networking.Curl" },
		{ "Curl", "Networking.Curl" },
		{ "CURL", "Networking.Curl" },
		{ "FWP", "Fwp" },
		{ "Gfx", "Gfx" },
		{ "gl", "GL" },
		{ "Graphine", "Graphine" },
		{ "physx", "PhysX" },
		{ "SpeedTree", "SpeedTree" },
		{ "tetgen", "TetGen" },
		{ "TextCore", "TextCore" },
		{ "unity.google.protobuf", "ProtoBuf" },
		{ "vk", "Vk" },
		{ "Vk", "Vk" },
		{ "vpx", "VPX" },
		{ "VPX", "VPX" },
		{ "yaml", "Yaml" },
		{ "UnityYAML", "Yaml" },
		{ "zlib", "Compression.ZLib" },
		{ "Wintermute", "Wintermute" },
		{ "unitytls", "TLS" },
		{ "mbedtls", "TLS" },
		{ "tag", "Tags" },
		{ "Shader", "Shaders" },
		{ "smolv", "Shaders" },
		{ "Collab", "Networking.Collab" },
		{ "RestService", "Networking" },
		{ "Unity.Http", "Networking" },
		{ "UnityConnect", "Networking" },
		{ "UnityCurl", "Networking.Curl" },
		{ "CacheServer", "Networking" },
		{ "ssl", "Networking.SSL" },
		{ "SSL", "Networking.SSL" },
		{ "pb", "Networking.PB" },
		{ "PB", "Networking.PB" },
		{ "Suite", "Suite" },
		{ "pubnub", "PubNub" },
		{ "profiling", "Profiling" },
		{ "Profiler", "Profiling" },
		{ "PlayerLoopCallbacks", "PlayerLoopCallbacks" },
		{ "mecanim", "Animation" },
		{ "Animation", "Animation" },
		{ "Animator", "Animation" },
		{ "lws", "LWS" },
		{ "Log", "Logging" },
		{ "JPEG", "Images.Jpeg" },
		{ "jpgd", "Images.Jpeg" },
		{ "FREE_IMAGE", "Images.FreeImage" },
		{ "freeimage", "Images.FreeImage" },
		{ "etccompress", "Images.Etc" },
		{ "EXR", "Images.Exr" },
		{ "astc", "Images.Astc" },
		{ "Job", "Jobs" },
		{ "JOB", "Jobs" },
		{ "BakeEnlighten", "Jobs.Baking" },
		{ "BakeJob", "Jobs.Baking" },
		{ "IPSEC", "Security" },
		{ "IKEEXT", "Security" },
		{ "Geo", "Geo" },
		{ "Enlighten", "Rendering.Enlighten" },
		{ "CreateEnlighten", "Rendering.Enlighten" },
		{ "baselib.UnityClassic", "UnityClassic" },
		{ "UnityClassic.Baselib", "UnityClassic" },
		{ "ArchiveStorage", "AssetBundles" },
		{ "`anonymous-namespace'", "Anonymous" },
		{ "MIDL", "MIDL" },
		{ "RadeonRays", "RadeonRays" },
		{ "radeonRays", "RadeonRays" },
		{ "UnityEngine.Analytics", "Networking.Analytics" },
	};

	private static readonly HashSet<string> vagueNames = new()
	{
		"Type", "type",
		"State", "state",
		"Event", "event", "EventType",
		"Code", "code",
		"Flag", "flag",
		"Flags", "flags", "FLAGS",
		"Level", "level",
		"Bits", "bits",
		"Tag", "tag",
		"Tiling", "tiling",
		"Shade", "shade",
		"Mode", "mode", "MODE",
		"Enum", "enum",
		"EType",
		"Error", "ErrorType", "ErrorFlags",
		"FailureCodes",
	};

	public static void DoPass()
	{
		DocumentationFile file = DocumentationFile.FromFile("native_enums.json");
		//MakeEnumsAsIs(file);
		MakeCleanEnums(file);
	}

	private static void MakeEnumsAsIs(DocumentationFile file)
	{
		foreach (EnumDocumentation enumDoc in file.Enums)
		{
			TypeDefinition type = EnumCreator.CreateFromDictionary(
				SharedState.Instance,
				SharedState.NativeEnumsNamespace,
				enumDoc.FullName.Replace('.', '_'),
				enumDoc.Members.Values.Select(member => new KeyValuePair<string, long>(member.Name, member.Value)),
				enumDoc.ElementType.ToEnumUnderlyingType());
			DocumentationHandler.AddTypeDefinitionLine(type, $"Full Name: {enumDoc.FullName}");
		}
	}

	private static void MakeCleanEnums(DocumentationFile file)
	{
		IMethodDefOrRef flagsConstructor = SharedState.Instance.Importer.ImportDefaultConstructor<FlagsAttribute>();
		Dictionary<string, Dictionary<EnumDocumentation, string>> enumDictionary = new();
		foreach (EnumDocumentation enumDoc in file.Enums)
		{
			string fullName = RemoveLeadingUnderscores(enumDoc.FullName);
			string name = GetNameNotVague(fullName, RemoveLeadingUnderscores(enumDoc.Name))
				.RemoveTrailingSuffix()
				.TryMakePascalCase()
				.FixBeginning();
			string @namespace = $"{SharedState.NativeEnumsNamespace}.{GetSubnamespace(fullName)}";

			enumDictionary.GetOrAdd(@namespace).Add(enumDoc, name);
		}
		foreach ((string @namespace, Dictionary<EnumDocumentation, string> nameDictionary) in enumDictionary)
		{
			Dictionary<string, int> conflicts = GetDuplicates(nameDictionary.Values).ToDictionary(key => key, key => 0);
			foreach ((EnumDocumentation enumDoc, string name) in nameDictionary)
			{
				EnumUnderlyingType enumUnderlyingType = enumDoc.ElementType.ToEnumUnderlyingType();

				string nonConflictingTypeName;
				if (conflicts.TryGetValue(name, out int typeNameIndex))
				{
					nonConflictingTypeName = $"{name}_{typeNameIndex}";
					conflicts[name] = typeNameIndex + 1;
				}
				else
				{
					nonConflictingTypeName = name;
				}

				TypeDefinition type = EnumCreator.CreateEmptyEnum(SharedState.Instance, @namespace, nonConflictingTypeName, enumUnderlyingType);
				if (name.Contains("flags", StringComparison.OrdinalIgnoreCase) || name.Contains("mask", StringComparison.OrdinalIgnoreCase))
				{
					type.AddCustomAttribute(flagsConstructor);
				}
				string memberPrefix = enumDoc.Members.Count > 1 ? GetSharedPrefix(enumDoc.Members.Keys) : "";
				Dictionary<EnumMemberDocumentation, string> members = new();
				HashSet<(string, long)> distinctFields = new();
				foreach (EnumMemberDocumentation enumMember in enumDoc.Members.Values)
				{
					string memberName = enumMember.Name
						.RemovePrefix(memberPrefix)
						.RemoveLeadingUnderscores()
						.RemoveTrailingSuffix()
						.TryMakePascalCase();
					if (memberName.Length == 0)
					{
						memberName = $"EnumField_{GetStringForLong(enumMember.Value)}";
					}
					if (distinctFields.Add((memberName, enumMember.Value)))
					{
						members.Add(enumMember, memberName);
					}
				}
				HashSet<string> memberConflicts = GetDuplicates(members.Values);
				foreach ((EnumMemberDocumentation enumMember, string memberName) in members.OrderBy(pair => pair.Key.Value))
				{
					string nonConflictingName = memberConflicts.Contains(memberName)
						? $"{memberName}_{GetStringForLong(enumMember.Value)}"
						: memberName;
					FieldDefinition field = type.AddEnumField(nonConflictingName, enumMember.Value, enumUnderlyingType);
					if (memberName != enumMember.Name)
					{
						DocumentationHandler.AddFieldDefinitionLine(field, $"Original name: {enumMember.Name}");
					}
				}
				DocumentationHandler.AddTypeDefinitionLine(type, $"Full Name: {enumDoc.FullName}");
			}
		}
		Console.WriteLine($"\t{file.Enums.Count} generated enums.");
	}

	private static HashSet<T> GetDuplicates<T>(IEnumerable<T> values)
	{
		HashSet<T> distinctValues = new();
		HashSet<T> duplicateValues = new();
		foreach (T value in values)
		{
			if (!distinctValues.Add(value))
			{
				duplicateValues.Add(value);
			}
		}
		return duplicateValues;
	}

	private static string GetStringForLong(long value)
	{
		return value >= 0 ? value.ToString() : $"N{value.ToString().AsSpan(1)}";
	}

	private static string GetSubnamespace(string fullName)
	{
		foreach ((string prefix, string prefixNamespace) in fullNamePrefixToNamespace)
		{
			if (fullName.StartsWith(prefix, StringComparison.Ordinal))
			{
				return prefixNamespace;
			}
		}

		return "Global";
	}

	private static string TryMakePascalCase(this string str)
	{
		if (str.IsAllUpperCaseAndUnderScores() || str.IsAllLowerCaseAndUnderScores())
		{
			return str.ConvertToPascalCase().FixBeginning();
		}
		else
		{
			return str.RemoveUnderscores().FixBeginning();
		}
	}

	private static string GetNameNotVague(string fullName, string name)
	{
		if (vagueNames.Contains(name))
		{
			string[] fullNameSplit = fullName.Split('.');
			if (fullNameSplit.Length < 2)
			{
				return name;
			}
			else
			{
				string newName = $"{fullNameSplit[fullNameSplit.Length - 2]}_{fullNameSplit[fullNameSplit.Length - 1]}";
				return RemoveLeadingUnderscores(newName);
			}
		}
		else
		{
			return name;
		}
	}

	private static string GetSharedPrefix(IEnumerable<string> names)
	{
		string? result = null;
		foreach (string name in names)
		{
			result = result is null ? name : GetSharedPrefix(result, name);
		}
		return result ?? "";
	}

	private static string GetSharedPrefix(string str1, string str2)
	{
		int i = 0;
		for (; i < str1.Length && i < str2.Length; i++)
		{
			if (str1[i] != str2[i])
			{
				break;
			}
		}
		return str1.Substring(0, i);
	}

	private static string RemovePrefix(this string str, string prefix)
	{
		return str.Substring(prefix.Length);
	}

	private static bool IsAllUpperCaseAndUnderScores(this string str)
	{
		return str.All(c => c == '_' || char.IsUpper(c) || char.IsDigit(c));
	}

	private static bool IsAllLowerCaseAndUnderScores(this string str)
	{
		return str.All(c => c == '_' || char.IsLower(c) || char.IsDigit(c));
	}

	private static string ConvertToPascalCase(this string str)
	{
		StringBuilder sb = new StringBuilder();
		bool nextCharacterIsUpperCase = true;
		foreach (char c in str)
		{
			if (c == '_')
			{
				nextCharacterIsUpperCase = true;
			}
			else if (nextCharacterIsUpperCase)
			{
				sb.Append(char.ToUpper(c));
				nextCharacterIsUpperCase = false;
			}
			else
			{
				sb.Append(char.ToLower(c));
			}
		}
		return sb.ToString();
	}

	private static string RemoveUnderscores(this string str)
	{
		if (!str.Contains('_'))
		{
			return str;
		}
		else
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in str)
			{
				if (c != '_')
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
	}

	private static string RemoveLeadingUnderscores(this string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] != '_')
			{
				return str.Substring(i);
			}
		}
		return str;
	}

	private static string RemoveTrailingSuffix(this string str)
	{
		if (str.EndsWith('_'))
		{
			return str.Substring(0, str.Length - 1);
		}
		else if (str.EndsWith("_t", StringComparison.Ordinal))
		{
			return str.Substring(0, str.Length - 2);
		}
		else
		{
			return str;
		}
	}

	private static string FixBeginning(this string str)
	{
		if (str.Length == 0)
		{
			return str;
		}
		char firstCharacter = str[0];
		if (char.IsDigit(firstCharacter))
		{
			return $"E{str}";
		}
		else if (char.IsLower(firstCharacter))
		{
			return $"{char.ToUpperInvariant(firstCharacter)}{str.AsSpan(1)}";
		}
		else
		{
			return str;
		}
	}
}
