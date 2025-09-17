using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass081_CreatePPtrProperties
{
	public static void DoPass()
	{
		ITypeDefOrRef pptrAccessListType = SharedState.Instance.Importer.ImportType(typeof(PPtrAccessList<,>));
		IMethodDefOrRef pptrAccessListConstructor = SharedState.Instance.Importer.ImportMethod(typeof(PPtrAccessList<,>), method =>
		{
			return method.IsConstructor && method.Parameters.Count == 2 && method.Parameters[1].ParameterType.Name == nameof(IUnityObjectBase);
		});
		IMethodDefOrRef pptrAccessListEmptyMethod = SharedState.Instance.Importer.ImportMethod(typeof(PPtrAccessList<,>), method =>
		{
			return method.Name == $"get_{nameof(PPtrAccessList<IPPtr<IUnityObjectBase>, IUnityObjectBase>.Empty)}";
		});
		IMethodDefOrRef getCollectionMethod = SharedState.Instance.Importer.ImportMethod(typeof(UnityObjectBase), method =>
		{
			return method.Name == $"get_{nameof(UnityObjectBase.Collection)}";
		});

		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
			{
				if (interfaceProperty.SpecialDefinition is null)
				{
					string pptrPropertyName = $"{interfaceProperty.Definition.Name}P";
					TypeSignature originalPropertySignature = interfaceProperty.Definition.Signature!.ReturnType;
					if (originalPropertySignature.IsPPtr(out TypeDefinition? pptrType, out TypeDefinition? parameterType))
					{
						interfaceProperty.SpecialDefinition = interfaceProperty.Group.Interface.AddFullProperty(
							pptrPropertyName,
							InterfaceUtils.InterfacePropertyDeclaration,
							parameterType.ToTypeSignature());
						interfaceProperty.SpecialDefinition.AddNullableAttributesForMaybeNull();

						foreach (ClassProperty classProperty in interfaceProperty.Implementations)
						{
							classProperty.SpecialDefinition = classProperty.Class.Type.AddFullProperty(
								pptrPropertyName,
								InterfaceUtils.InterfacePropertyImplementation,
								parameterType.ToTypeSignature());
							classProperty.SpecialDefinition.AddNullableAttributesForMaybeNull();

							//Get method
							{
								CilInstructionCollection instructions = classProperty.SpecialDefinition.GetMethod!.GetInstructions();
								if (classProperty.BackingField is null)
								{
									instructions.Add(CilOpCodes.Ldnull);
								}
								else
								{
									CilLocalVariable local = instructions.AddLocalVariable(parameterType.ToTypeSignature());
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, classProperty.Definition.GetMethod!);
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, getCollectionMethod);
									instructions.Add(CilOpCodes.Ldloca, local);
									instructions.Add(CilOpCodes.Callvirt, Pass080_PPtrConversions.PPtrsToTryGetAssetMethods[pptrType]);
									instructions.Add(CilOpCodes.Pop);
									instructions.Add(CilOpCodes.Ldloc, local);
								}
								instructions.Add(CilOpCodes.Ret);
								instructions.OptimizeMacros();
							}
							//Set method
							{
								CilInstructionCollection instructions = classProperty.SpecialDefinition.SetMethod!.GetInstructions();
								if (classProperty.BackingField is not null)
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, classProperty.Definition.GetMethod!);
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, getCollectionMethod);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, Pass080_PPtrConversions.PPtrsToSetAssetMethods[pptrType]);
								}
								instructions.Add(CilOpCodes.Ret);
							}
							//Debugger attribute
							if (classProperty.Class.Type.IsAbstract)
							{
								classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//Properties in base classes are redundant in the debugger.
							}
							else if (classProperty.BackingField is null)
							{
								classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//The property will always be null.
							}
						}
					}
					else if (originalPropertySignature.IsPPtrList(out pptrType, out parameterType))
					{
						GenericInstanceTypeSignature propertySignature = pptrAccessListType.MakeGenericInstanceType(pptrType.ToTypeSignature(), parameterType.ToTypeSignature());
						IMethodDefOrRef constructor = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, propertySignature, pptrAccessListConstructor);
						IMethodDefOrRef emptyMethod = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, propertySignature, pptrAccessListEmptyMethod);

						interfaceProperty.SpecialDefinition = interfaceProperty.Group.Interface.AddGetterProperty(
							pptrPropertyName,
							InterfaceUtils.InterfacePropertyDeclaration,
							propertySignature);

						foreach (ClassProperty classProperty in interfaceProperty.Implementations)
						{
							classProperty.SpecialDefinition = classProperty.Class.Type.AddGetterProperty(
								pptrPropertyName,
								InterfaceUtils.InterfacePropertyImplementation,
								propertySignature);

							//Get method
							{
								CilInstructionCollection instructions = classProperty.SpecialDefinition.GetMethod!.GetInstructions();
								if (classProperty.BackingField is null)
								{
									instructions.Add(CilOpCodes.Call, emptyMethod);
								}
								else
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, classProperty.Definition.GetMethod!);
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Newobj, constructor);
								}
								instructions.Add(CilOpCodes.Ret);
							}
							//Debugger attribute
							if (classProperty.Class.Type.IsAbstract)
							{
								classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//Properties in base classes are redundant in the debugger.
							}
							else if (classProperty.BackingField is null)
							{
								classProperty.SpecialDefinition.AddDebuggerBrowsableNeverAttribute();//The list will always be empty.
							}
						}
					}
				}
			}
		}
	}

	private static bool IsPPtr(this TypeSignature typeSignature, [NotNullWhen(true)] out TypeDefinition? pptrType, [NotNullWhen(true)] out TypeDefinition? parameterType)
	{
		if (typeSignature is TypeDefOrRefSignature typeDefOrRefSignature
			&& typeDefOrRefSignature.ToTypeDefOrRef() is TypeDefinition typeDefinition
			&& Pass080_PPtrConversions.PPtrsToParameters.TryGetValue(typeDefinition, out parameterType))
		{
			pptrType = typeDefinition;
			return true;
		}
		else
		{
			pptrType = null;
			parameterType = null;
			return false;
		}
	}

	private static bool IsPPtrList(this TypeSignature typeSignature, [NotNullWhen(true)] out TypeDefinition? pptrType, [NotNullWhen(true)] out TypeDefinition? parameterType)
	{
		if (typeSignature is GenericInstanceTypeSignature genericInstanceTypeSignature
			&& NameIsAssetListOrAccessListBase(genericInstanceTypeSignature)
			&& genericInstanceTypeSignature.TypeArguments.Count == 1//For an extra bit of certainty
			&& genericInstanceTypeSignature.TypeArguments[0].IsPPtr(out pptrType, out parameterType))
		{
			return true;
		}
		else
		{
			pptrType = null;
			parameterType = null;
			return false;
		}
	}

	private static bool NameIsAssetListOrAccessListBase(GenericInstanceTypeSignature genericInstanceTypeSignature)
	{
		return genericInstanceTypeSignature.GenericType.Name == $"{nameof(AssetList<int>)}`1"
			|| genericInstanceTypeSignature.GenericType.Name == $"{nameof(AccessListBase<int>)}`1";
	}
}
