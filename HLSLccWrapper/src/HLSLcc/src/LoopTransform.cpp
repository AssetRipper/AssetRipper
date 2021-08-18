#include "src/internal_includes/HLSLCrossCompilerContext.h"
#include "src/internal_includes/LoopTransform.h"
#include "src/internal_includes/Shader.h"
#include "src/internal_includes/debug.h"
#include <algorithm>
#include <vector>
#include <list>

namespace HLSLcc
{
    struct LoopInfo
    {
    public:
        LoopInfo() : m_StartLoop(0), m_EndLoop(0), m_ExitPoints(), m_IsSwitch(false) {}

        Instruction *   m_StartLoop; // OPCODE_LOOP
        Instruction *   m_EndLoop;   // OPCODE_ENDLOOP that matches the LOOP above.
        std::vector<Instruction *> m_ExitPoints; // Any BREAK/RET/BREAKC instructions within the same loop depth
        bool            m_IsSwitch; // True if this is a switch-case and not a LOOP/ENDLOOP pair. Used as a helper when parsing.
    };

    typedef std::list<LoopInfo> Loops;

    // Build a loopinfo array of all the loops in this shader phase
    void BuildLoopInfo(ShaderPhase &phase, Loops &res)
    {
        using namespace std;
        res.clear();

        // A stack of loopinfo elements (stored in res)
        list<LoopInfo *> loopStack;

        // Storage for dummy LoopInfo elements to be used for switch-cases. We don't want them cluttering the Loops list so store them here.
        list<LoopInfo> dummyLIForSwitches;

        for (std::vector<Instruction>::iterator instItr = phase.psInst.begin(); instItr != phase.psInst.end(); instItr++)
        {
            Instruction *i = &*instItr;

            if (i->eOpcode == OPCODE_LOOP)
            {
                LoopInfo *currLoopInfo = &*res.insert(res.end(), LoopInfo());
                currLoopInfo->m_StartLoop = i;
                loopStack.push_front(currLoopInfo);
            }
            else if (i->eOpcode == OPCODE_ENDLOOP)
            {
                ASSERT(!loopStack.empty());
                LoopInfo *li = *loopStack.begin();
                loopStack.pop_front();
                li->m_EndLoop = i;
            }
            else if (i->eOpcode == OPCODE_SWITCH)
            {
                // Create a dummy entry into the stack
                LoopInfo *li = &*dummyLIForSwitches.insert(dummyLIForSwitches.end(), LoopInfo());
                li->m_IsSwitch = true;
                loopStack.push_front(li);
            }
            else if (i->eOpcode == OPCODE_ENDSWITCH)
            {
                ASSERT(!loopStack.empty());
                LoopInfo *li = *loopStack.begin();
                loopStack.pop_front();
                ASSERT(li->m_IsSwitch);
            }
            else if (i->eOpcode == OPCODE_BREAK || i->eOpcode == OPCODE_BREAKC)
            {
                // Get the current loopstack head
                ASSERT(!loopStack.empty());
                LoopInfo *li = *loopStack.begin();
                // Ignore breaks from switch-cases
                if (!li->m_IsSwitch)
                {
                    li->m_ExitPoints.push_back(i);
                }
            }
        }
    }

    // Returns true if the given instruction is a non-vectorized int or uint comparison instruction that reads from at least one temp and writes to a temp
    static bool IsScalarTempComparisonInstruction(const Instruction *i)
    {
        switch (i->eOpcode)
        {
            default:
                return false;
            case OPCODE_IGE:
            case OPCODE_ILT:
            case OPCODE_IEQ:
            case OPCODE_INE:
            case OPCODE_UGE:
            case OPCODE_ULT:
                break;
        }

        if (i->asOperands[0].eType != OPERAND_TYPE_TEMP)
            return false;

        int tempOp = -1;
        if (i->asOperands[1].eType == OPERAND_TYPE_TEMP)
            tempOp = 1;
        else if (i->asOperands[2].eType == OPERAND_TYPE_TEMP)
            tempOp = 2;

        // Also reject comparisons where we compare temp.x vs temp.y
        if (i->asOperands[1].eType == OPERAND_TYPE_TEMP && i->asOperands[2].eType == OPERAND_TYPE_TEMP && i->asOperands[1].ui32RegisterNumber == i->asOperands[2].ui32RegisterNumber)
            return false;

        if (tempOp == -1)
            return false;

        if (i->asOperands[0].GetNumSwizzleElements() != 1)
            return false;

        return true;
    }

