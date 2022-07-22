﻿using ShaderTextRestorer.ShaderBlob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShaderLabConvert.USILOptimizerUtil;

namespace ShaderLabConvert
{
    // note: cbuffers must be converted to matrix type by this point
	// it's a miracle when this works. there's so many issues with how this works fundamentally.
	/// <summary>
	/// Converts multiple multiply operations into a single matrix one
	/// "instruction"
	/// </summary>
    public class USILMatrixMulOptimizer : IUSILOptimizer
    {
        UShaderProgram _shader;
        ShaderSubProgram _shaderData;

        private readonly int[] XYZW_MASK = new int[] { 0, 1, 2, 3 };
        private readonly int[] XXXX_MASK = new int[] { 0, 0, 0, 0 };
        private readonly int[] YYYY_MASK = new int[] { 1, 1, 1, 1 };
        private readonly int[] ZZZZ_MASK = new int[] { 2, 2, 2, 2 };
        private readonly int[] WWWW_MASK = new int[] { 3, 3, 3, 3 };

        private readonly int[] XYZ_MASK = new int[] { 0, 1, 2 };
        private readonly int[] XXX_MASK = new int[] { 0, 0, 0 };
        private readonly int[] YYY_MASK = new int[] { 1, 1, 1 };
        private readonly int[] ZZZ_MASK = new int[] { 2, 2, 2 };

        public bool Run(UShaderProgram shader, ShaderSubProgram shaderData)
        {
            _shader = shader;
            _shaderData = shaderData;

            bool changes = false;

            changes |= ReplaceMulMatrixVec4W1();
            changes |= ReplaceMulMatrixVec4();
            changes |= ReplaceMulMatrixVec3();

            return changes;
        }

        // mat4x4 * vec4(vec3, 1)
        private bool ReplaceMulMatrixVec4W1()
        {
            bool changes = false;

            var insts = _shader.instructions;
            for (int i = 0; i < insts.Count - 3; i++)
            {
                // do detection

                bool opcodesMatch = DoOpcodesMatch(insts, i, new[] {
                    USILInstructionType.Multiply,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.Add
                });

                if (!opcodesMatch)
                    continue;

                USILInstruction inst0 = insts[i];
                USILInstruction inst1 = insts[i + 1];
                USILInstruction inst2 = insts[i + 2];
                USILInstruction inst3 = insts[i + 3];

                bool matricesCorrect =
                    inst0.srcOperands[1].operandType == USILOperandType.Matrix &&
                    inst0.srcOperands[1].arrayIndex == 1 &&
                    DoMasksMatch(inst0.srcOperands[1], XYZW_MASK) &&

                    inst1.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst1.srcOperands[0].arrayIndex == 0 &&
                    DoMasksMatch(inst1.srcOperands[0], XYZW_MASK) &&

                    inst2.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst2.srcOperands[0].arrayIndex == 2 &&
                    DoMasksMatch(inst2.srcOperands[0], XYZW_MASK) &&

                    inst3.srcOperands[1].operandType == USILOperandType.Matrix &&
                    inst3.srcOperands[1].arrayIndex == 3 &&
                    DoMasksMatch(inst3.srcOperands[1], XYZW_MASK);

                if (!matricesCorrect)
                    continue;

                int tmp0Index = inst0.destOperand.registerIndex;
                int tmp1Index = inst1.destOperand.registerIndex;
                int tmp2Index = inst2.destOperand.registerIndex;
                int tmp3Index = inst3.destOperand.registerIndex;

				// registers can swap halfway through to be used for something else
				// don't try to convert the matrix because we can't handle this yet
				if (tmp0Index != tmp1Index || tmp1Index != tmp2Index || tmp2Index != tmp3Index)
					continue;

                bool tempRegisterCorrect =
                    inst0.destOperand.registerIndex == tmp0Index &&
                    inst1.destOperand.registerIndex == tmp0Index &&
                    inst1.srcOperands[2].registerIndex == tmp0Index &&
                    inst2.srcOperands[2].registerIndex == tmp0Index &&

                    inst2.destOperand.registerIndex == tmp1Index &&
                    inst3.srcOperands[0].registerIndex == tmp1Index;

                if (!tempRegisterCorrect)
                    continue;

                // todo: input isn't guaranteed temp
                // todo: is input guaranteed to start at x?
                int inpIndex = inst0.srcOperands[0].registerIndex;
                bool inputsCorrect =
                    inst0.srcOperands[0].registerIndex == inpIndex &&
                    DoMasksMatch(inst0.srcOperands[0], YYYY_MASK) &&

                    inst1.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst1.srcOperands[1], XXXX_MASK) &&

                    inst2.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst2.srcOperands[1], ZZZZ_MASK);

