using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Improves decompilation of obfuscated assemblies.
/// </summary>
public sealed class ObfuscationRepairProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();

		RemoveCompilerGeneratedAttributesFromSpeakableTypes(manager);

		RenameNormalProperties(manager);

		RenameExplicitProperties(manager);
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

			for (int i = type.CustomAttributes.Count - 1; i >= 0; i--)
			{
				if (type.CustomAttributes[i].IsCompilerGeneratedAttribute())
				{
					type.CustomAttributes.RemoveAt(i);
				}
			}
		}
	}

	/// <summary>
	/// Renames normal properties and events to match the names of their get/set/add/remove/raise methods.
	/// They can differ due to obfuscation renaming them, so this changes them back to be consistent.
	/// </summary>
	/// <remarks>
	/// This is needed because differing names can cause issues when attempting to recompile decompiled code.
	/// </remarks>
	private static void RenameNormalProperties(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			foreach (PropertyDefinition property in type.Properties)
			{
				if (property.GetMethod is { Name: not null } getMethod)
				{
					if (!IsExplicitOverride(type, getMethod) && getMethod.Name.Value.StartsWith("get_", StringComparison.Ordinal))
					{
						string propertyName = getMethod.Name.Value[4..];
						if (property.Name != propertyName)
						{
							property.Name = propertyName;
						}
					}
				}
				else if (property.SetMethod is { Name: not null } setMethod)
				{
					if (!IsExplicitOverride(type, setMethod) && setMethod.Name.Value.StartsWith("set_", StringComparison.Ordinal))
					{
						string propertyName = setMethod.Name.Value[4..];
						if (property.Name != propertyName)
						{
							property.Name = propertyName;
						}
					}
				}
			}
			foreach (EventDefinition @event in type.Events)
			{
				if (@event.AddMethod is { Name: not null } addMethod)
				{
					if (!IsExplicitOverride(type, addMethod) && addMethod.Name.Value.StartsWith("add_", StringComparison.Ordinal))
					{
						string eventName = addMethod.Name.Value[4..];
						if (@event.Name != eventName)
						{
							@event.Name = eventName;
						}
					}
				}
				else if (@event.RemoveMethod is { Name: not null } removeMethod)
				{
					if (!IsExplicitOverride(type, removeMethod) && removeMethod.Name.Value.StartsWith("remove_", StringComparison.Ordinal))
					{
						string eventName = removeMethod.Name.Value[7..];
						if (@event.Name != eventName)
						{
							@event.Name = eventName;
						}
					}
				}
				else if (@event.FireMethod is { Name: not null } fireMethod)
				{
					if (!IsExplicitOverride(type, fireMethod) && fireMethod.Name.Value.StartsWith("raise_", StringComparison.Ordinal))
					{
						string eventName = fireMethod.Name.Value[6..];
						if (@event.Name != eventName)
						{
							@event.Name = eventName;
						}
					}
				}
			}
		}

		static bool IsExplicitOverride(TypeDefinition type, MethodDefinition method)
		{
			return type.MethodImplementations.Any(impl => impl.Body == method);
		}
	}

	/// <summary>
	/// Renames explicit properties and events to match the names of their get/set/add/remove/raise methods.
	/// They can differ due to obfuscation renaming them, so this changes them back to be consistent.
	/// </summary>
	/// <remarks>
	/// This is needed because differing names can cause issues when attempting to recompile decompiled code.
	/// </remarks>
	private static void RenameExplicitProperties(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			foreach (MethodImplementation methodImplementation in type.MethodImplementations)
			{
				if (methodImplementation.Body is not MethodDefinition body || methodImplementation.Declaration is null)
				{
					continue;
				}

				if (body.IsGetMethod || body.IsSetMethod)
				{
					PropertyDefinition? property = type.Properties.FirstOrDefault(p => p.Semantics.Any(s => s.Method == body));
					if (property is null)
					{
						continue;
					}

					string? methodName = methodImplementation.Declaration.Name;
					if (string.IsNullOrEmpty(methodName))
					{
						continue;
					}

					string? propertyName;
					if (body.IsGetMethod && methodName.StartsWith("get_", StringComparison.Ordinal))
					{
						propertyName = methodName[4..];
					}
					else if (body.IsSetMethod && methodName.StartsWith("set_", StringComparison.Ordinal))
					{
						propertyName = methodName[4..];
					}
					else
					{
						propertyName = null;
					}

					string? interfaceTypeName = methodImplementation.Declaration.DeclaringType?.FullName.Replace('+', '.');

					body.Name = string.IsNullOrEmpty(interfaceTypeName)
						? methodName
						: $"{interfaceTypeName}.{methodName}";

					if (!string.IsNullOrEmpty(propertyName))
					{
						property.Name = string.IsNullOrEmpty(interfaceTypeName)
							? propertyName
							: $"{interfaceTypeName}.{propertyName}";
					}
				}

				if (body.IsAddMethod || body.IsRemoveMethod || body.IsFireMethod)
				{
					EventDefinition? @event = type.Events.FirstOrDefault(e => e.Semantics.Any(s => s.Method == body));
					if (@event is null)
					{
						continue;
					}

					string? methodName = methodImplementation.Declaration.Name;
					if (string.IsNullOrEmpty(methodName))
					{
						continue;
					}

					string? eventName;
					if (body.IsAddMethod && methodName.StartsWith("add_", StringComparison.Ordinal))
					{
						eventName = methodName[4..];
					}
					else if (body.IsRemoveMethod && methodName.StartsWith("remove_", StringComparison.Ordinal))
					{
						eventName = methodName[7..];
					}
					else if (body.IsFireMethod && methodName.StartsWith("raise_", StringComparison.Ordinal))
					{
						eventName = methodName[6..];
					}
					else
					{
						eventName = null;
					}

					string? interfaceTypeName = methodImplementation.Declaration.DeclaringType?.FullName.Replace('+', '.');
					body.Name = string.IsNullOrEmpty(interfaceTypeName)
						? methodName
						: $"{interfaceTypeName}.{methodName}";

					if (!string.IsNullOrEmpty(eventName))
					{
						@event.Name = string.IsNullOrEmpty(interfaceTypeName)
							? eventName
							: $"{interfaceTypeName}.{eventName}";
					}
				}
			}
		}
	}
}
