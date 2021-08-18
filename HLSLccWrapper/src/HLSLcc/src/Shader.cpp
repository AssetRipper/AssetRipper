#include "internal_includes/Shader.h"
#include "internal_includes/debug.h"
#include <algorithm>
#include "internal_includes/Instruction.h"
#include "internal_includes/Declaration.h"
#include "internal_includes/HLSLccToolkit.h"

uint32_t Shader::GetTempComponentCount(SHADER_VARIABLE_TYPE eType, uint32_t ui32Reg) const
{
    switch (eType)
    {
        case SVT_FLOAT:
            return psFloatTempSizes[ui32Reg];
        case SVT_FLOAT16:
            return psFloat16TempSizes[ui32Reg];
        case SVT_FLOAT10:
            return psFloat10TempSizes[ui32Reg];
        case SVT_INT:
            return psIntTempSizes[ui32Reg];
        case SVT_INT16:
            return psInt16TempSizes[ui32Reg];
        case SVT_INT12:
            return psInt12TempSizes[ui32Reg];
        case SVT_UINT:
            return psUIntTempSizes[ui32Reg];
        case SVT_UINT16:
            return psUInt16TempSizes[ui32Reg];
        case SVT_DOUBLE:
            return psDoubleTempSizes[ui32Reg];
        case SVT_BOOL:
            return psBoolTempSizes[ui32Reg];
        default:
            ASSERT(0);
    }
    return 0;
}

void Shader::ConsolidateHullTempVars()
{
    uint32_t i, phase;
    uint32_t numTemps = 0;
    for (phase = 0; phase < asPhases.size(); phase++)
    {
        for (i = 0; i < asPhases[phase].psDecl.size(); i++)
        {
            if (asPhases[phase].psDecl[i].eOpcode == OPCODE_DCL_TEMPS)
            {
                if (asPhases[phase].psDecl[i].value.ui32NumTemps > numTemps)
                    numTemps = asPhases[phase].psDecl[i].value.ui32NumTemps;
                asPhases[phase].psDecl[i].value.ui32NumTemps = 0;
            }
        }
    }
    // Now we have the max temps, write it back to the first one we see.
    for (phase = 0; phase < asPhases.size(); phase++)
    {
        for (i = 0; i < asPhases[phase].psDecl.size(); i++)
        {
            if (asPhases[phase].psDecl[i].eOpcode == OPCODE_DCL_TEMPS)
            {
                asPhases[phase].psDecl[i].value.ui32NumTemps = numTemps;
                return;
            }
        }
    }
}

