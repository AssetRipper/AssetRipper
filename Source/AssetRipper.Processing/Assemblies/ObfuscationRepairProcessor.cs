using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AssetRipper.Import.Structure.Assembly.Managers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Improves decompilation of obfuscated assemblies.
/// </summary>
public sealed partial class ObfuscationRepairProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();

		RemoveCompilerGeneratedAttributesFromSpeakableTypes(manager);

		RenameMembers(manager);

		RenameBackingFields(manager);
	}

	/// <summary>
	/// Removes compiler-generated attributes from types with speakable names.
	/// This prevents a decompiler from assuming that the type follows compiler-generated conventions
	/// when in fact the obfuscator may have modified it in a way that breaks those conventions.
	/// </summary>
	private static void RemoveCompilerGeneratedAttributesFromSpeakableTypes(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			string? name = type.Name;
			if (name is null || (name.Contains('<') && name.Contains('>')))
			{
				// Unspeakable name from the compiler. We should leave this type alone.
				continue;
			}

			RemoveCompilerGeneratedAttribute(type);
		}
	}

	private static void RenameMembers(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			if (type.Properties.Count == 0 && type.Events.Count == 0)
			{
				continue;
			}

			Dictionary<MethodDefinition, (string InterfaceTypeFullName, string InterfaceMethodName)> explicitOverrides = type.MethodImplementations
				.Select(GetExplicitMethodInfo)
				.Where(i => i.InterfaceTypeFullName is not null && i.InterfaceMethodName is not null && i.Implementation is not null)
				.DistinctBy(i => i.Implementation)
				.ToDictionary(i => i.Implementation!, i => (i.InterfaceTypeFullName!, i.InterfaceMethodName!));

			foreach (PropertyDefinition property in type.Properties)
			{
				MethodDefinition? getMethod = property.GetMethod;
				MethodDefinition? setMethod = property.SetMethod;
				MethodDefinition? primaryMethod = getMethod ?? setMethod;
				if (primaryMethod is null)
				{
					continue;
				}

				if (explicitOverrides.TryGetValue(primaryMethod, out (string InterfaceTypeFullName, string InterfaceMethodName) explicitInfo))
				{
					string interfaceTypeName = explicitInfo.InterfaceTypeFullName;
					string interfaceMethodName = explicitInfo.InterfaceMethodName;
					string? interfacePropertyName;
					if (primaryMethod.IsGetMethod && interfaceMethodName.StartsWith("get_", StringComparison.Ordinal))
					{
						interfacePropertyName = interfaceMethodName[4..];
					}
					else if (primaryMethod.IsSetMethod && interfaceMethodName.StartsWith("set_", StringComparison.Ordinal))
					{
						interfacePropertyName = interfaceMethodName[4..];
					}
					else
					{
						interfacePropertyName = interfaceMethodName;
					}

					if (string.IsNullOrEmpty(interfacePropertyName))
					{
						continue;
					}

					// Property
					{
						string expectedPropertyName = $"{interfaceTypeName}.{interfacePropertyName}";
						if (property.Name != expectedPropertyName)
						{
							property.Name = expectedPropertyName;
						}
					}

					if (getMethod is not null)
					{
						string expectedGetMethodName = $"{interfaceTypeName}.get_{interfacePropertyName}";
						if (getMethod.Name != expectedGetMethodName)
						{
							RemoveCompilerGeneratedAttribute(getMethod);
							MethodDefinition newMethod = CopyMethod(getMethod, expectedGetMethodName);
							ReplaceInMethodImplementations(type, getMethod, newMethod);
							property.GetMethod = newMethod;
							getMethod.IsSpecialName = false;
						}
					}

					if (setMethod is not null)
					{
						string expectedSetMethodName = $"{interfaceTypeName}.set_{interfacePropertyName}";
						if (setMethod.Name != expectedSetMethodName)
						{
							RemoveCompilerGeneratedAttribute(setMethod);
							MethodDefinition newMethod = CopyMethod(setMethod, expectedSetMethodName);
							ReplaceInMethodImplementations(type, setMethod, newMethod);
							property.SetMethod = newMethod;
							setMethod.IsSpecialName = false;
						}
					}
				}
				else
				{
					if (getMethod is not null)
					{
						string expectedGetMethodName = $"get_{property.Name}";
						if (getMethod.Name != expectedGetMethodName)
						{
							RemoveCompilerGeneratedAttribute(getMethod);
							MethodDefinition newMethod = CopyMethod(getMethod, expectedGetMethodName);
							property.GetMethod = newMethod;
							getMethod.IsSpecialName = false;
						}
					}

					if (setMethod is not null)
					{
						string expectedSetMethodName = $"set_{property.Name}";
						if (setMethod.Name != expectedSetMethodName)
						{
							RemoveCompilerGeneratedAttribute(setMethod);
							MethodDefinition newMethod = CopyMethod(setMethod, expectedSetMethodName);
							property.SetMethod = newMethod;
							setMethod.IsSpecialName = false;
						}
					}
				}
			}
			foreach (EventDefinition @event in type.Events)
			{
				MethodDefinition? addMethod = @event.AddMethod;
				MethodDefinition? removeMethod = @event.RemoveMethod;
				MethodDefinition? fireMethod = @event.FireMethod;
				MethodDefinition? primaryMethod = addMethod ?? removeMethod ?? fireMethod;
				if (primaryMethod is null)
				{
					continue;
				}

				if (explicitOverrides.TryGetValue(primaryMethod, out (string InterfaceTypeFullName, string InterfaceMethodName) explicitInfo))
				{
					string interfaceTypeName = explicitInfo.InterfaceTypeFullName;
					string interfaceMethodName = explicitInfo.InterfaceMethodName;
					string? interfaceEventName;
					if (primaryMethod.IsAddMethod && interfaceMethodName.StartsWith("add_", StringComparison.Ordinal))
					{
						interfaceEventName = interfaceMethodName[4..];
					}
					else if (primaryMethod.IsRemoveMethod && interfaceMethodName.StartsWith("remove_", StringComparison.Ordinal))
					{
						interfaceEventName = interfaceMethodName[7..];
					}
					else if (primaryMethod.IsFireMethod && interfaceMethodName.StartsWith("raise_", StringComparison.Ordinal))
					{
						interfaceEventName = interfaceMethodName[6..];
					}
					else
					{
						interfaceEventName = interfaceMethodName;
					}
					if (string.IsNullOrEmpty(interfaceEventName))
					{
						continue;
					}

					// Event
					{
						string expectedEventName = $"{interfaceTypeName}.{interfaceEventName}";
						if (@event.Name != expectedEventName)
						{
							@event.Name = expectedEventName;
						}
					}

					if (addMethod is not null)
					{
						string expectedAddMethodName = $"{interfaceTypeName}.add_{interfaceEventName}";
						if (addMethod.Name != expectedAddMethodName)
						{
							RemoveCompilerGeneratedAttribute(addMethod);
							MethodDefinition newMethod = CopyMethod(addMethod, expectedAddMethodName);
							ReplaceInMethodImplementations(type, addMethod, newMethod);
							@event.AddMethod = newMethod;
							addMethod.IsSpecialName = false;
						}
					}

					if (removeMethod is not null)
					{
						string expectedRemoveMethodName = $"{interfaceTypeName}.remove_{interfaceEventName}";
						if (removeMethod.Name != expectedRemoveMethodName)
						{
							RemoveCompilerGeneratedAttribute(removeMethod);
							MethodDefinition newMethod = CopyMethod(removeMethod, expectedRemoveMethodName);
							ReplaceInMethodImplementations(type, removeMethod, newMethod);
							@event.RemoveMethod = newMethod;
							removeMethod.IsSpecialName = false;
						}
					}

					if (fireMethod is not null)
					{
						string expectedFireMethodName = $"{interfaceTypeName}.raise_{interfaceEventName}";
						if (fireMethod.Name != expectedFireMethodName)
						{
							RemoveCompilerGeneratedAttribute(fireMethod);
							MethodDefinition newMethod = CopyMethod(fireMethod, expectedFireMethodName);
							ReplaceInMethodImplementations(type, fireMethod, newMethod);
							@event.FireMethod = newMethod;
							fireMethod.IsSpecialName = false;
						}
					}
				}
				else
				{
					if (addMethod is not null)
					{
						string expectedAddMethodName = $"add_{@event.Name}";
						if (addMethod.Name != expectedAddMethodName)
						{
							RemoveCompilerGeneratedAttribute(addMethod);
							MethodDefinition newMethod = CopyMethod(addMethod, expectedAddMethodName);
							ReplaceInMethodImplementations(type, addMethod, newMethod);
							@event.AddMethod = newMethod;
							addMethod.IsSpecialName = false;
						}
					}

					if (removeMethod is not null)
					{
						string expectedRemoveMethodName = $"remove_{@event.Name}";
						if (removeMethod.Name != expectedRemoveMethodName)
						{
							RemoveCompilerGeneratedAttribute(removeMethod);
							MethodDefinition newMethod = CopyMethod(removeMethod, expectedRemoveMethodName);
							ReplaceInMethodImplementations(type, removeMethod, newMethod);
							@event.RemoveMethod = newMethod;
							removeMethod.IsSpecialName = false;
						}
					}

					if (fireMethod is not null)
					{
						string expectedFireMethodName = $"raise_{@event.Name}";
						if (fireMethod.Name != expectedFireMethodName)
						{
							RemoveCompilerGeneratedAttribute(fireMethod);
							MethodDefinition newMethod = CopyMethod(fireMethod, expectedFireMethodName);
							ReplaceInMethodImplementations(type, fireMethod, newMethod);
							@event.FireMethod = newMethod;
							fireMethod.IsSpecialName = false;
						}
					}
				}
			}
		}
	}

	private static void ReplaceInMethodImplementations(TypeDefinition type, MethodDefinition original, MethodDefinition replacement)
	{
		for (int i = 0; i < type.MethodImplementations.Count; i++)
		{
			MethodImplementation methodImplementation = type.MethodImplementations[i];
			if (methodImplementation.Body == original)
			{
				type.MethodImplementations[i] = new MethodImplementation(methodImplementation.Declaration, replacement);
			}
		}
	}

	private static void RenameBackingFields(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			foreach (FieldDefinition field in type.Fields)
			{
				if (!field.IsCompilerGenerated())
				{
					continue;
				}
				string? name = field.Name;
				if (name is not null && name.StartsWith('<') && name.EndsWith(">k__BackingField", StringComparison.Ordinal))
				{
					string propertyName = name[1..^">k__BackingField".Length];
					PropertyDefinition? property = type.Properties.FirstOrDefault(p => p.Name == propertyName);
					if (property is null)
					{
						// No property with the expected name exists, so we can rename the field to match the property name.
						field.Name = propertyName;
						RemoveCompilerGeneratedAttribute(field);
					}
					else if (property.GetMethod?.IsCompilerGenerated() is not true && property.SetMethod?.IsCompilerGenerated() is not true)
					{
						// The property exists and none of its accessors are compiler-generated, so we rename the field to be more descriptive.
						field.Name = $"{propertyName}__BackingField";
						RemoveCompilerGeneratedAttribute(field);
					}
				}
				else
				{
					EventDefinition? @event = type.Events.FirstOrDefault(e => e.Name == name);
					if (@event is not null && @event.AddMethod?.IsCompilerGenerated() is not true && @event.RemoveMethod?.IsCompilerGenerated() is not true && @event.FireMethod?.IsCompilerGenerated() is not true)
					{
						field.Name = $"{name}__BackingField";
						RemoveCompilerGeneratedAttribute(field);
					}
				}
			}
		}
	}

	private static (string? InterfaceTypeFullName, string? InterfaceMethodName, MethodDefinition? Implementation) GetExplicitMethodInfo(MethodImplementation methodImplementation)
	{
		string? interfaceTypeFullName = methodImplementation.Declaration?.DeclaringType?.FullName.Replace('+', '.');
		string? interfaceMethodName = methodImplementation.Declaration?.Name;
		MethodDefinition? body = methodImplementation.Body as MethodDefinition;
		if (interfaceTypeFullName is not null)
		{
			// https://github.com/Washi1337/AsmResolver/blob/5aa0629348c523458780846b54a51f23ab8afced/src/AsmResolver.DotNet/MemberNameGenerator.cs#L399-L409
			if (body is { DeclaringType: { GenericParameters.Count: > 0 } declaringType })
			{
				for (int i = 0; i < declaringType.GenericParameters.Count; i++)
				{
					interfaceTypeFullName = interfaceTypeFullName.Replace($"!{i}", declaringType.GenericParameters[i].Name);
				}
			}

			// AsmResolver does not remove the backtick and number from generic type names. Roslyn does.
			interfaceTypeFullName = BacktickGenericTypeRegex.Replace(interfaceTypeFullName, "<");

			// AsmResolver includes a space after the comma in generic type arguments. Roslyn does not.
			interfaceTypeFullName = interfaceTypeFullName.Replace(", ", ",");
		}

		return (interfaceTypeFullName, interfaceMethodName, body);
	}

	private static MethodDefinition CopyMethod(MethodDefinition original, string newName)
	{
		MemberCloner cloner = new(original.DeclaringModule!);
		cloner.Include(original);
		MethodDefinition copy = (MethodDefinition)cloner.Clone().ClonedMembers.Single();
		copy.Name = newName;

		CilMethodBody newBody = new();
		if (!original.IsStatic)
		{
			newBody.Instructions.Add(CilOpCodes.Ldarg_0);
		}
		foreach (Parameter parameter in copy.Parameters)
		{
			newBody.Instructions.Add(CilOpCodes.Ldarg, parameter);
		}
		if (original.IsStatic)
		{
			newBody.Instructions.Add(CilOpCodes.Call, MakeReference(copy, original));
		}
		else
		{
			newBody.Instructions.Add(CilOpCodes.Callvirt, MakeReference(copy, original));
		}
		newBody.Instructions.Add(CilOpCodes.Ret);
		copy.CilMethodBody = newBody;

		original.DeclaringType!.Methods.Add(copy);

		return copy;

		static IMethodDescriptor MakeReference(MethodDefinition copy, MethodDefinition original)
		{
			Debug.Assert(copy.GenericParameters.Count == original.GenericParameters.Count);

			IMethodDefOrRef baseMethod;
			if (original.DeclaringType is { GenericParameters.Count: > 0 })
			{
				IEnumerable<GenericParameterSignature> typeArguments = Enumerable.Range(0, original.DeclaringType!.GenericParameters.Count).Select(i => new GenericParameterSignature(GenericParameterType.Type, i));
				GenericInstanceTypeSignature declaringType = original.DeclaringType.MakeGenericInstanceType(original.DeclaringModule!.RuntimeContext, typeArguments);
				baseMethod = new MemberReference(declaringType.ToTypeDefOrRef(), original.Name, original.Signature);
			}
			else
			{
				baseMethod = original;
			}

			if (original.GenericParameters.Count > 0)
			{
				IEnumerable<GenericParameterSignature> typeArguments = Enumerable.Range(0, copy.GenericParameters.Count).Select(i => new GenericParameterSignature(GenericParameterType.Method, i));
				return baseMethod.MakeGenericInstanceMethod(typeArguments);
			}
			else
			{
				return baseMethod;
			}
		}
	}

	private static void RemoveCompilerGeneratedAttribute(IHasCustomAttribute owner)
	{
		for (int i = owner.CustomAttributes.Count - 1; i >= 0; i--)
		{
			if (owner.CustomAttributes[i].IsCompilerGeneratedAttribute())
			{
				owner.CustomAttributes.RemoveAt(i);
			}
		}
	}

	[GeneratedRegex(@"`\d+<")]
	private static partial Regex BacktickGenericTypeRegex { get; }
}
