#include <stdio.h>
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "hlslcc.h"
#include "internal_includes/debug.h"
#include "internal_includes/Shader.h"
#include "internal_includes/toMetal.h"
#include <cmath>
#include <sstream>

#include <float.h>
#include <stdlib.h>

using namespace HLSLcc;

#ifdef _MSC_VER
#if _MSC_VER < 1900
#define snprintf _snprintf
#endif
#endif

#ifndef fpcheck
#ifdef _MSC_VER
#define fpcheck(x) (_isnan(x) || !_finite(x))
#else
#define fpcheck(x) (std::isnan(x) || std::isinf(x))
#endif
#endif // #ifndef fpcheck


// Returns nonzero if types are just different precisions of the same underlying type
static bool AreTypesCompatibleMetal(SHADER_VARIABLE_TYPE a, uint32_t ui32TOFlag)
{
    SHADER_VARIABLE_TYPE b = TypeFlagsToSVTType(ui32TOFlag);

    if (a == b)
        return true;

    // Special case for array indices: both uint and int are fine
    if ((ui32TOFlag & TO_FLAG_INTEGER) && (ui32TOFlag & TO_FLAG_UNSIGNED_INTEGER) &&
        (a == SVT_INT || a == SVT_INT16 || a == SVT_UINT || a == SVT_UINT16))
        return true;

    return false;
}

std::string ToMetal::TranslateOperandSwizzle(const Operand* psOperand, uint32_t ui32ComponentMask, int iRebase, bool includeDot /*= true*/)
{
    std::ostringstream oss;
    uint32_t accessMask = ui32ComponentMask & psOperand->GetAccessMask();
    if (psOperand->eType == OPERAND_TYPE_INPUT)
    {
        int regSpace = psOperand->GetRegisterSpace(psContext);
        // Skip swizzle for scalar inputs, but only if we haven't redirected them
        if (regSpace == 0)
        {
            if ((psContext->psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] == 0) &&
                (psContext->psShader->abScalarInput[regSpace][psOperand->ui32RegisterNumber] & accessMask))
            {
                return "";
            }
        }
        else
        {
            if ((psContext->psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0) &&
                (psContext->psShader->abScalarInput[regSpace][psOperand->ui32RegisterNumber] & accessMask))
            {
                return "";
            }
        }
    }
    if (psOperand->eType == OPERAND_TYPE_OUTPUT)
    {
        int regSpace = psOperand->GetRegisterSpace(psContext);
        // Skip swizzle for scalar outputs, but only if we haven't redirected them
        if (regSpace == 0)
        {
            if ((psContext->psShader->asPhases[psContext->currentPhase].acOutputNeedsRedirect[psOperand->ui32RegisterNumber] == 0) &&
                (psContext->psShader->abScalarOutput[regSpace][psOperand->ui32RegisterNumber] & accessMask))
            {
                return "";
            }
        }
        else
        {
            if ((psContext->psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0) &&
                (psContext->psShader->abScalarOutput[regSpace][psOperand->ui32RegisterNumber] & accessMask))
            {
                return "";
            }
        }
    }

    if (psOperand->iWriteMaskEnabled &&
        psOperand->iNumComponents != 1)
    {
        //Component Mask
        if (psOperand->eSelMode == OPERAND_4_COMPONENT_MASK_MODE)
        {
            uint32_t mask;
            if (psOperand->ui32CompMask != 0)
                mask = psOperand->ui32CompMask & ui32ComponentMask;
            else
                mask = ui32ComponentMask;

            if (mask != 0 && mask != OPERAND_4_COMPONENT_MASK_ALL)
            {
                if (includeDot)
                    oss << ".";
                if (mask & OPERAND_4_COMPONENT_MASK_X)
                {
                    ASSERT(iRebase == 0);
                    oss << "x";
                }
                if (mask & OPERAND_4_COMPONENT_MASK_Y)
                {
                    ASSERT(iRebase <= 1);
                    oss << "xy"[1 - iRebase];
                }
                if (mask & OPERAND_4_COMPONENT_MASK_Z)
                {
                    ASSERT(iRebase <= 2);
                    oss << "xyz"[2 - iRebase];
                }
                if (mask & OPERAND_4_COMPONENT_MASK_W)
                {
                    ASSERT(iRebase <= 3);
                    oss << "xyzw"[3 - iRebase];
                }
            }
        }
        else
        //Component Swizzle
        if (psOperand->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
        {
            if (ui32ComponentMask != OPERAND_4_COMPONENT_MASK_ALL ||
                !(psOperand->aui32Swizzle[0] == OPERAND_4_COMPONENT_X &&
                  psOperand->aui32Swizzle[1] == OPERAND_4_COMPONENT_Y &&
                  psOperand->aui32Swizzle[2] == OPERAND_4_COMPONENT_Z &&
                  psOperand->aui32Swizzle[3] == OPERAND_4_COMPONENT_W
                )
            )
            {
                uint32_t i;

                if (includeDot)
                    oss << ".";

                for (i = 0; i < 4; ++i)
                {
                    if (!(ui32ComponentMask & (OPERAND_4_COMPONENT_MASK_X << i)))
                        continue;

                    if (psOperand->aui32Swizzle[i] == OPERAND_4_COMPONENT_X)
                    {
                        ASSERT(iRebase == 0);
                        oss << "x";
                    }
                    else if (psOperand->aui32Swizzle[i] == OPERAND_4_COMPONENT_Y)
                    {
                        ASSERT(iRebase <= 1);
                        oss << "xy"[1 - iRebase];
                    }
                    else if (psOperand->aui32Swizzle[i] == OPERAND_4_COMPONENT_Z)
                    {
                        ASSERT(iRebase <= 2);
                        oss << "xyz"[2 - iRebase];
                    }
                    else if (psOperand->aui32Swizzle[i] == OPERAND_4_COMPONENT_W)
                    {
                        ASSERT(iRebase <= 3);
                        oss << "xyzw"[3 - iRebase];
                    }
                }
            }
        }
        else if (psOperand->eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE) // ui32ComponentMask is ignored in this case
        {
            if (includeDot)
                oss << ".";

            if (psOperand->aui32Swizzle[0] == OPERAND_4_COMPONENT_X)
            {
                ASSERT(iRebase == 0);
                oss << "x";
            }
            else if (psOperand->aui32Swizzle[0] == OPERAND_4_COMPONENT_Y)
            {
                ASSERT(iRebase <= 1);
                oss << "xy"[1 - iRebase];
            }
            else if (psOperand->aui32Swizzle[0] == OPERAND_4_COMPONENT_Z)
            {
                ASSERT(iRebase <= 2);
                oss << "xyz"[2 - iRebase];
            }
            else if (psOperand->aui32Swizzle[0] == OPERAND_4_COMPONENT_W)
            {
                ASSERT(iRebase <= 3);
                oss << "xyzw"[3 - iRebase];
            }
        }
    }
    return oss.str();
}

