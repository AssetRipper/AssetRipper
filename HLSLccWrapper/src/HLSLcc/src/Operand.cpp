#include "internal_includes/Operand.h"
#include "internal_includes/debug.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/Shader.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/Instruction.h"

uint32_t Operand::GetAccessMask() const
{
    int i;
    uint32_t accessMask = 0;
    // NOTE: Destination writemask can (AND DOES) affect access from sources, but we do it conservatively for now.
    switch (eSelMode)
    {
        default:
        case OPERAND_4_COMPONENT_MASK_MODE:
            // Update access mask
            accessMask = ui32CompMask;
            if (accessMask == 0)
                accessMask = OPERAND_4_COMPONENT_MASK_ALL;
            break;

        case OPERAND_4_COMPONENT_SWIZZLE_MODE:
            accessMask = 0;
            for (i = 0; i < 4; i++)
                accessMask |= 1 << (aui32Swizzle[i]);
            break;

        case OPERAND_4_COMPONENT_SELECT_1_MODE:
            accessMask = 1 << (aui32Swizzle[0]);
            break;
    }
    ASSERT(accessMask != 0);
    return accessMask;
}

int Operand::GetMaxComponent() const
{
    if (iWriteMaskEnabled &&
        iNumComponents == 4)
    {
        //Component Mask
        if (eSelMode == OPERAND_4_COMPONENT_MASK_MODE)
        {
            if (ui32CompMask != 0 && ui32CompMask != (OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z | OPERAND_4_COMPONENT_MASK_W))
            {
                if (ui32CompMask & OPERAND_4_COMPONENT_MASK_W)
                {
                    return 4;
                }
                if (ui32CompMask & OPERAND_4_COMPONENT_MASK_Z)
                {
                    return 3;
                }
                if (ui32CompMask & OPERAND_4_COMPONENT_MASK_Y)
                {
                    return 2;
                }
                if (ui32CompMask & OPERAND_4_COMPONENT_MASK_X)
                {
                    return 1;
                }
            }
        }
        else
        //Component Swizzle
        if (eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
        {
            if (ui32Swizzle == NO_SWIZZLE)
                return 4;

            uint32_t res = 0;
            for (int i = 0; i < 4; i++)
            {
                res = std::max(aui32Swizzle[i], res);
            }
            return (int)res + 1;
        }
        else if (eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE)
        {
            return 1;
        }
    }

    return 4;
}

//Single component repeated
//e..g .wwww
bool Operand::IsSwizzleReplicated() const
{
    if (iWriteMaskEnabled &&
        iNumComponents == 4)
    {
        if (eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
        {
            if (ui32Swizzle == WWWW_SWIZZLE ||
                ui32Swizzle == ZZZZ_SWIZZLE ||
                ui32Swizzle == YYYY_SWIZZLE ||
                ui32Swizzle == XXXX_SWIZZLE)
            {
                return true;
            }
        }
    }
    return false;
}

// Get the number of elements returned by operand, taking additional component mask into account
uint32_t Operand::GetNumSwizzleElements(uint32_t _ui32CompMask /* = OPERAND_4_COMPONENT_MASK_ALL */) const
{
    uint32_t count = 0;

    switch (eType)
    {
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP:
        case OPERAND_TYPE_INPUT_THREAD_ID:
        case OPERAND_TYPE_INPUT_THREAD_GROUP_ID:
            // Adjust component count and break to more processing
            ((Operand *)this)->iNumComponents = 3;
            break;
        case OPERAND_TYPE_IMMEDIATE32:
        case OPERAND_TYPE_IMMEDIATE64:
        case OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL:
        case OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL:
        case OPERAND_TYPE_OUTPUT_DEPTH:
        {
            // Translate numComponents into bitmask
            // 1 -> 1, 2 -> 3, 3 -> 7 and 4 -> 15
            uint32_t compMask = (1 << iNumComponents) - 1;

            compMask &= _ui32CompMask;
            // Calculate bits left in compMask
            return HLSLcc::GetNumberBitsSet(compMask);
        }
        default:
        {
            break;
        }
    }

    if (iWriteMaskEnabled &&
        iNumComponents != 1)
    {
        //Component Mask
        if (eSelMode == OPERAND_4_COMPONENT_MASK_MODE)
        {
            uint32_t compMask = ui32CompMask;
            if (compMask == 0)
                compMask = OPERAND_4_COMPONENT_MASK_ALL;
            compMask &= _ui32CompMask;

            if (compMask == OPERAND_4_COMPONENT_MASK_ALL)
                return 4;

            if (compMask & OPERAND_4_COMPONENT_MASK_X)
            {
                count++;
            }
            if (compMask & OPERAND_4_COMPONENT_MASK_Y)
            {
                count++;
            }
            if (compMask & OPERAND_4_COMPONENT_MASK_Z)
            {
                count++;
            }
            if (compMask & OPERAND_4_COMPONENT_MASK_W)
            {
                count++;
            }
        }
        else
        //Component Swizzle
        if (eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
        {
            uint32_t i;
            for (i = 0; i < 4; ++i)
            {
                if ((_ui32CompMask & (1 << i)) == 0)
                    continue;

                count++;
            }
        }
        else if (eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE)
        {
            if (aui32Swizzle[0] == OPERAND_4_COMPONENT_X && (_ui32CompMask & OPERAND_4_COMPONENT_MASK_X))
            {
                count++;
            }
            else if (aui32Swizzle[0] == OPERAND_4_COMPONENT_Y && (_ui32CompMask & OPERAND_4_COMPONENT_MASK_Y))
            {
                count++;
            }
            else if (aui32Swizzle[0] == OPERAND_4_COMPONENT_Z && (_ui32CompMask & OPERAND_4_COMPONENT_MASK_Z))
            {
                count++;
            }
            else if (aui32Swizzle[0] == OPERAND_4_COMPONENT_W && (_ui32CompMask & OPERAND_4_COMPONENT_MASK_W))
            {
                count++;
            }
        }

        //Component Select 1
    }

    if (!count)
    {
        // Translate numComponents into bitmask
        // 1 -> 1, 2 -> 3, 3 -> 7 and 4 -> 15
        uint32_t compMask = (1 << iNumComponents) - 1;

        compMask &= _ui32CompMask;
        // Calculate bits left in compMask
        return HLSLcc::GetNumberBitsSet(compMask);
    }

    return count;
}

// Returns 0 if the register used by the operand is per-vertex, or 1 if per-patch
int Operand::GetRegisterSpace(SHADER_TYPE eShaderType, SHADER_PHASE_TYPE eShaderPhaseType) const
{
    if (eShaderType != HULL_SHADER && eShaderType != DOMAIN_SHADER)
        return 0;

    if (eShaderType == HULL_SHADER && eShaderPhaseType == HS_CTRL_POINT_PHASE)
        return 0;

    if (eShaderType == DOMAIN_SHADER && eType == OPERAND_TYPE_OUTPUT)
        return 0;

    if (eType == OPERAND_TYPE_INPUT_CONTROL_POINT || eType == OPERAND_TYPE_OUTPUT_CONTROL_POINT)
        return 0;

    return 1;
}

int Operand::GetRegisterSpace(const HLSLCrossCompilerContext *psContext) const
{
    return GetRegisterSpace(psContext->psShader->eShaderType, psContext->psShader->asPhases[psContext->currentPhase].ePhase);
}

SHADER_VARIABLE_TYPE Operand::GetDataType(HLSLCrossCompilerContext* psContext, SHADER_VARIABLE_TYPE ePreferredTypeForImmediates /* = SVT_INT */) const
{
    // indexable temps (temp arrays) are always float
    if (eType == OPERAND_TYPE_INDEXABLE_TEMP)
        return SVT_FLOAT;

    // The min precision qualifier overrides all of the stuff below
    switch (eMinPrecision)
    {
        case OPERAND_MIN_PRECISION_FLOAT_16:
            return SVT_FLOAT16;
        case OPERAND_MIN_PRECISION_FLOAT_2_8:
            return SVT_FLOAT10;
        case OPERAND_MIN_PRECISION_SINT_16:
            return SVT_INT16;
        case OPERAND_MIN_PRECISION_UINT_16:
            return SVT_UINT16;
        default:
            break;
    }

    switch (eType)
    {
        case OPERAND_TYPE_TEMP:
        {
            SHADER_VARIABLE_TYPE eCurrentType = SVT_FLOAT;
            int i = 0;

            if (eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE)
            {
                return aeDataType[aui32Swizzle[0]];
            }
            if (eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
            {
                if (ui32Swizzle == (NO_SWIZZLE))
                {
                    return aeDataType[0];
                }

                return aeDataType[aui32Swizzle[0]];
            }

            if (eSelMode == OPERAND_4_COMPONENT_MASK_MODE)
            {
                uint32_t mask = ui32CompMask;
                if (!mask)
                {
                    mask = OPERAND_4_COMPONENT_MASK_ALL;
                }
                for (; i < 4; ++i)
                {
                    if (mask & (1 << i))
                    {
                        eCurrentType = aeDataType[i];
                        break;
                    }
                }

#ifdef _DEBUG
                //Check if all elements have the same basic type.
                for (; i < 4; ++i)
                {
                    if (mask & (1 << i))
                    {
                        if (eCurrentType != aeDataType[i])
                        {
                            ASSERT(0);
                        }
                    }
                }
#endif
                return eCurrentType;
            }

            ASSERT(0);

            break;
        }
        case OPERAND_TYPE_OUTPUT:
        {
            const uint32_t ui32Register = ui32RegisterNumber;
            int regSpace = GetRegisterSpace(psContext);
            const ShaderInfo::InOutSignature* psOut = NULL;

            if (regSpace == 0)
                psContext->psShader->sInfo.GetOutputSignatureFromRegister(ui32Register, GetAccessMask(), psContext->psShader->ui32CurrentVertexOutputStream,
                    &psOut);
            else
            {
                psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Register, GetAccessMask(), &psOut, true);
                if (!psOut)
                    return SVT_FLOAT;
            }

            ASSERT(psOut != NULL);
            if (psOut->eMinPrec != MIN_PRECISION_DEFAULT)
            {
                switch (psOut->eMinPrec)
                {
                    default:
                        ASSERT(0);
                        break;
                    case MIN_PRECISION_FLOAT_16:
                        return SVT_FLOAT16;
                    case MIN_PRECISION_FLOAT_2_8:
                        if (psContext->psShader->eTargetLanguage == LANG_METAL)
                            return SVT_FLOAT16;
                        else
                            return SVT_FLOAT10;
                    case MIN_PRECISION_SINT_16:
                        return SVT_INT16;
                    case MIN_PRECISION_UINT_16:
                        return SVT_UINT16;
                }
            }
            if (psOut->eComponentType == INOUT_COMPONENT_UINT32)
            {
                return SVT_UINT;
            }
            else if (psOut->eComponentType == INOUT_COMPONENT_SINT32)
            {
                return SVT_INT;
            }
            return SVT_FLOAT;
            break;
        }
        case OPERAND_TYPE_INPUT:
        case OPERAND_TYPE_INPUT_PATCH_CONSTANT:
        case OPERAND_TYPE_INPUT_CONTROL_POINT:
        {
            const uint32_t ui32Register = aui32ArraySizes[iIndexDims - 1];
            int regSpace = GetRegisterSpace(psContext);
            const ShaderInfo::InOutSignature* psIn = NULL;

            if (regSpace == 0)
            {
                if (psContext->psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[ui32Register] != 0)
                    return SVT_FLOAT; // All combined inputs are stored as floats
                psContext->psShader->sInfo.GetInputSignatureFromRegister(ui32Register, GetAccessMask(),
                    &psIn);
            }
            else
            {
                if (psContext->psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[ui32Register] != 0)
                    return SVT_FLOAT; // All combined inputs are stored as floats
                psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Register, GetAccessMask(), &psIn);
            }

            ASSERT(psIn != NULL);

            switch (eSpecialName)
            {
                //UINT in DX, INT in GL.
                case NAME_PRIMITIVE_ID:
                case NAME_VERTEX_ID:
                case NAME_INSTANCE_ID:
                case NAME_RENDER_TARGET_ARRAY_INDEX:
                case NAME_VIEWPORT_ARRAY_INDEX:
                case NAME_SAMPLE_INDEX:
                    return (psContext->psShader->eTargetLanguage == LANG_METAL) ? SVT_UINT : SVT_INT;

                case NAME_IS_FRONT_FACE:
                    return SVT_UINT;

                case NAME_POSITION:
                case NAME_CLIP_DISTANCE:
                case NAME_CULL_DISTANCE:
                    return SVT_FLOAT;

                default:
                    break;
                    // fall through
            }

            if (psIn->eSystemValueType == NAME_IS_FRONT_FACE)
                return SVT_UINT;

            //UINT in DX, INT in GL.
            if (psIn->eSystemValueType == NAME_PRIMITIVE_ID ||
                psIn->eSystemValueType == NAME_VERTEX_ID ||
                psIn->eSystemValueType == NAME_INSTANCE_ID ||
                psIn->eSystemValueType == NAME_RENDER_TARGET_ARRAY_INDEX ||
                psIn->eSystemValueType == NAME_VIEWPORT_ARRAY_INDEX ||
                psIn->eSystemValueType == NAME_SAMPLE_INDEX)
                return (psContext->psShader->eTargetLanguage == LANG_METAL) ? SVT_UINT : SVT_INT;

            if (psIn->eMinPrec != MIN_PRECISION_DEFAULT)
            {
                switch (psIn->eMinPrec)
                {
                    default:
                        ASSERT(0);
                        break;
                    case MIN_PRECISION_FLOAT_16:
                        return SVT_FLOAT16;
                    case MIN_PRECISION_FLOAT_2_8:
                        if (psContext->psShader->eTargetLanguage == LANG_METAL)
                            return SVT_FLOAT16;
                        else
                            return SVT_FLOAT10;
                    case MIN_PRECISION_SINT_16:
                        return SVT_INT16;
                    case MIN_PRECISION_UINT_16:
                        return SVT_UINT16;
                }
            }

            if (psIn->eComponentType == INOUT_COMPONENT_UINT32)
            {
                return SVT_UINT;
            }
            else if (psIn->eComponentType == INOUT_COMPONENT_SINT32)
            {
                return SVT_INT;
            }
            return SVT_FLOAT;
            break;
        }
        case OPERAND_TYPE_CONSTANT_BUFFER:
        {
            const ConstantBuffer* psCBuf = NULL;
            const ShaderVarType* psVarType = NULL;
            int32_t rebase = -1;
            bool isArray;
            psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, aui32ArraySizes[0], &psCBuf);
            if (psCBuf)
            {
                int foundVar = ShaderInfo::GetShaderVarFromOffset(aui32ArraySizes[1], aui32Swizzle, psCBuf, &psVarType, &isArray, NULL, &rebase, psContext->flags);
                if (foundVar)
                    return psVarType->Type;

                ASSERT(0);
            }
            else
                ASSERT(0);
            break;
        }
        case OPERAND_TYPE_IMMEDIATE32:
        {
            return ePreferredTypeForImmediates;
        }

        case OPERAND_TYPE_IMMEDIATE64:
        {
            return SVT_DOUBLE;
        }

        case OPERAND_TYPE_INPUT_THREAD_ID:
        case OPERAND_TYPE_INPUT_THREAD_GROUP_ID:
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP:
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED:
        {
            return SVT_UINT;
        }
        case OPERAND_TYPE_SPECIAL_ADDRESS:
        case OPERAND_TYPE_SPECIAL_LOOPCOUNTER:
        case OPERAND_TYPE_INPUT_FORK_INSTANCE_ID:
        case OPERAND_TYPE_INPUT_PRIMITIVEID:
        {
            return SVT_INT;
        }
        case OPERAND_TYPE_INPUT_GS_INSTANCE_ID:
        {
            return SVT_UINT;
        }
        case OPERAND_TYPE_OUTPUT_COVERAGE_MASK:
        {
            return SVT_INT;
        }
        case OPERAND_TYPE_OUTPUT_CONTROL_POINT_ID:
        {
            return SVT_INT;
        }
        case OPERAND_TYPE_IMMEDIATE_CONSTANT_BUFFER: // constant array is floats everywhere except on vulkan
        {
            return psContext->IsVulkan() ? SVT_UINT : SVT_FLOAT;
        }

        default:
        {
            return SVT_FLOAT;
        }
    }

    return SVT_FLOAT;
}

OPERAND_MIN_PRECISION Operand::ResourcePrecisionToOperandPrecision(REFLECT_RESOURCE_PRECISION ePrec)
{
    switch (ePrec)
    {
        default:
        case REFLECT_RESOURCE_PRECISION_UNKNOWN:
        case REFLECT_RESOURCE_PRECISION_LOWP:
            return OPERAND_MIN_PRECISION_FLOAT_2_8;
        case REFLECT_RESOURCE_PRECISION_MEDIUMP:
            return OPERAND_MIN_PRECISION_FLOAT_16;
        case REFLECT_RESOURCE_PRECISION_HIGHP:
            return OPERAND_MIN_PRECISION_DEFAULT;
    }
}

int Operand::GetNumInputElements(const HLSLCrossCompilerContext *psContext) const
{
    const ShaderInfo::InOutSignature *psSig = NULL;
    int regSpace = GetRegisterSpace(psContext);

    switch (eType)
    {
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED:
        case OPERAND_TYPE_INPUT_FORK_INSTANCE_ID:
        case OPERAND_TYPE_OUTPUT_CONTROL_POINT_ID:
            return 1;
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP:
        case OPERAND_TYPE_INPUT_THREAD_ID:
        case OPERAND_TYPE_INPUT_THREAD_GROUP_ID:
        case OPERAND_TYPE_INPUT_DOMAIN_POINT:
            return 3;
        default:
            break;
    }

    if (regSpace == 0)
    {
        if (psContext->psShader->sInfo.GetInputSignatureFromType(eType, &psSig) == false) {
            psContext->psShader->sInfo.GetInputSignatureFromRegister(ui32RegisterNumber, GetAccessMask(), &psSig);
        }
    }
    else
    {
        psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32RegisterNumber, GetAccessMask(), &psSig);
    }

    ASSERT(psSig != NULL);

    return HLSLcc::GetNumberBitsSet(psSig->ui32Mask);
}

