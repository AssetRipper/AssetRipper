using AssemblyDumper.Utils;
using AssetRipper.Core.Interfaces;

namespace AssemblyDumper.Passes
{
	public static class Pass103_FillDependencyMethods
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static ITypeDefOrRef commonPPtrTypeRef;
		private static ITypeDefOrRef unityObjectBaseInterfaceRef;
		private static GenericInstanceTypeSignature unityObjectBasePPtrRef;
		private static GenericInstanceTypeSignature unityObjectBasePPtrListType;
		private static IMethodDefOrRef unityObjectBasePPtrListConstructor;
		public static IMethodDefOrRef unityObjectBasePPtrListAddRange; // needed for monobehaviour pass
		private static IMethodDefOrRef emptyArray;
		private static MethodSpecification emptyArrayMethod;
		private static ITypeDefOrRef dependencyContextRef;
		private static IMethodDefOrRef fetchDependenciesFromDependent;
		private static IMethodDefOrRef fetchDependenciesFromArray;
		private static IMethodDefOrRef fetchDependenciesFromArrayArray;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		private readonly static List<TypeDefinition> processedTypes = new List<TypeDefinition>();
		private readonly static List<TypeDefinition> nonDependentTypes = new List<TypeDefinition>();

		private static void InitializeStaticFields()
		{
			commonPPtrTypeRef = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
			unityObjectBaseInterfaceRef = SharedState.Importer.ImportCommonType<IUnityObjectBase>();
			unityObjectBasePPtrRef = commonPPtrTypeRef.MakeGenericInstanceType(unityObjectBaseInterfaceRef.ToTypeSignature());
			unityObjectBasePPtrListType = SystemTypeGetter.List.MakeGenericInstanceType(unityObjectBasePPtrRef);
			unityObjectBasePPtrListConstructor = MethodUtils.MakeConstructorOnGenericType(unityObjectBasePPtrListType, 0);
			unityObjectBasePPtrListAddRange = MethodUtils.MakeMethodOnGenericType(unityObjectBasePPtrListType, "AddRange");

			emptyArray = SharedState.Importer.ImportSystemMethod<Array>(method => method.Name == "Empty");
			emptyArrayMethod = MethodUtils.MakeGenericInstanceMethod(emptyArray, unityObjectBasePPtrRef);

			dependencyContextRef = SharedState.Importer.ImportCommonType<AssetRipper.Core.Parser.Asset.DependencyContext>();
			fetchDependenciesFromDependent = SharedState.Importer.ImportCommonMethod<AssetRipper.Core.Parser.Asset.DependencyContext>(m => m.Name == "FetchDependenciesFromDependent");
			fetchDependenciesFromArray = SharedState.Importer.ImportCommonMethod<AssetRipper.Core.Parser.Asset.DependencyContext>(m => m.Name == "FetchDependenciesFromArray");
			fetchDependenciesFromArrayArray = SharedState.Importer.ImportCommonMethod<AssetRipper.Core.Parser.Asset.DependencyContext>(m => m.Name == "FetchDependenciesFromArrayArray");
		}

		public static void DoPass()
		{
			Console.WriteLine("Pass 103: Fill Fetch Dependency Methods");

			InitializeStaticFields();

			foreach (TypeDefinition type in SharedState.TypeDictionary.Values)
			{
				type.ProcessType();
			}
		}

		private static void ProcessType(this TypeDefinition type)
		{
			if (processedTypes.Contains(type))
				return;
			if (type.IsPPtrType())
			{
				type.FillPPtrType();
			}
			else
			{
				type.FillNormalType();
			}
			processedTypes.Add(type);
		}

		private static void ReplaceWithReturnEmptyArray(this CilInstructionCollection processor)
		{
			processor.Clear();
			processor.Add(CilOpCodes.Call, emptyArrayMethod);
			processor.Add(CilOpCodes.Ret);
		}

		private static MethodDefinition GetDependencyMethod(this TypeDefinition type)
		{
			return type.Methods.Single(x => x.Name == "FetchDependencies");
		}

		private static bool IsPPtrType(this TypeDefinition type)
		{
			return type.Name!.ToString().StartsWith("PPtr_");
		}