// Image (RWTexture in HLSL) declaration op does not provide enough info about the format and accessing.
// Go through all image declarations and instructions accessing it to see if it is readonly/writeonly.
// While doing that we also get the number of components expected in the image format.
// Also resolve access flags for other UAVs as well. No component count resolving for them.
void ShaderPhase::ResolveUAVProperties(const ShaderInfo& sInfo)
{
    Declaration *psFirstDeclaration = &psDecl[0];

    uint32_t ui32NumDeclarations = (uint32_t)psDecl.size();
    Instruction *psFirstInstruction = &psInst[0];
    uint32_t ui32NumInstructions = (uint32_t)psInst.size();

    if (ui32NumDeclarations == 0 || ui32NumInstructions == 0)
        return;

    Declaration *psLastDeclaration = psFirstDeclaration + ui32NumDeclarations - 1;
    Instruction *psLastInstruction = psFirstInstruction + ui32NumInstructions - 1;
    Declaration *psDecl;

    for (psDecl = psFirstDeclaration; psDecl <= psLastDeclaration; psDecl++)
    {
        Instruction *psInst;
        uint32_t uavReg;
        if (psDecl->eOpcode != OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED &&
            psDecl->eOpcode != OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED &&
            psDecl->eOpcode != OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW)
            continue;

        uavReg = psDecl->asOperands[0].ui32RegisterNumber;

        for (psInst = psFirstInstruction; psInst <= psLastInstruction; psInst++)
        {
            uint32_t opIndex;
            uint32_t accessFlags;
            uint32_t numComponents;

            switch (psInst->eOpcode)
            {
                case OPCODE_LD_UAV_TYPED:
                    opIndex = 2;
                    accessFlags = ACCESS_FLAG_READ;
                    numComponents = psInst->asOperands[0].GetNumSwizzleElements(); // get component count from the write target
                    break;

                case OPCODE_STORE_UAV_TYPED:
                    ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_UNORDERED_ACCESS_VIEW);
                    opIndex = 0;
                    accessFlags = ACCESS_FLAG_WRITE;
                    numComponents = 0; // store op does not contribute on the component count resolving
                    break;

                case OPCODE_ATOMIC_CMP_STORE:
                case OPCODE_ATOMIC_AND:
                case OPCODE_ATOMIC_IADD:
                case OPCODE_ATOMIC_OR:
                case OPCODE_ATOMIC_XOR:
                case OPCODE_ATOMIC_IMIN:
                case OPCODE_ATOMIC_UMIN:
                case OPCODE_ATOMIC_IMAX:
                case OPCODE_ATOMIC_UMAX:
                    opIndex = 0;
                    accessFlags = ACCESS_FLAG_READ | ACCESS_FLAG_WRITE | ACCESS_FLAG_ATOMIC;
                    numComponents = 1;
                    break;

                case OPCODE_IMM_ATOMIC_AND:
                case OPCODE_IMM_ATOMIC_IADD:
                case OPCODE_IMM_ATOMIC_IMAX:
                case OPCODE_IMM_ATOMIC_IMIN:
                case OPCODE_IMM_ATOMIC_UMAX:
                case OPCODE_IMM_ATOMIC_UMIN:
                case OPCODE_IMM_ATOMIC_OR:
                case OPCODE_IMM_ATOMIC_XOR:
                case OPCODE_IMM_ATOMIC_EXCH:
                case OPCODE_IMM_ATOMIC_CMP_EXCH:
                    opIndex = 1;
                    accessFlags = ACCESS_FLAG_READ | ACCESS_FLAG_WRITE | ACCESS_FLAG_ATOMIC;
                    numComponents = 1;
                    break;

                // The rest of the ops here are only for buffer UAVs. No need for component count resolving.
                case OPCODE_LD_STRUCTURED:
                    opIndex = 3;
                    accessFlags = ACCESS_FLAG_READ;
                    numComponents = 0;
                    break;

                case OPCODE_STORE_STRUCTURED:
                    opIndex = 0;
                    accessFlags = ACCESS_FLAG_WRITE;
                    numComponents = 0;
                    break;

                case OPCODE_LD_RAW:
                    opIndex = 2;
                    accessFlags = ACCESS_FLAG_READ;
                    numComponents = 0;
                    break;

                case OPCODE_STORE_RAW:
                    opIndex = 0;
                    accessFlags = ACCESS_FLAG_WRITE;
                    numComponents = 0;
                    break;

                case OPCODE_IMM_ATOMIC_ALLOC:
                case OPCODE_IMM_ATOMIC_CONSUME:
                    opIndex = 1;
                    accessFlags = ACCESS_FLAG_READ | ACCESS_FLAG_WRITE | ACCESS_FLAG_ATOMIC;
                    numComponents = 0;
                    break;

                default:
                    continue;
            }

            // Buffer loads can also happen on non-uav. Skip those.
            if (psInst->asOperands[opIndex].eType != OPERAND_TYPE_UNORDERED_ACCESS_VIEW)
                continue;

            // Check the instruction is operating on the declared uav
            if (psInst->asOperands[opIndex].ui32RegisterNumber != uavReg)
                continue;

            psDecl->sUAV.ui32AccessFlags |= accessFlags;

            // get the max components accessed, but only for typed (texture) UAVs
            if (numComponents > psDecl->sUAV.ui32NumComponents && psDecl->eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED)
            {
                psDecl->sUAV.ui32NumComponents = numComponents;
            }
        }

        if (psDecl->eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED)
        {
            const ResourceBinding* psBinding = 0;
            if (sInfo.GetResourceFromBindingPoint(RGROUP_UAV, uavReg, &psBinding))
            {
                // component count is stored in flags as 2 bits, 00: vec1, 01: vec2, 10: vec3, 11: vec4
                psDecl->sUAV.ui32NumComponents = ((psBinding->ui32Flags >> 2) & 3) + 1;
            }
        }
    }
}

static void GatherOperandAccessMasks(const Operand *psOperand, char *destTable)
{
    int i;
    uint32_t reg;
    for (i = 0; i < MAX_SUB_OPERANDS; i++)
    {
        if (psOperand->m_SubOperands[i].get())
            GatherOperandAccessMasks(psOperand->m_SubOperands[i].get(), destTable);
    }

    if (psOperand->eType != OPERAND_TYPE_TEMP)
        return;

    reg = psOperand->ui32RegisterNumber & 0xffff; // We add 0x10000 to all newly created ones earlier

    destTable[reg] |= (char)psOperand->GetAccessMask();
}

