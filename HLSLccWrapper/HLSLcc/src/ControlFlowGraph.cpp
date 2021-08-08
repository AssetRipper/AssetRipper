#include "internal_includes/debug.h"
#include "internal_includes/ControlFlowGraph.h"
#include "internal_includes/ControlFlowGraphUtils.h"
#include "internal_includes/Instruction.h"
#include "internal_includes/Operand.h"
#include "internal_includes/HLSLccToolkit.h"
#include <algorithm>

using namespace HLSLcc::ControlFlow;
using HLSLcc::ForEachOperand;

const BasicBlock &ControlFlowGraph::Build(const Instruction* firstInstruction, const Instruction* endInstruction)
{
    using std::for_each;

    m_BlockMap.clear();
    m_BlockStorage.clear();

    // Self-registering into m_BlockStorage so it goes out of the scope when ControlFlowGraph does
    BasicBlock *root = new BasicBlock(Utils::GetNextNonLabelInstruction(firstInstruction), *this, NULL, endInstruction);

    // Build the reachable set for each block
    bool hadChanges;
    do
    {
        hadChanges = false;
        for_each(m_BlockStorage.begin(), m_BlockStorage.end(), [&](const shared_ptr<BasicBlock> &bb)
        {
            BasicBlock &b = *bb.get();
            if (b.RebuildReachable())
            {
                hadChanges = true;
            }
        });
    }
    while (hadChanges == true);

    return *root;
}

const BasicBlock *ControlFlowGraph::GetBasicBlockForInstruction(const Instruction *instruction) const
{
    BasicBlockMap::const_iterator itr = m_BlockMap.find(Utils::GetNextNonLabelInstruction(instruction));
    if (itr == m_BlockMap.end())
        return NULL;

    return itr->second;
}

BasicBlock *ControlFlowGraph::GetBasicBlockForInstruction(const Instruction *instruction)
{
    BasicBlockMap::iterator itr = m_BlockMap.find(Utils::GetNextNonLabelInstruction(instruction));
    if (itr == m_BlockMap.end())
        return NULL;

    return itr->second;
}

// Generate a basic block. Private constructor, can only be constructed from ControlFlowGraph::Build().
// Auto-registers itself into ControlFlowGraph
BasicBlock::BasicBlock(const Instruction *psFirst, ControlFlowGraph &graph, const Instruction *psPrecedingBlockHead, const Instruction* endInstruction)
    : m_Graph(graph)
    , m_First(psFirst)
    , m_Last(NULL)
    , m_End(endInstruction)
{
    m_UEVar.clear();
    m_VarKill.clear();
    m_Preceding.clear();
    m_Succeeding.clear();
    m_DEDef.clear();
    m_Reachable.clear();

    // Check that we've pruned the labels
    ASSERT(psFirst == Utils::GetNextNonLabelInstruction(psFirst));

    // Insert to block storage, block map and connect to previous block
    m_Graph.m_BlockStorage.push_back(shared_ptr<BasicBlock>(this));

    bool didInsert = m_Graph.m_BlockMap.insert(std::make_pair(psFirst, this)).second;
    ASSERT(didInsert);

    if (psPrecedingBlockHead != NULL)
    {
        m_Preceding.insert(psPrecedingBlockHead);
        BasicBlock *prec = m_Graph.GetBasicBlockForInstruction(psPrecedingBlockHead);
        ASSERT(prec != 0);
        didInsert = prec->m_Succeeding.insert(psFirst).second;
        ASSERT(didInsert);
    }

    Build();
}

