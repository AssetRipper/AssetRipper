#include "ControlFlowGraphUtils.h"

#include "internal_includes/debug.h"
#include "internal_includes/Instruction.h"
#include "internal_includes/Operand.h"


// Get the next instruction that's not one of CASE, DEFAULT, LOOP, ENDSWITCH
const Instruction *HLSLcc::ControlFlow::Utils::GetNextNonLabelInstruction(const Instruction *psStart, bool *sawEndSwitch /*= 0*/)
{
    const Instruction *inst = psStart;
    // Skip CASE/DEFAULT/ENDSWITCH/LOOP labels
    while (inst->eOpcode == OPCODE_CASE || inst->eOpcode == OPCODE_DEFAULT || inst->eOpcode == OPCODE_ENDSWITCH || inst->eOpcode == OPCODE_LOOP)
    {
        // We really shouldn't be seeing ENDSWITCH without sawEndSwitch being set (as in, we're expecting it)
        ASSERT(inst->eOpcode != OPCODE_ENDSWITCH || sawEndSwitch != NULL);
        if (inst->eOpcode == OPCODE_ENDSWITCH && sawEndSwitch != NULL)
            *sawEndSwitch = true;
        inst++;
    }
    return inst;
}

// For a given flow-control instruction, find the corresponding jump location:
// If the input is OPCODE_IF, then find the next same-level ELSE or ENDIF +1
// For ELSE, find same level ENDIF + 1
// For BREAK/BREAKC, find next ENDLOOP or ENDSWITCH + 1
// For SWITCH, find next same-level CASE/DEFAULT (skip multiple consecutive case/default labels) or ENDSWITCH + 1
// For ENDLOOP, find previous same-level LOOP + 1
// For CASE/DEFAULT, find next same-level CASE/DEFAULT or ENDSWITCH + 1, skip multiple consecutive case/default labels
// For CONTINUE/C the previous LOOP + 1
// Note that LOOP/ENDSWITCH itself is nothing but a label but it still starts a new basic block.
// Note that CASE labels fall through.
// Always returns the beginning of the next block, so skip multiple CASE/DEFAULT labels etc.
const Instruction * HLSLcc::ControlFlow::Utils::GetJumpPoint(const Instruction *psStart, bool *sawEndSwitch /*= 0*/, bool *needConnectToParent /* = 0*/)
{
    const Instruction *inst = psStart;
    int depth = 0;
    OPCODE_TYPE op = psStart->eOpcode;
    ASSERT(op == OPCODE_IF || op == OPCODE_ELSE || op == OPCODE_BREAK || op == OPCODE_BREAKC
        || op == OPCODE_SWITCH || op == OPCODE_CASE || op == OPCODE_DEFAULT
        || op == OPCODE_ENDLOOP || op == OPCODE_CONTINUE || op == OPCODE_CONTINUEC);

    switch (op)
    {
        default:
            ASSERT(0);
            break;
        case OPCODE_IF:
        case OPCODE_ELSE:
            while (1)
            {
                inst++;
                if ((inst->eOpcode == OPCODE_ELSE || inst->eOpcode == OPCODE_ENDIF) && (depth == 0))
                {
                    return GetNextNonLabelInstruction(inst + 1, sawEndSwitch);
                }
                if (inst->eOpcode == OPCODE_IF)
                    depth++;
                if (inst->eOpcode == OPCODE_ENDIF)
                    depth--;
            }
        case OPCODE_BREAK:
        case OPCODE_BREAKC:
            while (1)
            {
                inst++;
                if ((inst->eOpcode == OPCODE_ENDLOOP || inst->eOpcode == OPCODE_ENDSWITCH) && (depth == 0))
                {
                    return GetNextNonLabelInstruction(inst + 1, sawEndSwitch);
                }
                if (inst->eOpcode == OPCODE_SWITCH || inst->eOpcode == OPCODE_LOOP)
                    depth++;
                if (inst->eOpcode == OPCODE_ENDSWITCH || inst->eOpcode == OPCODE_ENDLOOP)
                    depth--;
            }
        case OPCODE_CONTINUE:
        case OPCODE_CONTINUEC:
        case OPCODE_ENDLOOP:
            while (1)
            {
                inst--;
                if ((inst->eOpcode == OPCODE_LOOP) && (depth == 0))
                {
                    return GetNextNonLabelInstruction(inst + 1, sawEndSwitch);
                }
                if (inst->eOpcode == OPCODE_LOOP)
                    depth--;
                if (inst->eOpcode == OPCODE_ENDLOOP)
                    depth++;
            }
        case OPCODE_SWITCH:
        case OPCODE_CASE:
        case OPCODE_DEFAULT:
            while (1)
            {
                inst++;
                if ((inst->eOpcode == OPCODE_CASE || inst->eOpcode == OPCODE_DEFAULT || inst->eOpcode == OPCODE_ENDSWITCH) && (depth == 0))
                {
                    // Note that we'll skip setting sawEndSwitch if inst->eOpcode = OPCODE_ENDSWITCH
                    // so that BasicBlock::Build can distinguish between there being a direct route
                    // from SWITCH->ENDSWITCH (CASE followed directly by ENDSWITCH) and not.

                    if (inst->eOpcode == OPCODE_ENDSWITCH && sawEndSwitch != 0)
                        *sawEndSwitch = true;

                    return GetNextNonLabelInstruction(inst + 1, needConnectToParent);
                }
                if (inst->eOpcode == OPCODE_SWITCH)
                    depth++;
                if (inst->eOpcode == OPCODE_ENDSWITCH)
                    depth--;
            }
    }
    return 0;
}
