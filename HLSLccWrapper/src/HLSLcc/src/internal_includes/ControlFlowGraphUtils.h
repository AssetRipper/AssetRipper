#pragma once

struct Instruction;

namespace HLSLcc
{
namespace ControlFlow
{
    class Utils
    {
    public:
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
        // If sawEndSwitch != null, will bet set to true if the label skipping saw past ENDSWITCH
        // If needConnectToParent != null, will be set to true if sawEndSwitch == true and there are one or more case labels directly before it.
        static const Instruction * GetJumpPoint(const Instruction *psStart, bool *sawEndSwitch = 0, bool *needConnectToParent = 0);

        static const Instruction *GetNextNonLabelInstruction(const Instruction *psStart, bool *sawEndSwitch = 0);
    };
}
}
