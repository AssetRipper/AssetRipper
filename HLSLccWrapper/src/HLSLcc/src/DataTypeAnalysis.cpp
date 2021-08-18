#include "internal_includes/debug.h"
#include "internal_includes/tokens.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/DataTypeAnalysis.h"
#include "internal_includes/Shader.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/Instruction.h"
#include <algorithm>


// Helper function to set the vector type of 1 or more components in a vector
// If the existing values (in vector we're writing to) are all SVT_VOID, just upgrade the value and we're done
// Otherwise, set all the components in the vector that currently are set to that same value OR are now being written to
// to the "highest" type value (ordering int->uint->float)
static void SetVectorType(std::vector<SHADER_VARIABLE_TYPE> &aeTempVecType, uint32_t regBaseIndex, uint32_t componentMask, SHADER_VARIABLE_TYPE eType, int *psMadeProgress)
{
    int i = 0;

    // Expand the mask to include all components that are used, also upgrade type
    for (i = 0; i < 4; i++)
    {
        if (aeTempVecType[regBaseIndex + i] != SVT_VOID)
        {
            componentMask |= (1 << i);
            eType = HLSLcc::SelectHigherType(eType, aeTempVecType[regBaseIndex + i]);
        }
    }

    // Now componentMask contains the components we actually need to update and eType may have been changed to something else.
    // Write the results
    for (i = 0; i < 4; i++)
    {
        if (componentMask & (1 << i))
        {
            if (aeTempVecType[regBaseIndex + i] != eType)
            {
                aeTempVecType[regBaseIndex + i] = eType;
                if (psMadeProgress)
                    *psMadeProgress = 1;
            }
        }
    }
}

static SHADER_VARIABLE_TYPE OperandPrecisionToShaderVariableType(OPERAND_MIN_PRECISION prec, SHADER_VARIABLE_TYPE eDefault)
{
    SHADER_VARIABLE_TYPE eType = eDefault;
    switch (prec)
    {
        case OPERAND_MIN_PRECISION_DEFAULT:
            break;
        case OPERAND_MIN_PRECISION_SINT_16:
            eType = SVT_INT16;
            break;
        case OPERAND_MIN_PRECISION_UINT_16:
            eType = SVT_UINT16;
            break;
        case OPERAND_MIN_PRECISION_FLOAT_2_8:
            eType = SVT_FLOAT10;
            break;
        case OPERAND_MIN_PRECISION_FLOAT_16:
            eType = SVT_FLOAT16;
            break;
        default:
            ASSERT(0); // Catch this to see what's going on.
            break;
    }
    return eType;
}

static void MarkOperandAs(Operand *psOperand, SHADER_VARIABLE_TYPE eType, std::vector<SHADER_VARIABLE_TYPE> &aeTempVecType)
{
    if (psOperand->eType == OPERAND_TYPE_TEMP)
    {
        const uint32_t ui32RegIndex = psOperand->ui32RegisterNumber * 4;
        uint32_t mask = psOperand->GetAccessMask();
        // Adjust type based on operand precision
        eType = OperandPrecisionToShaderVariableType(psOperand->eMinPrecision, eType);

        SetVectorType(aeTempVecType, ui32RegIndex, mask, eType, NULL);
    }
}

static void MarkAllOperandsAs(Instruction* psInst, SHADER_VARIABLE_TYPE eType, std::vector<SHADER_VARIABLE_TYPE> &aeTempVecType)
{
    uint32_t i = 0;
    for (i = 0; i < psInst->ui32NumOperands; i++)
    {
        MarkOperandAs(&psInst->asOperands[i], eType, aeTempVecType);
    }
}

// Mark scalars from CBs. TODO: Do we need to do the same for vec2/3's as well? There may be swizzles involved which make it vec4 or something else again.
static void SetCBOperandComponents(HLSLCrossCompilerContext *psContext, Operand *psOperand)
{
    const ConstantBuffer* psCBuf = NULL;
    const ShaderVarType* psVarType = NULL;
    int32_t rebase = 0;
    bool isArray;

    if (psOperand->eType != OPERAND_TYPE_CONSTANT_BUFFER)
        return;

    // Ignore selection modes that access more than one component
    switch (psOperand->eSelMode)
    {
        case OPERAND_4_COMPONENT_SELECT_1_MODE:
            break;
        case OPERAND_4_COMPONENT_SWIZZLE_MODE:
            if (!psOperand->IsSwizzleReplicated())
                return;
            break;
        case OPERAND_4_COMPONENT_MASK_MODE:
            return;
    }

    psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, psOperand->aui32ArraySizes[0], &psCBuf);
    ShaderInfo::GetShaderVarFromOffset(psOperand->aui32ArraySizes[1], psOperand->aui32Swizzle, psCBuf, &psVarType, &isArray, NULL, &rebase, psContext->flags);

    if (psVarType->Class == SVC_SCALAR)
        psOperand->iNumComponents = 1;
}

