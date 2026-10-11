using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Export.UnityProjects.Scripts;

/// <summary>
/// Restores editor safety handles for URP's temporary views of its allocator memory.
/// </summary>
public static class UnityRendererAssemblyProcessor
{
	public static void Process(IAssemblyManager manager)
	{
		AssemblyDefinition? assembly = manager.GetAssemblies().FirstOrDefault(assembly => assembly.Name == "Unity.RenderPipelines.Universal.Runtime");
		if (assembly?.ManifestModule is ModuleDefinition module && MakeArrayViewsEditorCompatible(module))
		{
			manager.ClearStreamCache();
		}
	}

	public static bool MakeArrayViewsEditorCompatible(ModuleDefinition module)
	{
		if (!module.TryGetTopLevelType("UnityEngine.Rendering.Universal", "BuddyAllocator", out TypeDefinition? type))
		{
			return false;
		}
		MethodDefinition? method = type.Methods.FirstOrDefault(method => method.Name == "GetNativeArray" && method.GenericParameters.Count == 1);
		if (method?.CilMethodBody is not CilMethodBody body ||
			method.Signature?.ReturnType is not GenericInstanceTypeSignature { GenericType.FullName: "Unity.Collections.NativeArray`1" } arrayType ||
			body.Instructions.Count == 0 || body.Instructions[^1].OpCode != CilOpCodes.Ret || body.ExceptionHandlers.Count != 0 ||
			body.Instructions.Any(instruction => instruction.OpCode.FlowControl is CilFlowControl.Branch or CilFlowControl.ConditionalBranch) ||
			!body.Instructions.Any(instruction => instruction.Operand is IMethodDescriptor calledMethod && calledMethod.Name == "ConvertExistingDataToNativeArray") ||
			body.Instructions.Any(instruction => instruction.Operand is IMethodDescriptor calledMethod && calledMethod.Name == "SetAtomicSafetyHandle"))
		{
			return false;
		}
		AddSafetyHandle(module, body, arrayType);
		return true;
	}

	private static void AddSafetyHandle(ModuleDefinition module, CilMethodBody body, GenericInstanceTypeSignature arrayType)
	{
		TypeReference safetyType = new(module, arrayType.GenericType.Scope, "Unity.Collections.LowLevel.Unsafe", "AtomicSafetyHandle");
		TypeReference utilityType = new(module, arrayType.GenericType.Scope, "Unity.Collections.LowLevel.Unsafe", "NativeArrayUnsafeUtility");
		TypeDefOrRefSignature safetySignature = new(safetyType, true);
		MemberReference getHandle = new(safetyType, "GetTempUnsafePtrSliceHandle", MethodSignature.CreateStatic(safetySignature));
		MemberReference setHandle = new(utilityType, "SetAtomicSafetyHandle",
			MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, 1, [new ByReferenceTypeSignature(arrayType), safetySignature]));
		MethodSpecification setHandleInstance = new(setHandle,
			new GenericInstanceMethodSignature([new GenericParameterSignature(GenericParameterType.Method, 0)]));

		// These array views are used within each allocator operation. Attach the
		// editor's handle for temporary views without changing the allocator layout.
		CilLocalVariable array = new(arrayType);
		body.LocalVariables.Add(array);
		body.Instructions.ExpandMacros();
		int index = body.Instructions.Count - 1;
		body.Instructions.Insert(index++, new CilInstruction(CilOpCodes.Stloc, array));
		body.Instructions.Insert(index++, new CilInstruction(CilOpCodes.Ldloca, array));
		body.Instructions.Insert(index++, new CilInstruction(CilOpCodes.Call, getHandle));
		body.Instructions.Insert(index++, new CilInstruction(CilOpCodes.Call, setHandleInstance));
		body.Instructions.Insert(index, new CilInstruction(CilOpCodes.Ldloc, array));
	}
}