// Coalesce the split temps back based on their original temp register. Keep uint/int/float operations separate
static void CoalesceTemps(Shader *psShader, ShaderPhase *psPhase, uint32_t ui32MaxOrigTemps)
{
    // Just move all operations back to their original registers, but keep the data type assignments.
    uint32_t i, k;
    Instruction *psLastInstruction = &psPhase->psInst[psPhase->psInst.size() - 1];
    std::vector<char> opAccessMasks;

    // First move all newly created temps to high enough so they won't overlap with the rebased ones

    Instruction *inst = &psPhase->psInst[0];

    if (psPhase->psInst.size() == 0 || psPhase->ui32OrigTemps == 0)
        return;

    while (inst <= psLastInstruction)
    {
        // Update all operands and their suboperands
        for (i = psPhase->ui32OrigTemps; i < psPhase->ui32TotalTemps; i++)
        {
            for (k = 0; k < inst->ui32NumOperands; k++)
                inst->ChangeOperandTempRegister(&inst->asOperands[k], i, 0x10000 + i, OPERAND_4_COMPONENT_MASK_ALL, UD_CHANGE_ALL, 0);
        }
        inst++;
    }

    // Prune the original registers, rebase if necessary
    opAccessMasks.clear();
    opAccessMasks.resize(psPhase->ui32TotalTemps, 0);
    inst = &psPhase->psInst[0];
    while (inst <= psLastInstruction)
    {
        for (k = 0; k < inst->ui32NumOperands; k++)
            GatherOperandAccessMasks(&inst->asOperands[k], &opAccessMasks[0]);
        inst++;
    }

    for (i = 0; i < psPhase->ui32TotalTemps; i++)
    {
        uint32_t rebase, count;
        uint32_t newReg = i;
        uint32_t origReg = i;
        int needsMoving = 0;
        SHADER_VARIABLE_TYPE dataType;

        // Figure out rebase and count
        rebase = 0;
        count = 0;
        if (i < psPhase->ui32OrigTemps)
        {
            // One of the original registers
            k = opAccessMasks[i];
            if (k == 0)
                continue;

            while ((k & 1) == 0)
            {
                rebase++;
                k = k >> 1;
            }
            while (k != 0)
            {
                count++;
                k = k >> 1;
            }
            newReg = i + ui32MaxOrigTemps * rebase;
            if (rebase != 0)
                needsMoving = 1;
        }
        else
        {
            // Newly created split registers, read info from table
            // Read the count and rebase from split info table
            count = (psPhase->pui32SplitInfo[i] >> 24) & 0xff;
            rebase = (psPhase->pui32SplitInfo[i] >> 16) & 0xff;
            origReg = 0x10000 + i;
            newReg = (psPhase->pui32SplitInfo[i]) & 0xffff;
            while (psPhase->pui32SplitInfo[newReg] != 0xffffffff)
                newReg = (psPhase->pui32SplitInfo[newReg]) & 0xffff;

            // If count is 4, verify that we have both first and last bit set
            ASSERT(count != 4 || (opAccessMasks[i] & 9) == 9);

            newReg = newReg + ui32MaxOrigTemps * rebase;

            // Don't rebase again
            rebase = 0;
            needsMoving = 1;
        }

        if (needsMoving)
        {
            //          printf("Moving reg %d to %d, count %d rebase %d\n", origReg, newReg, count, rebase);

            // Move directly to correct location
            inst = &psPhase->psInst[0];
            while (inst <= psLastInstruction)
            {
                for (k = 0; k < inst->ui32NumOperands; k++)
                    inst->ChangeOperandTempRegister(&inst->asOperands[k], origReg, newReg, OPERAND_4_COMPONENT_MASK_ALL, UD_CHANGE_ALL, rebase);
                inst++;
            }
        }
        // Mark the count
        dataType = psPhase->peTempTypes[i * 4 + rebase];
        switch (dataType)
        {
            default:
                ASSERT(0);
                break;
            case SVT_BOOL:
                psShader->psBoolTempSizes[newReg] = std::max(psShader->psBoolTempSizes[newReg], (char)count);
                break;
            case SVT_FLOAT:
                psShader->psFloatTempSizes[newReg] = std::max(psShader->psFloatTempSizes[newReg], (char)count);
                break;
            case SVT_FLOAT16:
                psShader->psFloat16TempSizes[newReg] = std::max(psShader->psFloat16TempSizes[newReg], (char)count);
                break;
            case SVT_FLOAT10:
                psShader->psFloat10TempSizes[newReg] = std::max(psShader->psFloat10TempSizes[newReg], (char)count);
                break;
            case SVT_INT:
                psShader->psIntTempSizes[newReg] = std::max(psShader->psIntTempSizes[newReg], (char)count);
                break;
            case SVT_INT16:
                psShader->psInt16TempSizes[newReg] = std::max(psShader->psInt16TempSizes[newReg], (char)count);
                break;
            case SVT_INT12:
                psShader->psInt12TempSizes[newReg] = std::max(psShader->psInt12TempSizes[newReg], (char)count);
                break;
            case SVT_UINT:
                psShader->psUIntTempSizes[newReg] = std::max(psShader->psUIntTempSizes[newReg], (char)count);
                break;
            case SVT_UINT16:
                psShader->psUInt16TempSizes[newReg] = std::max(psShader->psUInt16TempSizes[newReg], (char)count);
                break;
            case SVT_DOUBLE:
                psShader->psDoubleTempSizes[newReg] = std::max(psShader->psDoubleTempSizes[newReg], (char)count);
                break;
        }
    }
}