		private static void FillPPtrType(this TypeDefinition type)
		{
			MethodDefinition? dependencyMethod = type.GetDependencyMethod();
			CilInstructionCollection processor = dependencyMethod.CilMethodBody!.Instructions;

			processor.Add(CilOpCodes.Ldc_I4_1);
			processor.Add(CilOpCodes.Newarr, unityObjectBasePPtrRef.ToTypeDefOrRef());

			processor.Add(CilOpCodes.Dup);
			processor.Add(CilOpCodes.Ldc_I4_0);
			processor.Add(CilOpCodes.Ldarg_0);
			MethodDefinition conversionMethod = PPtrUtils.GetExplicitConversion<IUnityObjectBase>(type);
			processor.Add(CilOpCodes.Call, conversionMethod);
			processor.Add(CilOpCodes.Stelem, unityObjectBasePPtrRef.ToTypeDefOrRef());
			processor.Add(CilOpCodes.Ret);
		}

		private static bool IsPrimitiveType(this TypeSignature type)
		{
			return type.IsValueType || type.FullName == "System.String";
		}

		private static void MaybeProcessType(this TypeDefinition type)
		{
			if (!processedTypes.Contains(type))
				type.ProcessType();
		}

		private static bool AddFetchDependenciesFromNormalField(this CilInstructionCollection processor, FieldDefinition field, CilLocalVariable listVariable)
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			if (fieldType.IsPrimitiveType())
			{
				return false;
			}
			else if (fieldType is GenericInstanceTypeSignature genericInstanceType)
			{
				processor.AddFetchDependenciesFromField(field, fieldType, listVariable, 0);
			}
			else if (fieldType.ToTypeDefOrRef() is TypeDefinition fieldTypeDef)
			{
				fieldTypeDef.MaybeProcessType();
				if (nonDependentTypes.Contains(fieldTypeDef))
					return false;
				processor.AddFetchDependenciesFromField(field, fieldType, listVariable, 0);
			}
			else
			{
				throw new NotSupportedException($"{fieldType.Name} {field.DeclaringType!.Name}.{field.Name}");
			}
			return true;
		}

		private static bool AddFetchDependenciesFromArrayField(this CilInstructionCollection processor, FieldDefinition field, CilLocalVariable listVariable)
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			if(fieldType is not SzArrayTypeSignature arrayType)
				throw new ArgumentException(nameof(field));
			TypeSignature genericTypeParameter = arrayType.BaseType;
			TypeSignature elementType = arrayType.BaseType;

			if (elementType.IsPrimitiveType())
			{
				return false;
			}
			else if (elementType is GenericInstanceTypeSignature genericInstanceType)
			{
				processor.AddFetchDependenciesFromField(field, genericTypeParameter, listVariable, 1);
			}
			else if (elementType.ToTypeDefOrRef() is TypeDefinition fieldTypeDef)
			{
				fieldTypeDef.MaybeProcessType();
				if (nonDependentTypes.Contains(fieldTypeDef))
					return false;
				processor.AddFetchDependenciesFromField(field, genericTypeParameter, listVariable, 1);
			}
			else
			{
				throw new NotSupportedException($"{fieldType.Name} {field.DeclaringType!.Name}.{field.Name}");
			}
			return true;
		}

		private static bool AddFetchDependenciesFromArrayArrayField(this CilInstructionCollection processor, FieldDefinition field, CilLocalVariable listVariable)
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			if (fieldType is not SzArrayTypeSignature arrayType)
				throw new ArgumentException(nameof(field));
			if (arrayType.BaseType is not SzArrayTypeSignature subArrayType)
				throw new ArgumentException(nameof(field));

			TypeSignature genericTypeParameter = subArrayType;
			TypeSignature elementType = subArrayType.BaseType;

			if (elementType.IsPrimitiveType())
			{
				return false;
			}
			else if (elementType is GenericInstanceTypeSignature genericInstanceType)
			{
				processor.AddFetchDependenciesFromField(field, genericTypeParameter, listVariable, 2);
			}
			else if (elementType.ToTypeDefOrRef() is TypeDefinition fieldTypeDef)
			{
				fieldTypeDef.MaybeProcessType();
				if (nonDependentTypes.Contains(fieldTypeDef))
					return false;
				processor.AddFetchDependenciesFromField(field, genericTypeParameter, listVariable, 2);
			}
			else
			{
				throw new NotSupportedException($"{fieldType.Name} {field.DeclaringType!.Name}.{field.Name}");
			}
			return true;
		}

		private static bool AddFetchDependenciesFromField(this CilInstructionCollection processor, FieldDefinition field, CilLocalVariable listVariable)
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			
			if (fieldType is SzArrayTypeSignature arrayType)
			{
				if (arrayType.BaseType is SzArrayTypeSignature)
				{
					return processor.AddFetchDependenciesFromArrayArrayField(field, listVariable);
				}
				else
				{
					return processor.AddFetchDependenciesFromArrayField(field, listVariable);
				}
			}
			else
			{
				return processor.AddFetchDependenciesFromNormalField(field, listVariable);
			}
		}

		private static void AddFetchDependenciesFromField(this CilInstructionCollection processor, FieldDefinition field, TypeSignature genericTypeParameter, CilLocalVariable listVariable, int depth)
		{
			MethodSpecification? fetchMethod = depth switch
			{
				0 => MethodUtils.MakeGenericInstanceMethod(fetchDependenciesFromDependent, genericTypeParameter),
				1 => MethodUtils.MakeGenericInstanceMethod(fetchDependenciesFromArray, genericTypeParameter),
				2 => MethodUtils.MakeGenericInstanceMethod(fetchDependenciesFromArrayArray, genericTypeParameter),
				_ => throw new ArgumentOutOfRangeException(nameof(depth)),
			};
			processor.Add(CilOpCodes.Ldloc, listVariable);
			processor.Add(CilOpCodes.Ldarg_1);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
			processor.Add(CilOpCodes.Ldstr, field.Name!);
			processor.Add(CilOpCodes.Call, fetchMethod);
			processor.Add(CilOpCodes.Call, unityObjectBasePPtrListAddRange);
		}

		private static void FillNormalType(this TypeDefinition type)
		{
			MethodDefinition dependencyMethod = type.GetDependencyMethod();
			CilInstructionCollection processor = dependencyMethod.CilMethodBody!.Instructions;
			dependencyMethod.CilMethodBody.InitializeLocals = true;

			processor.Add(CilOpCodes.Newobj, unityObjectBasePPtrListConstructor);

			CilLocalVariable list = new CilLocalVariable(unityObjectBasePPtrListType);
			processor.Owner.LocalVariables.Add(list);
			processor.Add(CilOpCodes.Stloc, list);

			int count = 0;

			if(type.BaseType is TypeDefinition baseType)
			{
				baseType.MaybeProcessType();

				if (!nonDependentTypes.Contains(baseType))
				{
					MethodDefinition baseDependencyMethod = baseType.GetDependencyMethod();
					processor.Add(CilOpCodes.Ldloc, list);
					processor.Add(CilOpCodes.Ldarg_0);
					processor.Add(CilOpCodes.Ldarg_1);
					processor.Add(CilOpCodes.Call, baseDependencyMethod);
					processor.Add(CilOpCodes.Call, unityObjectBasePPtrListAddRange);
					count++;
				}
			}

			foreach(FieldDefinition field in type.Fields)
			{
				bool increaseCount = processor.AddFetchDependenciesFromField(field, list);
				if(increaseCount)
					count++;
			}

			processor.Add(CilOpCodes.Ldloc, list);
			processor.Add(CilOpCodes.Ret);

			if (count == 1)
			{
				processor.Owner.LocalVariables.Clear();
				processor.Owner.InitializeLocals = false;
				processor.RemoveAt(0);//New list object
				processor.RemoveAt(0);//Store list object
				processor.RemoveAt(0);//Load list object
				processor.Pop();//return
				processor.Pop();//load list object
				processor.Pop();//call add range
				processor.Add(CilOpCodes.Ret);
			}
			else if(count == 0)
			{
				processor.ReplaceWithReturnEmptyArray();
				nonDependentTypes.Add(type);
			}
		}
	}
}
