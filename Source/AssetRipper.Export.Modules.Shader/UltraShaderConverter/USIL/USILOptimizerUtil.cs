namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public static class USILOptimizerUtil
{
	public static bool DoOpcodesMatch(List<USILInstruction> insts, int startIndex, USILInstructionType[] instTypes)
	{
		if (startIndex + instTypes.Length > insts.Count)
		{
			return false;
		}

		for (int i = 0; i < instTypes.Length; i++)
		{
			if (insts[startIndex + i].instructionType != instTypes[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool DoMasksMatch(USILOperand operand, int[] mask)
	{
		if (operand.mask.Length != mask.Length)
		{
			return false;
		}

		for (int i = 0; i < mask.Length; i++)
		{
			if (operand.mask[i] != mask[i])
			{
				return false;
			}
		}
		return true;
	}
}
