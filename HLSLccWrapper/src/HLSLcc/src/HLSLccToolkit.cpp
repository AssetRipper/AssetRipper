#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/debug.h"
#include "internal_includes/toGLSLOperand.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/Shader.h"
#include "internal_includes/languages.h"
#include "include/UnityInstancingFlexibleArraySize.h"
#include <sstream>
#include <cmath>

namespace HLSLcc
{
    uint32_t GetNumberBitsSet(uint32_t a)
    {
        // Calculate number of bits in a
        // Taken from https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSet64
        // Works only up to 14 bits (we're only using up to 4)
        return (a * 0x200040008001ULL & 0x111111111111111ULL) % 0xf;
    }

    uint32_t SVTTypeToFlag(const SHADER_VARIABLE_TYPE eType)
    {
        if (eType == SVT_FLOAT16)
        {
            return TO_FLAG_FORCE_HALF;
        }
        if (eType == SVT_UINT || eType == SVT_UINT16)
        {
            return TO_FLAG_UNSIGNED_INTEGER;
        }
        else if (eType == SVT_INT || eType == SVT_INT16 || eType == SVT_INT12)
        {
            return TO_FLAG_INTEGER;
        }
        else if (eType == SVT_BOOL)
        {
            return TO_FLAG_BOOL;
        }
        else
        {
            return TO_FLAG_NONE;
        }
    }

    SHADER_VARIABLE_TYPE TypeFlagsToSVTType(const uint32_t typeflags)
    {
        if (typeflags & TO_FLAG_FORCE_HALF)
            return SVT_FLOAT16;
        if (typeflags & (TO_FLAG_INTEGER | TO_AUTO_BITCAST_TO_INT))
            return SVT_INT;
        if (typeflags & (TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_BITCAST_TO_UINT))
            return SVT_UINT;
        if (typeflags & TO_FLAG_BOOL)
            return SVT_BOOL;
        return SVT_FLOAT;
    }

    const char * GetConstructorForTypeGLSL(const HLSLCrossCompilerContext *context, const SHADER_VARIABLE_TYPE eType, const int components, bool useGLSLPrecision)
    {
        static const char * const uintTypes[] = { " ", "uint", "uvec2", "uvec3", "uvec4" };
        static const char * const uint16Types[] = { " ", "mediump uint", "mediump uvec2", "mediump uvec3", "mediump uvec4" };
        static const char * const intTypes[] = { " ", "int", "ivec2", "ivec3", "ivec4" };
        static const char * const int16Types[] = { " ", "mediump int", "mediump ivec2", "mediump ivec3", "mediump ivec4" };
        static const char * const int12Types[] = { " ", "lowp int", "lowp ivec2", "lowp ivec3", "lowp ivec4" };
        static const char * const floatTypes[] = { " ", "float", "vec2", "vec3", "vec4" };
        static const char * const float16Types[] = { " ", "mediump float", "mediump vec2", "mediump vec3", "mediump vec4" };
        static const char * const float10Types[] = { " ", "lowp float", "lowp vec2", "lowp vec3", "lowp vec4" };
        static const char * const boolTypes[] = { " ", "bool", "bvec2", "bvec3", "bvec4" };

        ASSERT(components >= 1 && components <= 4);
        bool emitLowp = EmitLowp(context);

        switch (eType)
        {
            case SVT_UINT:
                return HaveUnsignedTypes(context->psShader->eTargetLanguage) ? uintTypes[components] : intTypes[components];
            case SVT_UINT16:
                return useGLSLPrecision ? uint16Types[components] : uintTypes[components];
            case SVT_INT:
                return intTypes[components];
            case SVT_INT16:
                return useGLSLPrecision ? int16Types[components] : intTypes[components];
            case SVT_INT12:
                return useGLSLPrecision ? (emitLowp ? int12Types[components] : int16Types[components]) : intTypes[components];
            case SVT_FLOAT:
                return floatTypes[components];
            case SVT_FLOAT16:
                return useGLSLPrecision ? float16Types[components] : floatTypes[components];
            case SVT_FLOAT10:
                return useGLSLPrecision ? (emitLowp ? float10Types[components] : float16Types[components]) : floatTypes[components];
            case SVT_BOOL:
                return boolTypes[components];
            default:
                ASSERT(0);
                return " ";
        }
    }

