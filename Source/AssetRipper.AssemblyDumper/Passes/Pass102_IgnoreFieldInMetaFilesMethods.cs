using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass102_IgnoreFieldInMetaFilesMethods
{
	private static Dictionary<GeneratedClassInstance, string[]> ClassesWithFields { get; } = new();

	public static void DoPass()
	{
		FillCollections();

		IMethodDefOrRef stringEqualsMethod = SharedState.Instance.Importer.ImportMethod(typeof(string), m => m.Name == "op_Equality");

		foreach (GeneratedClassInstance instance in SharedState.Instance.AllInstances)
		{
			if (NeedsMethodEmitted(instance))
			{
				MethodDefinition method = instance.Type.AddMethod(nameof(UnityAssetBase.IgnoreFieldInMetaFiles), Pass063_CreateEmptyMethods.OverrideMethodAttributes, SharedState.Instance.Importer.Boolean);
				method.AddParameter(SharedState.Instance.Importer.String, "fieldName");
				CilInstructionCollection instructions = method.GetInstructions();
				if (ClassesWithFields.TryGetValue(instance, out string[]? fields))
				{
					CilInstructionLabel trueLabel = new();
					CilInstructionLabel returnLabel = new();

					for (int i = 0; i < fields.Length; i++)
					{
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldstr, fields[i]);
						instructions.Add(CilOpCodes.Call, stringEqualsMethod);
						if (i == fields.Length - 1)
						{
							instructions.Add(CilOpCodes.Br, returnLabel);
						}
						else
						{
							instructions.Add(CilOpCodes.Brtrue, trueLabel);
						}
					}

					trueLabel.Instruction = instructions.Add(CilOpCodes.Ldc_I4_1);
					returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
				}
				else
				{
					instructions.Add(CilOpCodes.Ldc_I4_0);
					instructions.Add(CilOpCodes.Ret);
				}
			}
		}
	}

	private static bool NeedsMethodEmitted(GeneratedClassInstance instance)
	{
		if (instance.Base is null)
		{
			return ClassesWithFields.ContainsKey(instance);
		}

		if (ClassesWithFields.TryGetValue(instance, out string[]? derivedFields))
		{
			if (ClassesWithFields.TryGetValue(instance.Base, out string[]? baseFields))
			{
				if (derivedFields.Length != baseFields.Length)
				{
					return true;
				}

				foreach (string field in derivedFields)
				{
					if (Array.IndexOf(baseFields, field) < 0)
					{
						return true;
					}
				}

				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			return ClassesWithFields.ContainsKey(instance.Base);
		}
	}

	private static void FillCollections()
	{
		ClassesWithFields.Clear();
		List<string> fields = new();
		foreach (GeneratedClassInstance instance in SharedState.Instance.AllInstances)
		{
			if (instance.Class.EditorRootNode is null)
			{
				continue;
			}
			fields.Clear();
			foreach (UniversalNode node in instance.Class.EditorRootNode.SubNodes)
			{
				if (node.IgnoreInMetaFiles)
				{
					fields.Add(node.OriginalName);
				}
			}
			if (fields.Count > 0)
			{
				ClassesWithFields.Add(instance, fields.ToArray());
			}
		}
	}
}