struct SetPartialDataTypes
{
    SetPartialDataTypes(SHADER_VARIABLE_TYPE *_aeTempVec)
        : m_TempVec(_aeTempVec)
    {}
    SHADER_VARIABLE_TYPE *m_TempVec;

    template<typename ItrType> void operator()(ItrType inst, Operand *psOperand, uint32_t ui32OperandType) const
    {
        uint32_t mask = 0;
        SHADER_VARIABLE_TYPE *aeTempVecType = m_TempVec;
        SHADER_VARIABLE_TYPE newType;
        uint32_t i, reg;
        if (psOperand->eType != OPERAND_TYPE_TEMP)
            return;

        if (ui32OperandType == FEO_FLAG_SUBOPERAND)
        {
            // We really shouldn't ever be getting minprecision float indices here
            ASSERT(psOperand->eMinPrecision != OPERAND_MIN_PRECISION_FLOAT_16 && psOperand->eMinPrecision != OPERAND_MIN_PRECISION_FLOAT_2_8);

            mask = psOperand->GetAccessMask();
            reg = psOperand->ui32RegisterNumber;
            newType = OperandPrecisionToShaderVariableType(psOperand->eMinPrecision, SVT_INT_AMBIGUOUS);
            for (i = 0; i < 4; i++)
            {
                if (!(mask & (1 << i)))
                    continue;
                if (aeTempVecType[reg * 4 + i] == SVT_VOID)
                    aeTempVecType[reg * 4 + i] = newType;
            }
            return;
        }

        if (psOperand->eMinPrecision == OPERAND_MIN_PRECISION_DEFAULT)
            return;

        mask = psOperand->GetAccessMask();
        reg = psOperand->ui32RegisterNumber;
        newType = OperandPrecisionToShaderVariableType(psOperand->eMinPrecision, SVT_VOID);
        ASSERT(newType != SVT_VOID);
        for (i = 0; i < 4; i++)
        {
            if (!(mask & (1 << i)))
                continue;
            aeTempVecType[reg * 4 + i] = newType;
        }
    }
};

// Write back the temp datatypes into operands. Also mark scalars in constant buffers

struct WritebackDataTypes
{
    WritebackDataTypes(HLSLCrossCompilerContext *_ctx, SHADER_VARIABLE_TYPE *_aeTempVec)
        : m_Context(_ctx)
        , m_TempVec(_aeTempVec)
    {}
    HLSLCrossCompilerContext *m_Context;
    SHADER_VARIABLE_TYPE *m_TempVec;

    template<typename ItrType> void operator()(ItrType inst, Operand *psOperand, uint32_t ui32OperandType) const
    {
        SHADER_VARIABLE_TYPE *aeTempVecType = m_TempVec;
        uint32_t reg, mask, i;
        SHADER_VARIABLE_TYPE dtype;

        if (psOperand->eType == OPERAND_TYPE_CONSTANT_BUFFER)
            SetCBOperandComponents(m_Context, psOperand);

        if (psOperand->eType != OPERAND_TYPE_TEMP)
            return;

        reg = psOperand->ui32RegisterNumber;
        mask = psOperand->GetAccessMask();
        dtype = SVT_VOID;

        for (i = 0; i < 4; i++)
        {
            if (!(mask & (1 << i)))
                continue;

            // Check that all components have the same type
            ASSERT(dtype == SVT_VOID || dtype == aeTempVecType[reg * 4 + i]);

            dtype = aeTempVecType[reg * 4 + i];

            ASSERT(dtype != SVT_VOID);
            ASSERT(dtype == OperandPrecisionToShaderVariableType(psOperand->eMinPrecision, dtype));

            psOperand->aeDataType[i] = dtype;
        }
    }
};


