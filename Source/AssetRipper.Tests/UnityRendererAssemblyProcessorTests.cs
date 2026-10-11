using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Export.UnityProjects.Scripts;

namespace AssetRipper.Tests;

public class UnityRendererAssemblyProcessorTests
{
	[Test]
	public void ArrayViewRepairPreservesTheAllocatorLayoutAndSurvivesAssemblyWriting()
	{
		ModuleDefinition module = CreateModule();
		TypeDefinition allocator = module.TopLevelTypes.Single(type => type.Name == "BuddyAllocator");
		MethodDefinition method = allocator.Methods[0];
		string signature = method.FullName;

		Assert.That(UnityRendererAssemblyProcessor.MakeArrayViewsEditorCompatible(module), Is.True);
		Assert.That(UnityRendererAssemblyProcessor.MakeArrayViewsEditorCompatible(module), Is.False);
		using MemoryStream output = new();
		module.Write(output);
		ModuleDefinition restored = ModuleDefinition.FromBytes(output.ToArray());
		allocator = restored.TopLevelTypes.Single(type => type.Name == "BuddyAllocator");
		method = allocator.Methods.Single(method => method.Name == "GetNativeArray");

		Assert.That(allocator.IsSequentialLayout, Is.True);
		Assert.That(allocator.Fields.Select(field => field.Name!.ToString()), Is.EqualTo(new[] { "m_Data" }));
		Assert.That(method.FullName, Is.EqualTo(signature));
		Assert.That(method.GenericParameters.Count, Is.EqualTo(1));
		Assert.That(method.CilMethodBody!.Instructions[^1].OpCode, Is.EqualTo(CilOpCodes.Ret));
		IMethodDescriptor[] calls = method.CilMethodBody.Instructions.Select(instruction => instruction.Operand).OfType<IMethodDescriptor>().ToArray();
		Assert.That(calls.Select(call => call.Name!.ToString()), Is.EqualTo(new[]
		{
			"ConvertExistingDataToNativeArray", "GetTempUnsafePtrSliceHandle", "SetAtomicSafetyHandle",
		}));
		Assert.That(calls[^1].DeclaringType!.Scope!.Name!.ToString(), Is.EqualTo("UnityEngine.CoreModule"));
	}

	private static ModuleDefinition CreateModule()
	{
		ModuleDefinition module = new("Unity.RenderPipelines.Universal.Runtime.dll");
		AssemblyReference core = new("UnityEngine.CoreModule", new Version(0, 0, 0, 0));
		module.AssemblyReferences.Add(core);
		TypeReference nativeArray = new(module, core, "Unity.Collections", "NativeArray`1");
		GenericParameterSignature parameter = new(GenericParameterType.Method, 0);
		GenericInstanceTypeSignature arrayType = new(nativeArray, true, [parameter]);
		TypeReference utility = new(module, core, "Unity.Collections.LowLevel.Unsafe", "NativeArrayUnsafeUtility");
		MemberReference convert = new(utility, "ConvertExistingDataToNativeArray",
			MethodSignature.CreateStatic(arrayType, 1, [module.CorLibTypeFactory.IntPtr, module.CorLibTypeFactory.Int32, module.CorLibTypeFactory.Int32]));
		MethodSpecification convertInstance = new(convert, new GenericInstanceMethodSignature([parameter]));
		TypeDefinition allocator = new("UnityEngine.Rendering.Universal", "BuddyAllocator", TypeAttributes.SequentialLayout);
		module.TopLevelTypes.Add(allocator);
		allocator.Fields.Add(new FieldDefinition("m_Data", FieldAttributes.Private, module.CorLibTypeFactory.IntPtr));
		MethodDefinition method = new("GetNativeArray", MethodAttributes.Private,
			MethodSignature.CreateInstance(arrayType, 1, [module.CorLibTypeFactory.Int32, module.CorLibTypeFactory.Int32]));
		method.GenericParameters.Add(new GenericParameter("T"));
		allocator.Methods.Add(method);
		method.CilMethodBody = new CilMethodBody();
		method.CilMethodBody.Instructions.Add(CilOpCodes.Ldc_I4_0);
		method.CilMethodBody.Instructions.Add(CilOpCodes.Conv_I);
		method.CilMethodBody.Instructions.Add(CilOpCodes.Ldarg_1);
		method.CilMethodBody.Instructions.Add(CilOpCodes.Ldc_I4_1);
		method.CilMethodBody.Instructions.Add(CilOpCodes.Call, convertInstance);
		method.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		return module;
	}
}