// Mark whether the temp registers are used per each data type.
void Shader::PruneTempRegisters()
{
    uint32_t k;
    uint32_t maxOrigTemps = 0;
    uint32_t maxTotalTemps = 0;
    // First find the total amount of temps
    for (k = 0; k < asPhases.size(); k++)
    {
        ShaderPhase *psPhase = &asPhases[k];
        maxOrigTemps = std::max(maxOrigTemps, psPhase->ui32OrigTemps);
        maxTotalTemps = std::max(maxTotalTemps, psPhase->ui32TotalTemps);
    }

    if (maxTotalTemps == 0)
        return; // splitarrays are nulls, no need to free

    // Allocate and zero-initialize arrays for each temp sizes. *4 is for every possible rebase
    psIntTempSizes.clear();
    psIntTempSizes.resize(maxOrigTemps * 4, 0);
    psInt12TempSizes.clear();
    psInt12TempSizes.resize(maxOrigTemps * 4, 0);
    psInt16TempSizes.clear();
    psInt16TempSizes.resize(maxOrigTemps * 4, 0);
    psUIntTempSizes.clear();
    psUIntTempSizes.resize(maxOrigTemps * 4, 0);
    psUInt16TempSizes.clear();
    psUInt16TempSizes.resize(maxOrigTemps * 4, 0);
    psFloatTempSizes.clear();
    psFloatTempSizes.resize(maxOrigTemps * 4, 0);
    psFloat16TempSizes.clear();
    psFloat16TempSizes.resize(maxOrigTemps * 4, 0);
    psFloat10TempSizes.clear();
    psFloat10TempSizes.resize(maxOrigTemps * 4, 0);
    psDoubleTempSizes.clear();
    psDoubleTempSizes.resize(maxOrigTemps * 4, 0);
    psBoolTempSizes.clear();
    psBoolTempSizes.resize(maxOrigTemps * 4, 0);

    for (k = 0; k < asPhases.size(); k++)
    {
        ShaderPhase *psPhase = &asPhases[k];
        CoalesceTemps(this, psPhase, maxOrigTemps);
        if (psPhase->psTempDeclaration)
            psPhase->psTempDeclaration->value.ui32NumTemps = maxOrigTemps * 4;
    }
}

static void DoSignatureAnalysis(std::vector<ShaderInfo::InOutSignature> &psSignatures, std::vector<unsigned char> &outTable)
{
    // Fill the char, 2 bits per component so that each 2 bits encode the following info:
    // 0: unused OR used by the first signature we happened to see
    // 1: used by the second signature
    // 2: used by the third sig
    // 3: used by the fourth sig.

    // The counters for each input/output/patch. Start with 8 registers, grow as needed
    std::vector<unsigned char> counters(8, (unsigned char)0);
    outTable.clear();
    outTable.resize(8, (unsigned char)0);

    size_t i;
    for (i = 0; i < psSignatures.size(); i++)
    {
        ShaderInfo::InOutSignature *psSig = &psSignatures[i];
        char currCounter;
        char mask;
        ASSERT(psSig != NULL);

        // We'll skip SV_Depth and others that put -1 to the register.
        if (psSig->ui32Register == 0xffffffffu)
            continue;

        // Make sure there's enough room in the table
        if (psSig->ui32Register >= counters.size())
        {
            counters.resize(psSig->ui32Register * 2, 0);
            outTable.resize(psSig->ui32Register * 2, 0);
        }

        // Apply counter value to masked items
        currCounter = counters[psSig->ui32Register];
        // Duplicate counter bits
        currCounter = currCounter | (currCounter << 2) | (currCounter << 4) | (currCounter << 6);
        // Widen the mask
        mask = (unsigned char)psSig->ui32Mask;
        mask = ((mask & 8) << 3) | ((mask & 4) << 2) | ((mask & 2) << 1) | (mask & 1);
        mask = mask | (mask << 1);
        // Write output
        outTable[psSig->ui32Register] |= (currCounter & mask);
        // Update counter
        counters[psSig->ui32Register]++;
    }
}

void Shader::DoIOOverlapOperand(ShaderPhase *psPhase, Operand *psOperand)
{
    uint32_t i;
    uint32_t regSpace = psOperand->GetRegisterSpace(eShaderType, psPhase->ePhase);
    unsigned char *redirectTable = NULL;
    unsigned char redir = 0;
    unsigned char firstFound = 0;
    uint32_t mask;

    for (i = 0; i < MAX_SUB_OPERANDS; i++)
        if (psOperand->m_SubOperands[i].get())
            DoIOOverlapOperand(psPhase, psOperand->m_SubOperands[i].get());


    switch (psOperand->eType)
    {
        case OPERAND_TYPE_INPUT:
        case OPERAND_TYPE_INPUT_CONTROL_POINT:
        case OPERAND_TYPE_INPUT_PATCH_CONSTANT:
            redirectTable = regSpace == 0 ? &psPhase->acInputNeedsRedirect[0] : &psPhase->acPatchConstantsNeedsRedirect[0];
            break;

        case OPERAND_TYPE_OUTPUT:
        case OPERAND_TYPE_OUTPUT_CONTROL_POINT:
            redirectTable = regSpace == 0 ? &psPhase->acOutputNeedsRedirect[0] : &psPhase->acPatchConstantsNeedsRedirect[0];
            break;

        default:
            // Not a input or output, nothing to do here
            return;
    }

    redir = redirectTable[psOperand->ui32RegisterNumber];

    if (redir == 0xff) // Already found overlap?
        return;

    mask = psOperand->GetAccessMask();
    i = 0;
    // Find the first mask bit set.
    while ((mask & (1 << i)) == 0)
        i++;

    firstFound = (redir >> (i * 2)) & 3;
    for (; i < 4; i++)
    {
        unsigned char sig;
        if ((mask & (1 << i)) == 0)
            continue;

        sig = (redir >> (i * 2)) & 3;
        // All set bits must access the same signature
        if (sig != firstFound)
        {
            redirectTable[psOperand->ui32RegisterNumber] = 0xff;
            return;
        }
    }
}