void BasicBlock::Build()
{
    const Instruction *inst = m_First;
    while (inst != m_End)
    {
        // Process sources first
        ForEachOperand(inst, inst + 1, FEO_FLAG_SRC_OPERAND | FEO_FLAG_SUBOPERAND,
            [this](const Instruction *psInst, const Operand *psOperand, uint32_t ui32OperandType)
            {
                if (psOperand->eType != OPERAND_TYPE_TEMP)
                    return;

                uint32_t tempReg = psOperand->ui32RegisterNumber;
                uint32_t accessMask = psOperand->GetAccessMask();

                // Go through each component
                for (int k = 0; k < 4; k++)
                {
                    if (!(accessMask & (1 << k)))
                        continue;

                    uint32_t regIdx = tempReg * 4 + k;
                    // Is this idx already in the kill set, meaning that it's already been re-defined in this basic block? Ignore
                    if (m_VarKill.find(regIdx) != m_VarKill.end())
                        continue;

                    // Add to UEVars set. Doesn't matter if it's already there.
                    m_UEVar.insert(regIdx);
                }
                return;
            });

        // Then the destination operands
        ForEachOperand(inst, inst + 1, FEO_FLAG_DEST_OPERAND,
            [this](const Instruction *psInst, const Operand *psOperand, uint32_t ui32OperandType)
            {
                if (psOperand->eType != OPERAND_TYPE_TEMP)
                    return;

                uint32_t tempReg = psOperand->ui32RegisterNumber;
                uint32_t accessMask = psOperand->GetAccessMask();

                // Go through each component
                for (int k = 0; k < 4; k++)
                {
                    if (!(accessMask & (1 << k)))
                        continue;

                    uint32_t regIdx = tempReg * 4 + k;

                    // Add to kill set. Dupes are fine, this is a set.
                    m_VarKill.insert(regIdx);
                    // Also into the downward definitions. Overwrite the previous definition in this basic block, if any
                    Definition d(psInst, psOperand);
                    m_DEDef[regIdx].clear();
                    m_DEDef[regIdx].insert(d);
                }
                return;
            });

        // Check for flow control instructions
        bool blockDone = false;
        switch (inst->eOpcode)
        {
            default:
                break;
            case OPCODE_RET:
                // Continue processing, in the case of unreachable code we still need to translate it properly (case 1160309)
                // blockDone = true;
                break;
            case OPCODE_RETC:
                // Basic block is done, start a next one.
                // There REALLY should be no existing blocks for this one
                ASSERT(m_Graph.GetBasicBlockForInstruction(Utils::GetNextNonLabelInstruction(inst + 1)) == NULL);
                AddChildBasicBlock(Utils::GetNextNonLabelInstruction(inst + 1));
                blockDone = true;
                break;
            case OPCODE_LOOP:
            case OPCODE_CASE:
            case OPCODE_ENDIF:
            case OPCODE_ENDSWITCH:
                // Not a flow control branch, but need to start a new block anyway.
                AddChildBasicBlock(Utils::GetNextNonLabelInstruction(inst + 1));
                blockDone = true;
                break;

            // Branches
            case OPCODE_IF:
            case OPCODE_BREAKC:
            case OPCODE_CONTINUEC:
            {
                const Instruction *jumpPoint = Utils::GetJumpPoint(inst);
                ASSERT(jumpPoint != NULL);

                // The control branches to the next instruction or jumps to jumpPoint
                AddChildBasicBlock(Utils::GetNextNonLabelInstruction(inst + 1));
                AddChildBasicBlock(jumpPoint);

                blockDone = true;
                break;
            }
            case OPCODE_SWITCH:
            {
                bool sawEndSwitch = false;
                bool needConnectToParent = false;
                const Instruction *jumpPoint = Utils::GetJumpPoint(inst, &sawEndSwitch, &needConnectToParent);
                ASSERT(jumpPoint != NULL);

                while (1)
                {
                    if (!sawEndSwitch || needConnectToParent)
                        AddChildBasicBlock(jumpPoint);

                    if (sawEndSwitch)
                        break;

                    // The -1 is a bit of a hack: we always scroll past all labels so rewind to the last one so we'll know to search for the next label
                    ASSERT((jumpPoint - 1)->eOpcode == OPCODE_CASE || (jumpPoint - 1)->eOpcode == OPCODE_DEFAULT);
                    jumpPoint = Utils::GetJumpPoint(jumpPoint - 1, &sawEndSwitch, &needConnectToParent);
                    ASSERT(jumpPoint != NULL);
                }
                blockDone = true;
                break;
            }

            // Non-conditional jumps
            case OPCODE_BREAK:
            case OPCODE_ELSE:
            case OPCODE_CONTINUE:
            case OPCODE_ENDLOOP:
            {
                const Instruction *jumpPoint = Utils::GetJumpPoint(inst);
                ASSERT(jumpPoint != NULL);

                AddChildBasicBlock(jumpPoint);

                blockDone = true;
                break;
            }
        }

        if (blockDone)
            break;

        inst++;
    }
    // In initial building phase, just make m_Reachable equal to m_DEDef
    m_Reachable = m_DEDef;

    // Tag the end of the basic block
    m_Last = std::max(m_First, std::min(inst, m_End - 1));
//  printf("Basic Block %d -> %d\n", (int)m_First->id, (int)m_Last->id);
}

