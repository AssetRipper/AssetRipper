using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using System.Linq;

namespace AssetRipper.Library.Processors.Assemblies;
public sealed class MethodStubbingProcessor : IAssemblyProcessor
{
	public void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend == ScriptingBackend.IL2Cpp)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (ModuleDefinition module in manager.GetAssemblies().SelectMany(a => a.Modules).Where(m => m.TopLevelTypes.Count > 0))
		{
			MethodDefinition? injectedRefHelperMethod = null;
			foreach (TypeDefinition type in module.GetAllTypes())
			{
				//RemoveCompilerGeneratedNestedTypes(type);
				foreach (MethodDefinition method in type.Methods.Where(m => m.IsManagedMethodWithBody() && m != injectedRefHelperMethod))
				{
					FillMethodBodyWithStub(method, ref injectedRefHelperMethod, module);
				}
			}
		}
	}

	private static void FillMethodBodyWithStub(MethodDefinition methodDefinition, ref MethodDefinition? injectedRefHelperMethod, ModuleDefinition module)
	{
		methodDefinition.CilMethodBody = new(methodDefinition);
		CilInstructionCollection methodInstructions = methodDefinition.CilMethodBody.Instructions;

		if (methodDefinition.IsConstructor && !methodDefinition.IsStatic && !methodDefinition.DeclaringType!.IsValueType)
		{
			MethodDefinition? baseConstructor = TryGetBaseConstructor(methodDefinition);
			if (baseConstructor is not null)
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				foreach (AsmResolver.DotNet.Collections.Parameter baseParameter in baseConstructor.Parameters)
				{
					TypeSignature importedBaseParameterType = methodDefinition.DeclaringType.Module!.DefaultImporter.ImportTypeSignature(baseParameter.ParameterType);
					if (baseParameter.Definition is { IsOut: true })
					{
						CilLocalVariable variable = methodInstructions.AddLocalVariable(importedBaseParameterType);
						methodInstructions.Add(CilOpCodes.Ldloca, variable);
					}
					else if (baseParameter.Definition is { IsIn: true })
					{
						CilLocalVariable variable = methodInstructions.AddLocalVariable(importedBaseParameterType);
						if (importedBaseParameterType.IsValueType)
						{
							methodInstructions.Add(CilOpCodes.Ldloca, variable);
							methodInstructions.Add(CilOpCodes.Initobj, importedBaseParameterType.ToTypeDefOrRef());
						}
						else
						{
							methodInstructions.Add(CilOpCodes.Ldnull);
							methodInstructions.Add(CilOpCodes.Stloc, variable);
						}
						methodInstructions.Add(CilOpCodes.Ldloca, variable);
					}
					else if (importedBaseParameterType is ByReferenceTypeSignature byReferenceTypeSignature)
					{
						injectedRefHelperMethod ??= MakeRefHelper(module);
						TypeSignature referencedType = byReferenceTypeSignature.BaseType;
						GenericInstanceTypeSignature genericRefHelperInstance = injectedRefHelperMethod.DeclaringType!.MakeGenericInstanceType(referencedType);
						MemberReference memberReference = new MemberReference(genericRefHelperInstance.ToTypeDefOrRef(), injectedRefHelperMethod.Name, injectedRefHelperMethod.Signature);
						methodInstructions.Add(CilOpCodes.Call, memberReference);
					}
					else
					{
						AddDefaultValueForType(methodInstructions, importedBaseParameterType);
					}
				}
				methodInstructions.Add(CilOpCodes.Call, methodDefinition.DeclaringType.Module!.DefaultImporter.ImportMethod(baseConstructor));
			}
		}

		foreach (AsmResolver.DotNet.Collections.Parameter parameter in methodDefinition.Parameters)
		{
			if (parameter.Definition?.IsOut ?? false)
			{
				if (parameter.ParameterType.IsValueType)
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Initobj, parameter.ParameterType.ToTypeDefOrRef());
				}
				else
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Ldnull);
					methodInstructions.Add(CilOpCodes.Stind_Ref);
				}
			}
		}
		AddDefaultValueForType(methodInstructions, methodDefinition.Signature!.ReturnType);
		methodInstructions.Add(CilOpCodes.Ret);
		methodInstructions.OptimizeMacros();
	}

	private static void AddDefaultValueForType(CilInstructionCollection instructions, TypeSignature type)
	{
		if (type is CorLibTypeSignature { ElementType: ElementType.Void })
		{
		}
		else if (type.IsValueType)
		{
			CilLocalVariable variable = instructions.AddLocalVariable(type);
			instructions.Add(CilOpCodes.Ldloca, variable);
			instructions.Add(CilOpCodes.Initobj, type.ToTypeDefOrRef());
			instructions.Add(CilOpCodes.Ldloc, variable);
		}
		else
		{
			instructions.Add(CilOpCodes.Ldnull);
		}
	}

	private static MethodDefinition? TryGetBaseConstructor(MethodDefinition methodDefinition)
	{
		TypeDefinition declaringType = methodDefinition.DeclaringType!;
		TypeDefinition? baseType = declaringType.BaseType?.Resolve();
		if (baseType is null)
		{
			return null;
		}
		else if (declaringType.Module == baseType.Module)
		{
			return baseType.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && !m.IsPrivate);
		}
		else
		{
			return baseType.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && (m.IsFamily || m.IsPublic));
		}
	}

	private static MethodDefinition MakeRefHelper(ModuleDefinition module)
	{
		TypeDefinition staticClass = new TypeDefinition("AssetRipperInjected", "RefHelper", TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
		module.TopLevelTypes.Add(staticClass);
		staticClass.GenericParameters.Add(new GenericParameter("T"));

		GenericParameterSignature fieldType = new GenericParameterSignature(GenericParameterType.Type, 0);
		FieldSignature fieldSignature = new FieldSignature(fieldType);
		FieldDefinition field = new FieldDefinition("backingField", FieldAttributes.Private | FieldAttributes.Static, fieldSignature);
		staticClass.Fields.Add(field);

		MethodSignature staticConstructorSignature = MethodSignature.CreateStatic(module.CorLibTypeFactory.Void);
		MethodDefinition staticConstructor = new MethodDefinition("..ctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName, staticConstructorSignature);
		staticConstructor.CilMethodBody = new CilMethodBody(staticConstructor);
		CilInstructionCollection staticConstructorInstructions = staticConstructor.CilMethodBody.Instructions;
		staticConstructorInstructions.Add(CilOpCodes.Ldsflda, field);
		staticConstructorInstructions.Add(CilOpCodes.Initobj, fieldType.ToTypeDefOrRef());
		staticConstructorInstructions.Add(CilOpCodes.Ret);

		GenericInstanceTypeSignature genericInstance = staticClass.MakeGenericInstanceType(fieldType);
		MemberReference memberReference = new MemberReference(genericInstance.ToTypeDefOrRef(), field.Name, field.Signature);

		MethodSignature methodSignature = MethodSignature.CreateStatic(new ByReferenceTypeSignature(fieldType));
		MethodDefinition method = new MethodDefinition("GetSharedReference", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, methodSignature);
		staticClass.Methods.Add(method);
		method.CilMethodBody = new CilMethodBody(method);
		CilInstructionCollection methodInstructions = method.CilMethodBody.Instructions;
		methodInstructions.Add(CilOpCodes.Ldsflda, memberReference);
		methodInstructions.Add(CilOpCodes.Ret);

		return method;
	}

	//Possible improvement for the future: removing unnecessary nested types.
	//If this is implemented, it might be better for it to be its own processor.
	private static void RemoveCompilerGeneratedNestedTypes(TypeDefinition type)
	{
		for (int i = type.NestedTypes.Count - 1; i >= 0; i--)
		{
			TypeDefinition nestedType = type.NestedTypes[i];

			//This check is insufficient to determine if a nested type can be removed.
			//The problem is that it may be used in a member signature.
			//Solving this requires checking each of the members on the declaring type
			//for any reference to the nested type.
			//It has to be thorough since the nested type could be in a generic, array, by ref, etc.
			if (nestedType.IsNestedPrivate && nestedType.IsCompilerGenerated())
			{
				type.NestedTypes.RemoveAt(i);
			}
		}
	}
}
