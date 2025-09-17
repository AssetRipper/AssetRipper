using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass505_FixOldAudioClip
{
	public static void DoPass()
	{
		TypeDefinition helperType = SharedState.Instance.InjectHelperType(typeof(AudioClipHelper));
		MethodDefinition helperMethod = helperType.Methods.Single(m => m.Name == nameof(AudioClipHelper.ReadOldByteArray));

		foreach (GeneratedClassInstance instance in SharedState.Instance.ClassGroups[83].Instances)
		{
			if (instance.TryGetStreamField(out FieldDefinition? streamField))
			{
				MethodDefinition readReleaseMethod = instance.Type.Methods.Single(m => m.Name == nameof(UnityObjectBase.ReadRelease));
				MethodDefinition readEditorMethod = instance.Type.Methods.Single(m => m.Name == nameof(UnityObjectBase.ReadEditor));
				FieldDefinition dataField = instance.Type.GetFieldByName("m_AudioData");
				FixMethod(readReleaseMethod, helperMethod, dataField, streamField);
				FixMethod(readEditorMethod, helperMethod, dataField, streamField);
			}
		}
	}

	private static void FixMethod(MethodDefinition readMethod, MethodDefinition helperMethod, FieldDefinition dataField, FieldDefinition streamField)
	{
		CilInstructionCollection instructions = readMethod.GetInstructions();

		//remove bad instructions
		while (instructions.Count > 0)
		{
			int index = instructions.Count - 1;
			CilInstruction instruction = instructions[index];
			instructions.RemoveAt(index);
			if (instruction.OpCode == CilOpCodes.Ldarg_0)
			{
				break;
			}
		}

		instructions.Add(CilOpCodes.Ldarg_0);//for the store field

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, streamField);
		instructions.Add(CilOpCodes.Call, helperMethod);

		instructions.Add(CilOpCodes.Stfld, dataField);

		instructions.Add(CilOpCodes.Ret);
	}

	private static bool TryGetStreamField(this GeneratedClassInstance instance, [NotNullWhen(true)] out FieldDefinition? streamField)
	{
		if (instance.Type.TryGetFieldByName("m_Stream", out streamField))
		{
			CorLibTypeSignature? fieldType = streamField.Signature?.FieldType as CorLibTypeSignature;
			if (fieldType is not null && fieldType.ElementType == ElementType.I4)
			{
				return true;
			}
			else
			{
				streamField = null;
				return false;
			}
		}
		else
		{
			return false;
		}
	}
}