BasicBlock * BasicBlock::AddChildBasicBlock(const Instruction *psFirst)
{
    // First see if this already exists
    BasicBlock *b = m_Graph.GetBasicBlockForInstruction(psFirst);
    if (b)
    {
        // Just add dependency and we're done
        b->m_Preceding.insert(m_First);
        m_Succeeding.insert(psFirst);
        return b;
    }
    // Otherwise create one. Self-registering and self-connecting
    return new BasicBlock(psFirst, m_Graph, m_First, m_End);
}

bool BasicBlock::RebuildReachable()
{
    // Building the Reachable set is an iterative process, where each block gets rebuilt until nothing changes.
    // Formula: reachable = this.DEDef union ( each preceding.Reachable() minus this.VarKill())

    ReachableVariables newReachable = m_DEDef;
    bool hasChanges = false;

    // Loop each predecessor
    std::for_each(Preceding().begin(), Preceding().end(), [&](const Instruction *instr)
    {
        const BasicBlock *prec = m_Graph.GetBasicBlockForInstruction(instr);
        const ReachableVariables &precReachable = prec->Reachable();

        // Loop each variable*component
        std::for_each(precReachable.begin(), precReachable.end(), [&](const std::pair<uint32_t, BasicBlock::ReachableDefinitionsPerVariable> &itr2)
        {
            uint32_t regIdx = itr2.first;
            const BasicBlock::ReachableDefinitionsPerVariable &defs = itr2.second;

            // Already killed in this block?
            if (VarKill().find(regIdx) != VarKill().end())
                return;

            // Only do comparisons against current definitions if we've yet to find any changes
            BasicBlock::ReachableDefinitionsPerVariable *currReachablePerVar = 0;
            if (!hasChanges)
                currReachablePerVar = &m_Reachable[regIdx];

            BasicBlock::ReachableDefinitionsPerVariable &newReachablePerVar = newReachable[regIdx];

            // Loop each definition
            std::for_each(defs.begin(), defs.end(), [&](const BasicBlock::Definition &d)
            {
                if (!hasChanges)
                {
                    // Check if already there
                    if (currReachablePerVar->find(d) == currReachablePerVar->end())
                        hasChanges = true;
                }
                newReachablePerVar.insert(d);
            }); // definition
        }); // variable*component
    }); // predecessor

    if (hasChanges)
    {
        std::swap(m_Reachable, newReachable);
    }

    return hasChanges;
}

void BasicBlock::RVarUnion(ReachableVariables &a, const ReachableVariables &b)
{
    std::for_each(b.begin(), b.end(), [&a](const std::pair<uint32_t, ReachableDefinitionsPerVariable> &rpvPair)
    {
        uint32_t regIdx = rpvPair.first;
        const ReachableDefinitionsPerVariable &rpv = rpvPair.second;
        // No previous definitions for this variable?
        auto aRPVItr = a.find(regIdx);
        if (aRPVItr == a.end())
        {
            // Just set the definitions and continue
            a[regIdx] = rpv;
            return;
        }
        ReachableDefinitionsPerVariable &aRPV = aRPVItr->second;
        aRPV.insert(rpv.begin(), rpv.end());
    });
}

#if ENABLE_UNIT_TESTS

#define UNITY_EXTERNAL_TOOL 1
#include "Projects/PrecompiledHeaders/UnityPrefix.h" // Needed for defines such as ENABLE_CPP_EXCEPTIONS
#include "Testing.h" // From Runtime/Testing