    const char * GetConstructorForTypeMetal(const SHADER_VARIABLE_TYPE eType, const int components)
    {
        static const char * const uintTypes[] = { " ", "uint", "uint2", "uint3", "uint4" };
        static const char * const ushortTypes[] = { " ", "ushort", "ushort2", "ushort3", "ushort4" };
        static const char * const intTypes[] = { " ", "int", "int2", "int3", "int4" };
        static const char * const shortTypes[] = { " ", "short", "short2", "short3", "short4" };
        static const char * const floatTypes[] = { " ", "float", "float2", "float3", "float4" };
        static const char * const halfTypes[] = { " ", "half", "half2", "half3", "half4" };
        static const char * const boolTypes[] = { " ", "bool", "bool2", "bool3", "bool4" };

        ASSERT(components >= 1 && components <= 4);

        switch (eType)
        {
            case SVT_UINT:
                return uintTypes[components];
            case SVT_UINT16:
            case SVT_UINT8: // there is not uint8 in metal so treat it as ushort
                return ushortTypes[components];
            case SVT_INT:
                return intTypes[components];
            case SVT_INT16:
            case SVT_INT12:
                return shortTypes[components];
            case SVT_FLOAT:
                return floatTypes[components];
            case SVT_FLOAT16:
            case SVT_FLOAT10:
                return halfTypes[components];
            case SVT_BOOL:
                return boolTypes[components];
            default:
                ASSERT(0);
                return " ";
        }
    }

    const char * GetConstructorForType(const HLSLCrossCompilerContext *psContext, const SHADER_VARIABLE_TYPE eType, const int components, bool useGLSLPrecision /* = true*/)
    {
        if (psContext->psShader->eTargetLanguage == LANG_METAL)
            return GetConstructorForTypeMetal(eType, components);
        else
            return GetConstructorForTypeGLSL(psContext, eType, components, useGLSLPrecision);
    }

    std::string GetMatrixTypeName(const HLSLCrossCompilerContext *psContext, const SHADER_VARIABLE_TYPE eBaseType, const int columns, const int rows)
    {
        std::string result;
        std::ostringstream oss;
        if (psContext->psShader->eTargetLanguage == LANG_METAL)
        {
            switch (eBaseType)
            {
                case SVT_FLOAT:
                    oss << "float" << columns << "x" << rows;
                    break;
                case SVT_FLOAT16:
                case SVT_FLOAT10:
                    oss << "half" << columns << "x" << rows;
                    break;
                default:
                    ASSERT(0);
                    break;
            }
        }
        else
        {
            switch (eBaseType)
            {
                case SVT_FLOAT:
                    oss << "mat" << columns << "x" << rows;
                    break;
                case SVT_FLOAT16:
                    oss << "mediump mat" << columns << "x" << rows;
                    break;
                case SVT_FLOAT10:
                    oss << "lowp mat" << columns << "x" << rows;
                    break;
                default:
                    ASSERT(0);
                    break;
            }
        }
        result = oss.str();
        return result;
    }

    void AddSwizzleUsingElementCount(bstring dest, uint32_t count)
    {
        if (count == 4)
            return;
        if (count)
        {
            bcatcstr(dest, ".");
            bcatcstr(dest, "x");
            count--;
        }
        if (count)
        {
            bcatcstr(dest, "y");
            count--;
        }
        if (count)
        {
            bcatcstr(dest, "z");
            count--;
        }
        if (count)
        {
            bcatcstr(dest, "w");
            count--;
        }
    }

    // Calculate the bits set in mask
    int WriteMaskToComponentCount(uint32_t writeMask)
    {
        // In HLSL bytecode writemask 0 also means everything
        if (writeMask == 0)
            return 4;

        return (int)GetNumberBitsSet(writeMask);
    }