static void PruneRedirectEntry(unsigned char &itr)
{
    if (itr != 0xff)
        itr = 0;
}

// Check if inputs and outputs are accessed across semantic boundaries
// as in, 2x texcoord vec2's are packed together as vec4 but still accessed together.
void Shader::AnalyzeIOOverlap()
{
    uint32_t i, k;
    std::vector<unsigned char> outData;
    DoSignatureAnalysis(sInfo.psInputSignatures, outData);

    // Now data has the values, copy them to all phases
    for (i = 0; i < asPhases.size(); i++)
        asPhases[i].acInputNeedsRedirect = outData;

    DoSignatureAnalysis(sInfo.psOutputSignatures, outData);
    for (i = 0; i < asPhases.size(); i++)
        asPhases[i].acOutputNeedsRedirect = outData;

    DoSignatureAnalysis(sInfo.psPatchConstantSignatures, outData);
    for (i = 0; i < asPhases.size(); i++)
        asPhases[i].acPatchConstantsNeedsRedirect = outData;

    // Now walk through all operands and suboperands in all instructions and write 0xff to the dest (cannot occur otherwise)
    // if we're crossing signature borders
    for (i = 0; i < asPhases.size(); i++)
    {
        ShaderPhase *psPhase = &asPhases[i];
        for (k = 0; k < psPhase->psInst.size(); k++)
        {
            Instruction *psInst = &psPhase->psInst[k];
            uint32_t j;
            for (j = 0; j < psInst->ui32NumOperands; j++)
                DoIOOverlapOperand(psPhase, &psInst->asOperands[j]);
        }

        // Now prune all tables from anything except 0xff.
        std::for_each(psPhase->acInputNeedsRedirect.begin(), psPhase->acInputNeedsRedirect.end(), PruneRedirectEntry);
        std::for_each(psPhase->acOutputNeedsRedirect.begin(), psPhase->acOutputNeedsRedirect.end(), PruneRedirectEntry);
        std::for_each(psPhase->acPatchConstantsNeedsRedirect.begin(), psPhase->acPatchConstantsNeedsRedirect.end(), PruneRedirectEntry);
    }
}

void Shader::SetMaxSemanticIndex()
{
    for (std::vector<ShaderInfo::InOutSignature>::iterator it = sInfo.psInputSignatures.begin(); it != sInfo.psInputSignatures.end(); ++it)
        maxSemanticIndex = std::max(maxSemanticIndex, it->ui32SemanticIndex);

    for (std::vector<ShaderInfo::InOutSignature>::iterator it = sInfo.psOutputSignatures.begin(); it != sInfo.psOutputSignatures.end(); ++it)
        maxSemanticIndex = std::max(maxSemanticIndex, it->ui32SemanticIndex);

    for (std::vector<ShaderInfo::InOutSignature>::iterator it = sInfo.psPatchConstantSignatures.begin(); it != sInfo.psPatchConstantSignatures.end(); ++it)
        maxSemanticIndex = std::max(maxSemanticIndex, it->ui32SemanticIndex);
}

