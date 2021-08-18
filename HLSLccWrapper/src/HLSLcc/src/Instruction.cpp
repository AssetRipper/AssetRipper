#include "internal_includes/Instruction.h"
#include "internal_includes/debug.h"
#include "include/ShaderInfo.h"

// Returns the result swizzle operand for an instruction, or NULL if all src operands have swizzles
static Operand *GetSrcSwizzleOperand(Instruction *psInst)
{
    switch (psInst->eOpcode)
    {
        case OPCODE_DP2:
        case OPCODE_DP3:
        case OPCODE_DP4:
        case OPCODE_NOP:
        case OPCODE_SWAPC:
        case OPCODE_SAMPLE_C:
        case OPCODE_SAMPLE_C_LZ:
            ASSERT(0);
            return NULL;

        // Normal arithmetics, all srcs have swizzles
        case OPCODE_ADD:
        case OPCODE_AND:
        case OPCODE_DERIV_RTX:
        case OPCODE_DERIV_RTX_COARSE:
        case OPCODE_DERIV_RTX_FINE:
        case OPCODE_DERIV_RTY:
        case OPCODE_DERIV_RTY_COARSE:
        case OPCODE_DERIV_RTY_FINE:
        case OPCODE_DIV:
        case OPCODE_EQ:
        case OPCODE_EXP:
        case OPCODE_FRC:
        case OPCODE_FTOI:
        case OPCODE_FTOU:
        case OPCODE_GE:
        case OPCODE_IADD:
        case OPCODE_IEQ:
        case OPCODE_IGE:
        case OPCODE_ILT:
        case OPCODE_IMAD:
        case OPCODE_IMAX:
        case OPCODE_IMIN:
        case OPCODE_IMUL:
        case OPCODE_INE:
        case OPCODE_INEG:
        case OPCODE_ITOF:
        case OPCODE_LOG:
        case OPCODE_LT:
        case OPCODE_MAD:
        case OPCODE_MAX:
        case OPCODE_MIN:
        case OPCODE_MOV:
        case OPCODE_MUL:
        case OPCODE_NE:
        case OPCODE_NOT:
        case OPCODE_OR:
        case OPCODE_ROUND_NE:
        case OPCODE_ROUND_NI:
        case OPCODE_ROUND_PI:
        case OPCODE_ROUND_Z:
        case OPCODE_RSQ:
        case OPCODE_SINCOS:
        case OPCODE_SQRT:
        case OPCODE_UDIV:
        case OPCODE_UGE:
        case OPCODE_ULT:
        case OPCODE_UMAD:
        case OPCODE_UMAX:
        case OPCODE_UMIN:
        case OPCODE_UMUL:
        case OPCODE_UTOF:
        case OPCODE_XOR:

        case OPCODE_BFI:
        case OPCODE_BFREV:
        case OPCODE_COUNTBITS:
        case OPCODE_DADD:
        case OPCODE_DDIV:
        case OPCODE_DEQ:
        case OPCODE_DFMA:
        case OPCODE_DGE:
        case OPCODE_DLT:
        case OPCODE_DMAX:
        case OPCODE_DMIN:
        case OPCODE_DMUL:
        case OPCODE_DMOV:
        case OPCODE_DNE:
        case OPCODE_DRCP:
        case OPCODE_DTOF:
        case OPCODE_F16TOF32:
        case OPCODE_F32TOF16:
        case OPCODE_FIRSTBIT_HI:
        case OPCODE_FIRSTBIT_LO:
        case OPCODE_FIRSTBIT_SHI:
        case OPCODE_FTOD:
        case OPCODE_IBFE:
        case OPCODE_RCP:
        case OPCODE_UADDC:
        case OPCODE_UBFE:
        case OPCODE_USUBB:
        case OPCODE_MOVC:
        case OPCODE_DMOVC:
            return NULL;

        // Special cases:
        case OPCODE_GATHER4:
        case OPCODE_GATHER4_C:
        case OPCODE_LD:
        case OPCODE_LD_MS:
        case OPCODE_LOD:
        case OPCODE_LD_UAV_TYPED:
        case OPCODE_LD_RAW:
        case OPCODE_SAMPLE:
        case OPCODE_SAMPLE_B:
        case OPCODE_SAMPLE_L:
        case OPCODE_SAMPLE_D:
        case OPCODE_RESINFO:
            return &psInst->asOperands[2];

        case OPCODE_GATHER4_PO:
        case OPCODE_GATHER4_PO_C:
        case OPCODE_LD_STRUCTURED:
            return &psInst->asOperands[3];

        case OPCODE_SAMPLE_INFO:
            return &psInst->asOperands[1];

        case OPCODE_ISHL:
        case OPCODE_ISHR:
        case OPCODE_USHR:
            // sm4 variant has single component selection on src1 -> only src0 has swizzle
            if (psInst->asOperands[2].eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE)
                return &psInst->asOperands[1];
            else // whereas sm5 variant has swizzle also on src1
                return NULL;

        default:
            ASSERT(0);
            return NULL;
    }
}