    uint32_t BuildComponentMaskFromElementCount(int count)
    {
        // Translate numComponents into bitmask
        // 1 -> 1, 2 -> 3, 3 -> 7 and 4 -> 15
        return (1 << count) - 1;
    }

    // Returns true if we can do direct assignment between types (mostly for mediump<->highp floats etc)
    bool DoAssignmentDataTypesMatch(SHADER_VARIABLE_TYPE dest, SHADER_VARIABLE_TYPE src)
    {
        if (src == dest)
            return true;

        if ((dest == SVT_FLOAT || dest == SVT_FLOAT10 || dest == SVT_FLOAT16) &&
            (src == SVT_FLOAT || src == SVT_FLOAT10 || src == SVT_FLOAT16))
            return true;

        if ((dest == SVT_INT || dest == SVT_INT12 || dest == SVT_INT16) &&
            (src == SVT_INT || src == SVT_INT12 || src == SVT_INT16))
            return true;

        if ((dest == SVT_UINT || dest == SVT_UINT16) &&
            (src == SVT_UINT || src == SVT_UINT16))
            return true;

        return false;
    }

    uint32_t ResourceReturnTypeToFlag(const RESOURCE_RETURN_TYPE eType)
    {
        if (eType == RETURN_TYPE_SINT)
        {
            return TO_FLAG_INTEGER;
        }
        else if (eType == RETURN_TYPE_UINT)
        {
            return TO_FLAG_UNSIGNED_INTEGER;
        }
        else
        {
            return TO_FLAG_NONE;
        }
    }

    SHADER_VARIABLE_TYPE ResourceReturnTypeToSVTType(const RESOURCE_RETURN_TYPE eType, const REFLECT_RESOURCE_PRECISION ePrec)
    {
        if (eType == RETURN_TYPE_SINT)
        {
            switch (ePrec)
            {
                default:
                    return SVT_INT;
                case REFLECT_RESOURCE_PRECISION_LOWP:
                    return SVT_INT12;
                case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                    return SVT_INT16;
            }
        }
        else if (eType == RETURN_TYPE_UINT)
        {
            switch (ePrec)
            {
                default:
                    return SVT_UINT;
                case REFLECT_RESOURCE_PRECISION_LOWP:
                    return SVT_UINT8;
                case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                    return SVT_UINT16;
            }
        }
        else
        {
            switch (ePrec)
            {
                default:
                    return SVT_FLOAT;
                case REFLECT_RESOURCE_PRECISION_LOWP:
                    return SVT_FLOAT10;
                case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                    return SVT_FLOAT16;
            }
        }
    }

    RESOURCE_RETURN_TYPE SVTTypeToResourceReturnType(SHADER_VARIABLE_TYPE type)
    {
        switch (type)
        {
            case SVT_INT:
            case SVT_INT12:
            case SVT_INT16:
                return RETURN_TYPE_SINT;
            case SVT_UINT:
            case SVT_UINT16:
                return RETURN_TYPE_UINT;
            case SVT_FLOAT:
            case SVT_FLOAT10:
            case SVT_FLOAT16:
                return RETURN_TYPE_FLOAT;
            default:
                return RETURN_TYPE_UNUSED;
        }
    }

    REFLECT_RESOURCE_PRECISION SVTTypeToPrecision(SHADER_VARIABLE_TYPE type)
    {
        switch (type)
        {
            case SVT_INT:
            case SVT_UINT:
            case SVT_FLOAT:
                return REFLECT_RESOURCE_PRECISION_HIGHP;
            case SVT_INT16:
            case SVT_UINT16:
            case SVT_FLOAT16:
                return REFLECT_RESOURCE_PRECISION_MEDIUMP;
            case SVT_INT12:
            case SVT_FLOAT10:
            case SVT_UINT8:
                return REFLECT_RESOURCE_PRECISION_LOWP;
            default:
                return REFLECT_RESOURCE_PRECISION_UNKNOWN;
        }
    }

    uint32_t ElemCountToAutoExpandFlag(uint32_t elemCount)
    {
        return TO_AUTO_EXPAND_TO_VEC2 << (elemCount - 2);
    }