    // Returns true iff both instructions perform identical operation. For the purposes of Loop transformation, we only consider operations of type tX = tX <op> imm32
    static bool AreInstructionsIdentical(const Instruction *a, const Instruction *b)
    {
        if (a->eOpcode != b->eOpcode)
            return false;
        ASSERT(a->ui32NumOperands == b->ui32NumOperands);
        uint32_t dstReg = 0;
        if (a->asOperands[0].eType != OPERAND_TYPE_TEMP)
            return false;
        dstReg = a->asOperands[0].ui32RegisterNumber;

        for (uint32_t i = 0; i < a->ui32NumOperands; i++)
        {
            const Operand &aop = a->asOperands[i];
            const Operand &bop = b->asOperands[i];
            if (aop.eType != bop.eType)
                return false;

            if (aop.GetAccessMask() != bop.GetAccessMask())
                return false;

            if (aop.GetNumSwizzleElements() != 1)
                return false;

            if (aop.eType == OPERAND_TYPE_TEMP)
            {
                if (aop.ui32RegisterNumber != bop.ui32RegisterNumber)
                    return false;
                if (aop.ui32RegisterNumber != dstReg)
                    return false;
            }
            else if (aop.eType == OPERAND_TYPE_IMMEDIATE32)
            {
                if (memcmp(aop.afImmediates, bop.afImmediates, 4 * sizeof(float)) != 0)
                    return false;
            }
        }
        return true;
    }

