using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Numerics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass206_BoneWeights4ExplicitConversions
{
	public static void DoPass()
	{
		AddConversion(SharedState.Instance.SubclassGroups["BoneWeights4"]);
	}

	private static void AddConversion(SubclassGroup group)
	{
		foreach (TypeDefinition type in group.Types)
		{
			AddConversion(type);
			AddReverseConversion(type);
		}
	}

	private static void AddConversion(TypeDefinition type)
	{
		TypeSignature commonType = SharedState.Instance.Importer.ImportTypeSignature<BoneWeight4>();

		IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportConstructor<BoneWeight4>(8);

		MethodDefinition method = type.AddEmptyConversion(type.ToTypeSignature(), commonType, true);
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_Weight_0_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_Weight_1_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_Weight_2_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_Weight_3_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_BoneIndex_0_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_BoneIndex_1_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_BoneIndex_2_"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "get_BoneIndex_3_"));

		instructions.Add(CilOpCodes.Newobj, constructor);

		instructions.Add(CilOpCodes.Ret);
	}

	private static void AddReverseConversion(TypeDefinition type)
	{
		TypeSignature commonType = SharedState.Instance.Importer.ImportTypeSignature<BoneWeight4>();

		MethodDefinition constructor = type.GetDefaultConstructor();

		MethodDefinition method = type.AddEmptyConversion(commonType, type.ToTypeSignature(), false);
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Newobj, constructor);

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Index0)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_BoneIndex_0_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Index1)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_BoneIndex_1_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Index2)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_BoneIndex_2_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Index3)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_BoneIndex_3_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Weight0)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_Weight_0_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Weight1)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_Weight_1_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Weight2)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_Weight_2_"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga_S, method.Parameters[0]);
		instructions.Add(CilOpCodes.Call, SharedState.Instance.Importer.ImportMethod<BoneWeight4>(m => m.Name == $"get_{nameof(BoneWeight4.Weight3)}"));
		instructions.Add(CilOpCodes.Call, type.Methods.Single(m => m.Name == "set_Weight_3_"));

		instructions.Add(CilOpCodes.Ret);
	}
}