// In DX bytecode, all const arrays are vec4's, and all arrays are stuffed to one large array.
// Luckily, each chunk is always accessed with suboperand plus <constant> (in ui32RegisterNumber)
// So do an analysis pass. Also trim the vec4's into smaller formats if the extra components are never read.
void ShaderPhase::PruneConstArrays()
{
    using namespace std;
    auto customDataItr = find_if(psDecl.begin(), psDecl.end(), [](const Declaration &d) { return d.eOpcode == OPCODE_CUSTOMDATA; });
    // Not found? We're done.
    if (customDataItr == psDecl.end())
        return;

    // Store the original declaration
    m_ConstantArrayInfo.m_OrigDeclaration = &(*customDataItr);

    // Loop through each operand and pick up usage masks
    HLSLcc::ForEachOperand(psInst.begin(), psInst.end(), FEO_FLAG_ALL, [this](const std::vector<Instruction>::iterator &psInst, const Operand *psOperand, uint32_t ui32OperandType)
    {
        using namespace std;
        if (psOperand->eType == OPERAND_TYPE_IMMEDIATE_CONSTANT_BUFFER)
        {
            uint32_t accessMask = psOperand->GetAccessMask();
            uint32_t offset = psOperand->ui32RegisterNumber;

            // Update the chunk access mask

            // Find all existing entries that have anything common with the access mask
            auto cbrange = m_ConstantArrayInfo.m_Chunks.equal_range(offset);
            vector<ChunkMap::iterator> matchingEntries;
            for (auto itr = cbrange.first; itr != cbrange.second; itr++)
            {
                if ((itr->second.m_AccessMask & accessMask) != 0)
                {
                    matchingEntries.push_back(itr);
                }
            }

            if (matchingEntries.empty())
            {
                // Not found, create new entry
                m_ConstantArrayInfo.m_Chunks.insert(make_pair(offset, ConstantArrayChunk(0u, accessMask, (Operand *)psOperand)));
            }
            else if (matchingEntries.size() == 1)
            {
                // Update access mask of the one existing entry
                matchingEntries[0]->second.m_AccessMask |= accessMask;
                matchingEntries[0]->second.m_UseSites.push_back((Operand *)psOperand);
            }
            else
            {
                // Multiple entries with (now) overlapping mask. Merge to the first one.
                ChunkMap::iterator tgt = matchingEntries[0];
                tgt->second.m_AccessMask |= accessMask;
                tgt->second.m_UseSites.push_back((Operand *)psOperand);
                ChunkMap &chunks = m_ConstantArrayInfo.m_Chunks;
                for_each(matchingEntries.begin() + 1, matchingEntries.end(), [&tgt, &chunks](ChunkMap::iterator itr)
                {
                    tgt->second.m_AccessMask |= itr->second.m_AccessMask;
                    chunks.erase(itr);
                });
            }
        }
    });

    // Figure out how large each chunk is by finding the next chunk that uses any bits from the current mask (or the max size if not found)

    uint32_t totalSize = (uint32_t)m_ConstantArrayInfo.m_OrigDeclaration->asImmediateConstBuffer.size();
    for (auto chunk = m_ConstantArrayInfo.m_Chunks.begin(); chunk != m_ConstantArrayInfo.m_Chunks.end(); chunk++)
    {
        // Find the next chunk that shares any bits in the access mask
        auto nextItr = find_if(m_ConstantArrayInfo.m_Chunks.lower_bound(chunk->first + 1), m_ConstantArrayInfo.m_Chunks.end(), [&chunk](ChunkMap::value_type &itr)
        {
            return (chunk->second.m_AccessMask & itr.second.m_AccessMask) != 0;
        });

        // Not found? Must continue until the end of array
        if (nextItr == m_ConstantArrayInfo.m_Chunks.end())
            chunk->second.m_Size = totalSize - chunk->first;
        else
        {
            // Otherwise we know the chunk size directly.
            chunk->second.m_Size = nextItr->first - chunk->first;
        }

        // Do rebase on the operands if necessary
        chunk->second.m_Rebase = 0;
        uint32_t t = chunk->second.m_AccessMask;
        ASSERT(t != 0);
        while ((t & 1) == 0)
        {
            chunk->second.m_Rebase++;
            t >>= 1;
        }
        uint32_t rebase = chunk->second.m_Rebase;
        uint32_t componentCount = 0;
        while (t != 0)
        {
            componentCount++;
            t >>= 1;
        }
        chunk->second.m_ComponentCount = componentCount;

        for_each(chunk->second.m_UseSites.begin(), chunk->second.m_UseSites.end(), [&rebase, &componentCount](Operand *op)
        {
            // Store the rebase value to each operand and do the actual rebase.
            op->m_Rebase = rebase;
            op->m_Size = componentCount;

            if (rebase != 0)
            {
                // Update component mask. Note that we don't need to do anything to the suboperands. They do not affect destination writemask.
                switch (op->eSelMode)
                {
                    case OPERAND_4_COMPONENT_MASK_MODE:
                        {
                            uint32_t oldMask = op->ui32CompMask;
                            if (oldMask == 0)
                                oldMask = OPERAND_4_COMPONENT_MASK_ALL;

                            // Check that we're not losing any information
                            ASSERT((oldMask >> rebase) << rebase == oldMask);
                            op->ui32CompMask = (oldMask >> rebase);
                            break;
                        }
                    case OPERAND_4_COMPONENT_SELECT_1_MODE:
                        ASSERT(op->aui32Swizzle[0] >= rebase);
                        op->aui32Swizzle[0] -= rebase;
                        break;
                    case OPERAND_4_COMPONENT_SWIZZLE_MODE:
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                // Note that this rebase is different from the one done for source operands
                                ASSERT(op->aui32Swizzle[i] >= rebase);
                                op->aui32Swizzle[i] -= rebase;
                            }
                            break;
                        }
                    default:
                        ASSERT(0);
                }
            }
        });
    }


    // We'll do the actual declaration and pruning later on, now that we have the info stored up.
}

HLSLcc::ControlFlow::ControlFlowGraph &ShaderPhase::GetCFG()
{
    if (!m_CFGInitialized)
    {
        m_CFG.Build(psInst.data(), psInst.data() + psInst.size());
        m_CFGInitialized = true;
    }

    return m_CFG;
}