    // Attempt to transform a single loop into a for-statement
    static void AttemptLoopTransform(HLSLCrossCompilerContext *psContext, ShaderPhase &phase, LoopInfo &li)
    {
        // In order to transform a loop into a for, the following has to hold:
        // - The loop must start with a comparison instruction where one of the src operands is a temp (induction variable), followed by OPCODE_BREAKC.
        // - The loop must end with an arithmetic operation (SUB or ADD) where the dest operand is the same temp as one of the sources in the comparison instruction above
        // Additionally, if the loop induction variable is initialized before the start of the loop and it has only uses inside the LOOP/ENDLOOP pair, we can declare that inside the for statement.
        // Also, the loop induction variable must be standalone (as in, never used as part of a larger vector)

        Instruction *cmpInst = li.m_StartLoop + 1;

        if (!IsScalarTempComparisonInstruction(cmpInst))
            return;

        Instruction *breakInst = li.m_StartLoop + 2;
        if (breakInst->eOpcode != OPCODE_BREAKC)
            return;
        if (breakInst->asOperands[0].eType != OPERAND_TYPE_TEMP)
            return;
        if (breakInst->asOperands[0].ui32RegisterNumber != cmpInst->asOperands[0].ui32RegisterNumber)
            return;

        // Check that the comparison result isn't used anywhere else
        if (cmpInst->m_Uses.size() != 1)
            return;

        ASSERT(cmpInst->m_Uses[0].m_Inst == breakInst);

        // Ok, at least we have the comparison + breakc combo at top. Try to find the induction variable
        uint32_t inductionVarIdx = 0;

        Instruction *lastInst = li.m_EndLoop - 1;
        if (lastInst->eOpcode != OPCODE_IADD)
            return;
        if (lastInst->asOperands[0].eType != OPERAND_TYPE_TEMP)
            return;

        if (lastInst->asOperands[0].GetNumSwizzleElements() != 1)
            return;

        uint32_t indVar = lastInst->asOperands[0].ui32RegisterNumber;
        // Verify that the induction variable actually matches.
        if (cmpInst->asOperands[1].eType == OPERAND_TYPE_TEMP && cmpInst->asOperands[1].ui32RegisterNumber == indVar)
            inductionVarIdx = 1;
        else if (cmpInst->asOperands[2].eType == OPERAND_TYPE_TEMP && cmpInst->asOperands[2].ui32RegisterNumber == indVar)
            inductionVarIdx = 2;
        else
            return;

        // Verify that we also read from the induction variable in the last instruction
        if (!((lastInst->asOperands[1].eType == OPERAND_TYPE_TEMP && lastInst->asOperands[1].ui32RegisterNumber == indVar) ||
              (lastInst->asOperands[2].eType == OPERAND_TYPE_TEMP && lastInst->asOperands[2].ui32RegisterNumber == indVar)))
            return;

        // Nvidia compiler bug workaround: The shader compiler tries to be smart and unrolls constant loops,
        // but then fails miserably if the loop variable is used as an index to UAV loads/stores or some other cases ("array access too complex")
        // This is also triggered when the driver optimizer sees "simple enough" arithmetics (whatever that is) done on the loop variable before indexing.
        // So, disable for-loop transformation altogether whenever we see a UAV load or store inside a loop.
        if (psContext->psShader->eTargetLanguage >= LANG_400 && psContext->psShader->eTargetLanguage < LANG_GL_LAST && !psContext->IsVulkan())
        {
            for (auto itr = li.m_StartLoop; itr != li.m_EndLoop; itr++)
            {
                switch (itr->eOpcode)
                {
                    case OPCODE_LD_RAW:
                    case OPCODE_LD_STRUCTURED:
                    case OPCODE_LD_UAV_TYPED:
                    case OPCODE_STORE_RAW:
                    case OPCODE_STORE_STRUCTURED:
                    case OPCODE_STORE_UAV_TYPED:
                        return; // Nope, can't do a for, not even a partial one.
                    default:
                        break;
                }
            }
        }

        // One more thing to check: The comparison input may only see 1 definition that originates from inside the loop range: the one in lastInst.
        // Anything else means that there's a continue statement, or another break/breakc and that means that lastInst wouldn't get called.
        // Of course, if all those instructions are identical, then it's fine.
        // Ideally, if there's only one definition that's from outside the loop range, then we can use that as the initializer, as well.

        Instruction *initializer = NULL;
        std::vector<const Operand::Define *> definitionsOutsideRange;
        std::vector<const Operand::Define *> definitionsInsideRange;
        std::for_each(cmpInst->asOperands[inductionVarIdx].m_Defines.begin(), cmpInst->asOperands[inductionVarIdx].m_Defines.end(), [&](const Operand::Define &def)
        {
            if (def.m_Inst < li.m_StartLoop || def.m_Inst > li.m_EndLoop)
                definitionsOutsideRange.push_back(&def);
            else
                definitionsInsideRange.push_back(&def);
        });

        if (definitionsInsideRange.size() != 1)
        {
            // All definitions must be identical
            for (std::vector<const Operand::Define*>::iterator itr = definitionsInsideRange.begin() + 1; itr != definitionsInsideRange.end(); itr++)
            {
                if (!AreInstructionsIdentical((*itr)->m_Inst, definitionsInsideRange[0]->m_Inst))
                    return;
            }
        }

        ASSERT(definitionsOutsideRange.size() > 0);
        if (definitionsOutsideRange.size() == 1)
            initializer = definitionsOutsideRange[0]->m_Inst;

        // Initializer must only write to one component
        if (initializer && initializer->asOperands[0].GetNumSwizzleElements() != 1)
            initializer = 0;
        // Initializer data type must be int or uint
        if (initializer)
        {
            SHADER_VARIABLE_TYPE dataType = initializer->asOperands[0].GetDataType(psContext);
            if (dataType != SVT_INT && dataType != SVT_UINT)
                return;
        }

        // Check that the initializer is only used within the range so we can move it to for statement
        if (initializer)
        {
            bool hasUsesOutsideRange = false;
            std::for_each(initializer->m_Uses.begin(), initializer->m_Uses.end(), [&](const Instruction::Use &u)
            {
                if (u.m_Inst < li.m_StartLoop || u.m_Inst > li.m_EndLoop)
                    hasUsesOutsideRange = true;
            });
            // Has outside uses? we cannot pull that up to the for statement
            if (hasUsesOutsideRange)
                initializer = 0;
        }

        // Check that the loop adder instruction only has uses inside the loop range, otherwise we cannot move the initializer either
        if (initializer)
        {
            bool cannotDoInitializer = false;
            for (auto itr = lastInst->m_Uses.begin(); itr != lastInst->m_Uses.end(); itr++)
            {
                const Instruction::Use &u = *itr;
                if (u.m_Inst < li.m_StartLoop || u.m_Inst > li.m_EndLoop)
                {
                    cannotDoInitializer = true;
                    break;
                }
                // Also check that the uses are not vector ops (temp splitting has already pulled everything to .x if this is a standalone var)
                if (u.m_Op->GetAccessMask() != 1)
                {
                    cannotDoInitializer = true;
                    break;
                }
            }
            // Has outside uses? we cannot pull that up to the for statement
            if (cannotDoInitializer)
                initializer = 0;
        }


        if (initializer)
        {
            // We can declare the initializer in the for loop header, allocate a new number for it and change all uses into that.
            uint32_t newRegister = phase.m_NextFreeTempRegister++;
            li.m_StartLoop->m_InductorRegister = newRegister;
            std::for_each(initializer->m_Uses.begin(), initializer->m_Uses.end(), [newRegister](const Instruction::Use &u)
            {
                u.m_Op->m_ForLoopInductorName = newRegister;
            });
            // Also tweak the destinations for cmpInst, and lastInst
            if (cmpInst->asOperands[1].eType == OPERAND_TYPE_TEMP && cmpInst->asOperands[1].ui32RegisterNumber == initializer->asOperands[0].ui32RegisterNumber)
                cmpInst->asOperands[1].m_ForLoopInductorName = newRegister;
            else
                cmpInst->asOperands[2].m_ForLoopInductorName = newRegister;

            if (lastInst->asOperands[1].eType == OPERAND_TYPE_TEMP && lastInst->asOperands[1].ui32RegisterNumber == initializer->asOperands[0].ui32RegisterNumber)
                lastInst->asOperands[1].m_ForLoopInductorName = newRegister;
            else
                lastInst->asOperands[2].m_ForLoopInductorName = newRegister;

            lastInst->asOperands[0].m_ForLoopInductorName = newRegister;
            initializer->asOperands[0].m_ForLoopInductorName = newRegister;
        }

        // This loop can be transformed to for-loop. Do the necessary magicks.
        li.m_StartLoop->m_LoopInductors[0] = initializer;
        li.m_StartLoop->m_LoopInductors[1] = cmpInst;
        li.m_StartLoop->m_LoopInductors[2] = breakInst;
        li.m_StartLoop->m_LoopInductors[3] = lastInst;

        if (initializer)
            initializer->m_SkipTranslation = true;
        cmpInst->m_SkipTranslation = true;
        breakInst->m_SkipTranslation = true;
        lastInst->m_SkipTranslation = true;
    }

    void DoLoopTransform(HLSLCrossCompilerContext *psContext, ShaderPhase &phase)
    {
        Loops loops;
        BuildLoopInfo(phase, loops);

        std::for_each(loops.begin(), loops.end(), [&phase, psContext](LoopInfo &li)
        {
            // Some sanity checks: start and end points must be initialized, we shouldn't have any switches here, and each loop must have at least one exit point
            // Also that there's at least 2 instructions in loop body
            ASSERT(li.m_StartLoop != 0);
            ASSERT(li.m_EndLoop != 0);
            ASSERT(li.m_EndLoop > li.m_StartLoop + 2);
            ASSERT(!li.m_IsSwitch);
            ASSERT(!li.m_ExitPoints.empty());
            AttemptLoopTransform(psContext, phase, li);
        });
    }
}