    // Returns true if the operation is commutative
    bool IsOperationCommutative(int eOpCode)
    {
        switch ((OPCODE_TYPE)eOpCode)
        {
            case OPCODE_DADD:
            case OPCODE_IADD:
            case OPCODE_ADD:
            case OPCODE_MUL:
            case OPCODE_IMUL:
            case OPCODE_OR:
            case OPCODE_AND:
                return true;
            default:
                return false;
        }
    }

    // Returns true if operands are identical, only cares about temp registers currently.
    bool AreTempOperandsIdentical(const Operand * psA, const Operand * psB)
    {
        if (!psA || !psB)
            return 0;

        if (psA->eType != OPERAND_TYPE_TEMP || psB->eType != OPERAND_TYPE_TEMP)
            return 0;

        if (psA->eModifier != psB->eModifier)
            return 0;

        if (psA->iNumComponents != psB->iNumComponents)
            return 0;

        if (psA->ui32RegisterNumber != psB->ui32RegisterNumber)
            return 0;

        if (psA->eSelMode != psB->eSelMode)
            return 0;

        if (psA->eSelMode == OPERAND_4_COMPONENT_MASK_MODE && psA->ui32CompMask != psB->ui32CompMask)
            return 0;

        if (psA->eSelMode == OPERAND_4_COMPONENT_SELECT_1_MODE && psA->aui32Swizzle[0] != psB->aui32Swizzle[0])
            return 0;

        if (psA->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE && std::equal(&psA->aui32Swizzle[0], &psA->aui32Swizzle[4], &psB->aui32Swizzle[0]))
            return 0;

        return 1;
    }

    bool IsAddOneInstruction(const Instruction *psInst)
    {
        if (psInst->eOpcode != OPCODE_IADD)
            return false;
        if (psInst->asOperands[0].eType != OPERAND_TYPE_TEMP)
            return false;

        if (psInst->asOperands[1].eType == OPERAND_TYPE_TEMP)
        {
            if (psInst->asOperands[1].ui32RegisterNumber != psInst->asOperands[0].ui32RegisterNumber)
                return false;
            if (psInst->asOperands[2].eType != OPERAND_TYPE_IMMEDIATE32)
                return false;

            if (*(int *)&psInst->asOperands[2].afImmediates[0] != 1)
                return false;
        }
        else
        {
            if (psInst->asOperands[1].eType != OPERAND_TYPE_IMMEDIATE32)
                return false;
            if (psInst->asOperands[2].eType != OPERAND_TYPE_TEMP)
                return false;

            if (psInst->asOperands[2].ui32RegisterNumber != psInst->asOperands[0].ui32RegisterNumber)
                return false;

            if (*(int *)&psInst->asOperands[1].afImmediates[0] != 1)
                return false;
        }
        return true;
    }

    int GetNumTextureDimensions(int /* RESOURCE_DIMENSION */ eResDim)
    {
        switch ((RESOURCE_DIMENSION)eResDim)
        {
            case RESOURCE_DIMENSION_TEXTURE1D:
                return 1;
            case RESOURCE_DIMENSION_TEXTURE2D:
            case RESOURCE_DIMENSION_TEXTURE2DMS:
            case RESOURCE_DIMENSION_TEXTURE1DARRAY:
            case RESOURCE_DIMENSION_TEXTURECUBE:
                return 2;
            case RESOURCE_DIMENSION_TEXTURE3D:
            case RESOURCE_DIMENSION_TEXTURE2DARRAY:
            case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
            case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                return 3;
            default:
                ASSERT(0);
                break;
        }
        return 0;
    }

    // Returns the "more important" type of a and b, currently int < uint < float
    SHADER_VARIABLE_TYPE SelectHigherType(SHADER_VARIABLE_TYPE a, SHADER_VARIABLE_TYPE b)
    {
#define DO_CHECK(type) if( a == type || b == type ) return type

        // Priority ordering
        DO_CHECK(SVT_FLOAT16);
        DO_CHECK(SVT_FLOAT10);
        DO_CHECK(SVT_UINT16);
        DO_CHECK(SVT_UINT8);
        DO_CHECK(SVT_INT16);
        DO_CHECK(SVT_INT12);
        DO_CHECK(SVT_FORCED_INT);
        DO_CHECK(SVT_FLOAT);
        DO_CHECK(SVT_UINT);
        DO_CHECK(SVT_INT);
        DO_CHECK(SVT_INT_AMBIGUOUS);

#undef DO_CHECK
        // After these just rely on ordering.
        return a > b ? a : b;
    }