void ShaderPhase::UnvectorizeImmMoves()
{
    // NOTE must be called before datatype analysis and other analysis phases are done, as the pointers won't match anymore
    // (we insert new instructions there)
    using namespace std;
    vector<Instruction> nInst;
    // Reserve 1.5x space
    nInst.reserve(psInst.size() * 3 / 2);

    for_each(psInst.begin(), psInst.end(), [&](Instruction &i)
    {
        if (i.eOpcode != OPCODE_MOV || i.asOperands[0].eType != OPERAND_TYPE_TEMP || i.asOperands[1].eType != OPERAND_TYPE_IMMEDIATE32 || i.asOperands[0].GetNumSwizzleElements() == 1)
        {
            nInst.push_back(i);
            return;
        }
        // Ok, found one to unvectorize.
        ASSERT(i.asOperands[0].eSelMode == OPERAND_4_COMPONENT_MASK_MODE);
        uint32_t mask = i.asOperands[0].ui32CompMask;
        for (uint32_t j = 0; j < 4; j++)
        {
            if ((mask & (1 << j)) == 0)
                continue;

            Instruction ni = i;
            ni.asOperands[0].ui32CompMask = (1 << j);
            nInst.push_back(ni);
        }
    });
    psInst.clear();
    psInst.swap(nInst);
}

void ShaderPhase::ExpandSWAPCs()
{
    // First find the DCL_TEMPS declaration
    auto dcitr = std::find_if(psDecl.begin(), psDecl.end(), [](const Declaration &decl) -> bool { return decl.eOpcode == OPCODE_DCL_TEMPS; });
    if (dcitr == psDecl.end())
    {
        // No temp declaration? Probably we won't have SWAPC either, then.
        return;
    }
    Declaration &tmpDecl = *dcitr;

    uint32_t extraTemp = 0;
    bool extraTempAllocated = false;

    // Parse through instructions, open up SWAPCs if necessary
    while (1)
    {
        // Need to find from top every time, because we're inserting stuff into the vector
        auto swapItr = std::find_if(psInst.begin(), psInst.end(), [](const Instruction &inst) -> bool { return inst.eOpcode == OPCODE_SWAPC; });
        if (swapItr == psInst.end())
            break;

        // Ok swapItr now points to a SWAPC instruction that we'll have to split up like this (from MSDN):

/*      swapc dest0[.mask],
            dest1[.mask],
            src0[.swizzle],
            src1[.swizzle],
            src2[.swizzle]

            expands to :

        movc temp[dest0s mask],
            src0[.swizzle],
            src2[.swizzle], src1[.swizzle]

            movc dest1[.mask],
            src0[.swizzle],
            src1[.swizzle], src2[.swizzle]

            mov  dest0.mask, temp
*/
        // Allocate a new temp, if not already done
        if (!extraTempAllocated)
        {
            extraTemp = tmpDecl.value.ui32NumTemps++;
            extraTempAllocated = true;
        }

        Instruction origSwapInst;
#if _DEBUG
        origSwapInst.id = swapItr->id;
#endif
        std::swap(*swapItr, origSwapInst); // Store the original swapc for reading

        // OP 1: MOVC temp[dest0 mask], src0, src2, stc1
        swapItr->eOpcode = OPCODE_MOVC;
        swapItr->ui32NumOperands = 4;
        swapItr->ui32FirstSrc = 1;
        swapItr->asOperands[0] = origSwapInst.asOperands[0];
        swapItr->asOperands[0].eType = OPERAND_TYPE_TEMP;
        swapItr->asOperands[0].ui32RegisterNumber = extraTemp;
        // mask is already fine
        swapItr->asOperands[1] = origSwapInst.asOperands[2]; // src0
        swapItr->asOperands[2] = origSwapInst.asOperands[4]; // src2
        swapItr->asOperands[3] = origSwapInst.asOperands[3]; // src1
        // swapItr is already in the psInst vector.

        Instruction newInst[2] = { Instruction(), Instruction() };
        // OP 2: MOVC dest1, src0, src1, src2
        newInst[0].eOpcode = OPCODE_MOVC;
        newInst[0].ui32NumOperands = 4;
        newInst[0].ui32FirstSrc = 1;
        newInst[0].asOperands[0] = origSwapInst.asOperands[1]; // dest1
        newInst[0].asOperands[1] = origSwapInst.asOperands[2]; // src0
        newInst[0].asOperands[2] = origSwapInst.asOperands[3]; // src1
        newInst[0].asOperands[3] = origSwapInst.asOperands[4]; // src2
#if _DEBUG
        newInst[0].id = swapItr->id;
#endif

        // OP 3: mov  dest0.mask, temp
        newInst[1].eOpcode = OPCODE_MOV;
        newInst[1].ui32NumOperands = 2;
        newInst[1].ui32FirstSrc = 1;
        newInst[1].asOperands[0] = origSwapInst.asOperands[0]; // dest 0
        // First copy dest0 to src as well to get the mask set up correctly
        newInst[1].asOperands[1] = origSwapInst.asOperands[0]; // dest 0;
        // Then overwrite with temp reg
        newInst[1].asOperands[1].eType = OPERAND_TYPE_TEMP;
        newInst[1].asOperands[1].ui32RegisterNumber = extraTemp;
#if _DEBUG
        newInst[1].id = swapItr->id;
#endif

        // Insert the new instructions to the vector
        psInst.insert(swapItr + 1, newInst, newInst + 2);
    }
}