                if (!inputsCorrect)
                    continue;

                // make replacement

                USILOperand mulInputVec3Operand = new USILOperand(inst0.srcOperands[0]);
                USILOperand mulInputMat4x4Operand = new USILOperand(inst0.srcOperands[1]);
                USILOperand mulOutputOperand = new USILOperand(inst3.destOperand);

                mulInputMat4x4Operand.displayMask = false;
                mulInputVec3Operand.mask = new int[] { 0, 1, 2 };

                USILOperand mulInput1Operand = new USILOperand()
                {
                    operandType = USILOperandType.ImmediateFloat,
                    immValueFloat = new[] { 1f },
                };

                USILOperand mulInputVec4Operand = new USILOperand()
                {
                    operandType = USILOperandType.Multiple,
                    children = new[] { mulInputVec3Operand, mulInput1Operand }
                };

                USILInstruction mulInstruction = new USILInstruction()
                {
                    instructionType = USILInstructionType.MultiplyMatrixByVector,
                    destOperand = mulOutputOperand,
                    srcOperands = new List<USILOperand> { mulInputMat4x4Operand, mulInputVec4Operand }
                };

                insts.RemoveRange(i, 4);
                insts.Insert(i, mulInstruction);

                changes = true;
            }
            return changes;
        }

        // mat4x4 * vec4
        private bool ReplaceMulMatrixVec4()
        {
            bool changes = false;

            var insts = _shader.instructions;
            for (int i = 0; i < insts.Count - 3; i++)
            {
                // do detection

                bool opcodesMatch = DoOpcodesMatch(insts, i, new[] {
                    USILInstructionType.Multiply,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.MultiplyAdd
                });

                if (!opcodesMatch)
                    continue;

                USILInstruction inst0 = insts[i];
                USILInstruction inst1 = insts[i + 1];
                USILInstruction inst2 = insts[i + 2];
                USILInstruction inst3 = insts[i + 3];

                bool matricesCorrect =
                    inst0.srcOperands[1].operandType == USILOperandType.Matrix &&
                    inst0.srcOperands[1].arrayIndex == 1 &&
                    DoMasksMatch(inst0.srcOperands[1], XYZW_MASK) &&

                    inst1.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst1.srcOperands[0].arrayIndex == 0 &&
                    DoMasksMatch(inst1.srcOperands[0], XYZW_MASK) &&

                    inst2.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst2.srcOperands[0].arrayIndex == 2 &&
                    DoMasksMatch(inst2.srcOperands[0], XYZW_MASK) &&

                    inst3.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst3.srcOperands[0].arrayIndex == 3 &&
                    DoMasksMatch(inst3.srcOperands[0], XYZW_MASK);

                if (!matricesCorrect)
                    continue;

				int tmp0Index = inst0.destOperand.registerIndex;
				int tmp1Index = inst1.destOperand.registerIndex;
				int tmp2Index = inst2.destOperand.registerIndex;
				int tmp3Index = inst3.destOperand.registerIndex;

				// registers can swap halfway through to be used for something else
				// don't try to convert the matrix because we can't handle this yet
				if (tmp0Index != tmp1Index || tmp1Index != tmp2Index || tmp2Index != tmp3Index)
					continue;

				int tmpIndex = inst0.destOperand.registerIndex;
                bool tempRegisterCorrect =
                    inst0.destOperand.registerIndex == tmpIndex &&

                    inst1.destOperand.registerIndex == tmpIndex &&
                    inst1.srcOperands[2].registerIndex == tmpIndex &&

                    inst2.destOperand.registerIndex == tmpIndex &&
                    inst2.srcOperands[2].registerIndex == tmpIndex &&

                    inst3.srcOperands[2].registerIndex == tmpIndex;

                if (!tempRegisterCorrect)
                    continue;

                // todo: input isn't guaranteed temp
                int inpIndex = inst0.srcOperands[0].registerIndex;
                bool inputsCorrect =
                    inst0.srcOperands[0].registerIndex == inpIndex &&
                    DoMasksMatch(inst0.srcOperands[0], YYYY_MASK) &&

                    inst1.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst1.srcOperands[1], XXXX_MASK) &&

                    inst2.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst2.srcOperands[1], ZZZZ_MASK) &&

                    inst3.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst3.srcOperands[1], WWWW_MASK);

                if (!inputsCorrect)
                    continue;

                // make replacement

                USILOperand mulInputVec4Operand = new USILOperand(inst0.srcOperands[0]);
                USILOperand mulInputMat4x4Operand = new USILOperand(inst0.srcOperands[1]);
                USILOperand mulOutputOperand = new USILOperand(inst3.destOperand);

                mulInputMat4x4Operand.displayMask = false;
                mulInputVec4Operand.mask = new int[] { 0, 1, 2, 3 };

                USILInstruction mulInstruction = new USILInstruction()
                {
                    instructionType = USILInstructionType.MultiplyMatrixByVector,
                    destOperand = mulOutputOperand,
                    srcOperands = new List<USILOperand> { mulInputMat4x4Operand, mulInputVec4Operand }
                };

                insts.RemoveRange(i, 4);
                insts.Insert(i, mulInstruction);

                changes = true;
            }
            return changes;
        }

        // mat3x3 * vec3
        private bool ReplaceMulMatrixVec3()
        {

            bool changes = false;

            var insts = _shader.instructions;
            for (int i = 0; i < insts.Count - 3; i++)
            {
                // do detection

                bool opcodesMatch = DoOpcodesMatch(insts, i, new[] {
                    USILInstructionType.Multiply,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.MultiplyAdd,
                    USILInstructionType.Add
                });

                if (!opcodesMatch)
                    continue;

                USILInstruction inst0 = insts[i];
                USILInstruction inst1 = insts[i + 1];
                USILInstruction inst2 = insts[i + 2];
                USILInstruction inst3 = insts[i + 3];

                bool matricesCorrect =
                    inst0.srcOperands[1].operandType == USILOperandType.Matrix &&
                    inst0.srcOperands[1].arrayIndex == 1 &&
                    DoMasksMatch(inst0.srcOperands[1], XYZ_MASK) &&

                    inst1.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst1.srcOperands[0].arrayIndex == 0 &&
                    DoMasksMatch(inst1.srcOperands[0], XYZ_MASK) &&

                    inst2.srcOperands[0].operandType == USILOperandType.Matrix &&
                    inst2.srcOperands[0].arrayIndex == 2 &&
                    DoMasksMatch(inst2.srcOperands[0], XYZ_MASK) &&

                    inst3.srcOperands[1].operandType == USILOperandType.Matrix &&
                    inst3.srcOperands[1].arrayIndex == 3 &&
                    DoMasksMatch(inst3.srcOperands[1], XYZ_MASK);

                if (!matricesCorrect)
                    continue;

				int tmp0Index = inst0.destOperand.registerIndex;
				int tmp1Index = inst1.destOperand.registerIndex;
				int tmp2Index = inst2.destOperand.registerIndex;
				int tmp3Index = inst3.destOperand.registerIndex;

				// registers can swap halfway through to be used for something else
				// don't try to convert the matrix because we can't handle this yet
				if (tmp0Index != tmp1Index || tmp1Index != tmp2Index || tmp2Index != tmp3Index)
					continue;

				bool tempRegisterCorrect =
                    inst0.destOperand.registerIndex == tmp0Index &&
                    inst1.destOperand.registerIndex == tmp0Index &&
                    inst1.srcOperands[2].registerIndex == tmp0Index &&
                    inst2.srcOperands[2].registerIndex == tmp0Index &&

                    inst2.destOperand.registerIndex == tmp1Index &&
                    inst3.srcOperands[0].registerIndex == tmp1Index;

                if (!tempRegisterCorrect)
                    continue;

                // todo: input isn't guaranteed temp
                // todo: is input guaranteed to start at x?
                int inpIndex = inst0.srcOperands[0].registerIndex;
                bool inputsCorrect =
                    inst0.srcOperands[0].registerIndex == inpIndex &&
                    DoMasksMatch(inst0.srcOperands[0], YYY_MASK) &&

                    inst1.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst1.srcOperands[1], XXX_MASK) &&

                    inst2.srcOperands[1].registerIndex == inpIndex &&
                    DoMasksMatch(inst2.srcOperands[1], ZZZ_MASK);

                if (!inputsCorrect)
                    continue;

                // make replacement

                USILOperand mulInputVec3Operand = new USILOperand(inst0.srcOperands[0]);
                USILOperand mulInputMat3x3Operand = new USILOperand(inst0.srcOperands[1]);
                USILOperand mulOutputOperand = new USILOperand(inst3.destOperand);

                mulInputMat3x3Operand.displayMask = false;
                mulInputVec3Operand.mask = new int[] { 0, 1, 2 };

                USILInstruction mulInstruction = new USILInstruction()
                {
                    instructionType = USILInstructionType.MultiplyMatrixByVector,
                    destOperand = mulOutputOperand,
                    srcOperands = new List<USILOperand> { mulInputMat3x3Operand, mulInputVec3Operand }
                };

                insts.RemoveRange(i, 4);
                insts.Insert(i, mulInstruction);

                changes = true;
            }
            return changes;
        }
    }
}
