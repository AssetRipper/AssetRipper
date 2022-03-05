using AsmResolver.DotNet.Signatures;
using AssetRipper.Core.Classes;

namespace AssemblyDumper.Utils
{
	internal static class PropertyCreator
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

		public static PropertyDefinition ImplementHasFieldProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, params string[] fieldNames)
		{
			PropertyDefinition property = type.AddGetterProperty(propertyName, methodAttributes, SystemTypeGetter.Boolean);
			CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
			foreach(string fieldName in fieldNames)
			{
				if(type.TryGetFieldByName(fieldName, out var _))
				{
					processor.Add(CilOpCodes.Ldc_I4_1);
					processor.Add(CilOpCodes.Ret);
					return property;
				}
			}
			processor.Add(CilOpCodes.Ldc_I4_0);
			processor.Add(CilOpCodes.Ret);
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
			if(property.GetMethod != null)
				throw new ArgumentException("Property already has a get method",nameof(property));
			MethodDefinition getter = property.DeclaringType!.AddMethod($"get_{propertyName}", methodAttributes, returnType);
			property.Semantics.Add(new MethodSemantics(getter, MethodSemanticsAttributes.Getter));
			return getter;
		}

		private static MethodDefinition AddSetMethod(this PropertyDefinition property, string propertyName, MethodAttributes methodAttributes, TypeSignature returnType)
		{
			if (property.SetMethod != null)
				throw new ArgumentException("Property already has a set method", nameof(property));
			MethodDefinition setter = property.DeclaringType!.AddMethod($"set_{propertyName}", methodAttributes, SystemTypeGetter.Void!);
			setter.AddParameter("value", returnType);
			property.Semantics.Add(new MethodSemantics(setter, MethodSemanticsAttributes.Setter));
			return setter;
		}

		private static MethodDefinition FillGetter(this PropertyDefinition property, FieldDefinition? field, TypeSignature returnType)
		{
			MethodDefinition getter = property.GetMethod!;

			CilInstructionCollection processor = getter.CilMethodBody!.Instructions;
			if (field != null)
			{
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				if(returnType is SzArrayTypeSignature arrayType)
				{
					SignatureComparer comparer = new SignatureComparer();
					if (!comparer.Equals(arrayType, field.Signature!.FieldType))
					{
						CilLocalVariable local = new CilLocalVariable(arrayType);
						getter.CilMethodBody.LocalVariables.Add(local);
						processor.Add(CilOpCodes.Stloc, local);
						processor.Add(CilOpCodes.Ldloc, local);
					}
				}
			}
			else
			{
				processor.AddDefaultValue(returnType);
			}
			processor.Add(CilOpCodes.Ret);
			processor.OptimizeMacros();
			return getter;
		}

		private static MethodDefinition FillSetter(this PropertyDefinition property, FieldDefinition? field)
		{
			MethodDefinition setter = property.SetMethod!;

			CilInstructionCollection processor = setter.CilMethodBody!.Instructions;
			if (field != null)
			{
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldarg_1); //value
				processor.Add(CilOpCodes.Stfld, field);
			}
			processor.Add(CilOpCodes.Ret);
			processor.OptimizeMacros();
			return setter;
		}

		public static PropertyDefinition ImplementStringProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, FieldDefinition? field, PropertyAttributes propertyAttributes = PropertyAttributes.None)
		{
			if(field == null)
			{
				return type.ImplementFullProperty(propertyName, methodAttributes, SystemTypeGetter.String, field, propertyAttributes);
			}

			PropertyDefinition property = type.AddFullProperty(propertyName, methodAttributes, SystemTypeGetter.String, propertyAttributes);

			IMethodDefOrRef getRef = SharedState.Importer.ImportCommonMethod<Utf8StringBase>(m => m.Name == $"get_{nameof(Utf8StringBase.String)}");
			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, field);
			getProcessor.Add(CilOpCodes.Call, getRef);
			getProcessor.Add(CilOpCodes.Ret);

			IMethodDefOrRef setRef = SharedState.Importer.ImportCommonMethod<Utf8StringBase>(m => m.Name == $"set_{nameof(Utf8StringBase.String)}");
			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, setRef);
			setProcessor.Add(CilOpCodes.Ret);

			return property;
		}
	}
}