void Shader::ExpandSWAPCs()
{
    // Just call ExpandSWAPCs for each phase
    for (int i = 0; i < asPhases.size(); i++)
    {
        asPhases[i].ExpandSWAPCs();
    }
}

void Shader::ForcePositionToHighp()
{
    // Only sensible in vertex shaders (TODO: is this an issue in tessellation shaders? Do we even care?)
    if (eShaderType != VERTEX_SHADER)
        return;

    ShaderPhase &phase = asPhases[0];

    // Find the output declaration
    std::vector<Declaration>::iterator itr = std::find_if(phase.psDecl.begin(), phase.psDecl.end(), [this](const Declaration &decl) -> bool
    {
        if (decl.eOpcode == OPCODE_DCL_OUTPUT_SIV)
        {
            const SPECIAL_NAME specialName = decl.asOperands[0].eSpecialName;
            if (specialName == NAME_POSITION ||
                specialName == NAME_UNDEFINED) // This might be SV_Position (because d3dcompiler is weird).
            {
                const ShaderInfo::InOutSignature *sig = NULL;
                sInfo.GetOutputSignatureFromRegister(decl.asOperands[0].ui32RegisterNumber, decl.asOperands[0].GetAccessMask(), 0, &sig);
                ASSERT(sig != NULL);
                if ((sig->eSystemValueType == NAME_POSITION || sig->semanticName == "POS") && sig->ui32SemanticIndex == 0)
                {
                    ((ShaderInfo::InOutSignature *)sig)->eMinPrec = MIN_PRECISION_DEFAULT;
                    return true;
                }
            }
            return false;
        }
        else if (decl.eOpcode == OPCODE_DCL_OUTPUT)
        {
            const ShaderInfo::InOutSignature *sig = NULL;
            sInfo.GetOutputSignatureFromRegister(decl.asOperands[0].ui32RegisterNumber, decl.asOperands[0].GetAccessMask(), 0, &sig);
            ASSERT(sig != NULL);
            if ((sig->eSystemValueType == NAME_POSITION || sig->semanticName == "POS") && sig->ui32SemanticIndex == 0)
            {
                ((ShaderInfo::InOutSignature *)sig)->eMinPrec = MIN_PRECISION_DEFAULT;
                return true;
            }
            return false;
        }
        return false;
    });

    // Do nothing if we don't find suitable output. This may well be INTERNALTESSPOS for tessellation etc.
    if (itr == phase.psDecl.end())
        return;

    uint32_t outputPosReg = itr->asOperands[0].ui32RegisterNumber;

    HLSLcc::ForEachOperand(phase.psInst.begin(), phase.psInst.end(), FEO_FLAG_DEST_OPERAND, [outputPosReg](std::vector<Instruction>::iterator itr, Operand *op, uint32_t flags)
    {
        if (op->eType == OPERAND_TYPE_OUTPUT && op->ui32RegisterNumber == outputPosReg)
            op->eMinPrecision = OPERAND_MIN_PRECISION_DEFAULT;
    });
}

void Shader::FindUnusedGlobals(uint32_t flags)
{
    for (int i = 0; i < asPhases.size(); i++)
    {
        ShaderPhase &phase = asPhases[i];

        // Loop through every operand and pick up usages
        HLSLcc::ForEachOperand(phase.psInst.begin(), phase.psInst.end(), FEO_FLAG_SRC_OPERAND | FEO_FLAG_SUBOPERAND, [&](std::vector<Instruction>::iterator inst, Operand *op, uint32_t flags)
        {
            // Not a constant buffer read? continue
            if (op->eType != OPERAND_TYPE_CONSTANT_BUFFER)
                return;

            const uint32_t ui32BindingPoint = op->aui32ArraySizes[0];
            const ConstantBuffer *psCBuf = NULL;
            sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, ui32BindingPoint, &psCBuf);

            if (!psCBuf)
                return;

            // Get all the struct members that can be reached from this usage:
            uint32_t mask = op->GetAccessMask();
            for (uint32_t k = 0; k < 4; k++)
            {
                if ((mask & (1 << k)) == 0)
                    continue;

                uint32_t tmpSwizzle[4] = {k, k, k, k};
                int rebase;
                bool isArray;

                ShaderVarType *psVarType = NULL;

                ShaderInfo::GetShaderVarFromOffset(op->aui32ArraySizes[1], tmpSwizzle, psCBuf, (const ShaderVarType**)&psVarType, &isArray, NULL, &rebase, flags);

                // Mark as used. Also all parents.
                while (psVarType)
                {
                    psVarType->m_IsUsed = true;
                    psVarType = psVarType->Parent;
                }
            }
        });
    }
}
