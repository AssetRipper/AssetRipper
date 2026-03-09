using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Renames obfuscated MonoBehaviour fields to the serialized names recovered from the type tree.
/// </summary>
public sealed class TypeTreeNamingBridgeProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		if (!gameData.AssemblyManager.IsSet)
		{
			return;
		}

		ProcessGameData(gameData);
	}

	private static void ProcessGameData(GameData gameData)
	{
		gameData.AssemblyManager.ClearStreamCache();

		Dictionary<ScriptIdentity, string[]> fieldLookup = BuildRecoveredFieldLookup(gameData);
		int renamedTypeCount = 0;
		int renamedFieldCount = 0;

		foreach ((ScriptIdentity script, string[] expectedFieldNames) in fieldLookup)
		{
			if (!TryGetTypeDefinition(gameData.AssemblyManager, script, out TypeDefinition? type) || type is null)
			{
				continue;
			}

			int renamedForType = ApplyTypeTreeNames(type, expectedFieldNames);
			if (renamedForType > 0)
			{
				renamedTypeCount++;
				renamedFieldCount += renamedForType;
			}
		}

		if (renamedFieldCount > 0)
		{
			Logger.Info(LogCategory.Processing, $"Type-tree naming bridge renamed {renamedFieldCount} fields across {renamedTypeCount} types.");
		}
	}

	private static Dictionary<ScriptIdentity, string[]> BuildRecoveredFieldLookup(GameData gameData)
	{
		Dictionary<ScriptIdentity, string[]> lookup = new();

		foreach (IMonoBehaviour monoBehaviour in gameData.GameBundle.FetchAssets().OfType<IMonoBehaviour>())
		{
			IMonoScript? script = monoBehaviour.ScriptP;
			if (script is null || monoBehaviour.LoadStructure() is not SerializableStructure structure)
			{
				continue;
			}

			ScriptIdentity identity = new(script.GetAssemblyNameFixed(), script.Namespace.String, script.ClassName_R.String);
			if (identity.IsInjected || identity.IsDiscord)
			{
				continue;
			}

			string[] fieldNames = ExtractRecoverableFieldNames(structure);
			if (fieldNames.Length == 0)
			{
				continue;
			}

			if (!lookup.TryGetValue(identity, out string[]? existing) || fieldNames.Length > existing.Length)
			{
				lookup[identity] = fieldNames;
			}
		}

		return lookup;
	}

	private static string[] ExtractRecoverableFieldNames(SerializableStructure structure)
	{
		List<string> fieldNames = new(structure.Type.Fields.Count);
		HashSet<string> seenNames = new(StringComparer.Ordinal);

		for (int i = 0; i < structure.Type.Fields.Count; i++)
		{
			string fieldName = structure.Type.Fields[i].Name;
			if (ShouldSkipRecoveredField(fieldName) || !seenNames.Add(fieldName))
			{
				continue;
			}

			fieldNames.Add(fieldName);
		}

		return fieldNames.ToArray();
	}

	private static bool TryGetTypeDefinition(IAssemblyManager manager, ScriptIdentity script, out TypeDefinition? type)
	{
		foreach (AssemblyDefinition assembly in manager.GetAssemblies())
		{
			if (!string.Equals(assembly.Name, script.Assembly, StringComparison.Ordinal))
			{
				continue;
			}

			type = assembly.Modules
				.SelectMany(static module => module.GetAllTypes())
				.FirstOrDefault(candidate =>
					string.Equals(candidate.Name, script.ClassName, StringComparison.Ordinal)
					&& string.Equals(candidate.Namespace, script.Namespace, StringComparison.Ordinal));

			if (type is not null)
			{
				return true;
			}
		}

		type = null;
		return false;
	}

	private static int ApplyTypeTreeNames(TypeDefinition type, string[] expectedFieldNames)
	{
		List<FieldDefinition> candidateFields = GetRenameCandidates(type);
		if (candidateFields.Count != expectedFieldNames.Length)
		{
			return 0;
		}

		int matchingSlots = 0;
		bool hasObfuscatedMismatch = false;
		HashSet<string> reservedNames = new(StringComparer.Ordinal);

		foreach (FieldDefinition field in type.Fields)
		{
			if (field.Name is { Length: > 0 } fieldName && !candidateFields.Contains(field))
			{
				reservedNames.Add(fieldName);
			}
		}
		foreach (PropertyDefinition property in type.Properties)
		{
			if (property.Name is { Length: > 0 } propertyName)
			{
				reservedNames.Add(propertyName);
			}
		}

		HashSet<string> plannedNames = new(StringComparer.Ordinal);
		for (int i = 0; i < candidateFields.Count; i++)
		{
			FieldDefinition field = candidateFields[i];
			string currentName = field.Name ?? string.Empty;
			string targetName = expectedFieldNames[i];

			if (string.Equals(currentName, targetName, StringComparison.Ordinal))
			{
				matchingSlots++;
				plannedNames.Add(targetName);
				continue;
			}

			if (reservedNames.Contains(targetName) || !plannedNames.Add(targetName))
			{
				return 0;
			}

			hasObfuscatedMismatch |= LooksObfuscated(currentName);
		}

		if (!hasObfuscatedMismatch && matchingSlots == 0)
		{
			return 0;
		}

		int renamedCount = 0;
		for (int i = 0; i < candidateFields.Count; i++)
		{
			FieldDefinition field = candidateFields[i];
			string targetName = expectedFieldNames[i];
			if (!string.Equals(field.Name, targetName, StringComparison.Ordinal))
			{
				field.Name = targetName;
				renamedCount++;
			}
		}

		return renamedCount;
	}

	private static List<FieldDefinition> GetRenameCandidates(TypeDefinition type)
	{
		List<FieldDefinition> fields = new();
		foreach (FieldDefinition field in type.Fields)
		{
			string? fieldName = field.Name;
			if (string.IsNullOrWhiteSpace(fieldName))
			{
				continue;
			}
			if (field.IsStatic || field.IsLiteral)
			{
				continue;
			}
			if (fieldName.StartsWith('<') || fieldName.StartsWith("CS$", StringComparison.Ordinal))
			{
				continue;
			}
			if (field.CustomAttributes.Any(CustomAttributeExtensions.IsNonSerializedAttribute))
			{
				continue;
			}

			fields.Add(field);
		}

		return fields;
	}

	private static bool ShouldSkipRecoveredField(string fieldName)
	{
		if (string.IsNullOrWhiteSpace(fieldName))
		{
			return true;
		}

		return fieldName.StartsWith("rgctx", StringComparison.Ordinal)
			|| fieldName.StartsWith('<')
			|| fieldName is "m_ObjectHideFlags"
				or "m_CorrespondingSourceObject"
				or "m_PrefabInstance"
				or "m_PrefabAsset"
				or "m_GameObject"
				or "m_Enabled"
				or "m_EditorHideFlags"
				or "m_Script"
				or "m_Name"
				or "serializedVersion"
				or "references"
				or "managedReferences";
	}

	private static bool LooksObfuscated(string name)
	{
		if (name.Length <= 2)
		{
			return true;
		}
		if (name.StartsWith('_') && name.Skip(1).All(char.IsLetterOrDigit))
		{
			return true;
		}
		if (name.StartsWith("f_", StringComparison.Ordinal) && name.Skip(2).All(char.IsDigit))
		{
			return true;
		}

		int letterCount = name.Count(char.IsLetter);
		int digitCount = name.Count(char.IsDigit);
		return letterCount <= 2 && digitCount > 0;
	}

	private static bool ContainsIgnoreCase(string value, string token)
	{
		return value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private readonly record struct ScriptIdentity(string Assembly, string Namespace, string ClassName)
	{
		public bool IsInjected => Assembly.Length == 0 && Namespace.Length == 0 && ClassName.Length == 0;
		public bool IsDiscord => ContainsIgnoreCase(Assembly, "Discord")
			|| ContainsIgnoreCase(Namespace, "Discord")
			|| ContainsIgnoreCase(ClassName, "Discord");
	}
}