std::string ToMetal::TranslateOperandIndex(const Operand* psOperand, int index)
{
    int i = index;
    std::ostringstream oss;
    ASSERT(index < psOperand->iIndexDims);

    switch (psOperand->eIndexRep[i])
    {
        case OPERAND_INDEX_IMMEDIATE32:
        {
            oss << "[" << psOperand->aui32ArraySizes[i] << "]";
            return oss.str();
        }
        case OPERAND_INDEX_RELATIVE:
        {
            oss << "[" << TranslateOperand(psOperand->m_SubOperands[i].get(), TO_FLAG_INTEGER) << "]";
            return oss.str();
        }
        case OPERAND_INDEX_IMMEDIATE32_PLUS_RELATIVE:
        {
            oss << "[" << TranslateOperand(psOperand->m_SubOperands[i].get(), TO_FLAG_INTEGER) << " + " << psOperand->aui32ArraySizes[i] << "]";
            return oss.str();
        }
        default:
        {
            ASSERT(0);
            return "";
            break;
        }
    }
}

/*static std::string GetBitcastOp(HLSLCrossCompilerContext *psContext, SHADER_VARIABLE_TYPE from, SHADER_VARIABLE_TYPE to, uint32_t numComponents)
{
    if (psContext->psShader->eTargetLanguage == LANG_METAL)
    {
        std::ostringstream oss;
        oss << "as_type<";
        oss << GetConstructorForTypeMetal(to, numComponents);
        oss << ">";
        return oss.str();
    }
    else
    {
        if ((to == SVT_FLOAT || to == SVT_FLOAT16 || to == SVT_FLOAT10) && from == SVT_INT)
            return "intBitsToFloat";
        else if ((to == SVT_FLOAT || to == SVT_FLOAT16 || to == SVT_FLOAT10) && from == SVT_UINT)
            return "uintBitsToFloat";
        else if (to == SVT_INT && from == SVT_FLOAT)
            return "floatBitsToInt";
        else if (to == SVT_UINT && from == SVT_FLOAT)
            return "floatBitsToUint";
    }

    ASSERT(0);
    return "ERROR missing components in GetBitcastOp()";
}*/


// Helper function to print floats with full precision
static std::string printFloat(float f)
{
    char temp[30];

    snprintf(temp, 30, "%.9g", f);
    char * ePos = strchr(temp, 'e');
    char * pointPos = strchr(temp, '.');

    if (ePos == NULL && pointPos == NULL && !fpcheck(f))
        return std::string(temp) + ".0";
    else
        return std::string(temp);
}

// Helper function to print out a single 32-bit immediate value in desired format
static std::string printImmediate32(uint32_t value, SHADER_VARIABLE_TYPE eType)
{
    std::ostringstream oss;
    int needsParenthesis = 0;

    // Print floats as bit patterns.
    if ((eType == SVT_FLOAT || eType == SVT_FLOAT16 || eType == SVT_FLOAT10) && fpcheck(*((float *)(&value))))
    {
        oss << "as_type<float>(";
        eType = SVT_INT;
        needsParenthesis = 1;
    }

    switch (eType)
    {
        default:
            ASSERT(0);
        case SVT_INT:
        case SVT_INT16:
        case SVT_INT12:
            // Need special handling for anything >= uint 0x3fffffff
            if (value > 0x3ffffffe)
                oss << "int(0x" << std::hex << value << "u)";
            else
                oss << "0x" << std::hex << value << "";
            break;
        case SVT_UINT:
        case SVT_UINT16:
            oss << "0x" << std::hex << value << "u";
            break;
        case SVT_FLOAT:
        case SVT_FLOAT10:
        case SVT_FLOAT16:
            oss << printFloat(*((float *)(&value)));
            break;
        case SVT_BOOL:
            if (value == 0)
                oss << "false";
            else
                oss << "true";
    }
    if (needsParenthesis)
        oss << ")";

    return oss.str();
}

static std::string MakeCBVarName(const std::string &cbName, const std::string &fullName, bool isUnityInstancingBuffer)
{
    // For Unity instancing buffer: "CBufferName.StructTypeName[] -> CBufferName[]". See ToMetal::DeclareConstantBuffer.
    if (isUnityInstancingBuffer && !cbName.empty() && cbName[cbName.size() - 1] == '.' && fullName.find_first_of('[') != std::string::npos)
    {
        return cbName.substr(0, cbName.size() - 1) + fullName.substr(fullName.find_first_of('['));
    }
    return cbName + fullName;
}