// Tweak the source operands of an instruction so that the rebased write mask will still work
static void DoSrcOperandRebase(Operand *psOperand, uint32_t rebase)
{
    uint32_t i;
    switch (psOperand->eSelMode)
    {
        default:
        case OPERAND_4_COMPONENT_MASK_MODE:
            ASSERT(psOperand->ui32CompMask == 0 || psOperand->ui32CompMask == OPERAND_4_COMPONENT_MASK_ALL);

            // Special case for immediates, they do not have swizzles
            if (psOperand->eType == OPERAND_TYPE_IMMEDIATE32)
            {
                if (psOperand->iNumComponents > 1)
                    std::copy(&psOperand->afImmediates[rebase], &psOperand->afImmediates[4], &psOperand->afImmediates[0]);
                return;
            }
            if (psOperand->eType == OPERAND_TYPE_IMMEDIATE64)
            {
                if (psOperand->iNumComponents > 1)
                    std::copy(&psOperand->adImmediates[rebase], &psOperand->adImmediates[4], &psOperand->adImmediates[0]);
                return;
            }

            // Need to change this to swizzle
            psOperand->eSelMode = OPERAND_4_COMPONENT_SWIZZLE_MODE;
            psOperand->ui32Swizzle = 0;
            for (i = 0; i < 4 - rebase; i++)
                psOperand->aui32Swizzle[i] = i + rebase;
            for (; i < 4; i++)
                psOperand->aui32Swizzle[i] = rebase; // The first actual input.
            break;
        case OPERAND_4_COMPONENT_SELECT_1_MODE:
            // Nothing to do
            break;
        case OPERAND_4_COMPONENT_SWIZZLE_MODE:
            for (i = rebase; i < 4; i++)
                psOperand->aui32Swizzle[i - rebase] = psOperand->aui32Swizzle[i];
            break;
    }
}