UNIT_TEST_SUITE(HLSLcc)
{
    TEST(ControlFlowGraph_Build_Simple_Works)
    {
        Instruction inst[] =
        {
            // MOV t0.xyzw, I0.xyzw
            Instruction(0, OPCODE_MOV, 0, 0xf, 0xffffffff, 0xf),
            Instruction(1, OPCODE_RET)
        };

        ControlFlowGraph cfg;
        const BasicBlock &root = cfg.Build(inst, inst + ARRAY_SIZE(inst));

        CHECK_EQUAL(&inst[0], root.First());
        CHECK_EQUAL(&inst[1], root.Last());

        CHECK(root.Preceding().empty());
        CHECK(root.Succeeding().empty());

        CHECK_EQUAL(4, root.VarKill().size());

        // Check that all components from t0 are killed
        CHECK_EQUAL(1, root.VarKill().count(0));
        CHECK_EQUAL(1, root.VarKill().count(1));
        CHECK_EQUAL(1, root.VarKill().count(2));
        CHECK_EQUAL(1, root.VarKill().count(3));

        CHECK_EQUAL(&inst[0], root.DEDef().find(0)->second.begin()->m_Instruction);
        CHECK_EQUAL(&inst[0].asOperands[0], root.DEDef().find(0)->second.begin()->m_Operand);
        CHECK_EQUAL(&inst[0], root.DEDef().find(1)->second.begin()->m_Instruction);
        CHECK_EQUAL(&inst[0].asOperands[0], root.DEDef().find(1)->second.begin()->m_Operand);
        CHECK_EQUAL(&inst[0], root.DEDef().find(2)->second.begin()->m_Instruction);
        CHECK_EQUAL(&inst[0].asOperands[0], root.DEDef().find(2)->second.begin()->m_Operand);
        CHECK_EQUAL(&inst[0], root.DEDef().find(3)->second.begin()->m_Instruction);
        CHECK_EQUAL(&inst[0].asOperands[0], root.DEDef().find(3)->second.begin()->m_Operand);
    }

    TEST(ControlFlowGraph_Build_If_Works)
    {
        Instruction inst[] =
        {
            // B0
            // 0: MOV t1.xyzw, i0.xyzw
            Instruction(0, OPCODE_MOV, 1, 0xf, 0xffffffff, 0xf),
            // 1: MUL t0, t1, t1
            Instruction(1, OPCODE_MUL, 0, 0xf, 1, 0xf, 1, 0xf),
            // 2: IF t1.y
            Instruction(2, OPCODE_IF, 1, 2),
            // B1
            // 3: MOV o0, t0
            Instruction(3, OPCODE_MOV, 0xffffffff, 0xf, 0, 0xf),
            // 4:
            Instruction(4, OPCODE_ELSE),
            // B2
            // 5: MOV o0, t1
            Instruction(5, OPCODE_MOV, 0xffffffff, 0xf, 1, 0xf),
            // 6:
            Instruction(6, OPCODE_ENDIF),
            // B3
            // 7:
            Instruction(7, OPCODE_NOP),
            // 8:
            Instruction(8, OPCODE_RET)
        };

        ControlFlowGraph cfg;
        const BasicBlock &root = cfg.Build(inst, inst + ARRAY_SIZE(inst));

        CHECK_EQUAL(root.First(), &inst[0]);
        CHECK_EQUAL(root.Last(), &inst[2]);

        CHECK(root.Preceding().empty());

        const BasicBlock *b1 = cfg.GetBasicBlockForInstruction(&inst[3]);
        const BasicBlock *b2 = cfg.GetBasicBlockForInstruction(&inst[5]);
        const BasicBlock *b3 = cfg.GetBasicBlockForInstruction(&inst[7]);

        CHECK(b1 != NULL);
        CHECK(b2 != NULL);
        CHECK(b3 != NULL);

        CHECK_EQUAL(&inst[3], b1->First());
        CHECK_EQUAL(&inst[5], b2->First());
        CHECK_EQUAL(&inst[7], b3->First());

        CHECK_EQUAL(&inst[4], b1->Last());
        CHECK_EQUAL(&inst[6], b2->Last());
        CHECK_EQUAL(&inst[8], b3->Last());

        CHECK_EQUAL(1, root.Succeeding().count(&inst[3]));
        CHECK_EQUAL(1, root.Succeeding().count(&inst[5]));
        CHECK_EQUAL(2, root.Succeeding().size());

        CHECK_EQUAL(1, b1->Preceding().size());
        CHECK_EQUAL(1, b1->Preceding().count(&inst[0]));

        CHECK_EQUAL(1, b2->Preceding().size());
        CHECK_EQUAL(1, b2->Preceding().count(&inst[0]));

        CHECK_EQUAL(2, b3->Preceding().size());
        CHECK_EQUAL(0, b3->Preceding().count(&inst[0]));
        CHECK_EQUAL(1, b3->Preceding().count(&inst[3]));
        CHECK_EQUAL(1, b3->Preceding().count(&inst[5]));

        // The if block must have upwards-exposed t0
        CHECK_EQUAL(1, b1->UEVar().count(0));
        CHECK_EQUAL(1, b1->UEVar().count(1));
        CHECK_EQUAL(1, b1->UEVar().count(2));
        CHECK_EQUAL(1, b1->UEVar().count(3));

        // The else block must have upwards-exposed t1
        CHECK_EQUAL(1, b2->UEVar().count(4));
        CHECK_EQUAL(1, b2->UEVar().count(5));
        CHECK_EQUAL(1, b2->UEVar().count(6));
        CHECK_EQUAL(1, b2->UEVar().count(7));

        CHECK_EQUAL(8, root.VarKill().size());

        // Check that all components from t0 and t1 are killed
        CHECK_EQUAL(1, root.VarKill().count(0));
        CHECK_EQUAL(1, root.VarKill().count(1));
        CHECK_EQUAL(1, root.VarKill().count(2));
        CHECK_EQUAL(1, root.VarKill().count(3));

        CHECK_EQUAL(1, root.VarKill().count(4));
        CHECK_EQUAL(1, root.VarKill().count(5));
        CHECK_EQUAL(1, root.VarKill().count(6));
        CHECK_EQUAL(1, root.VarKill().count(7));

        // The expected downwards-exposed definitions:
        // B0: t0, t1
        // B1-B3: none

        CHECK_EQUAL(8, root.DEDef().size());
        CHECK_EQUAL(0, b1->DEDef().size());
        CHECK_EQUAL(0, b2->DEDef().size());
        CHECK_EQUAL(0, b3->DEDef().size());

        CHECK(root.DEDef() == root.Reachable());

        CHECK(root.Reachable() == b1->Reachable());
        CHECK(root.Reachable() == b2->Reachable());
        CHECK(root.Reachable() == b3->Reachable());
    }

    TEST(ControlFlowGraph_Build_SwitchCase_Works)
    {
        Instruction inst[] =
        {
            // Start B0
            // i0: MOV t0.x, I0.x
            Instruction(0, OPCODE_MOV, 0, 1, 0xffffffff, 1),
            // i1: MOVE t1.xyz, I0.yzw
            Instruction(1, OPCODE_MOV, 1, 7, 0xffffffff, 0xe),
            // i2: MOVE t1.w, t0.x
            Instruction(2, OPCODE_MOV, 1, 8, 0xffffffff, 0x1),
            // i3: MOVE t2, I0
            Instruction(3, OPCODE_MOV, 2, 0xf, 0xffffffff, 0xf),
            // i4: SWITCH t0.y
            Instruction(4, OPCODE_SWITCH, 1, 2),
            // End B0
            // i5: CASE
            Instruction(5, OPCODE_CASE),
            // i6: DEFAULT
            Instruction(6, OPCODE_DEFAULT),
            // Start B1
            // i7: MOC t1.z, t0.x
            Instruction(7, OPCODE_MOV, 1, 4, 0, 1),
            // i8: CASE
            Instruction(8, OPCODE_CASE),
            // End B1
            // Start B2
            // i9: MOV t1.z, t2.x
            Instruction(9, OPCODE_MOV, 1, 4, 2, 1),
            // i10: BREAK
            Instruction(10, OPCODE_BREAK),
            // End B2
            // i11: CASE
            Instruction(11, OPCODE_CASE),
            // Start B3
            // i12: MOV t1.z, t2.y
            Instruction(12, OPCODE_MOV, 1, 4, 2, 2),
            // i13: BREAKC t0.x
            Instruction(13, OPCODE_BREAKC, 0, 1),
            // End B3
            // i14: CASE
            Instruction(14, OPCODE_CASE),
            // Start B4
            // i15: MOV t1.z, t2.z
            Instruction(15, OPCODE_MOV, 1, 4, 2, 4),
            // i16: ENDSWITCH
            Instruction(16, OPCODE_ENDSWITCH),
            // End B4
            // Start B5
            // i17: MOV o0, t1
            Instruction(17, OPCODE_MOV, 0xffffffff, 0xf, 1, 0xf),
            // i18: RET
            Instruction(18, OPCODE_RET)
            // End B5
        };

        ControlFlowGraph cfg;
        const BasicBlock &root = cfg.Build(inst, inst + ARRAY_SIZE(inst));

        CHECK_EQUAL(&inst[0], root.First());
        CHECK_EQUAL(&inst[4], root.Last());

        const BasicBlock *b1 = cfg.GetBasicBlockForInstruction(&inst[7]);
        const BasicBlock *b2 = cfg.GetBasicBlockForInstruction(&inst[9]);
        const BasicBlock *b3 = cfg.GetBasicBlockForInstruction(&inst[12]);
        const BasicBlock *b4 = cfg.GetBasicBlockForInstruction(&inst[15]);
        const BasicBlock *b5 = cfg.GetBasicBlockForInstruction(&inst[17]);

        CHECK(b1 != NULL);
        CHECK(b2 != NULL);
        CHECK(b3 != NULL);
        CHECK(b4 != NULL);
        CHECK(b5 != NULL);

        // Check instruction ranges
        CHECK_EQUAL(&inst[8], b1->Last());
        CHECK_EQUAL(&inst[10], b2->Last());
        CHECK_EQUAL(&inst[13], b3->Last());
        CHECK_EQUAL(&inst[16], b4->Last());
        CHECK_EQUAL(&inst[18], b5->Last());

        // Nothing before the root, nothing after b5
        CHECK(root.Preceding().empty());
        CHECK(b5->Succeeding().empty());

        // Check that all connections are there and no others.

        // B0->B1
        // B0->B2
        // B0->B3
        // B0->B4
        CHECK_EQUAL(1, root.Succeeding().count(&inst[7]));
        CHECK_EQUAL(1, root.Succeeding().count(&inst[9]));
        CHECK_EQUAL(1, root.Succeeding().count(&inst[12]));
        CHECK_EQUAL(1, root.Succeeding().count(&inst[15]));

        CHECK_EQUAL(4, root.Succeeding().size());

        // B1

        // B1->B2
        CHECK_EQUAL(1, b1->Succeeding().count(&inst[9]));
        CHECK_EQUAL(1, b1->Succeeding().size());

        // B0->B1, reverse
        CHECK_EQUAL(1, b1->Preceding().count(&inst[0]));
        CHECK_EQUAL(1, b1->Preceding().size());

        // B2

        // B2->B5
        CHECK_EQUAL(1, b2->Succeeding().count(&inst[17]));
        CHECK_EQUAL(1, b2->Succeeding().size());
        CHECK_EQUAL(1, b2->Preceding().count(&inst[7]));
        CHECK_EQUAL(1, b2->Preceding().count(&inst[0]));
        CHECK_EQUAL(2, b2->Preceding().size());

        // B3
        // B3->B4
        // B3->B5
        CHECK_EQUAL(1, b3->Succeeding().count(&inst[15]));
        CHECK_EQUAL(1, b3->Succeeding().count(&inst[17]));
        CHECK_EQUAL(2, b3->Succeeding().size());
        CHECK_EQUAL(1, b3->Preceding().count(&inst[0]));
        CHECK_EQUAL(1, b3->Preceding().size());

        // B4
        CHECK_EQUAL(1, b4->Succeeding().count(&inst[17]));
        CHECK_EQUAL(1, b4->Succeeding().size());
        CHECK_EQUAL(1, b4->Preceding().count(&inst[0]));
        CHECK_EQUAL(2, b4->Preceding().size());

        // B5
        CHECK_EQUAL(0, b5->Succeeding().size());
        CHECK_EQUAL(3, b5->Preceding().size()); //b2, b3, b4
        CHECK_EQUAL(1, b5->Preceding().count(&inst[9]));
        CHECK_EQUAL(1, b5->Preceding().count(&inst[12]));
        CHECK_EQUAL(1, b5->Preceding().count(&inst[15]));


        // Verify reachable sets

        CHECK(root.Reachable() == root.DEDef());
        CHECK_EQUAL(9, root.Reachable().size());

        // B5 should have these reachables:
        // t0.x only from b0
        // t1.xy from b0, i1
        // t1.z from b2,i9 + b3,i12 + b4,i15 (the defs from b0 and b1 are killed by b2)
        // t1.w from b0, i2
        // t2.xyzw from b0, i3

        // Cast away const so [] works.
        BasicBlock::ReachableVariables &r = (BasicBlock::ReachableVariables &)b5->Reachable();

        CHECK_EQUAL(9, r.size());

        CHECK_EQUAL(1, r[0].size());
        CHECK_EQUAL(0, r[1].size());
        CHECK_EQUAL(0, r[2].size());
        CHECK_EQUAL(0, r[3].size());
        CHECK_EQUAL(&inst[0], r[0].begin()->m_Instruction);

        CHECK_EQUAL(1, r[4].size());
        CHECK_EQUAL(1, r[5].size());
        CHECK_EQUAL(3, r[6].size());
        CHECK_EQUAL(1, r[7].size());

        const BasicBlock::ReachableDefinitionsPerVariable &d = r[6];
        BasicBlock::ReachableDefinitionsPerVariable t;
        t.insert(BasicBlock::Definition(&inst[9], &inst[9].asOperands[0]));
        t.insert(BasicBlock::Definition(&inst[12], &inst[12].asOperands[0]));
        t.insert(BasicBlock::Definition(&inst[15], &inst[15].asOperands[0]));

        CHECK(t == d);

        CHECK_EQUAL(1, r[8].size());
        CHECK_EQUAL(1, r[9].size());
        CHECK_EQUAL(1, r[10].size());
        CHECK_EQUAL(1, r[11].size());
    }

    TEST(ControlFlowGraph_Build_Loop_Works)
    {
        Instruction inst[] =
        {
            // Start B0
            // i0: MOV t0.x, I0.x
            Instruction(0, OPCODE_MOV, 0, 1, 0xffffffff, 1),
            // i1: MOVE t1.xy, I0.zw // The .x definition should not make it past the loop, .y should.
            Instruction(1, OPCODE_MOV, 1, 3, 0xffffffff, 0xc),
            // i2: LOOP
            Instruction(2, OPCODE_LOOP, 1, 2),
            // End B0 -> B1
            // Begin B1
            // i3: MOV t1.x, t0.x
            Instruction(3, OPCODE_MOV, 1, 1, 0, 1),
            // i4: BREAKC t0.x
            Instruction(4, OPCODE_BREAKC, 0, 1),
            // End B1 -> B2, B3
            // Begin B2
            // i5: ADD t0.x, t0.y
            Instruction(5, OPCODE_ADD, 0, 1, 0, 2),
            // i6: MOV t1.x, t0.x  // This should never show up as definition
            Instruction(6, OPCODE_MOV, 1, 1, 0, 1),
            // i7: ENDLOOP
            Instruction(7, OPCODE_ENDLOOP),
            // End B2 -> B1
            // Start B3
            // i8: MOV O0.x, t1.x
            Instruction(8, OPCODE_MOV, 0xffffffff, 1, 1, 1),
            // i9: RET
            Instruction(9, OPCODE_RET),
            // End B3
        };

        ControlFlowGraph cfg;
        const BasicBlock &root = cfg.Build(inst, inst + ARRAY_SIZE(inst));

        CHECK_EQUAL(&inst[0], root.First());
        CHECK_EQUAL(&inst[2], root.Last());

        const BasicBlock *b1 = cfg.GetBasicBlockForInstruction(&inst[3]);
        const BasicBlock *b2 = cfg.GetBasicBlockForInstruction(&inst[5]);
        const BasicBlock *b3 = cfg.GetBasicBlockForInstruction(&inst[8]);

        CHECK(b1 != NULL);
        CHECK(b2 != NULL);
        CHECK(b3 != NULL);

        // Check instruction ranges
        CHECK_EQUAL(&inst[4], b1->Last());
        CHECK_EQUAL(&inst[7], b2->Last());
        CHECK_EQUAL(&inst[9], b3->Last());

        // Nothing before the root, nothing after b3
        CHECK(root.Preceding().empty());
        CHECK(b3->Succeeding().empty());

        // Check that all connections are there and no others.

        // B0->B1
        CHECK_EQUAL(1, root.Succeeding().count(&inst[3]));
        CHECK_EQUAL(1, root.Succeeding().size());

        // B1

        // B1->B2
        // B1->B3
        CHECK_EQUAL(1, b1->Succeeding().count(&inst[5]));
        CHECK_EQUAL(1, b1->Succeeding().count(&inst[8]));
        CHECK_EQUAL(2, b1->Succeeding().size());

        // B0->B1, reverse
        CHECK_EQUAL(1, b1->Preceding().count(&inst[0]));
        // We may also come from B2
        CHECK_EQUAL(1, b1->Preceding().count(&inst[5]));
        CHECK_EQUAL(2, b1->Preceding().size());

        // B2

        // B2->B1
        CHECK_EQUAL(1, b2->Succeeding().count(&inst[3]));
        CHECK_EQUAL(1, b2->Succeeding().size());
        CHECK_EQUAL(1, b2->Preceding().count(&inst[3]));
        CHECK_EQUAL(1, b2->Preceding().size());

        // B3
        CHECK_EQUAL(1, b3->Preceding().count(&inst[3]));
        CHECK_EQUAL(1, b3->Preceding().size());

        // Verify reachable sets


        BasicBlock::ReachableVariables t;

        // B0 DEDef and Reachable
        t.clear();
        t[0].insert(BasicBlock::Definition(&inst[0], &inst[0].asOperands[0]));
        t[4].insert(BasicBlock::Definition(&inst[1], &inst[1].asOperands[0]));
        t[5].insert(BasicBlock::Definition(&inst[1], &inst[1].asOperands[0]));

        CHECK(root.DEDef() == t);
        CHECK(root.Reachable() == root.DEDef());

        // B1 DEDef and Reachable
        t.clear();
        t[4].insert(BasicBlock::Definition(&inst[3], &inst[3].asOperands[0]));
        CHECK(b1->DEDef() == t);

        t = b1->DEDef();
        // t0.x from i0, t1.y (but not .x) from i1
        t[0].insert(BasicBlock::Definition(&inst[0], &inst[0].asOperands[0]));
        t[5].insert(BasicBlock::Definition(&inst[1], &inst[1].asOperands[0]));

        // t0.x from i5, but nothing from i6
        t[0].insert(BasicBlock::Definition(&inst[5], &inst[5].asOperands[0]));
        CHECK(b1->Reachable() == t);

        // B2
        t.clear();
        t[0].insert(BasicBlock::Definition(&inst[5], &inst[5].asOperands[0]));
        t[4].insert(BasicBlock::Definition(&inst[6], &inst[6].asOperands[0]));
        CHECK(b2->DEDef() == t);

        t = b2->DEDef();
        t[5].insert(BasicBlock::Definition(&inst[1], &inst[1].asOperands[0]));

        CHECK(b2->Reachable() == t);

        // B3
        t.clear();
        CHECK(b3->DEDef() == t);
        // t0.x from i0, t1.y from i1
        t[0].insert(BasicBlock::Definition(&inst[0], &inst[0].asOperands[0]));
        t[5].insert(BasicBlock::Definition(&inst[1], &inst[1].asOperands[0]));

        // t1.x from i3
        t[4].insert(BasicBlock::Definition(&inst[3], &inst[3].asOperands[0]));

        // t0.x from i5
        t[0].insert(BasicBlock::Definition(&inst[5], &inst[5].asOperands[0]));

        CHECK(b3->Reachable() == t);
    }
}

#endif
