using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.DocExtraction.Extensions;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass053_HasMethodsAndNullableAttributes
{
	public static void DoPass()
	{
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			ApplyNullableAttributesToTypes(group);
			AddHasMethodsAndApplyNullableAttributesToProperties(group);
			AddIsMethods(group);
			AddMemberNotNullAttributesToInterfaceMethods(group);
		}
	}

	private static void ApplyNullableAttributesToTypes(ClassGroupBase group)
	{
		group.Interface.AddNullableContextAttribute(NullableAnnotation.NotNull);
		group.Interface.AddNullableAttribute(NullableAnnotation.Oblivious);
		foreach (TypeDefinition instanceType in group.Types)
		{
			instanceType.AddNullableContextAttribute(NullableAnnotation.NotNull);
			instanceType.AddNullableAttribute(NullableAnnotation.Oblivious);
		}
	}

	private static void AddHasMethodsAndApplyNullableAttributesToProperties(ClassGroupBase group)
	{
		foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
		{
			if (interfaceProperty.AbsentRange.IsEmpty())
			{
				continue;
			}

			bool isValueType = interfaceProperty.Definition.IsValueType();
			string propertyName = interfaceProperty.Definition.Name!;
			interfaceProperty.HasMethod = group.Interface.AddHasMethodDeclaration(propertyName);

			if (!isValueType)
			{
				interfaceProperty.Definition.AddNullableAttributesForMaybeNull();
			}

			foreach (ClassProperty classProperty in interfaceProperty.Implementations)
			{
				classProperty.HasMethod = classProperty.Class.Type.AddHasMethodImplementation(classProperty.Definition.Name!, classProperty.BackingField is not null);

				if (!isValueType)
				{
					classProperty.Definition.AddNullableAttributesForMaybeNull();
					if (classProperty.BackingField is not null)
					{
						classProperty.Definition.GetMethod!.AddNotNullAttribute();
					}
				}
			}
		}
	}

	private static void AddIsMethods(ClassGroupBase group)
	{
		foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
		{
			string propertyName = interfaceProperty.Definition.Name!;
			if (!interfaceProperty.ReleaseOnlyRange.IsEmpty() && interfaceProperty.ReleaseOnlyRange != interfaceProperty.PresentRange)
			{
				interfaceProperty.ReleaseOnlyMethod = group.Interface.AddReleaseOnlyMethodDeclaration(propertyName);
				foreach (ClassProperty classProperty in interfaceProperty.Implementations)
				{
					classProperty.ReleaseOnlyMethod = classProperty.Class.Type.AddReleaseOnlyMethodImplementation(propertyName, classProperty.IsReleaseOnly);
				}
			}
			if (!interfaceProperty.EditorOnlyRange.IsEmpty() && interfaceProperty.EditorOnlyRange != interfaceProperty.PresentRange)
			{
				interfaceProperty.EditorOnlyMethod = group.Interface.AddEditorOnlyMethodDeclaration(propertyName);
				foreach (ClassProperty classProperty in interfaceProperty.Implementations)
				{
					classProperty.EditorOnlyMethod = classProperty.Class.Type.AddEditorOnlyMethodImplementation(propertyName, classProperty.IsEditorOnly);
				}
			}
		}
	}

	private static void AddMemberNotNullAttributesToInterfaceMethods(ClassGroupBase group)
	{
		foreach (InterfaceProperty property in group.InterfaceProperties)
		{
			if (property.HasMethod is null && property.ReleaseOnlyMethod is null && property.EditorOnlyMethod is null)
			{
				continue;
			}

			foreach (InterfaceProperty otherProperty in group.InterfaceProperties)
			{
				if (otherProperty.HasMethod is null || otherProperty.Definition.IsValueType())
				{
					continue;
				}
				if (property.HasMethod is not null)
				{
					//other is always present when this is present
					if (otherProperty.PresentRange.Contains(property.PresentRange))
					{
						property.HasMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, true, otherProperty.Definition.Name!);
					}
					//other is always present when this is absent
					else if (otherProperty.PresentRange.Contains(property.AbsentRange))
					{
						property.HasMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, false, otherProperty.Definition.Name!);
					}
				}
				if (property.ReleaseOnlyMethod is not null)
				{
					//other is always present when this is release only
					if (otherProperty.PresentRange.Contains(property.ReleaseOnlyRange))
					{
						property.ReleaseOnlyMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, true, otherProperty.Definition.Name!);
					}
					//other is always present when this is not release only
					else if (otherProperty.PresentRange.Contains(property.NotReleaseOnlyRange))
					{
						property.ReleaseOnlyMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, false, otherProperty.Definition.Name!);
					}
				}
				if (property.EditorOnlyMethod is not null)
				{
					//other is always present when this is editor only
					if (otherProperty.PresentRange.Contains(property.EditorOnlyRange))
					{
						property.EditorOnlyMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, true, otherProperty.Definition.Name!);
					}
					//other is always present when this is not editor only
					else if (otherProperty.PresentRange.Contains(property.NotEditorOnlyRange))
					{
						property.EditorOnlyMethod.AddMemberNotNullWhenAttribute(SharedState.Instance.Importer, false, otherProperty.Definition.Name!);
					}
				}
			}
		}
	}

	private static MethodDefinition AddHasMethodDeclaration(this TypeDefinition @interface, string propertyName)
	{
		return @interface.AddBooleanMethodDeclaration(GeneratedInterfaceUtils.GetHasMethodName(propertyName));
	}

	private static MethodDefinition AddHasMethodImplementation(this TypeDefinition declaringType, string propertyName, bool returnTrue)
	{
		return declaringType.AddBooleanMethodImplementation(GeneratedInterfaceUtils.GetHasMethodName(propertyName), returnTrue);
	}

	private static MethodDefinition AddReleaseOnlyMethodDeclaration(this TypeDefinition @interface, string propertyName)
	{
		return @interface.AddBooleanMethodDeclaration(GeneratedInterfaceUtils.GetReleaseOnlyMethodName(propertyName));
	}

	private static MethodDefinition AddReleaseOnlyMethodImplementation(this TypeDefinition declaringType, string propertyName, bool returnTrue)
	{
		return declaringType.AddBooleanMethodImplementation(GeneratedInterfaceUtils.GetReleaseOnlyMethodName(propertyName), returnTrue);
	}

	private static MethodDefinition AddEditorOnlyMethodDeclaration(this TypeDefinition @interface, string propertyName)
	{
		return @interface.AddBooleanMethodDeclaration(GeneratedInterfaceUtils.GetEditorOnlyMethodName(propertyName));
	}

	private static MethodDefinition AddEditorOnlyMethodImplementation(this TypeDefinition declaringType, string propertyName, bool returnTrue)
	{
		return declaringType.AddBooleanMethodImplementation(GeneratedInterfaceUtils.GetEditorOnlyMethodName(propertyName), returnTrue);
	}

	private static MethodDefinition AddBooleanMethodDeclaration(this TypeDefinition @interface, string methodName)
	{
		return @interface.AddMethod(
			methodName,
			InterfaceUtils.InterfaceMethodDeclaration,
			SharedState.Instance.Importer.Boolean);
	}

	private static MethodDefinition AddBooleanMethodImplementation(this TypeDefinition declaringType, string methodName, bool returnTrue)
	{
		MethodDefinition method = declaringType.AddMethod(
			methodName,
			InterfaceUtils.InterfaceMethodImplementation,
			SharedState.Instance.Importer.Boolean);
		method.CilMethodBody!.Instructions.FillWithSimpleBooleanReturn(returnTrue);
		return method;
	}
}