    // Returns true if a direct constructor can convert src->dest
    bool CanDoDirectCast(const HLSLCrossCompilerContext *context, SHADER_VARIABLE_TYPE src, SHADER_VARIABLE_TYPE dest)
    {
        // uint<->int<->bool conversions possible
        if ((src == SVT_INT || src == SVT_UINT || src == SVT_BOOL || src == SVT_INT12 || src == SVT_INT16 || src == SVT_UINT16) &&
            (dest == SVT_INT || dest == SVT_UINT || dest == SVT_BOOL || dest == SVT_INT12 || dest == SVT_INT16 || dest == SVT_UINT16))
            return true;

        // float<->double possible
        if ((src == SVT_FLOAT || src == SVT_DOUBLE || src == SVT_FLOAT16 || src == SVT_FLOAT10) &&
            (dest == SVT_FLOAT || dest == SVT_DOUBLE || dest == SVT_FLOAT16 || dest == SVT_FLOAT10))
            return true;

        if (context->psShader->eTargetLanguage == LANG_METAL)
        {
            // avoid compiler error: cannot use as_type to cast from 'half' to 'unsigned int' or 'int', types of different size
            if ((src == SVT_FLOAT16 || src == SVT_FLOAT10) && (dest == SVT_UINT || dest == SVT_INT))
                return true;
        }

        return false;
    }

    bool IsUnityFlexibleInstancingBuffer(const ConstantBuffer* psCBuf)
    {
        return psCBuf != NULL && psCBuf->asVars.size() == 1
            && psCBuf->asVars[0].sType.Class == SVC_STRUCT && psCBuf->asVars[0].sType.Elements == 2
            && IsUnityInstancingConstantBufferName(psCBuf->name.c_str());
    }

#ifndef fpcheck
#ifdef _MSC_VER
#define fpcheck(x) (_isnan(x) || !_finite(x))
#else
#define fpcheck(x) (std::isnan(x) || std::isinf(x))
#endif
#endif // #ifndef fpcheck

    // Helper function to print floats with full precision
    void PrintFloat(bstring b, float f)
    {
        bstring temp;
        int ePos;
        int pointPos;

        temp = bformat("%.9g", f);
        ePos = bstrchrp(temp, 'e', 0);
        pointPos = bstrchrp(temp, '.', 0);

        bconcat(b, temp);
        bdestroy(temp);

        if (ePos < 0 && pointPos < 0 && !fpcheck(f))
            bcatcstr(b, ".0");
    }

    bstring GetEarlyMain(HLSLCrossCompilerContext *psContext)
    {
        bstring *oldString = psContext->currentGLSLString;
        bstring *str = &psContext->psShader->asPhases[psContext->currentPhase].earlyMain;
        int indent = psContext->indent;

        if (psContext->psShader->eTargetLanguage == LANG_METAL && !psContext->indent)
            ++psContext->indent;

        psContext->currentGLSLString = str;
        psContext->AddIndentation();
        psContext->currentGLSLString = oldString;
        psContext->indent = indent;

        return *str;
    }

    bstring GetPostShaderCode(HLSLCrossCompilerContext *psContext)
    {
        bstring *oldString = psContext->currentGLSLString;
        bstring *str = &psContext->psShader->asPhases[psContext->currentPhase].postShaderCode;
        int indent = psContext->indent;

        if (psContext->psShader->eTargetLanguage == LANG_METAL && !psContext->indent)
            ++psContext->indent;

        psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode = 1;

        psContext->currentGLSLString = str;
        psContext->AddIndentation();
        psContext->currentGLSLString = oldString;
        psContext->indent = indent;

        return *str;
    }
}
