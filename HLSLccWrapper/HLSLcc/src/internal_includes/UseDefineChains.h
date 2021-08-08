#pragma once

#include <set>
#include <map>
#include <list>
#include <vector>
#include <algorithm>

#include <stdint.h>
#include <string.h>

struct DefineUseChainEntry;
struct UseDefineChainEntry;

typedef std::set<DefineUseChainEntry *> DefineSet;
typedef std::set<UseDefineChainEntry *> UsageSet;

struct Instruction;
class Operand;
class ShaderInfo;
namespace HLSLcc
{
namespace ControlFlow
{
    class ControlFlowGraph;
}
}


// Def-Use chain per temp component
struct DefineUseChainEntry
{
    DefineUseChainEntry()
        : psInst(0)
        , psOp(0)
        , usages()
        , writeMask(0)
        , index(0)
        , isStandalone(0)
    {
        memset(psSiblings, 0, 4 * sizeof(DefineUseChainEntry *));
    }

    Instruction *psInst;            // The declaration (write to this temp component)
    Operand *psOp;                  // The operand within this instruction for the write target
    UsageSet usages;                // List of usages that are dependent on this write
    uint32_t writeMask;             // Access mask; which all components were written to in the same op
    uint32_t index;                 // For which component was this definition created for?
    uint32_t isStandalone;          // A shortcut for analysis: if nonzero, all siblings of all usages for both this and all this siblings
    struct DefineUseChainEntry *psSiblings[4];  // In case of vectorized op, contains pointer to this define's corresponding entries for the other components.

#if _DEBUG
    bool operator==(const DefineUseChainEntry &a) const
    {
        if (psInst != a.psInst)
            return false;
        if (psOp != a.psOp)
            return false;
        if (writeMask != a.writeMask)
            return false;
        if (index != a.index)
            return false;
        if (isStandalone != a.isStandalone)
            return false;

        // Just check that each one has the same amount of usages
        if (usages.size() != a.usages.size())
            return false;

        return true;
    }

#endif
};

typedef std::list<DefineUseChainEntry> DefineUseChain;

struct UseDefineChainEntry
{
    UseDefineChainEntry()
        : psInst(0)
        , psOp(0)
        , defines()
        , accessMask(0)
        , index(0)
    {
        memset(psSiblings, 0, 4 * sizeof(UseDefineChainEntry *));
    }

    Instruction *psInst;            // The use (read from this temp component)
    Operand *psOp;                  // The operand within this instruction for the read
    DefineSet defines;              // List of writes that are visible to this read
    uint32_t accessMask;            // Which all components were read together with this one
    uint32_t index;                 // For which component was this usage created for?
    struct UseDefineChainEntry *psSiblings[4];  // In case of vectorized op, contains pointer to this usage's corresponding entries for the other components.

#if _DEBUG
    bool operator==(const UseDefineChainEntry &a) const
    {
        if (psInst != a.psInst)
            return false;
        if (psOp != a.psOp)
            return false;
        if (accessMask != a.accessMask)
            return false;
        if (index != a.index)
            return false;

        // Just check that each one has the same amount of usages
        if (defines.size() != a.defines.size())
            return false;

        return true;
    }

#endif
};

typedef std::list<UseDefineChainEntry> UseDefineChain;

typedef std::map<uint32_t, UseDefineChain> UseDefineChains;
typedef std::map<uint32_t, DefineUseChain> DefineUseChains;
typedef std::vector<DefineUseChainEntry *> ActiveDefinitions;

// Do flow control analysis on the instructions and build the define-use and use-define chains
void BuildUseDefineChains(std::vector<Instruction> &instructions, uint32_t ui32NumTemps, DefineUseChains &psDUChains, UseDefineChains &psUDChains, HLSLcc::ControlFlow::ControlFlowGraph &cfg);

// Do temp splitting based on use-define chains
void UDSplitTemps(uint32_t *psNumTemps, DefineUseChains &psDUChains, UseDefineChains &psUDChains, std::vector<uint32_t> &pui32SplitTable);

// Based on the sampler precisions, downgrade the definitions if possible.
void UpdateSamplerPrecisions(const ShaderInfo &psContext, DefineUseChains &psDUChains, uint32_t ui32NumTemps);

// Optimization pass for successive passes: Mark Operand->isStandalone for definitions that are "standalone": all usages (and all their sibligns) of this and all its siblings only see this definition.
void CalculateStandaloneDefinitions(DefineUseChains &psDUChains, uint32_t ui32NumTemps);

// Write the uses and defines back to Instruction and Operand member lists.
void WriteBackUsesAndDefines(DefineUseChains &psDUChains);
