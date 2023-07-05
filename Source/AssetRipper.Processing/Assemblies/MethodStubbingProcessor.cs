using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;
public sealed class MethodStubbingProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend == ScriptingBackend.IL2Cpp)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (ModuleDefinition module in manager.GetAssemblies().SelectMany(a => a.Modules).Where(m => m.TopLevelTypes.Count > 0))
		{
			foreach (TypeDefinition type in module.GetAllTypes())
			{
				//RemoveCompilerGeneratedNestedTypes(type);
				foreach (MethodDefinition method in type.Methods.Where(m => m.IsManagedMethodWithBody()))
				{
					FillMethodBodyWithStub(method);
				}
			}
		}
	}

	private static void FillMethodBodyWithStub(MethodDefinition methodDefinition)
	{
		methodDefinition.CilMethodBody = new(methodDefinition);
		CilInstructionCollection methodInstructions = methodDefinition.CilMethodBody.Instructions;

		if (methodDefinition.IsConstructor && !methodDefinition.IsStatic && !methodDefinition.DeclaringType!.IsValueType)
		{
			if (TryGetBaseConstructor(methodDefinition, out MethodDefinition? baseConstructor))
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				foreach (Parameter baseParameter in baseConstructor.Parameters)
				{
					TypeSignature importedBaseParameterType = methodDefinition.DeclaringType.Module!.DefaultImporter.ImportTypeSignature(baseParameter.ParameterType);
					methodInstructions.AddDefaultValueForType(importedBaseParameterType);
				}
				methodInstructions.Add(CilOpCodes.Call, methodDefinition.DeclaringType.Module!.DefaultImporter.ImportMethod(baseConstructor));
			}
		}

		foreach (Parameter parameter in methodDefinition.Parameters)
		{
			//Although Roslyn-compiled code will only emit the out flag on ByReferenceTypeSignatures,
			//Some Unity libraries have it on a handful (less than 100) of parameters with incompatible type signatures.
			//One example on 2021.3.6 is int System.IO.CStreamReader.Read([In][Out] char[] dest, int index, int count)
			//All the instances I investigated were clearly not meant to be out parameters.
			//The [In][Out] attributes are not a decompilation issue and compile fine on .NET 7.
			if (parameter.IsOutParameter(out TypeSignature? parameterType))
			{
				if (parameterType.IsValueTypeOrGenericParameter())
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Initobj, parameterType.ToTypeDefOrRef());
				}
				else
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Ldnull);
					methodInstructions.Add(CilOpCodes.Stind_Ref);
				}
			}
		}
		methodInstructions.AddDefaultValueForType(methodDefinition.Signature!.ReturnType);
		methodInstructions.Add(CilOpCodes.Ret);
		methodInstructions.OptimizeMacros();
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

	private static bool TryGetBaseConstructor(MethodDefinition methodDefinition, [NotNullWhen(true)] out MethodDefinition? baseConstructor)
	{
		baseConstructor = TryGetBaseConstructor(methodDefinition);
		return baseConstructor is not null;
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
