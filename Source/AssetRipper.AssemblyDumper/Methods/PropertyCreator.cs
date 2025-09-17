using AssetRipper.AssemblyDumper.Attributes;

namespace AssetRipper.AssemblyDumper.Methods;

public static class PropertyCreator
{
	public static PropertyDefinition AddFullProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, TypeSignature returnTypeSignature, PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		PropertyDefinition property = type.AddEmptyProperty(propertyName, methodAttributes, returnTypeSignature, propertyAttributes);
		property.AddGetMethod(propertyName, methodAttributes, returnTypeSignature);
		property.AddSetMethod(propertyName, methodAttributes, returnTypeSignature);
		return property;
	}

	public static PropertyDefinition ImplementFullProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, TypeSignature? returnTypeSignature, FieldDefinition? field, PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		TypeSignature returnType = returnTypeSignature ?? field?.Signature!.FieldType ?? throw new Exception($"{nameof(returnTypeSignature)} and {nameof(field)} cannot both be null");
		PropertyDefinition property = type.AddFullProperty(propertyName, methodAttributes, returnType, propertyAttributes);
		property.FillGetter(field, returnType);
		property.FillSetter(field);
		return property;
	}

	public static PropertyDefinition AddGetterProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, TypeSignature returnTypeSignature, PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		PropertyDefinition property = type.AddEmptyProperty(propertyName, methodAttributes, returnTypeSignature, propertyAttributes);
		property.AddGetMethod(propertyName, methodAttributes, returnTypeSignature);
		return property;
	}

	public static PropertyDefinition ImplementGetterProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, TypeSignature? returnTypeSignature, FieldDefinition? field, PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		TypeSignature returnType = returnTypeSignature ?? field?.Signature!.FieldType ?? throw new Exception($"{nameof(returnTypeSignature)} and {nameof(field)} cannot both be null");
		PropertyDefinition property = type.AddGetterProperty(propertyName, methodAttributes, returnType, propertyAttributes);
		property.FillGetter(field, returnType);
		return property;
	}

	private static PropertyDefinition AddEmptyProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, TypeSignature returnTypeSignature, PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
		PropertySignature methodSignature =
			isStatic ?
			PropertySignature.CreateStatic(returnTypeSignature) :
			PropertySignature.CreateInstance(returnTypeSignature);

		PropertyDefinition property = new PropertyDefinition(propertyName, propertyAttributes, methodSignature);
		type.Properties.Add(property);

		return property;
	}

	private static MethodDefinition AddGetMethod(this PropertyDefinition property, string propertyName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		if (property.GetMethod != null)
		{
			throw new ArgumentException("Property already has a get method", nameof(property));
		}

		MethodDefinition getter = property.DeclaringType!.AddMethod($"get_{propertyName}", methodAttributes, returnType);
		property.Semantics.Add(new MethodSemantics(getter, MethodSemanticsAttributes.Getter));
		return getter;
	}

	private static MethodDefinition AddSetMethod(this PropertyDefinition property, string propertyName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		if (property.SetMethod != null)
		{
			throw new ArgumentException("Property already has a set method", nameof(property));
		}

		TypeDefinition declaringType = property.DeclaringType!;
		MethodDefinition setter = declaringType.AddMethod($"set_{propertyName}", methodAttributes, declaringType.DeclaringModule!.CorLibTypeFactory.Void);
		setter.AddParameter(returnType, "value");
		property.Semantics.Add(new MethodSemantics(setter, MethodSemanticsAttributes.Setter));
		return setter;
	}

	private static MethodDefinition FillGetter(this PropertyDefinition property, FieldDefinition? field, TypeSignature returnType)
	{
		MethodDefinition getter = property.GetMethod!;

		CilInstructionCollection instructions = getter.CilMethodBody!.Instructions;
		if (field != null)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field);
			if (returnType is SzArrayTypeSignature arrayType)
			{
				SignatureComparer comparer = new SignatureComparer();
				if (!comparer.Equals(arrayType, field.Signature!.FieldType))
				{
					CilLocalVariable local = new CilLocalVariable(arrayType);
					getter.CilMethodBody.LocalVariables.Add(local);
					instructions.Add(CilOpCodes.Stloc, local);
					instructions.Add(CilOpCodes.Ldloc, local);
				}
			}
		}
		else
		{
			instructions.AddDefaultValue(returnType);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return getter;
	}

	private static MethodDefinition FillSetter(this PropertyDefinition property, FieldDefinition? field)
	{
		MethodDefinition setter = property.SetMethod!;

		CilInstructionCollection instructions = setter.CilMethodBody!.Instructions;
		if (field != null)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1); //value
			instructions.Add(CilOpCodes.Stfld, field);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return setter;
	}

	public static PropertyDefinition ImplementFullAutoProperty(
		this TypeDefinition declaringType,
		string propertyName,
		MethodAttributes methodAttributes,
		TypeSignature propertyType,
		CachedReferenceImporter importer,
		out FieldDefinition field,
		PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		field = CreateBackingField(declaringType, propertyName, propertyType, importer);
		PropertyDefinition property = declaringType.ImplementFullProperty(propertyName, methodAttributes, propertyType, field, propertyAttributes);
		property.GetMethod!.AddCompilerGeneratedAttribute(importer);
		property.SetMethod!.AddCompilerGeneratedAttribute(importer);
		return property;
	}

	public static PropertyDefinition ImplementGetterAutoProperty(
		this TypeDefinition declaringType,
		string propertyName,
		MethodAttributes methodAttributes,
		TypeSignature propertyType,
		CachedReferenceImporter importer,
		out FieldDefinition field,
		PropertyAttributes propertyAttributes = PropertyAttributes.None)
	{
		field = CreateBackingField(declaringType, propertyName, propertyType, importer);
		PropertyDefinition property = declaringType.ImplementGetterProperty(propertyName, methodAttributes, propertyType, field, propertyAttributes);
		property.GetMethod!.AddCompilerGeneratedAttribute(importer);
		return property;
	}

	private static FieldDefinition CreateBackingField(TypeDefinition declaringType, string propertyName, TypeSignature fieldType, CachedReferenceImporter importer)
	{
		FieldDefinition field = declaringType.AddField($"<{propertyName}>k__BackingField", fieldType, false, Visibility.Private);
		field.AddCompilerGeneratedAttribute(importer);
		return field;
	}
}
