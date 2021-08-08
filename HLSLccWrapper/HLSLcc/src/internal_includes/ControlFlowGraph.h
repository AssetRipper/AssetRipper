#pragma once

#include <set>
#include <map>
#include <utility>
#include <vector>
#include <memory>

#include <stdint.h>

struct Instruction;
class Operand;

namespace HLSLcc
{
    using namespace std;

namespace ControlFlow
{
    class BasicBlock;

    class ControlFlowGraph
    {
        friend class BasicBlock;
    public:
        ControlFlowGraph()
            : m_BlockMap()
            , m_BlockStorage()
        {}

        typedef std::vector<shared_ptr<BasicBlock> > BasicBlockStorage;

        const BasicBlock &Build(const Instruction* firstInstruction, const Instruction* endInstruction);

        // Only works for instructions that start the basic block
        const BasicBlock *GetBasicBlockForInstruction(const Instruction *instruction) const;

        // non-const version for BasicBlock
        BasicBlock *GetBasicBlockForInstruction(const Instruction *instruction);

        const BasicBlockStorage &AllBlocks() const { return m_BlockStorage; }
    private:

        // Map for storing the created basic blocks. Map key is the pointer to the first instruction in the block
        typedef std::map<const Instruction *, BasicBlock *> BasicBlockMap;

        BasicBlockMap m_BlockMap;

        // auto_ptr -type storage for multiple BasicBlocks. BlockMap above only has pointers into these
        BasicBlockStorage m_BlockStorage;
    };


    class BasicBlock
    {
        friend class ControlFlowGraph;
    public:
        // A set of register indices, one per each vec4 component per register
        typedef std::set<uint32_t> RegisterSet;
        // The connections (either incoming or outgoing) from this block. The instruction is the same one as the key in ControlFlowGraph to that basic block
        typedef std::set<const Instruction *> ConnectionSet;

        struct Definition
        {
            Definition(const Instruction* i = nullptr, const Operand* o = nullptr)
                : m_Instruction(i)
                , m_Operand(o)
            {}

            Definition(const Definition& a) = default;
            Definition(Definition&& a) = default;
            ~Definition() = default;

            Definition& operator=(const Definition& a) = default;
            Definition& operator=(Definition&& a) = default;

            bool operator==(const Definition& a) const
            {
                if (a.m_Instruction != m_Instruction)
                    return false;
                return a.m_Operand == m_Operand;
            }

            bool operator!=(const Definition& a) const
            {
                if (a.m_Instruction == m_Instruction)
                    return false;
                return a.m_Operand != m_Operand;
            }

            bool operator<(const Definition& a) const
            {
                if (m_Instruction != a.m_Instruction)
                    return m_Instruction < a.m_Instruction;
                return m_Operand < a.m_Operand;
            }

            const Instruction   *m_Instruction;
            const Operand       *m_Operand;
        };

        typedef std::set<Definition> ReachableDefinitionsPerVariable;     // A set of possibly visible definitions for one component of one vec4 variable
        typedef std::map<uint32_t, ReachableDefinitionsPerVariable> ReachableVariables;     // A VisibleDefinitionSet for each variable*component.

        const Instruction *First() const { return m_First; }
        const Instruction *Last() const { return m_Last; }

        const RegisterSet &UEVar() const { return m_UEVar; }
        const RegisterSet &VarKill() const { return m_VarKill; }

        const ConnectionSet &Preceding() const { return m_Preceding; }
        const ConnectionSet &Succeeding() const { return m_Succeeding; }

        const ReachableVariables &DEDef() const { return m_DEDef; }
        const ReachableVariables &Reachable() const { return m_Reachable; }

        // Helper function: Do union of 2 ReachableVariables, store result in a.
        static void RVarUnion(ReachableVariables &a, const ReachableVariables &b);

    private:

        // Generate a basic block. Private constructor, can only be constructed from ControlFlowGraph::Build()
        BasicBlock(const Instruction *psFirst, ControlFlowGraph &graph, const Instruction *psPrecedingBlockHead, const Instruction* psEnd);

        // Walk through the instructions and build UEVar and VarKill sets, create succeeding nodes if they don't exist already.
        void Build();

        bool RebuildReachable();     // Rebuild m_Reachable from preceding blocks and this one. Returns true if current value changed.


        BasicBlock * AddChildBasicBlock(const Instruction *psFirst);

    private:
        ControlFlowGraph &m_Graph;     // The graph object containing this block

        const Instruction *m_First;     // The first instruction in the basic block
        const Instruction *m_Last;     // The last instruction in the basic block. Either OPCODE_RET or a branch/jump/loop instruction
        const Instruction *m_End; // past-the-end pointer

        RegisterSet m_UEVar;        // Upwards-exposed variables (temps that need definition from upstream and are used in this basic block)
        RegisterSet m_VarKill;      // Set of variables that are defined in this block.

        ConnectionSet m_Preceding;     // Set of blocks that immediately precede this block in the CFG
        ConnectionSet m_Succeeding;     // Set of blocks that follow this block in the CFG

        ReachableVariables m_DEDef;     // Downward-exposed definitions from this basic block. Always only one item per set.

        ReachableVariables m_Reachable;     // The set of variable definitions that are visible at the end of this block.
    };
}
}
