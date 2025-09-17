using AssetRipper.Assets;
using AssetRipper.IO.Endian;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass506_FixUnityConnectSettings
{
	/// <summary>
	/// On some platforms, the UnityConnectSettings asset doesn't contain any settings. As this is platform specific, it is not reflected in the type trees.
	/// </summary>
	public static void DoPass()
	{
		foreach (GeneratedClassInstance instance in SharedState.Instance.ClassGroups[310].Instances)
		{
			FixMethod(instance.Type.Methods.Single(m => m.Name == nameof(UnityAssetBase.ReadRelease)));
			FixMethod(instance.Type.Methods.Single(m => m.Name == nameof(UnityAssetBase.ReadEditor)));
		}
	}

	private static void FixMethod(MethodDefinition method)
	{
		CilInstructionCollection instructions = method.GetInstructions();
		InsertInstructions(instructions, FindInsertionPoint(instructions));
	}

	private static int FindInsertionPoint(CilInstructionCollection instructions)
	{
		for (int i = 0; i < instructions.Count; i++)
		{
			CilInstruction instruction = instructions[i];
			if (instruction.OpCode == CilOpCodes.Ldfld
				&& instruction.Operand is FieldDefinition field
				&& (field.Name == "m_UnityPurchasingSettings" || field.Name == "m_CrashReportingSettings"))
			{
				//ldarg.0  asset
				//ldfld    field
				//ldarg.1  reader
				//callvirt ReadRelease

				return i - 1;
			}
		}
		throw new Exception("Could not determine the insertion point");
	}

	private static void InsertInstructions(CilInstructionCollection instructions, int insertionPoint)
	{
		ICilLabel returnLabel = instructions[instructions.Count - 1].CreateLabel();
		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Brtrue, returnLabel));
		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Ceq));

		IMethodDefOrRef getLength = SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == $"get_{nameof(EndianSpanReader.Length)}");
		IMethodDefOrRef getPosition = SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == $"get_{nameof(EndianSpanReader.Position)}");

		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Call, getLength));
		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Ldarg_1));

		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Call, getPosition));
		instructions.Insert(insertionPoint, new CilInstruction(CilOpCodes.Ldarg_1));

		instructions.OptimizeMacros();
	}
}