void Instruction::ChangeOperandTempRegister(Operand *psOperand, uint32_t oldReg, uint32_t newReg, uint32_t compMask, uint32_t flags, uint32_t rebase)
{
    uint32_t i = 0;
    uint32_t accessMask = 0;
    int isDestination = 0;
    Operand *psSwizzleOperand = NULL;

    if (flags & UD_CHANGE_SUBOPERANDS)
    {
        for (i = 0; i < MAX_SUB_OPERANDS; i++)
        {
            if (psOperand->m_SubOperands[i].get())
                ChangeOperandTempRegister(psOperand->m_SubOperands[i].get(), oldReg, newReg, compMask, UD_CHANGE_ALL, rebase);
        }
    }

    if ((flags & UD_CHANGE_MAIN_OPERAND) == 0)
        return;

    if (psOperand->eType != OPERAND_TYPE_TEMP)
        return;

    if (psOperand->ui32RegisterNumber != oldReg)
        return;

    accessMask = psOperand->GetAccessMask();
    // If this operation touches other components than the one(s) we're splitting, skip it
    if ((accessMask & (~compMask)) != 0)
    {
        // Verify that we've not messed up in reachability analysis.
        // This would mean that we've encountered an instruction that accesses
        // a component in multi-component mode and we're supposed to treat it as single-use only.
        // Now that we track operands we can bring this back
        ASSERT((accessMask & compMask) == 0);
        return;
    }

#if 0
    printf("Updating operand %d with access mask %X\n", (int)psOperand->id, accessMask);
#endif
    psOperand->ui32RegisterNumber = newReg;

    if (rebase == 0)
        return;

    // Update component mask. Note that we don't need to do anything to the suboperands. They do not affect destination writemask.
    switch (psOperand->eSelMode)
    {
        case OPERAND_4_COMPONENT_MASK_MODE:
        {
            uint32_t oldMask = psOperand->ui32CompMask;
            if (oldMask == 0)
                oldMask = OPERAND_4_COMPONENT_MASK_ALL;

            // Check that we're not losing any information
            ASSERT((oldMask >> rebase) << rebase == oldMask);
            psOperand->ui32CompMask = (oldMask >> rebase);
            break;
        }
        case OPERAND_4_COMPONENT_SELECT_1_MODE:
            ASSERT(psOperand->aui32Swizzle[0] >= rebase);
            psOperand->aui32Swizzle[0] -= rebase;
            break;
        case OPERAND_4_COMPONENT_SWIZZLE_MODE:
        {
            for (i = 0; i < 4; i++)
            {
                // Note that this rebase is different from the one done for source operands
                ASSERT(psOperand->aui32Swizzle[i] >= rebase);
                psOperand->aui32Swizzle[i] -= rebase;
            }
            break;
        }
        default:
            ASSERT(0);
    }

    // Tweak operand datatypes
    std::copy(&psOperand->aeDataType[rebase], &psOperand->aeDataType[4], &psOperand->aeDataType[0]);

    // If this operand is a destination, we'll need to tweak sources as well
    for (i = 0; i < ui32FirstSrc; i++)
    {
        if (psOperand == &asOperands[i])
        {
            isDestination = 1;
            break;
        }
    }

    if (isDestination == 0)
        return;

    // Nasty corner case of 2 destinations, not supported if both targets are written
    ASSERT((ui32FirstSrc < 2) || (asOperands[0].eType == OPERAND_TYPE_NULL) || (asOperands[1].eType == OPERAND_TYPE_NULL));

    // If we made it this far, we're rebasing a destination temp (and the only destination), need to tweak sources depending on the instruction
    switch (eOpcode)
    {
        // The opcodes that do not need tweaking:
        case OPCODE_DP2:
        case OPCODE_DP3:
        case OPCODE_DP4:
        case OPCODE_BUFINFO:
        case OPCODE_SAMPLE_C:
        case OPCODE_SAMPLE_C_LZ:
            return;

        default:
            psSwizzleOperand = GetSrcSwizzleOperand(this); // Null means tweak all source operands
            if (psSwizzleOperand)
            {
                DoSrcOperandRebase(psSwizzleOperand, rebase);
                return;
            }
            else
            {
                for (i = ui32FirstSrc; i < ui32NumOperands; i++)
                {
                    DoSrcOperandRebase(&asOperands[i], rebase);
                }
            }
            return;
    }
}

// Returns nonzero if psInst is a sample instruction and the sampler has medium or low precision
bool Instruction::IsPartialPrecisionSamplerInstruction(const ShaderInfo &info, OPERAND_MIN_PRECISION *pType) const
{
    const Operand *op;
    const ResourceBinding *psBinding = NULL;
    OPERAND_MIN_PRECISION sType = OPERAND_MIN_PRECISION_DEFAULT;
    switch (eOpcode)
    {
        default:
            return false;
        case OPCODE_SAMPLE:
        case OPCODE_SAMPLE_B:
        case OPCODE_SAMPLE_L:
        case OPCODE_SAMPLE_D:
        case OPCODE_SAMPLE_C:
        case OPCODE_SAMPLE_C_LZ:
            break;
    }

    op = &asOperands[3];
    ASSERT(op->eType == OPERAND_TYPE_SAMPLER);

    info.GetResourceFromBindingPoint(RGROUP_SAMPLER, op->ui32RegisterNumber, &psBinding);
    if (!psBinding)
    {
        /* Try to look from texture group */
        info.GetResourceFromBindingPoint(RGROUP_TEXTURE, op->ui32RegisterNumber, &psBinding);
    }

    sType = Operand::ResourcePrecisionToOperandPrecision(psBinding ? psBinding->ePrecision : REFLECT_RESOURCE_PRECISION_UNKNOWN);

    if (sType == OPERAND_MIN_PRECISION_DEFAULT)
        return false;

    if (pType)
        *pType = sType;

    return true;
}
