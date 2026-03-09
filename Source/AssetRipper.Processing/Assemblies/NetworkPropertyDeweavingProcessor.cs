using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Cil;
using AssetRipper.CIL;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Rewrites obvious Fusion/Mirror-style network properties into simple auto-property patterns for cleaner decompilation.
/// </summary>
public sealed class NetworkPropertyDeweavingProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		ModuleDefinition? mscorlib = manager.Mscorlib?.ManifestModule;
		MethodDefinition? compilerGeneratedConstructor = TryGetCompilerGeneratedConstructor(mscorlib);

		manager.ClearStreamCache();

		int rewrittenPropertyCount = 0;
		foreach (AssemblyDefinition assembly in manager.GetAssemblies().Where(IsAssemblyCSharpAssembly))
		{
			foreach (ModuleDefinition module in assembly.Modules)
			{
				ICustomAttributeType? importedCompilerGeneratedConstructor = compilerGeneratedConstructor is null
					? null
					: (ICustomAttributeType)module.DefaultImporter.ImportMethod(compilerGeneratedConstructor);

				foreach (TypeDefinition type in module.GetAllTypes())
				{
					if (IsDiscordType(type))
					{
						continue;
					}

					foreach (PropertyDefinition property in type.Properties)
					{
						if (!ShouldRewriteProperty(property) || !TryGetPropertyType(property, out TypeSignature? propertyType) || propertyType is null)
						{
							continue;
						}

						FieldDefinition backingField = GetOrCreateBackingField(type, property, propertyType, importedCompilerGeneratedConstructor);
						RewriteGetter(property.GetMethod!, backingField);
						RewriteSetter(property.SetMethod!, backingField);
						AddCompilerGeneratedAttribute(backingField, importedCompilerGeneratedConstructor);
						AddCompilerGeneratedAttribute(property.GetMethod!, importedCompilerGeneratedConstructor);
						AddCompilerGeneratedAttribute(property.SetMethod!, importedCompilerGeneratedConstructor);
						rewrittenPropertyCount++;
					}
				}
			}
		}

		if (rewrittenPropertyCount > 0)
		{
			Logger.Info(LogCategory.Processing, $"Network de-weaver rewrote {rewrittenPropertyCount} properties into auto-property form.");
		}
	}

	private static MethodDefinition? TryGetCompilerGeneratedConstructor(ModuleDefinition? mscorlib)
	{
		if (mscorlib is null || !mscorlib.TryGetTopLevelType("System.Runtime.CompilerServices", "CompilerGeneratedAttribute", out TypeDefinition? attributeType))
		{
			return null;
		}

		return attributeType.Methods.FirstOrDefault(static method => method.IsConstructor && method.Parameters.Count == 0);
	}

	private static bool ShouldRewriteProperty(PropertyDefinition property)
	{
		string? name = property.Name;
		if (string.IsNullOrWhiteSpace(name) || name.IndexOf('.') >= 0)
		{
			return false;
		}
		if (property.GetMethod is null || property.SetMethod is null)
		{
			return false;
		}
		if (property.GetMethod.Signature?.ParameterTypes.Count != 0 || property.SetMethod.Signature?.ParameterTypes.Count != 1)
		{
			return false;
		}

		return HasNetworkAttributes(property.CustomAttributes)
			|| HasNetworkAttributes(property.GetMethod.CustomAttributes)
			|| HasNetworkAttributes(property.SetMethod.CustomAttributes);
	}

	private static bool TryGetPropertyType(PropertyDefinition property, out TypeSignature? propertyType)
	{
		propertyType = property.Signature?.ReturnType ?? property.GetMethod?.Signature?.ReturnType;
		return propertyType is not null;
	}

	private static FieldDefinition GetOrCreateBackingField(TypeDefinition type, PropertyDefinition property, TypeSignature propertyType, ICustomAttributeType? compilerGeneratedConstructor)
	{
		string backingFieldName = $"<{property.Name}>k__BackingField";
		FieldDefinition? backingField = type.Fields.FirstOrDefault(field => string.Equals(field.Name, backingFieldName, StringComparison.Ordinal));
		if (backingField is not null)
		{
			return backingField;
		}

		backingField = type.Fields.FirstOrDefault(field =>
			!field.IsStatic
			&& !field.IsLiteral
			&& field.Signature is { FieldType: TypeSignature fieldType }
			&& SignatureComparer.Default.Equals(fieldType, propertyType)
				&& (LooksObfuscated(field.Name ?? string.Empty)
				|| ContainsIgnoreCase(field.Name, "network")
				|| ContainsIgnoreCase(field.Name, "buffer")
				|| ContainsIgnoreCase(field.Name, "state")));

		if (backingField is not null)
		{
			backingField.Name = backingFieldName;
			backingField.IsPrivate = true;
			backingField.IsPublic = false;
			AddCompilerGeneratedAttribute(backingField, compilerGeneratedConstructor);
			return backingField;
		}

		FieldAttributes attributes = FieldAttributes.Private;
		if (property.GetMethod!.IsStatic)
		{
			attributes |= FieldAttributes.Static;
		}

		backingField = new FieldDefinition(backingFieldName, attributes, new FieldSignature(propertyType));
		type.Fields.Add(backingField);
		AddCompilerGeneratedAttribute(backingField, compilerGeneratedConstructor);
		return backingField;
	}

	private static void RewriteGetter(MethodDefinition getter, FieldDefinition backingField)
	{
		getter.CilMethodBody = new CilMethodBody();
		CilInstructionCollection instructions = getter.CilMethodBody.Instructions;
		if (getter.IsStatic)
		{
			instructions.Add(CilOpCodes.Ldsfld, backingField);
		}
		else
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, backingField);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
	}

	private static void RewriteSetter(MethodDefinition setter, FieldDefinition backingField)
	{
		setter.CilMethodBody = new CilMethodBody();
		CilInstructionCollection instructions = setter.CilMethodBody.Instructions;
		if (setter.IsStatic)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Stsfld, backingField);
		}
		else
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Stfld, backingField);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
	}

	private static bool HasNetworkAttributes(IEnumerable<CustomAttribute> attributes)
	{
		foreach (CustomAttribute attribute in attributes)
		{
			string? namespaceName = attribute.Constructor?.DeclaringType?.Namespace;
			string? typeName = attribute.Constructor?.DeclaringType?.Name;
			if (string.IsNullOrEmpty(typeName))
			{
				continue;
			}

			if (ContainsIgnoreCase(namespaceName, "Fusion")
				|| ContainsIgnoreCase(namespaceName, "Mirror")
				|| ContainsIgnoreCase(typeName, "Networked")
				|| ContainsIgnoreCase(typeName, "SyncVar"))
			{
				return true;
			}
		}

		return false;
	}

	private static void AddCompilerGeneratedAttribute(IHasCustomAttribute provider, ICustomAttributeType? constructor)
	{
		if (constructor is null || provider.CustomAttributes.Any(CustomAttributeExtensions.IsCompilerGeneratedAttribute))
		{
			return;
		}

		provider.CustomAttributes.Add(new CustomAttribute(constructor));
	}

	private static bool IsAssemblyCSharpAssembly(AssemblyDefinition assembly)
	{
		return NormalizeAssemblyName(assembly.Name).StartsWith("assemblycsharp", StringComparison.Ordinal);
	}

	private static string NormalizeAssemblyName(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}

		return new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static bool IsDiscordType(TypeDefinition type)
	{
		string fullName = type.FullName ?? string.Empty;
		return ContainsIgnoreCase(fullName, "Discord");
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

	private static bool ContainsIgnoreCase(string? value, string token)
	{
		return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