std::string ToMetal::TranslateVariableName(const Operand* psOperand, uint32_t ui32TOFlag, uint32_t* pui32IgnoreSwizzle, uint32_t ui32CompMask, int *piRebase)
{
    std::ostringstream oss;
    int numParenthesis = 0;
    int hasCtor = 0;
    int needsBoolUpscale = 0; // If nonzero, bools need * 0xffffffff in them
    SHADER_VARIABLE_TYPE requestedType = TypeFlagsToSVTType(ui32TOFlag);
    SHADER_VARIABLE_TYPE eType = psOperand->GetDataType(psContext, requestedType);
    int numComponents = psOperand->GetNumSwizzleElements(ui32CompMask);
    int requestedComponents = 0;
    int scalarWithSwizzle = 0;

    *pui32IgnoreSwizzle = 0;

    if (psOperand->eType == OPERAND_TYPE_TEMP)
    {
        // Check for scalar
        if (psContext->psShader->GetTempComponentCount(eType, psOperand->ui32RegisterNumber) == 1 && psOperand->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE)
        {
            scalarWithSwizzle = 1; // Going to need a constructor
        }
    }

    if (psOperand->eType == OPERAND_TYPE_INPUT)
    {
        // Check for scalar
        // You would think checking would be easy but there is a caveat:
        //   checking abScalarInput might report as scalar, while in reality that was redirected and now is vector so swizzle must be preserved
        //   as an example consider we have input:
        //     float2 x; float y;
        //   and later on we do
        //     tex2D(xxx, fixed2(x.x, y));
        //   in that case we will generate redirect but which ui32RegisterNumber will be used for it is not strictly "specified"
        //   so we may end up with treating it as scalar (even though it is vector now)
        const int redirectInput = psContext->psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber];
        const bool wasRedirected = redirectInput == 0xFF || redirectInput == 0xFE;

        const int scalarInput = psContext->psShader->abScalarInput[psOperand->GetRegisterSpace(psContext)][psOperand->ui32RegisterNumber];
        if (!wasRedirected && (scalarInput & psOperand->GetAccessMask()) && (psOperand->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE))
        {
            scalarWithSwizzle = 1;
            *pui32IgnoreSwizzle = 1;
        }
    }

    if (piRebase)
        *piRebase = 0;

    if (ui32TOFlag & TO_AUTO_EXPAND_TO_VEC2)
        requestedComponents = 2;
    else if (ui32TOFlag & TO_AUTO_EXPAND_TO_VEC3)
        requestedComponents = 3;
    else if (ui32TOFlag & TO_AUTO_EXPAND_TO_VEC4)
        requestedComponents = 4;

    requestedComponents = std::max(requestedComponents, numComponents);

    if (!(ui32TOFlag & (TO_FLAG_DESTINATION | TO_FLAG_NAME_ONLY | TO_FLAG_DECLARATION_NAME)))
    {
        if (psOperand->eType == OPERAND_TYPE_IMMEDIATE32 || psOperand->eType == OPERAND_TYPE_IMMEDIATE64)
        {
            // Mark the operand type to match whatever we're asking for in the flags.
            ((Operand *)psOperand)->aeDataType[0] = requestedType;
            ((Operand *)psOperand)->aeDataType[1] = requestedType;
            ((Operand *)psOperand)->aeDataType[2] = requestedType;
            ((Operand *)psOperand)->aeDataType[3] = requestedType;
        }

        bool bitcast = false;
        if (AreTypesCompatibleMetal(eType, ui32TOFlag) == 0)
        {
            if (CanDoDirectCast(psContext, eType, requestedType))
            {
                hasCtor = 1;
                if (eType == SVT_BOOL)
                {
                    needsBoolUpscale = 1;
                    // make sure to wrap the whole thing in parens so the upscale
                    // multiply only applies to the bool
                    oss << "(";
                    numParenthesis++;
                }
                oss << GetConstructorForType(psContext, requestedType, requestedComponents, false) << "(";
                numParenthesis++;
            }
            else
            {
                // Direct cast not possible, need to do bitcast.
                oss << "as_type<" << GetConstructorForTypeMetal(requestedType, requestedComponents) << ">(";
                hasCtor = 1;
                bitcast = true;
                numParenthesis++;
            }
        }

        // Add ctor if needed (upscaling). Type conversion is already handled above, so here we must
        // use the original type to not make type conflicts in bitcasts
        bool needsUpscaling = ((numComponents < requestedComponents) || (scalarWithSwizzle != 0)) && (hasCtor == 0 || bitcast);

        // Add constuctor if half precision is forced to avoid template ambiguity error from compiler
        bool needsForcedCtor = (ui32TOFlag & TO_FLAG_FORCE_HALF) && (psOperand->eType == OPERAND_TYPE_IMMEDIATE32 || psOperand->eType == OPERAND_TYPE_IMMEDIATE64);

        if (needsForcedCtor)
            requestedComponents = std::max(requestedComponents, 1);

        if (needsUpscaling || needsForcedCtor)
        {
            oss << GetConstructorForType(psContext, eType, requestedComponents, false) << "(";

            numParenthesis++;
            hasCtor = 1;
        }
    }


    switch (psOperand->eType)
    {
        case OPERAND_TYPE_IMMEDIATE32:
        {
            if (psOperand->iNumComponents == 1)
            {
                oss << printImmediate32(*((unsigned int*)(&psOperand->afImmediates[0])), requestedType);
            }
            else
            {
                int i;
                int firstItemAdded = 0;
                if (hasCtor == 0)
                {
                    oss << GetConstructorForTypeMetal(requestedType, requestedComponents) << "(";
                    numParenthesis++;
                    hasCtor = 1;
                }
                for (i = 0; i < 4; i++)
                {
                    uint32_t uval;
                    if (!(ui32CompMask & (1 << i)))
                        continue;

                    if (firstItemAdded)
                        oss << ", ";
                    uval = *((uint32_t*)(&psOperand->afImmediates[i >= psOperand->iNumComponents ? psOperand->iNumComponents - 1 : i]));
                    oss << printImmediate32(uval, requestedType);
                    firstItemAdded = 1;
                }
                oss << ")";
                *pui32IgnoreSwizzle = 1;
                numParenthesis--;
            }
            break;
        }
        case OPERAND_TYPE_IMMEDIATE64:
        {
            ASSERT(0); // doubles not supported on Metal
            break;
        }
        case OPERAND_TYPE_INPUT:
        {
            int regSpace = psOperand->GetRegisterSpace(psContext);
            switch (psOperand->iIndexDims)
            {
                case INDEX_2D:
                {
                    const ShaderInfo::InOutSignature *psSig = NULL;
                    psContext->psShader->sInfo.GetInputSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->ui32CompMask, &psSig);
                    if (psContext->psShader->eShaderType == HULL_SHADER || psContext->psShader->eShaderType == DOMAIN_SHADER)
                    {
                        oss << "input.cp";
                        oss << TranslateOperandIndex(psOperand, 0);//Vertex index
                        oss << "." << psContext->GetDeclaredInputName(psOperand, piRebase, 1, pui32IgnoreSwizzle);
                    }
                    else
                    {
                        // Not sure if this codepath is active outside hull/domain
                        oss << psContext->GetDeclaredInputName(psOperand, piRebase, 0, pui32IgnoreSwizzle);

                        oss << TranslateOperandIndex(psOperand, 0);//Vertex index
                    }
                    break;
                }
                default:
                {
                    if (psOperand->eIndexRep[0] == OPERAND_INDEX_IMMEDIATE32_PLUS_RELATIVE)
                    {
                        ASSERT(psContext->psShader->aIndexedInput[regSpace][psOperand->ui32RegisterNumber] != 0);
                        oss << "phase" << psContext->currentPhase << "_Input" << regSpace << "_" << psOperand->ui32RegisterNumber << "[";
                        oss << TranslateOperand(psOperand->m_SubOperands[0].get(), TO_FLAG_INTEGER);
                        oss << "]";
                    }
                    else
                    {
                        if (psContext->psShader->aIndexedInput[regSpace][psOperand->ui32RegisterNumber] != 0)
                        {
                            const uint32_t parentIndex = psContext->psShader->aIndexedInputParents[regSpace][psOperand->ui32RegisterNumber];
                            oss << "phase" << psContext->currentPhase << "_Input" << regSpace << "_" << parentIndex << "[" << (psOperand->ui32RegisterNumber - parentIndex) << "]";
                        }
                        else
                        {
                            oss << psContext->GetDeclaredInputName(psOperand, piRebase, 0, pui32IgnoreSwizzle);
                        }
                    }
                    break;
                }
            }
            break;
        }
        case OPERAND_TYPE_OUTPUT:
        case OPERAND_TYPE_OUTPUT_DEPTH:
        case OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL:
        case OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL:
        {
            int stream = 0;
            oss << psContext->GetDeclaredOutputName(psOperand, &stream, pui32IgnoreSwizzle, piRebase, 0);
            if (psOperand->m_SubOperands[0].get())
            {
                oss << "[";
                oss << TranslateOperand(psOperand->m_SubOperands[0].get(), TO_AUTO_BITCAST_TO_INT);
                oss << "]";
            }
            break;
        }
        case OPERAND_TYPE_TEMP:
        {
            SHADER_VARIABLE_TYPE eTempType = psOperand->GetDataType(psContext);

            if (psOperand->eSpecialName == NAME_UNDEFINED && psOperand->specialName.length())
            {
                oss << psOperand->specialName;
                break;
            }

            oss << HLSLCC_TEMP_PREFIX;
            ASSERT(psOperand->ui32RegisterNumber < 0x10000); // Sanity check after temp splitting.
            switch (eTempType)
            {
                case SVT_FLOAT:
                    ASSERT(psContext->psShader->psFloatTempSizes[psOperand->ui32RegisterNumber] != 0);
                    if (psContext->psShader->psFloatTempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_FLOAT16:
                    ASSERT(psContext->psShader->psFloat16TempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("16_");
                    if (psContext->psShader->psFloat16TempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_FLOAT10:
                    ASSERT(psContext->psShader->psFloat10TempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("10_");
                    if (psContext->psShader->psFloat10TempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_INT:
                    ASSERT(psContext->psShader->psIntTempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("i");
                    if (psContext->psShader->psIntTempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_INT16:
                    ASSERT(psContext->psShader->psInt16TempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("i16_");
                    if (psContext->psShader->psInt16TempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_INT12:
                    ASSERT(psContext->psShader->psInt12TempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("i12_");
                    if (psContext->psShader->psInt12TempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_UINT:
                    ASSERT(psContext->psShader->psUIntTempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("u");
                    if (psContext->psShader->psUIntTempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_UINT16:
                    ASSERT(psContext->psShader->psUInt16TempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("u16_");
                    if (psContext->psShader->psUInt16TempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_DOUBLE:
                    ASSERT(psContext->psShader->psDoubleTempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("d");
                    if (psContext->psShader->psDoubleTempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                case SVT_BOOL:
                    ASSERT(psContext->psShader->psBoolTempSizes[psOperand->ui32RegisterNumber] != 0);
                    oss << ("b");
                    if (psContext->psShader->psBoolTempSizes[psOperand->ui32RegisterNumber] == 1)
                        *pui32IgnoreSwizzle = 1;
                    break;
                default:
                    ASSERT(0 && "Should never get here!");
            }
            oss << psOperand->ui32RegisterNumber;
            break;
        }
        case OPERAND_TYPE_SPECIAL_IMMCONSTINT:
        case OPERAND_TYPE_SPECIAL_IMMCONST:
        case OPERAND_TYPE_SPECIAL_OUTBASECOLOUR:
        case OPERAND_TYPE_SPECIAL_OUTOFFSETCOLOUR:
        case OPERAND_TYPE_SPECIAL_FOG:
        case OPERAND_TYPE_SPECIAL_ADDRESS:
        case OPERAND_TYPE_SPECIAL_LOOPCOUNTER:
        case OPERAND_TYPE_SPECIAL_TEXCOORD:
        {
            ASSERT(0 && "DX9 shaders no longer supported!");
            break;
        }
        case OPERAND_TYPE_SPECIAL_POSITION:
        {
            ASSERT(0 && "TODO normal shader support");
//          bcatcstr(glsl, "gl_Position");
            break;
        }
        case OPERAND_TYPE_SPECIAL_POINTSIZE:
        {
            ASSERT(0 && "TODO normal shader support");
            //          bcatcstr(glsl, "gl_PointSize");
            break;
        }
        case OPERAND_TYPE_CONSTANT_BUFFER:
        {
            const ConstantBuffer* psCBuf = NULL;
            const ShaderVarType* psVarType = NULL;
            int32_t index = -1;
            std::vector<uint32_t> arrayIndices;
            bool isArray = false;
            bool isFBInput = false;
            psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, psOperand->aui32ArraySizes[0], &psCBuf);
            ASSERT(psCBuf != NULL);

            if (ui32TOFlag & TO_FLAG_DECLARATION_NAME)
            {
                pui32IgnoreSwizzle[0] = 1;
            }
            std::string cbName = "";
            if (psCBuf)
            {
                //$Globals.
                cbName = GetCBName(psCBuf->name);
                cbName += ".";
                // Drop the constant buffer name from subpass inputs
                if (cbName.substr(0, 19) == "hlslcc_SubpassInput")
                    cbName = "";
            }

            if ((ui32TOFlag & TO_FLAG_DECLARATION_NAME) != TO_FLAG_DECLARATION_NAME)
            {
                //Work out the variable name. Don't apply swizzle to that variable yet.
                int32_t rebase = 0;

                ASSERT(psCBuf != NULL);

                uint32_t componentsNeeded = 1;
                if (psOperand->eSelMode != OPERAND_4_COMPONENT_SELECT_1_MODE)
                {
                    uint32_t minSwiz = 3;
                    uint32_t maxSwiz = 0;
                    int i;
                    for (i = 0; i < 4; i++)
                    {
                        if ((ui32CompMask & (1 << i)) == 0)
                            continue;
                        minSwiz = std::min(minSwiz, psOperand->aui32Swizzle[i]);
                        maxSwiz = std::max(maxSwiz, psOperand->aui32Swizzle[i]);
                    }
                    componentsNeeded = maxSwiz - minSwiz + 1;
                }

                // When we have a component mask that doesn't have .x set (this basically only happens when we manually open operands into components)
                // We have to pull down the swizzle array to match the first bit that's actually set
                uint32_t tmpSwizzle[4] = { 0 };
                int firstBitSet = 0;
                if (ui32CompMask == 0)
                    ui32CompMask = 0xf;
                while ((ui32CompMask & (1 << firstBitSet)) == 0)
                    firstBitSet++;
                std::copy(&psOperand->aui32Swizzle[firstBitSet], &psOperand->aui32Swizzle[4], &tmpSwizzle[0]);

                ShaderInfo::GetShaderVarFromOffset(psOperand->aui32ArraySizes[1], tmpSwizzle, psCBuf, &psVarType, &isArray, &arrayIndices, &rebase, psContext->flags);

                // Get a possible dynamic array index
                std::string dynamicIndexStr;
                bool needsIndexCalcRevert = false;
                bool isAoS = ((!isArray && arrayIndices.size() > 0) || (isArray && arrayIndices.size() > 1));
                bool isUnityInstancingBuffer = isAoS && IsUnityFlexibleInstancingBuffer(psCBuf);
                Operand *psDynIndexOp = psOperand->GetDynamicIndexOperand(psContext, psVarType, isAoS, &needsIndexCalcRevert);

                if (psDynIndexOp != NULL)
                {
                    SHADER_VARIABLE_TYPE eType = psDynIndexOp->GetDataType(psContext);
                    uint32_t opFlags = TO_FLAG_INTEGER;

                    if (eType != SVT_INT && eType != SVT_UINT)
                        opFlags = TO_AUTO_BITCAST_TO_INT;

                    dynamicIndexStr = TranslateOperand(psDynIndexOp, opFlags, 0x1); // Just take the first component for the index
                }

                if (psOperand->eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE || (componentsNeeded <= psVarType->Columns))
                {
                    // Simple case: just access one component
                    std::string fullName = ShaderInfo::GetShaderVarIndexedFullName(psVarType, arrayIndices, dynamicIndexStr, needsIndexCalcRevert, psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES);

                    // Special hack for MSAA subpass inputs: in Metal we can only read the "current" sample, so ignore the index
                    if (strncmp(fullName.c_str(), "hlslcc_fbinput", 14) == 0)
                        isFBInput = true;

                    if (((psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES) != 0) && ((psVarType->Class == SVC_MATRIX_ROWS) || (psVarType->Class == SVC_MATRIX_COLUMNS)))
                    {
                        // We'll need to add the prefix only to the last section of the name
                        size_t commaPos = fullName.find_last_of('.');
                        char prefix[256];
                        sprintf(prefix, HLSLCC_TRANSLATE_MATRIX_FORMAT_STRING, psVarType->Rows, psVarType->Columns);
                        if (commaPos == std::string::npos)
                            fullName.insert(0, prefix);
                        else
                            fullName.insert(commaPos + 1, prefix);
                    }

                    oss << MakeCBVarName(cbName, fullName, isUnityInstancingBuffer);
                }
                else
                {
                    // Non-simple case: build vec4 and apply mask
                    uint32_t i;
                    int32_t tmpRebase;
                    std::vector<uint32_t> tmpArrayIndices;
                    bool tmpIsArray;
                    int firstItemAdded = 0;

                    oss << GetConstructorForTypeMetal(psVarType->Type, GetNumberBitsSet(ui32CompMask)) << "(";
                    for (i = 0; i < 4; i++)
                    {
                        const ShaderVarType *tmpVarType = NULL;
                        if ((ui32CompMask & (1 << i)) == 0)
                            continue;
                        tmpRebase = 0;
                        if (firstItemAdded != 0)
                            oss << ", ";
                        else
                            firstItemAdded = 1;

                        uint32_t tmpSwizzle[4] = { 0 };
                        std::copy(&psOperand->aui32Swizzle[i], &psOperand->aui32Swizzle[4], &tmpSwizzle[0]);

                        ShaderInfo::GetShaderVarFromOffset(psOperand->aui32ArraySizes[1], tmpSwizzle, psCBuf, &tmpVarType, &tmpIsArray, &tmpArrayIndices, &tmpRebase, psContext->flags);
                        std::string fullName = ShaderInfo::GetShaderVarIndexedFullName(tmpVarType, tmpArrayIndices, dynamicIndexStr, needsIndexCalcRevert, psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES);
                        oss << MakeCBVarName(cbName, fullName, isUnityInstancingBuffer);

                        if (tmpVarType->Class != SVC_SCALAR)
                        {
                            uint32_t swizzle;
                            tmpRebase /= 4; // 0 => 0, 4 => 1, 8 => 2, 12 /= 3
                            swizzle = psOperand->aui32Swizzle[i] - tmpRebase;

                            oss << "." << ("xyzw"[swizzle]);
                        }
                    }
                    oss << ")";
                    // Clear rebase, we've already done it.
                    rebase = 0;
                    // Also swizzle.
                    *pui32IgnoreSwizzle = 1;
                }


                if (isArray)
                {
                    index = arrayIndices.back();

                    // Dynamic index is atm supported only at the root array level. Add here only if there is no such parent.
                    bool hasDynamicIndex = !dynamicIndexStr.empty() && (arrayIndices.size() <= 1);
                    bool hasImmediateIndex = (index != -1) && !(hasDynamicIndex && index == 0);

                    // Ignore index altogether on fb inputs
                    if (isFBInput)
                    {
                        // Nothing to do here
                    }
                    else if (hasDynamicIndex || hasImmediateIndex)
                    {
                        std::ostringstream fullIndexOss;
                        if (hasDynamicIndex && hasImmediateIndex)
                            fullIndexOss << "(" << dynamicIndexStr << " + " << index << ")";
                        else if (hasDynamicIndex)
                            fullIndexOss << dynamicIndexStr;
                        else // hasImmediateStr
                            fullIndexOss << index;

                        if (((psVarType->Class == SVC_MATRIX_COLUMNS) || (psVarType->Class == SVC_MATRIX_ROWS)) && (psVarType->Elements > 1) && ((psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES) == 0))
                        {
                            // Special handling for old matrix arrays
                            oss << "[" << fullIndexOss.str() << " / 4]";
                            oss << "[" << fullIndexOss.str() << " %% 4]";
                        }
                        else // This path is atm the default
                        {
                            oss << "[" << fullIndexOss.str() << "]";
                        }
                    }
                }

                if (psVarType->Class == SVC_VECTOR && !*pui32IgnoreSwizzle)
                {
                    switch (rebase)
                    {
                        case 4:
                        {
                            if (psVarType->Columns == 2)
                            {
                                //.x(GLSL) is .y(HLSL). .y(GLSL) is .z(HLSL)
                                oss << ".xxyx";
                            }
                            else if (psVarType->Columns == 3)
                            {
                                //.x(GLSL) is .y(HLSL). .y(GLSL) is .z(HLSL) .z(GLSL) is .w(HLSL)
                                oss << ".xxyz";
                            }
                            break;
                        }
                        case 8:
                        {
                            if (psVarType->Columns == 2)
                            {
                                //.x(GLSL) is .z(HLSL). .y(GLSL) is .w(HLSL)
                                oss << ".xxxy";
                            }
                            break;
                        }
                        case 0:
                        default:
                        {
                            //No rebase, but extend to vec4.
                            if (psVarType->Columns == 2)
                            {
                                oss << ".xyxx";
                            }
                            else if (psVarType->Columns == 3)
                            {
                                oss << ".xyzx";
                            }
                            break;
                        }
                    }
                }

                if (psVarType->Class == SVC_SCALAR)
                {
                    *pui32IgnoreSwizzle = 1;

                    // CB arrays are all declared as 4-component vectors to match DX11 data layout.
                    // Therefore add swizzle here to access the element corresponding to the scalar var.
                    if ((psVarType->Elements > 0) && (psContext->psShader->eShaderType == COMPUTE_SHADER))
                    {
                        oss << ".x";
                    }
                }
            }
            break;
        }
        case OPERAND_TYPE_RESOURCE:
        {
            oss << ResourceName(RGROUP_TEXTURE, psOperand->ui32RegisterNumber);
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_SAMPLER:
        {
            oss << ResourceName(RGROUP_SAMPLER, psOperand->ui32RegisterNumber);
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_FUNCTION_BODY:
        {
            ASSERT(0);
            break;
        }
        case OPERAND_TYPE_INPUT_FORK_INSTANCE_ID:
        case OPERAND_TYPE_INPUT_JOIN_INSTANCE_ID:
        {
            oss << "phaseInstanceID"; // Not a real builtin, but passed as a function parameter.
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_IMMEDIATE_CONSTANT_BUFFER:
        {
            oss << "ImmCB_" << psContext->currentPhase;
            oss << TranslateOperandIndex(psOperand, 0);
            break;
        }
        case OPERAND_TYPE_INPUT_DOMAIN_POINT:
        {
            oss << "mtl_TessCoord";
            break;
        }
        case OPERAND_TYPE_INPUT_CONTROL_POINT:
        {
            int ignoreRedirect = 1;
            int regSpace = psOperand->GetRegisterSpace(psContext);

            if ((regSpace == 0 && psContext->psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe) ||
                (regSpace == 1 && psContext->psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe))
            {
                ignoreRedirect = 0;
            }

            if (ignoreRedirect)
            {
                oss << "input.cp";
                oss << TranslateOperandIndex(psOperand, 0);//Vertex index
                oss << "." << psContext->GetDeclaredInputName(psOperand, piRebase, ignoreRedirect, pui32IgnoreSwizzle);
            }
            else
            {
                oss << psContext->GetDeclaredInputName(psOperand, piRebase, ignoreRedirect, pui32IgnoreSwizzle);
                oss << TranslateOperandIndex(psOperand, 0);//Vertex index
            }

            // Check for scalar
            if ((psContext->psShader->abScalarInput[psOperand->GetRegisterSpace(psContext)][psOperand->ui32RegisterNumber] & psOperand->GetAccessMask()) != 0)
                *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_NULL:
        {
            // Null register, used to discard results of operations
            oss << "//null";
            break;
        }
        case OPERAND_TYPE_OUTPUT_CONTROL_POINT_ID:
        {
            oss << "controlPointID";
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_OUTPUT_COVERAGE_MASK:
        {
            oss << "mtl_CoverageMask";
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_INPUT_COVERAGE_MASK:
        {
            oss << "mtl_CoverageMask";
            //Skip swizzle on scalar types.
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_INPUT_THREAD_ID://SV_DispatchThreadID
        {
            oss << "mtl_ThreadID";
            break;
        }
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP://SV_GroupThreadID
        {
            oss << "mtl_ThreadIDInGroup";
            break;
        }
        case OPERAND_TYPE_INPUT_THREAD_GROUP_ID://SV_GroupID
        {
            oss << "mtl_ThreadGroupID";
            break;
        }
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED://SV_GroupIndex
        {
            if (requestedComponents > 1 && !hasCtor)
            {
                oss << GetConstructorForType(psContext, eType, requestedComponents, false) << "(";
                numParenthesis++;
                hasCtor = 1;
            }
            for (uint32_t i = 0; i < requestedComponents; i++)
            {
                oss << "mtl_ThreadIndexInThreadGroup";
                if (i < requestedComponents - 1)
                    oss << ", ";
            }
            *pui32IgnoreSwizzle = 1; // No swizzle meaningful for scalar.
            break;
        }
        case OPERAND_TYPE_UNORDERED_ACCESS_VIEW:
        {
            oss << ResourceName(RGROUP_UAV, psOperand->ui32RegisterNumber);
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY:
        {
            oss << "TGSM" << psOperand->ui32RegisterNumber;
            *pui32IgnoreSwizzle = 1;
            break;
        }
        case OPERAND_TYPE_INPUT_PRIMITIVEID:
        {
            // Not supported on Metal
            ASSERT(0);
            break;
        }
        case OPERAND_TYPE_INDEXABLE_TEMP:
        {
            oss << "TempArray" << psOperand->aui32ArraySizes[0] << "[";
            if (psOperand->aui32ArraySizes[1] != 0 || !psOperand->m_SubOperands[1].get())
                oss << psOperand->aui32ArraySizes[1];

            if (psOperand->m_SubOperands[1].get())
            {
                if (psOperand->aui32ArraySizes[1] != 0)
                    oss << "+";
                oss << TranslateOperand(psOperand->m_SubOperands[1].get(), TO_FLAG_INTEGER);
            }
            oss << "]";
            break;
        }
        case OPERAND_TYPE_STREAM:
        {
            // Not supported on Metal
            ASSERT(0);
            break;
        }
        case OPERAND_TYPE_INPUT_GS_INSTANCE_ID:
        {
            // Not supported on Metal
            ASSERT(0);
            break;
        }
        case OPERAND_TYPE_THIS_POINTER:
        {
            ASSERT(0); // Nope.
            break;
        }
        case OPERAND_TYPE_INPUT_PATCH_CONSTANT:
        {
            const ShaderInfo::InOutSignature* psIn;
            psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), &psIn);
            *piRebase = psIn->iRebase;
            switch (psIn->eSystemValueType)
            {
                case NAME_POSITION:
                    oss << "mtl_Position";
                    break;
                case NAME_RENDER_TARGET_ARRAY_INDEX:
                    oss << "mtl_Layer";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_CLIP_DISTANCE:
                    // this is temp variable, declaration and redirecting to actual output is handled in DeclareClipPlanes
                    char tmpName[128]; sprintf(tmpName, "phase%d_ClipDistance%d", psContext->currentPhase, psIn->ui32SemanticIndex);
                    oss << tmpName;
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_VIEWPORT_ARRAY_INDEX:
                    oss << "mtl_ViewPortIndex";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_VERTEX_ID:
                    oss << "mtl_VertexID";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_INSTANCE_ID:
                    oss << "mtl_InstanceID";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_IS_FRONT_FACE:
                    oss << "(mtl_FrontFace ? 0xffffffffu : uint(0))";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_PRIMITIVE_ID:
                    // Not on Metal
                    ASSERT(0);
                    break;

                // as far as i understand tesselation factors are always coming from tessFactor variable (it is always declared in ToMetal::Translate)
                case NAME_FINAL_QUAD_U_EQ_0_EDGE_TESSFACTOR:
                case NAME_FINAL_TRI_U_EQ_0_EDGE_TESSFACTOR:
                case NAME_FINAL_LINE_DENSITY_TESSFACTOR:
                    if (psContext->psShader->aIndexedOutput[1][psOperand->ui32RegisterNumber])
                        oss << "tessFactor.edgeTessellationFactor";
                    else
                        oss << "tessFactor.edgeTessellationFactor[0]";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_FINAL_QUAD_V_EQ_0_EDGE_TESSFACTOR:
                case NAME_FINAL_TRI_V_EQ_0_EDGE_TESSFACTOR:
                case NAME_FINAL_LINE_DETAIL_TESSFACTOR:
                    oss << "tessFactor.edgeTessellationFactor[1]";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_FINAL_QUAD_U_EQ_1_EDGE_TESSFACTOR:
                case NAME_FINAL_TRI_W_EQ_0_EDGE_TESSFACTOR:
                    oss << "tessFactor.edgeTessellationFactor[2]";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_FINAL_QUAD_V_EQ_1_EDGE_TESSFACTOR:
                    oss << "tessFactor.edgeTessellationFactor[3]";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_FINAL_TRI_INSIDE_TESSFACTOR:
                case NAME_FINAL_QUAD_U_INSIDE_TESSFACTOR:
                    if (psContext->psShader->aIndexedOutput[1][psOperand->ui32RegisterNumber])
                        oss << "tessFactor.insideTessellationFactor";
                    else
                        oss << "tessFactor.insideTessellationFactor[0]";
                    *pui32IgnoreSwizzle = 1;
                    break;
                case NAME_FINAL_QUAD_V_INSIDE_TESSFACTOR:
                    oss << "tessFactor.insideTessellationFactor[1]";
                    *pui32IgnoreSwizzle = 1;
                    break;

                default:
                    const std::string patchPrefix = "patch.";

                    if (psContext->psShader->eShaderType == DOMAIN_SHADER)
                        oss << psContext->inputPrefix << patchPrefix << psIn->semanticName << psIn->ui32SemanticIndex;
                    else
                        oss << patchPrefix << psIn->semanticName << psIn->ui32SemanticIndex;

                    // Disable swizzles if this is a scalar
                    if (psContext->psShader->eShaderType == HULL_SHADER)
                    {
                        if ((psContext->psShader->abScalarOutput[1][psOperand->ui32RegisterNumber] & psOperand->GetAccessMask()) != 0)
                            *pui32IgnoreSwizzle = 1;
                    }
                    else
                    {
                        if ((psContext->psShader->abScalarInput[1][psOperand->ui32RegisterNumber] & psOperand->GetAccessMask()) != 0)
                            *pui32IgnoreSwizzle = 1;
                    }
                    break;
            }
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }

    if (hasCtor && (*pui32IgnoreSwizzle == 0))
    {
        oss << TranslateOperandSwizzle(psOperand, ui32CompMask, piRebase ? *piRebase : 0);
        *pui32IgnoreSwizzle = 1;
    }

    if (needsBoolUpscale)
    {
        if (requestedType == SVT_UINT || requestedType == SVT_UINT16 || requestedType == SVT_UINT8)
            oss << ") * 0xffffffffu";
        else
            oss << ") * int(0xffffffffu)";
        numParenthesis--;

        oss << ")";
        numParenthesis--;
    }

    while (numParenthesis != 0)
    {
        oss << ")";
        numParenthesis--;
    }
    return oss.str();
}

std::string ToMetal::TranslateOperand(const Operand* psOperand, uint32_t ui32TOFlag, uint32_t ui32ComponentMask)
{
    std::ostringstream oss;
    uint32_t ui32IgnoreSwizzle = 0;
    int iRebase = 0;

    // in single-component mode there is no need to use mask
    if (psOperand->eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE)
        ui32ComponentMask = OPERAND_4_COMPONENT_MASK_ALL;

    if (ui32TOFlag & TO_FLAG_NAME_ONLY)
    {
        return TranslateVariableName(psOperand, ui32TOFlag, &ui32IgnoreSwizzle, OPERAND_4_COMPONENT_MASK_ALL, &iRebase);
    }

    switch (psOperand->eModifier)
    {
        case OPERAND_MODIFIER_NONE:
        {
            break;
        }
        case OPERAND_MODIFIER_NEG:
        {
            oss << ("(-");
            break;
        }
        case OPERAND_MODIFIER_ABS:
        {
            oss << ("abs(");
            break;
        }
        case OPERAND_MODIFIER_ABSNEG:
        {
            oss << ("-abs(");
            break;
        }
    }

    oss << TranslateVariableName(psOperand, ui32TOFlag, &ui32IgnoreSwizzle, ui32ComponentMask, &iRebase);

    if (!ui32IgnoreSwizzle)
    {
        oss << TranslateOperandSwizzle(psOperand, ui32ComponentMask, iRebase);
    }

    switch (psOperand->eModifier)
    {
        case OPERAND_MODIFIER_NONE:
        {
            break;
        }
        case OPERAND_MODIFIER_NEG:
        {
            oss << (")");
            break;
        }
        case OPERAND_MODIFIER_ABS:
        {
            oss << (")");
            break;
        }
        case OPERAND_MODIFIER_ABSNEG:
        {
            oss << (")");
            break;
        }
    }
    return oss.str();
}
