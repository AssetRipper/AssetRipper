using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Optimizers;

/// <summary>
/// Turns loops into for loop
/// </summary>
public class USILForLoopOptimizer : IUSILOptimizer
{
	public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		bool changes = false;

		changes |= ReplaceForLoop(shader);

		return changes;
	}

	// Loop
	// BreakConditional (If / Break / EndIf)
	// ...
	// Add
	// EndLoop
	// ->
	// ForLoop
	// ...
	// EndLoop
	private bool ReplaceForLoop(UShaderProgram shader)
	{
		bool changes = false;
		Stack<LoopInstanceInfo> loopInfos = new();

		int loopDepth = 0;

		List<USILInstruction> insts = shader.instructions;
		for (int i = 0; i < insts.Count - 5; i++)
		{
			// do detection

			if (insts[i].instructionType == USILInstructionType.EndLoop)
			{
				loopDepth--;
				if (loopInfos.Count > 0 && loopInfos.Peek().loopDepth == loopDepth)
				{
					// didn't match and we're exiting this loop now
					loopInfos.Pop();
				}
			}

			bool startOpcodesMatch =
				insts[i].instructionType == USILInstructionType.Loop &&
				IsComparisonInstruction(insts[i + 1]) &&
				IsIfInstruction(insts[i + 2]) &&
				insts[i + 3].instructionType == USILInstructionType.Break &&
				insts[i + 4].instructionType == USILInstructionType.EndIf;

			if (startOpcodesMatch)
			{
				USILInstruction compInst = insts[i + 1];
				USILInstruction ifInst = insts[i + 2];

				// todo doesn't check for iterator in other side of comparison
				bool breakcUsesComp =
					compInst.destOperand!.registerIndex == ifInst.srcOperands[0].registerIndex &&
					DoMasksMatch(compInst.destOperand.mask, ifInst.srcOperands[0].mask);

				if (breakcUsesComp)
				{
					USILOperand iterRegOp = new USILOperand(compInst.srcOperands[0]);
					USILOperand compOp = new USILOperand(compInst.srcOperands[1]);
					USILInstructionType compType = compInst.instructionType;
					bool isInt = compInst.isIntVariant;
					bool isUnsigned = compInst.isIntUnsigned;

					LoopInstanceInfo loopInfo = new LoopInstanceInfo(iterRegOp, compOp, compType, isInt, isUnsigned, i, loopDepth);
					loopInfos.Push(loopInfo);
				}
			}

			bool endOpcodesMatch =
				IsAddInstruction(insts[i]) &&
				insts[i + 1].instructionType == USILInstructionType.EndLoop;

			if (endOpcodesMatch)
			{
				if (loopInfos.Count > 0 && loopInfos.Peek().loopDepth == loopDepth - 1)
				{
					LoopInstanceInfo loopInfo = loopInfos.Pop();

					int startIndex = loopInfo.startIndex;
					USILInstruction forLoopInst = insts[startIndex];
					USILInstruction addIterInst = insts[i];

					USILNumberType numberType;
					float addCount;

					if (addIterInst.isIntVariant)
					{
						if (addIterInst.isIntUnsigned)
						{
							numberType = USILNumberType.UnsignedInt;
						}
						else
						{
							numberType = USILNumberType.Int;
						}

						addCount = addIterInst.srcOperands[1].immValueInt[0];
					}
					else
					{
						numberType = USILNumberType.Float;
						addCount = addIterInst.srcOperands[1].immValueFloat[0];
					}

					if (addIterInst.instructionType == USILInstructionType.Subtract)
					{
						addCount *= -1;
					}

					forLoopInst.instructionType = USILInstructionType.ForLoop;
					forLoopInst.srcOperands = new List<USILOperand>
					{
						loopInfo.iterRegOp,
						loopInfo.compOp,
						new USILOperand((int)InvertCompareType(loopInfo.compType)),
						new USILOperand((int)numberType),
						new USILOperand(addCount),
						new USILOperand(loopDepth - 1)
					};

					insts.RemoveAt(i); // Add/Subtract
					insts.RemoveAt(startIndex + 1); // Compare
					insts.RemoveAt(startIndex + 1); // If
					insts.RemoveAt(startIndex + 1); // Break
					insts.RemoveAt(startIndex + 1); // EndIf

					i -= 4 - 1; // move iterator back for these five

					changes = true;
				}
			}

			if (loopInfos.Count > 0)
			{
				List<USILOperand> allOperands = GetAllOperands(insts[i]);
				foreach (USILOperand op in allOperands)
				{
					// todo: split mask from instruction if more than one component
					if (op.operandType == USILOperandType.TempRegister && op.mask.Length == 1)
					{
						foreach (LoopInstanceInfo loopInfo in loopInfos)
						{
							USILOperand iterRegOp = loopInfo.iterRegOp;
							bool matchesIter = op.registerIndex == iterRegOp.registerIndex &&
								op.mask[0] == iterRegOp.mask[0];

							if (!matchesIter)
							{
								break;
							}

							op.metadataName = USILConstants.ITER_CHARS[loopInfo.loopDepth].ToString();
							op.metadataNameAssigned = true;

							op.displayMask = false;
						}
					}
				}
			}

			if (insts[i].instructionType == USILInstructionType.Loop)
			{
				loopDepth++;
			}
		}
		return changes;
	}

	private static bool IsComparisonInstruction(USILInstruction instruction)
	{
		switch (instruction.instructionType)
		{
			case USILInstructionType.Equal:
			case USILInstructionType.NotEqual:
			case USILInstructionType.GreaterThan:
			case USILInstructionType.GreaterThanOrEqual:
			case USILInstructionType.LessThan:
			case USILInstructionType.LessThanOrEqual:
				return true;
			default:
				return false;
		}
	}

	private static bool IsIfInstruction(USILInstruction instruction)
	{
		switch (instruction.instructionType)
		{
			case USILInstructionType.IfTrue:
			case USILInstructionType.IfFalse:
				return true;
			default:
				return false;
		}
	}

	private static bool IsAddInstruction(USILInstruction instruction)
	{
		switch (instruction.instructionType)
		{
			case USILInstructionType.Add:
			case USILInstructionType.Subtract:
				return true;
			default:
				return false;
		}
	}

	private static USILInstructionType InvertCompareType(USILInstructionType type)
	{
		switch (type)
		{
			case USILInstructionType.LessThan:
				return USILInstructionType.GreaterThanOrEqual;
			case USILInstructionType.GreaterThan:
				return USILInstructionType.LessThanOrEqual;
			case USILInstructionType.LessThanOrEqual:
				return USILInstructionType.GreaterThan;
			case USILInstructionType.GreaterThanOrEqual:
				return USILInstructionType.LessThan;
			default:
				return type;
		}
	}

	private static bool DoOpcodesMatch(List<USILInstruction> insts, int startIndex, USILInstructionType[] instTypes)
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

	private static bool DoMasksMatch(int[] maskA, int[] maskB)
	{
		if (maskA.Length != maskB.Length)
		{
			return false;
		}

		for (int i = 0; i < maskB.Length; i++)
		{
			if (maskA[i] != maskB[i])
			{
				return false;
			}
		}
		return true;
	}

	private List<USILOperand> GetAllOperands(USILInstruction inst)
	{
		List<USILOperand> operands = new List<USILOperand>();

		if (inst.destOperand != null)
		{
			operands.AddRange(GetAllOperands(inst.destOperand));
		}

		if (inst.srcOperands != null)
		{
			foreach (USILOperand srcOp in inst.srcOperands)
			{
				operands.AddRange(GetAllOperands(srcOp));
			}
		}

		return operands;
	}

	private List<USILOperand> GetAllOperands(USILOperand operand)
	{
		if (operand.arrayRelative == null && (operand.children == null || operand.children.Length == 0))
		{
			return new List<USILOperand> { operand };
		}

		List<USILOperand> operands = new()
		{
			operand
		};

		if (operand.arrayRelative != null)
		{
			operands.AddRange(GetAllOperands(operand.arrayRelative));
		}

		if (operand.children != null)
		{
			foreach (USILOperand child in operand.children)
			{
				operands.AddRange(GetAllOperands(child));
			}
		}

		return operands;
	}

	class LoopInstanceInfo
	{
		public USILOperand iterRegOp;
		public USILOperand compOp;
		public USILInstructionType compType;
		public bool isInt;
		public bool isUnsigned;
		public int startIndex;
		public int loopDepth;

		public LoopInstanceInfo(
			USILOperand iterRegOp, USILOperand compOp, USILInstructionType compType,
			bool isInt, bool isUnsigned, int startIndex, int loopDepth)
		{
			this.iterRegOp = iterRegOp;
			this.compOp = compOp;
			this.compType = compType;
			this.isInt = isInt;
			this.isUnsigned = isUnsigned;
			this.startIndex = startIndex;
			this.loopDepth = loopDepth;
		}
	}
}