void HLSLcc::DataTypeAnalysis::SetDataTypes(HLSLCrossCompilerContext* psContext, std::vector<Instruction> & instructions, uint32_t ui32TempCount, std::vector<SHADER_VARIABLE_TYPE> &results)
{
    uint32_t i;
    Instruction *psFirstInst = &instructions[0];
    Instruction *psInst = psFirstInst;
    // Start with void, then move up the chain void->ambiguous int->minprec int/uint->int/uint->minprec float->float
    std::vector<SHADER_VARIABLE_TYPE> &aeTempVecType = results;

    aeTempVecType.clear();
    aeTempVecType.resize(ui32TempCount * 4, SVT_VOID);

    if (ui32TempCount == 0)
        return;

    // Go through the instructions, pick up partial datatypes, because we at least know those for a fact.
    // Also set all suboperands to be integers (they're always used as indices)
    ForEachOperand(instructions.begin(), instructions.end(), FEO_FLAG_ALL, SetPartialDataTypes(&aeTempVecType[0]));

    //  if (psContext->psShader->ui32MajorVersion <= 3)
    {
        // First pass, do analysis: deduce the data type based on opcodes, fill out aeTempVecType table
        // Only ever to int->float promotion (or int->uint), never the other way around
        for (i = 0; i < (uint32_t)instructions.size(); ++i, psInst++)
        {
            if (psInst->ui32NumOperands == 0)
                continue;
#ifdef _DEBUG
            for (int k = 0; k < (int)psInst->ui32NumOperands; k++)
            {
                if (psInst->asOperands[k].eType == OPERAND_TYPE_TEMP)
                {
                    ASSERT(psInst->asOperands[k].ui32RegisterNumber < ui32TempCount);
                }
            }
#endif

            switch (psInst->eOpcode)
            {
                // All float-only ops
                case OPCODE_ADD:
                case OPCODE_DERIV_RTX:
                case OPCODE_DERIV_RTY:
                case OPCODE_DIV:
                case OPCODE_DP2:
                case OPCODE_DP3:
                case OPCODE_DP4:
                case OPCODE_EXP:
                case OPCODE_FRC:
                case OPCODE_LOG:
                case OPCODE_MAD:
                case OPCODE_MIN:
                case OPCODE_MAX:
                case OPCODE_MUL:
                case OPCODE_ROUND_NE:
                case OPCODE_ROUND_NI:
                case OPCODE_ROUND_PI:
                case OPCODE_ROUND_Z:
                case OPCODE_RSQ:
                case OPCODE_SAMPLE:
                case OPCODE_SAMPLE_C:
                case OPCODE_SAMPLE_C_LZ:
                case OPCODE_SAMPLE_L:
                case OPCODE_SAMPLE_D:
                case OPCODE_SAMPLE_B:
                case OPCODE_SQRT:
                case OPCODE_SINCOS:
                case OPCODE_LOD:
                case OPCODE_GATHER4:

                case OPCODE_DERIV_RTX_COARSE:
                case OPCODE_DERIV_RTX_FINE:
                case OPCODE_DERIV_RTY_COARSE:
                case OPCODE_DERIV_RTY_FINE:
                case OPCODE_GATHER4_C:
                case OPCODE_GATHER4_PO:
                case OPCODE_GATHER4_PO_C:
                case OPCODE_RCP:

                    MarkAllOperandsAs(psInst, SVT_FLOAT, aeTempVecType);
                    break;

                // Comparison ops, need to enable possibility for going boolean
                case OPCODE_IEQ:
                case OPCODE_INE:
                    MarkOperandAs(&psInst->asOperands[0], SVT_BOOL, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT_AMBIGUOUS, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT_AMBIGUOUS, aeTempVecType);
                    break;

                case OPCODE_IF:
                case OPCODE_BREAKC:
                case OPCODE_CALLC:
                case OPCODE_CONTINUEC:
                case OPCODE_RETC:
                    MarkOperandAs(&psInst->asOperands[0], SVT_BOOL, aeTempVecType);
                    break;

                case OPCODE_ILT:
                case OPCODE_IGE:
                    MarkOperandAs(&psInst->asOperands[0], SVT_BOOL, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT, aeTempVecType);
                    break;

                case OPCODE_ULT:
                case OPCODE_UGE:
                    MarkOperandAs(&psInst->asOperands[0], SVT_BOOL, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_UINT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_UINT, aeTempVecType);
                    break;

                case OPCODE_AND:
                case OPCODE_OR:
                    MarkOperandAs(&psInst->asOperands[0], SVT_INT_AMBIGUOUS, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_BOOL, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_BOOL, aeTempVecType);
                    break;

                // Integer ops that don't care of signedness
                case OPCODE_IADD:
                case OPCODE_INEG:
                case OPCODE_ISHL:
                case OPCODE_NOT:
                case OPCODE_XOR:
                case OPCODE_BUFINFO:
                case OPCODE_COUNTBITS:
                case OPCODE_FIRSTBIT_HI:
                case OPCODE_FIRSTBIT_LO:
                case OPCODE_FIRSTBIT_SHI:
                case OPCODE_BFI:
                case OPCODE_BFREV:
                case OPCODE_ATOMIC_AND:
                case OPCODE_ATOMIC_OR:
                case OPCODE_ATOMIC_XOR:
                case OPCODE_ATOMIC_CMP_STORE:
                case OPCODE_ATOMIC_IADD:
                case OPCODE_IMM_ATOMIC_IADD:
                case OPCODE_IMM_ATOMIC_AND:
                case OPCODE_IMM_ATOMIC_OR:
                case OPCODE_IMM_ATOMIC_XOR:
                case OPCODE_IMM_ATOMIC_EXCH:
                case OPCODE_IMM_ATOMIC_CMP_EXCH:


                    MarkAllOperandsAs(psInst, SVT_INT_AMBIGUOUS, aeTempVecType);
                    break;


                // Integer ops
                case OPCODE_IMAD:
                case OPCODE_IMAX:
                case OPCODE_IMIN:
                case OPCODE_IMUL:
                case OPCODE_ISHR:
                case OPCODE_IBFE:

                case OPCODE_ATOMIC_IMAX:
                case OPCODE_ATOMIC_IMIN:
                case OPCODE_IMM_ATOMIC_IMAX:
                case OPCODE_IMM_ATOMIC_IMIN:
                    MarkAllOperandsAs(psInst, SVT_INT, aeTempVecType);
                    break;


                // uint ops
                case OPCODE_UDIV:
                case OPCODE_UMUL:
                case OPCODE_UMAD:
                case OPCODE_UMAX:
                case OPCODE_UMIN:
                case OPCODE_USHR:
                case OPCODE_UADDC:
                case OPCODE_USUBB:
                case OPCODE_ATOMIC_UMAX:
                case OPCODE_ATOMIC_UMIN:
                case OPCODE_IMM_ATOMIC_UMAX:
                case OPCODE_IMM_ATOMIC_UMIN:
                case OPCODE_IMM_ATOMIC_ALLOC:
                case OPCODE_IMM_ATOMIC_CONSUME:
                    MarkAllOperandsAs(psInst, SVT_UINT, aeTempVecType);
                    break;
                case OPCODE_UBFE:
                    MarkOperandAs(&psInst->asOperands[0], SVT_UINT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[3], SVT_UINT, aeTempVecType);
                    break;

                // Need special handling
                case OPCODE_FTOI:
                case OPCODE_FTOU:
                    MarkOperandAs(&psInst->asOperands[0], psInst->eOpcode == OPCODE_FTOI ? SVT_INT : SVT_UINT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_FLOAT, aeTempVecType);
                    break;

                case OPCODE_GE:
                case OPCODE_LT:
                case OPCODE_EQ:
                case OPCODE_NE:

                    MarkOperandAs(&psInst->asOperands[0], SVT_BOOL, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_FLOAT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_FLOAT, aeTempVecType);
                    break;

                case OPCODE_ITOF:
                case OPCODE_UTOF:
                    MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], psInst->eOpcode == OPCODE_ITOF ? SVT_INT : SVT_UINT, aeTempVecType);
                    break;

                case OPCODE_LD:
                case OPCODE_LD_MS:
                {
                    SHADER_VARIABLE_TYPE samplerReturnType = psInst->asOperands[2].aeDataType[0];
                    MarkOperandAs(&psInst->asOperands[0], samplerReturnType, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_UINT, aeTempVecType);
                    break;
                }

                case OPCODE_MOVC:
                    MarkOperandAs(&psInst->asOperands[1], SVT_BOOL, aeTempVecType);
                    break;

                case OPCODE_SWAPC:
                    MarkOperandAs(&psInst->asOperands[2], SVT_BOOL, aeTempVecType);
                    break;

                case OPCODE_RESINFO:
                    // Operand 0 depends on the return type declaration, op 1 is always uint
                    MarkOperandAs(&psInst->asOperands[1], SVT_UINT, aeTempVecType);
                    switch (psInst->eResInfoReturnType)
                    {
                        default:
                        case RESINFO_INSTRUCTION_RETURN_FLOAT:
                        case RESINFO_INSTRUCTION_RETURN_RCPFLOAT:
                            MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                            break;
                        case RESINFO_INSTRUCTION_RETURN_UINT:
                            MarkOperandAs(&psInst->asOperands[0], SVT_UINT, aeTempVecType);
                            break;
                    }
                    break;

                case OPCODE_SAMPLE_INFO:
                    // Sample_info uses the same RESINFO_RETURN_TYPE for storage. 0 = float, 1 = uint.
                    MarkOperandAs(&psInst->asOperands[0], psInst->eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_FLOAT ? SVT_FLOAT : SVT_UINT, aeTempVecType);
                    break;

                case OPCODE_SAMPLE_POS:
                    MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                    break;


                case OPCODE_LD_UAV_TYPED:
                    // translates to gvec4 loadImage(gimage i, ivec p).
                    MarkOperandAs(&psInst->asOperands[0], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType); // ivec p
                    break;

                case OPCODE_STORE_UAV_TYPED:
                    // translates to storeImage(gimage i, ivec p, gvec4 data)
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType); // ivec p
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT, aeTempVecType); // gvec4 data
                    break;

                case OPCODE_LD_RAW:
                    if (psInst->asOperands[2].eType == OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
                        MarkOperandAs(&psInst->asOperands[0], SVT_UINT, aeTempVecType);
                    else
                        MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    break;

                case OPCODE_STORE_RAW:
                    if (psInst->asOperands[0].eType == OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
                        MarkOperandAs(&psInst->asOperands[0], SVT_UINT, aeTempVecType);
                    else
                        MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    break;

                case OPCODE_LD_STRUCTURED:
                    MarkOperandAs(&psInst->asOperands[0], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT, aeTempVecType);
                    break;

                case OPCODE_STORE_STRUCTURED:
                    MarkOperandAs(&psInst->asOperands[1], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[2], SVT_INT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[3], SVT_INT, aeTempVecType);
                    break;

                case OPCODE_F32TOF16:
                    MarkOperandAs(&psInst->asOperands[0], SVT_UINT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_FLOAT, aeTempVecType);
                    break;

                case OPCODE_F16TOF32:
                    MarkOperandAs(&psInst->asOperands[0], SVT_FLOAT, aeTempVecType);
                    MarkOperandAs(&psInst->asOperands[1], SVT_UINT, aeTempVecType);
                    break;


                // No-operands, should never get here anyway
                /*              case OPCODE_BREAK:
                case OPCODE_CALL:
                case OPCODE_CASE:
                case OPCODE_CONTINUE:
                case OPCODE_CUT:
                case OPCODE_DEFAULT:
                case OPCODE_DISCARD:
                case OPCODE_ELSE:
                case OPCODE_EMIT:
                case OPCODE_EMITTHENCUT:
                case OPCODE_ENDIF:
                case OPCODE_ENDLOOP:
                case OPCODE_ENDSWITCH:

                case OPCODE_LABEL:
                case OPCODE_LOOP:
                case OPCODE_CUSTOMDATA:
                case OPCODE_NOP:
                case OPCODE_RET:
                case OPCODE_SWITCH:
                case OPCODE_DCL_RESOURCE: // DCL* opcodes have
                case OPCODE_DCL_CONSTANT_BUFFER: // custom operand formats.
                case OPCODE_DCL_SAMPLER:
                case OPCODE_DCL_INDEX_RANGE:
                case OPCODE_DCL_GS_OUTPUT_PRIMITIVE_TOPOLOGY:
                case OPCODE_DCL_GS_INPUT_PRIMITIVE:
                case OPCODE_DCL_MAX_OUTPUT_VERTEX_COUNT:
                case OPCODE_DCL_INPUT:
                case OPCODE_DCL_INPUT_SGV:
                case OPCODE_DCL_INPUT_SIV:
                case OPCODE_DCL_INPUT_PS:
                case OPCODE_DCL_INPUT_PS_SGV:
                case OPCODE_DCL_INPUT_PS_SIV:
                case OPCODE_DCL_OUTPUT:
                case OPCODE_DCL_OUTPUT_SGV:
                case OPCODE_DCL_OUTPUT_SIV:
                case OPCODE_DCL_TEMPS:
                case OPCODE_DCL_INDEXABLE_TEMP:
                case OPCODE_DCL_GLOBAL_FLAGS:


                case OPCODE_HS_DECLS: // token marks beginning of HS sub-shader
                case OPCODE_HS_CONTROL_POINT_PHASE: // token marks beginning of HS sub-shader
                case OPCODE_HS_FORK_PHASE: // token marks beginning of HS sub-shader
                case OPCODE_HS_JOIN_PHASE: // token marks beginning of HS sub-shader

                case OPCODE_EMIT_STREAM:
                case OPCODE_CUT_STREAM:
                case OPCODE_EMITTHENCUT_STREAM:
                case OPCODE_INTERFACE_CALL:


                case OPCODE_DCL_STREAM:
                case OPCODE_DCL_FUNCTION_BODY:
                case OPCODE_DCL_FUNCTION_TABLE:
                case OPCODE_DCL_INTERFACE:

                case OPCODE_DCL_INPUT_CONTROL_POINT_COUNT:
                case OPCODE_DCL_OUTPUT_CONTROL_POINT_COUNT:
                case OPCODE_DCL_TESS_DOMAIN:
                case OPCODE_DCL_TESS_PARTITIONING:
                case OPCODE_DCL_TESS_OUTPUT_PRIMITIVE:
                case OPCODE_DCL_HS_MAX_TESSFACTOR:
                case OPCODE_DCL_HS_FORK_PHASE_INSTANCE_COUNT:
                case OPCODE_DCL_HS_JOIN_PHASE_INSTANCE_COUNT:

                case OPCODE_DCL_THREAD_GROUP:
                case OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED:
                case OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW:
                case OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED:
                case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_RAW:
                case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_STRUCTURED:
                case OPCODE_DCL_RESOURCE_RAW:
                case OPCODE_DCL_RESOURCE_STRUCTURED:
                case OPCODE_SYNC:

                case OPCODE_EVAL_SNAPPED:
                case OPCODE_EVAL_SAMPLE_INDEX:
                case OPCODE_EVAL_CENTROID:

                case OPCODE_DCL_GS_INSTANCE_COUNT:

                case OPCODE_ABORT:
                case OPCODE_DEBUG_BREAK:

                 // Double not supported
                 case OPCODE_DADD:
                 case OPCODE_DMAX:
                 case OPCODE_DMIN:
                 case OPCODE_DMUL:
                 case OPCODE_DEQ:
                 case OPCODE_DGE:
                 case OPCODE_DLT:
                 case OPCODE_DNE:
                 case OPCODE_DMOV:
                 case OPCODE_DMOVC:
                 case OPCODE_DTOF:
                 case OPCODE_FTOD:
                 */

                default:
                    break;
            }
        }
    }

    {
        int madeProgress = 0;
        // Next go through MOV and MOVC and propagate the data type of whichever parameter we happen to have
        do
        {
            madeProgress = 0;
            psInst = psFirstInst;
            for (i = 0; i < (uint32_t)instructions.size(); ++i, psInst++)
            {
                if (psInst->eOpcode == OPCODE_MOV || psInst->eOpcode == OPCODE_MOVC)
                {
                    // Figure out the data type
                    uint32_t k;
                    SHADER_VARIABLE_TYPE dataType = SVT_VOID;
                    int foundImmediate = 0;
                    for (k = 0; k < psInst->ui32NumOperands; k++)
                    {
                        uint32_t mask, j;
                        if (psInst->eOpcode == OPCODE_MOVC && k == 1)
                            continue; // Ignore the condition operand, it's always int

                        if (psInst->asOperands[k].eType == OPERAND_TYPE_IMMEDIATE32)
                        {
                            foundImmediate = 1;
                            continue; // We don't know the data type of immediates yet, but if this is the only one found, mark as int, it'll get promoted later if needed
                        }

                        if (psInst->asOperands[k].eType != OPERAND_TYPE_TEMP)
                        {
                            dataType = psInst->asOperands[k].GetDataType(psContext);
                            break;
                        }

                        if (psInst->asOperands[k].eModifier != OPERAND_MODIFIER_NONE)
                        {
                            // If any modifiers are used in MOV or MOVC, that automatically is treated as float.
                            dataType = SVT_FLOAT;
                            break;
                        }

                        mask = psInst->asOperands[k].GetAccessMask();
                        for (j = 0; j < 4; j++)
                        {
                            if (!(mask & (1 << j)))
                                continue;
                            if (aeTempVecType[psInst->asOperands[k].ui32RegisterNumber * 4 + j] != SVT_VOID)
                            {
                                dataType = HLSLcc::SelectHigherType(dataType, aeTempVecType[psInst->asOperands[k].ui32RegisterNumber * 4 + j]);
                            }
                        }
                    }

                    // Use at minimum int type when any operand is immediate.
                    // Allowing bool could lead into bugs like case 883080
                    if (foundImmediate && (dataType == SVT_VOID || dataType == SVT_BOOL))
                        dataType = SVT_INT;

                    if (dataType != SVT_VOID)
                    {
                        // Found data type, write to all operands
                        // First adjust it to not have precision qualifiers in it
                        switch (dataType)
                        {
                            case SVT_FLOAT10:
                            case SVT_FLOAT16:
                                dataType = SVT_FLOAT;
                                break;
                            case SVT_INT12:
                            case SVT_INT16:
                                dataType = SVT_INT;
                                break;
                            case SVT_UINT16:
                            case SVT_UINT8:
                                dataType = SVT_UINT;
                                break;
                            default:
                                break;
                        }
                        for (k = 0; k < psInst->ui32NumOperands; k++)
                        {
                            uint32_t mask;
                            if (psInst->eOpcode == OPCODE_MOVC && k == 1)
                                continue; // Ignore the condition operand, it's always int

                            if (psInst->asOperands[k].eType != OPERAND_TYPE_TEMP)
                                continue;
                            if (psInst->asOperands[k].eMinPrecision != OPERAND_MIN_PRECISION_DEFAULT)
                                continue;

                            mask = psInst->asOperands[k].GetAccessMask();
                            SetVectorType(aeTempVecType, psInst->asOperands[k].ui32RegisterNumber * 4, mask, dataType, &madeProgress);
                        }
                    }
                }
            }
        }
        while (madeProgress != 0);
    }


    // translate forced_int and int_ambiguous back to int
    for (i = 0; i < ui32TempCount * 4; i++)
    {
        if (aeTempVecType[i] == SVT_FORCED_INT || aeTempVecType[i] == SVT_INT_AMBIGUOUS)
            aeTempVecType[i] = SVT_INT;
    }

    ForEachOperand(instructions.begin(), instructions.end(), FEO_FLAG_ALL, WritebackDataTypes(psContext, &aeTempVecType[0]));

    // Propagate boolean data types over logical operators
    bool didProgress = false;
    do
    {
        didProgress = false;
        std::for_each(instructions.begin(), instructions.end(), [&didProgress, &psContext, &aeTempVecType](Instruction &i)
        {
            if ((i.eOpcode == OPCODE_AND || i.eOpcode == OPCODE_OR)
                && (i.asOperands[1].GetDataType(psContext) == SVT_BOOL && i.asOperands[2].GetDataType(psContext) == SVT_BOOL)
                && (i.asOperands[0].eType == OPERAND_TYPE_TEMP && i.asOperands[0].GetDataType(psContext) != SVT_BOOL))
            {
                // Check if all uses see only this define
                bool isStandalone = true;
                std::for_each(i.m_Uses.begin(), i.m_Uses.end(), [&isStandalone](Instruction::Use &u)
                {
                    if (u.m_Op->m_Defines.size() > 1)
                        isStandalone = false;
                });

                if (isStandalone)
                {
                    didProgress = true;
                    // Change data type of this and all uses
                    i.asOperands[0].aeDataType[0] = i.asOperands[0].aeDataType[1] = i.asOperands[0].aeDataType[2] = i.asOperands[0].aeDataType[3] = SVT_BOOL;
                    uint32_t reg = i.asOperands[0].ui32RegisterNumber;
                    aeTempVecType[reg * 4 + 0] = aeTempVecType[reg * 4 + 1] = aeTempVecType[reg * 4 + 2] = aeTempVecType[reg * 4 + 3] = SVT_BOOL;

                    std::for_each(i.m_Uses.begin(), i.m_Uses.end(), [](Instruction::Use &u)
                    {
                        u.m_Op->aeDataType[0] = u.m_Op->aeDataType[1] = u.m_Op->aeDataType[2] = u.m_Op->aeDataType[3] = SVT_BOOL;
                    });
                }
            }
        });
    }
    while (didProgress);
}