Operand* Operand::GetDynamicIndexOperand(HLSLCrossCompilerContext *psContext, const ShaderVarType* psVar, bool isAoS, bool *needsIndexCalcRevert) const
{
    Operand *psDynIndexOp = m_SubOperands[0].get();
    if (psDynIndexOp == NULL)
        psDynIndexOp = m_SubOperands[1].get();

    *needsIndexCalcRevert = false;
    if (psDynIndexOp != NULL && isAoS)
    {
        // if dynamically indexing array of structs, try using the original index var before the float4 address calc
        bool indexVarFound = false;
        *needsIndexCalcRevert = true;
        Instruction *psDynIndexOrigin = psDynIndexOp->m_Defines[0].m_Inst;
        Operand *asOps = psDynIndexOrigin->asOperands;
        Operand *psOriginOp = NULL;

        // DXBC always addresses as float4, find the address calculation

        // Special case where struct is float4 size, no extra calc is done
        if (ShaderInfo::GetCBVarSize(psVar->Parent, true) <= 16) // matrixAsVectors arg does not matter here as with matrices the size will go over the limit anyway
        {
            indexVarFound = true;
            *needsIndexCalcRevert = false;
        }
        else if (psDynIndexOrigin->eOpcode == OPCODE_IMUL)
        {
            // check which one of the src operands is the original index
            if ((asOps[2].eType == OPERAND_TYPE_TEMP || asOps[2].eType == OPERAND_TYPE_INPUT || asOps[2].eType == OPERAND_TYPE_CONSTANT_BUFFER) && asOps[3].eType == OPERAND_TYPE_IMMEDIATE32)
                psOriginOp = &asOps[2];
            else if ((asOps[3].eType == OPERAND_TYPE_TEMP || asOps[3].eType == OPERAND_TYPE_INPUT || asOps[3].eType == OPERAND_TYPE_CONSTANT_BUFFER) &&  asOps[2].eType == OPERAND_TYPE_IMMEDIATE32)
                psOriginOp = &asOps[3];
        }
        else if (psDynIndexOrigin->eOpcode == OPCODE_ISHL)
        {
            if (asOps[2].eType == OPERAND_TYPE_IMMEDIATE32 && asOps[1].eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
                psOriginOp = &asOps[0];
            else if (asOps[2].eType == OPERAND_TYPE_IMMEDIATE32)
                psOriginOp = &asOps[1];
        }

        if (psOriginOp != NULL)
        {
            indexVarFound = true;

            // Check if the mul dest is not the same temp as the src. Also check that the temp
            // does not have multiple uses (which could override the value)
            // -> we can use src straight and no index revert calc is needed
            if ((psOriginOp->eType == OPERAND_TYPE_INPUT)
                || ((psOriginOp->ui32RegisterNumber != psDynIndexOp->ui32RegisterNumber || psOriginOp->GetDataType(psContext) != psDynIndexOp->GetDataType(psContext))
                    && (!psOriginOp->m_Defines.empty()) && psOriginOp->m_Defines[0].m_Inst->m_Uses.size() == 1))
            {
                psDynIndexOp = psOriginOp;
                *needsIndexCalcRevert = false;
            }
        }

        // Atm we support only this very basic case of dynamic indexing array of structs.
        // Return error if something else is encountered.
        if (!indexVarFound)
            psContext->m_Reflection.OnDiagnostics("Unsupported dynamic indexing scheme on constant buffer vars.", 0, true);
    }

    return psDynIndexOp;
}
