#include "internal_includes/toGLSLOperand.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/languages.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "bstrlib.h"
#include "stdio.h"
#include <stdlib.h>
#include <algorithm>
#include "internal_includes/debug.h"
#include "internal_includes/Shader.h"
#include "internal_includes/Instruction.h"
#include "internal_includes/toGLSL.h"
#include <sstream>

using namespace HLSLcc;

// In toGLSLDeclaration.cpp
const char* GetSamplerType(HLSLCrossCompilerContext* psContext,
    const RESOURCE_DIMENSION eDimension,
    const uint32_t ui32RegisterNumber);
bool DeclareRWStructuredBufferTemplateTypeAsInteger(HLSLCrossCompilerContext* psContext, const Operand* psOperand);

// This function prints out the destination name, possible destination writemask, assignment operator
// and any possible conversions needed based on the eSrcType+ui32SrcElementCount (type and size of data expected to be coming in)
// As an output, pNeedsParenthesis will be filled with the amount of closing parenthesis needed
// and pSrcCount will be filled with the number of components expected
// ui32CompMask can be used to only write to 1 or more components (used by MOVC)
void ToGLSL::AddOpAssignToDestWithMask(const Operand* psDest,
    SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int *pNeedsParenthesis, uint32_t ui32CompMask)
{
    uint32_t ui32DestElementCount = psDest->GetNumSwizzleElements(ui32CompMask);
    bstring glsl = *psContext->currentGLSLString;
    SHADER_VARIABLE_TYPE eDestDataType = psDest->GetDataType(psContext);
    ASSERT(pNeedsParenthesis != NULL);

    *pNeedsParenthesis = 0;

    TranslateOperand(psDest, TO_FLAG_DESTINATION, ui32CompMask);

    bcatcstr(glsl, " = ");

    if (precise && HavePreciseQualifier(psContext->psShader->eTargetLanguage))
    {
        char const *t, *s;
        switch (eDestDataType)
        {
            case SVT_BOOL: t = "bvec4"; break;
            case SVT_INT: t = "ivec4"; break;
            case SVT_FLOAT: t = "vec4"; break;
            case SVT_UINT: t = "uvec4"; break;
            default: ASSERT(0); t = NULL; break;
        }
        switch (ui32DestElementCount)
        {
            case 1: s = ".x"; break;
            case 2: s = ".xy"; break;
            case 3: s = ".xyz"; break;
            case 4: s = ".xyzw"; break;
            default: ASSERT(0); s = NULL; break;
        }
        if (t && s)
        {
            bformata(glsl, "(u_xlat_precise_%s%s = (", t, s);
            (*pNeedsParenthesis) += 2;
        }
    }

    // Simple path: types match.
    if (DoAssignmentDataTypesMatch(eDestDataType, eSrcType))
    {
        // Cover cases where the HLSL language expects the rest of the components to be default-filled
        // eg. MOV r0, c0.x => Temp[0] = vec4(c0.x);
        if (ui32DestElementCount > ui32SrcElementCount)
        {
            bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eDestDataType, ui32DestElementCount, false));
            (*pNeedsParenthesis)++;
        }

        return;
    }

    switch (eDestDataType)
    {
        case SVT_INT:
        case SVT_INT12:
        case SVT_INT16:
            // Bitcasts from lower precisions are ambiguous
            ASSERT(eSrcType != SVT_FLOAT10 && eSrcType != SVT_FLOAT16);
            if (eSrcType == SVT_FLOAT && psContext->psShader->ui32MajorVersion > 3 && HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
            {
                bcatcstr(glsl, "floatBitsToInt(");
                // Cover cases where the HLSL language expects the rest of the components to be default-filled
                if (ui32DestElementCount > ui32SrcElementCount)
                {
                    bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eSrcType, ui32DestElementCount, false));
                    (*pNeedsParenthesis)++;
                }
            }
            else
                bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eDestDataType, ui32DestElementCount, false));

            (*pNeedsParenthesis)++;
            break;
        case SVT_UINT:
        case SVT_UINT16:
            ASSERT(eSrcType != SVT_FLOAT10 && eSrcType != SVT_FLOAT16);
            if (eSrcType == SVT_FLOAT && psContext->psShader->ui32MajorVersion > 3 && HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
            {
                bcatcstr(glsl, "floatBitsToUint(");
                // Cover cases where the HLSL language expects the rest of the components to be default-filled
                if (ui32DestElementCount > ui32SrcElementCount)
                {
                    bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eSrcType, ui32DestElementCount, false));
                    (*pNeedsParenthesis)++;
                }
            }
            else
                bformata(glsl, " %s(", GetConstructorForTypeGLSL(psContext, eDestDataType, ui32DestElementCount, false));

            (*pNeedsParenthesis)++;
            break;

        case SVT_FLOAT:
        case SVT_FLOAT10:
        case SVT_FLOAT16:
            ASSERT(eSrcType != SVT_INT12 || (eSrcType != SVT_INT16 && eSrcType != SVT_UINT16));
            if (psContext->psShader->ui32MajorVersion > 3 && HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
            {
                if (eSrcType == SVT_INT)
                    bcatcstr(glsl, "intBitsToFloat(");
                else
                    bcatcstr(glsl, "uintBitsToFloat(");
                // Cover cases where the HLSL language expects the rest of the components to be default-filled
                if (ui32DestElementCount > ui32SrcElementCount)
                {
                    bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eSrcType, ui32DestElementCount, false));
                    (*pNeedsParenthesis)++;
                }
            }
            else
                bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, eDestDataType, ui32DestElementCount, false));

            (*pNeedsParenthesis)++;
            break;
        case SVT_BOOL:
            bformata(glsl, " %s(", GetConstructorForTypeGLSL(psContext, eDestDataType, ui32DestElementCount, false));
            (*pNeedsParenthesis)++;
            break;
        default:
            ASSERT(0);
            break;
    }
}

void ToGLSL::AddAssignToDest(const Operand* psDest,
    SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int* pNeedsParenthesis)
{
    AddOpAssignToDestWithMask(psDest, eSrcType, ui32SrcElementCount, precise, pNeedsParenthesis, OPERAND_4_COMPONENT_MASK_ALL);
}

void ToGLSL::AddAssignPrologue(int numParenthesis, bool isEmbedded /* = false*/)
{
    bstring glsl = *psContext->currentGLSLString;
    while (numParenthesis != 0)
    {
        bcatcstr(glsl, ")");
        numParenthesis--;
    }
    if (!isEmbedded)
        bcatcstr(glsl, ";\n");
}

void ToGLSL::AddComparison(Instruction* psInst, ComparisonType eType,
    uint32_t typeFlag)
{
    // Multiple cases to consider here:
    // For shader model <=3: all comparisons are floats
    // otherwise:
    // OPCODE_LT, _GT, _NE etc: inputs are floats, outputs UINT 0xffffffff or 0. typeflag: TO_FLAG_NONE
    // OPCODE_ILT, _IGT etc: comparisons are signed ints, outputs UINT 0xffffffff or 0 typeflag TO_FLAG_INTEGER
    // _ULT, UGT etc: inputs unsigned ints, outputs UINTs typeflag TO_FLAG_UNSIGNED_INTEGER
    //
    // Additional complexity: if dest swizzle element count is 1, we can use normal comparison operators, otherwise glsl intrinsics.


    bstring glsl = *psContext->currentGLSLString;
    const uint32_t destElemCount = psInst->asOperands[0].GetNumSwizzleElements();
    const uint32_t s0ElemCount = psInst->asOperands[1].GetNumSwizzleElements();
    const uint32_t s1ElemCount = psInst->asOperands[2].GetNumSwizzleElements();
    int isBoolDest = psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL;

    int floatResult = 0;

    ASSERT(s0ElemCount == s1ElemCount || s1ElemCount == 1 || s0ElemCount == 1);
    if (s0ElemCount != s1ElemCount)
    {
        // Set the proper auto-expand flag is either argument is scalar
        typeFlag |= (TO_AUTO_EXPAND_TO_VEC2 << (std::max(s0ElemCount, s1ElemCount) - 2));
    }

    if (psContext->psShader->ui32MajorVersion < 4)
    {
        floatResult = 1;
    }

    if (destElemCount > 1)
    {
        const char* glslOpcode[] = {
            "equal",
            "lessThan",
            "greaterThanEqual",
            "notEqual",
        };

        int needsParenthesis = 0;
        psContext->AddIndentation();
        if (isBoolDest)
        {
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION | TO_FLAG_BOOL);
            bcatcstr(glsl, " = ");
        }
        else
        {
            AddAssignToDest(&psInst->asOperands[0], floatResult ? SVT_FLOAT : SVT_UINT, destElemCount, psInst->ui32PreciseMask, &needsParenthesis);

            bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, floatResult ? SVT_FLOAT : SVT_UINT, destElemCount, false));
            bcatcstr(glsl, "(");
        }
        bformata(glsl, "%s(", glslOpcode[eType]);
        TranslateOperand(&psInst->asOperands[1], typeFlag);
        bcatcstr(glsl, ", ");
        TranslateOperand(&psInst->asOperands[2], typeFlag);
        bcatcstr(glsl, ")");
        TranslateOperandSwizzle(psContext, &psInst->asOperands[0], 0);
        if (!isBoolDest)
        {
            bcatcstr(glsl, ")");
            if (!floatResult)
            {
                if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                    bcatcstr(glsl, " * 0xFFFFFFFFu");
                else
                    bcatcstr(glsl, " * -1");    // GLSL ES 2 spec: high precision ints are guaranteed to have a range of at least (-2^16, 2^16)
            }
        }

        AddAssignPrologue(needsParenthesis);
    }
    else
    {
        const char* glslOpcode[] = {
            "==",
            "<",
            ">=",
            "!=",
        };

        //Scalar compare

        const bool workaroundAdrenoBugs = psContext->psShader->eTargetLanguage == LANG_ES_300;

        if (workaroundAdrenoBugs)
        {
            // Workarounds for bug cases 777617, 735299, 776827
            bcatcstr(glsl, "#ifdef UNITY_ADRENO_ES3\n");

            int needsParenthesis = 0;
            psContext->AddIndentation();
            if (isBoolDest)
            {
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION | TO_FLAG_BOOL);
                bcatcstr(glsl, " = !!(");
                needsParenthesis += 1;
                TranslateOperand(&psInst->asOperands[1], typeFlag);
                bformata(glsl, "%s", glslOpcode[eType]);
                TranslateOperand(&psInst->asOperands[2], typeFlag);
                AddAssignPrologue(needsParenthesis);
            }
            else
            {
                bcatcstr(glsl, "{ bool cond = ");
                TranslateOperand(&psInst->asOperands[1], typeFlag);
                bformata(glsl, "%s", glslOpcode[eType]);
                TranslateOperand(&psInst->asOperands[2], typeFlag);
                bcatcstr(glsl, "; ");
                AddAssignToDest(&psInst->asOperands[0], floatResult ? SVT_FLOAT : SVT_UINT, destElemCount, psInst->ui32PreciseMask, &needsParenthesis);
                bcatcstr(glsl, "!!cond ? ");
                if (floatResult)
                    bcatcstr(glsl, "1.0 : 0.0");
                else
                {
                    // Old ES3.0 Adrenos treat 0u as const int.
                    // GLSL ES 2 spec: high precision ints are guaranteed to have a range of at least (-2^16, 2^16)
                    bcatcstr(glsl, HaveUnsignedTypes(psContext->psShader->eTargetLanguage) ? "0xFFFFFFFFu : uint(0)" : "-1 : 0");
                }
                AddAssignPrologue(needsParenthesis, true);
                bcatcstr(glsl, "; }\n");
            }

            bcatcstr(glsl, "#else\n");
        }

        int needsParenthesis = 0;
        psContext->AddIndentation();
        if (isBoolDest)
        {
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION | TO_FLAG_BOOL);
            bcatcstr(glsl, " = ");
        }
        else
        {
            AddAssignToDest(&psInst->asOperands[0], floatResult ? SVT_FLOAT : SVT_UINT, destElemCount, psInst->ui32PreciseMask, &needsParenthesis);
            bcatcstr(glsl, "(");
        }
        TranslateOperand(&psInst->asOperands[1], typeFlag);
        bformata(glsl, "%s", glslOpcode[eType]);
        TranslateOperand(&psInst->asOperands[2], typeFlag);
        if (!isBoolDest)
        {
            if (floatResult)
                bcatcstr(glsl, ") ? 1.0 : 0.0");
            else
            {
                // Old ES3.0 Adrenos treat 0u as const int.
                // GLSL ES 2 spec: high precision ints are guaranteed to have a range of at least (-2^16, 2^16)
                bcatcstr(glsl, HaveUnsignedTypes(psContext->psShader->eTargetLanguage) ? ") ? 0xFFFFFFFFu : uint(0)" : ") ? -1 : 0");
            }
        }
        AddAssignPrologue(needsParenthesis);

        if (workaroundAdrenoBugs)
            bcatcstr(glsl, "#endif\n");
    }
}

void ToGLSL::AddMOVBinaryOp(const Operand *pDest, Operand *pSrc, uint32_t precise, bool isEmbedded /* = false*/)
{
    int numParenthesis = 0;
    int srcSwizzleCount = pSrc->GetNumSwizzleElements();
    uint32_t writeMask = pDest->GetAccessMask();

    const SHADER_VARIABLE_TYPE eSrcType = pSrc->GetDataType(psContext, pDest->GetDataType(psContext));
    uint32_t flags = SVTTypeToFlag(eSrcType);

    AddAssignToDest(pDest, eSrcType, srcSwizzleCount, precise, &numParenthesis);
    TranslateOperand(pSrc, flags, writeMask);

    AddAssignPrologue(numParenthesis, isEmbedded);
}

void ToGLSL::AddMOVCBinaryOp(const Operand *pDest, const Operand *src0, Operand *src1, Operand *src2, uint32_t precise)
{
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destElemCount = pDest->GetNumSwizzleElements();
    uint32_t s0ElemCount = src0->GetNumSwizzleElements();
    uint32_t s1ElemCount = src1->GetNumSwizzleElements();
    uint32_t s2ElemCount = src2->GetNumSwizzleElements();
    uint32_t destWriteMask = pDest->GetAccessMask();
    uint32_t destElem;

    const SHADER_VARIABLE_TYPE eDestType = pDest->GetDataType(psContext);
    /*
    for each component in dest[.mask]
    if the corresponding component in src0 (POS-swizzle)
    has any bit set
    {
    copy this component (POS-swizzle) from src1 into dest
    }
    else
    {
    copy this component (POS-swizzle) from src2 into dest
    }
    endfor
    */

    /* Single-component conditional variable (src0) */
    if (s0ElemCount == 1 || src0->IsSwizzleReplicated())
    {
        int numParenthesis = 0;
        SHADER_VARIABLE_TYPE s0Type = src0->GetDataType(psContext);
        psContext->AddIndentation();
        AddAssignToDest(pDest, eDestType, destElemCount, precise, &numParenthesis);
        bcatcstr(glsl, "(");
        if (s0Type == SVT_UINT || s0Type == SVT_UINT16)
            TranslateOperand(src0, TO_AUTO_BITCAST_TO_UINT, OPERAND_4_COMPONENT_MASK_X);
        else if (s0Type == SVT_BOOL)
            TranslateOperand(src0, TO_FLAG_BOOL, OPERAND_4_COMPONENT_MASK_X);
        else
            TranslateOperand(src0, TO_AUTO_BITCAST_TO_INT, OPERAND_4_COMPONENT_MASK_X);

        if (psContext->psShader->ui32MajorVersion < 4)
        {
            //cmp opcode uses >= 0
            bcatcstr(glsl, " >= 0) ? ");
        }
        else
        {
            if (s0Type == SVT_UINT || s0Type == SVT_UINT16)
                bcatcstr(glsl, HaveUnsignedTypes(psContext->psShader->eTargetLanguage) ? " != uint(0)) ? " : " != 0) ? ");  // Old ES3.0 Adrenos treat 0u as const int.
            else if (s0Type == SVT_BOOL)
                bcatcstr(glsl, ") ? ");
            else
                bcatcstr(glsl, " != 0) ? ");
        }

        if (s1ElemCount == 1 && destElemCount > 1)
            TranslateOperand(src1, SVTTypeToFlag(eDestType) | ElemCountToAutoExpandFlag(destElemCount));
        else
            TranslateOperand(src1, SVTTypeToFlag(eDestType), destWriteMask);

        bcatcstr(glsl, " : ");
        if (s2ElemCount == 1 && destElemCount > 1)
            TranslateOperand(src2, SVTTypeToFlag(eDestType) | ElemCountToAutoExpandFlag(destElemCount));
        else
            TranslateOperand(src2, SVTTypeToFlag(eDestType), destWriteMask);

        AddAssignPrologue(numParenthesis);
    }
    else
    {
        // NOTE: mix() cannot be used to implement MOVC, because it propagates
        // NaN from both endpoints.
        int srcElem = -1;
        SHADER_VARIABLE_TYPE dstType = pDest->GetDataType(psContext);
        SHADER_VARIABLE_TYPE s0Type = src0->GetDataType(psContext);

        // Use an extra temp if dest is also one of the sources. Without this some swizzle combinations
        // might alter the source before all components are handled.
        const std::string tempName = "hlslcc_movcTemp";
        bool dstIsSrc1 = (pDest->eType == src1->eType)
            && (dstType == src1->GetDataType(psContext))
            && (pDest->ui32RegisterNumber == src1->ui32RegisterNumber);
        bool dstIsSrc2 = (pDest->eType == src2->eType)
            && (dstType == src2->GetDataType(psContext))
            && (pDest->ui32RegisterNumber == src2->ui32RegisterNumber);

        if (dstIsSrc1 || dstIsSrc2)
        {
            psContext->AddIndentation();
            bcatcstr(glsl, "{\n");
            ++psContext->indent;
            psContext->AddIndentation();
            int numComponents = (pDest->eType == OPERAND_TYPE_TEMP) ?
                psContext->psShader->GetTempComponentCount(eDestType, pDest->ui32RegisterNumber) :
                pDest->iNumComponents;

            const char* constructorStr = HLSLcc::GetConstructorForType(psContext, eDestType, numComponents, false);
            bformata(glsl, "%s %s = ", constructorStr, tempName.c_str());
            TranslateOperand(pDest, TO_FLAG_NAME_ONLY);
            bformata(glsl, ";\n");

            // Override OPERAND_TYPE_TEMP name temporarily
            const_cast<Operand *>(pDest)->specialName.assign(tempName);
        }

        for (destElem = 0; destElem < 4; ++destElem)
        {
            int numParenthesis = 0;
            srcElem++;
            if (pDest->eSelMode == OPERAND_4_COMPONENT_MASK_MODE && pDest->ui32CompMask != 0 && !(pDest->ui32CompMask & (1 << destElem)))
                continue;

            psContext->AddIndentation();
            AddOpAssignToDestWithMask(pDest, eDestType, 1, precise, &numParenthesis, 1 << destElem);
            bcatcstr(glsl, "(");
            if (s0Type == SVT_BOOL)
            {
                TranslateOperand(src0, TO_FLAG_BOOL, 1 << srcElem);
                bcatcstr(glsl, ") ? ");
            }
            else
            {
                TranslateOperand(src0, TO_AUTO_BITCAST_TO_INT, 1 << srcElem);

                if (psContext->psShader->ui32MajorVersion < 4)
                {
                    //cmp opcode uses >= 0
                    bcatcstr(glsl, " >= 0) ? ");
                }
                else
                {
                    bcatcstr(glsl, " != 0) ? ");
                }
            }

            TranslateOperand(src1, SVTTypeToFlag(eDestType), 1 << srcElem);
            bcatcstr(glsl, " : ");
            TranslateOperand(src2, SVTTypeToFlag(eDestType), 1 << srcElem);
            AddAssignPrologue(numParenthesis);
        }

        if (dstIsSrc1 || dstIsSrc2)
        {
            const_cast<Operand *>(pDest)->specialName.clear();

            psContext->AddIndentation();
            TranslateOperand(glsl, pDest, TO_FLAG_NAME_ONLY);
            bformata(glsl, " = %s;\n", tempName.c_str());

            --psContext->indent;
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
        }
    }
}

void ToGLSL::CallBinaryOp(const char* name, Instruction* psInst,
    int dest, int src0, int src1, SHADER_VARIABLE_TYPE eDataType, bool isEmbedded /* = false*/)
{
    uint32_t ui32Flags = SVTTypeToFlag(eDataType);
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = psInst->asOperands[dest].GetAccessMask();
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int needsParenthesis = 0;

    if (!HaveNativeBitwiseOps(psContext->psShader->eTargetLanguage))
    {
        const char *binaryOpWrap = NULL;

        if (!strcmp("%", name))
            binaryOpWrap = "op_modi";
        else if (!strcmp("&", name))
            binaryOpWrap = "op_and";
        else if (!strcmp("|", name))
            binaryOpWrap = "op_or";
        else if (!strcmp("^", name))
            binaryOpWrap = "op_xor";
        else if (!strcmp(">>", name))
            binaryOpWrap = "op_shr";
        else if (!strcmp("<<", name))
            binaryOpWrap = "op_shl";
        // op_not handled separately at OPCODE_NOT

        if (binaryOpWrap)
        {
            UseExtraFunctionDependency(binaryOpWrap);
            CallHelper2Int(binaryOpWrap, psInst, 0, 1, 2, 1);
            return;
        }
    }

    if (src1SwizCount != src0SwizCount)
    {
        uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    if (!isEmbedded)
        psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], eDataType, dstSwizCount, psInst->ui32PreciseMask, &needsParenthesis);

    // Adreno 3xx fails on binary ops that operate on vectors
    bool opComponentWiseOnAdreno = (!strcmp("&", name) || !strcmp("|", name) || !strcmp("^", name) || !strcmp(">>", name) || !strcmp("<<", name));
    if (psContext->psShader->eTargetLanguage == LANG_ES_300 && opComponentWiseOnAdreno)
    {
        uint32_t i;
        int firstPrinted = 0;
        bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, eDataType, dstSwizCount, false));
        bcatcstr(glsl, "(");
        for (i = 0; i < 4; i++)
        {
            if (!(destMask & (1 << i)))
                continue;

            if (firstPrinted != 0)
                bcatcstr(glsl, ", ");
            else
                firstPrinted = 1;

            // Remove the auto expand flags
            ui32Flags &= ~(TO_AUTO_EXPAND_TO_VEC2 | TO_AUTO_EXPAND_TO_VEC3 | TO_AUTO_EXPAND_TO_VEC4);

            TranslateOperand(&psInst->asOperands[src0], ui32Flags, 1 << i);
            bformata(glsl, " %s ", name);
            TranslateOperand(&psInst->asOperands[src1], ui32Flags, 1 << i);
        }
        bcatcstr(glsl, ")");
    }
    else
    {
        TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
        bformata(glsl, " %s ", name);
        TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    }

    AddAssignPrologue(needsParenthesis, isEmbedded);
}

void ToGLSL::CallTernaryOp(const char* op1, const char* op2, Instruction* psInst,
    int dest, int src0, int src1, int src2, uint32_t dataType)
{
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = psInst->asOperands[dest].GetAccessMask();
    uint32_t src2SwizCount = psInst->asOperands[src2].GetNumSwizzleElements(destMask);
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();

    uint32_t ui32Flags = dataType;
    int numParenthesis = 0;

    if (src1SwizCount != src0SwizCount || src2SwizCount != src0SwizCount)
    {
        uint32_t maxElems = std::max(src2SwizCount, std::max(src1SwizCount, src0SwizCount));
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], TypeFlagsToSVTType(dataType), dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bformata(glsl, " %s ", op1);
    TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    bformata(glsl, " %s ", op2);
    TranslateOperand(&psInst->asOperands[src2], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::CallHelper3(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int src2, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_FLOAT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    uint32_t src2SwizCount = psInst->asOperands[src2].GetNumSwizzleElements(destMask);
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int numParenthesis = 0;

    if ((src1SwizCount != src0SwizCount || src2SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src2SwizCount, std::max(src1SwizCount, src0SwizCount));
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], SVT_FLOAT, dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    TranslateOperand(&psInst->asOperands[src2], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::CallHelper2(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_FLOAT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();

    int isDotProduct = (strncmp(name, "dot", 3) == 0) ? 1 : 0;
    int numParenthesis = 0;

    if ((src1SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();
    AddAssignToDest(&psInst->asOperands[dest], SVT_FLOAT, isDotProduct ? 1 : dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;

    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);

    AddAssignPrologue(numParenthesis);
}

void ToGLSL::CallHelper2Int(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_INT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int numParenthesis = 0;

    if ((src1SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], SVT_INT, dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::CallHelper2UInt(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_UINT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int numParenthesis = 0;

    if ((src1SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], SVT_UINT, dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::CallHelper1(const char* name, Instruction* psInst,
    int dest, int src0, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_FLOAT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    int numParenthesis = 0;

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], SVT_FLOAT, dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

//Result is an int.
void ToGLSL::CallHelper1Int(
    const char* name,
    Instruction* psInst,
    const int dest,
    const int src0,
    int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_INT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    int numParenthesis = 0;

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], SVT_INT, dstSwizCount, psInst->ui32PreciseMask, &numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

// Texel fetches etc need a dummy sampler (because glslang wants one, for Reasons(tm)).
// Any non-shadow sampler will do, so try to get one from sampler registers. If the current shader doesn't have any, declare a dummy one.
std::string ToGLSL::GetVulkanDummySamplerName()
{
    std::string dummySmpName = "hlslcc_dummyPointClamp";
    if (!psContext->IsVulkan())
        return "";

    const ResourceBinding *pSmpInfo = NULL;
    int smpIdx = 0;

    while (psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_SAMPLER, smpIdx, &pSmpInfo) != 0)
    {
        if (pSmpInfo->m_SamplerMode != D3D10_SB_SAMPLER_MODE_COMPARISON)
            return ResourceName(psContext, RGROUP_SAMPLER, smpIdx, 0);

        smpIdx++;
    }

    if (!psContext->psShader->m_DummySamplerDeclared)
    {
        GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(dummySmpName);
        bstring code = bfromcstr("");
        bformata(code, "layout(set = %d, binding = %d) uniform mediump sampler %s;", binding.set, binding.binding, dummySmpName.c_str());
        DeclareExtraFunction(dummySmpName, code);
        bdestroy(code);
        psContext->psShader->m_DummySamplerDeclared = true;
    }
    return dummySmpName;
}

void ToGLSL::TranslateTexelFetch(
    Instruction* psInst,
    const ResourceBinding* psBinding,
    bstring glsl)
{
    int numParenthesis = 0;

    std::string vulkanSamplerName = GetVulkanDummySamplerName();

    std::string texName = ResourceName(psContext, RGROUP_TEXTURE, psInst->asOperands[2].ui32RegisterNumber, 0);
    const bool hasOffset = (psInst->bAddressOffset != 0);

    // On Vulkan wrap the tex name with the sampler constructor
    if (psContext->IsVulkan())
    {
        const RESOURCE_DIMENSION eResDim = psContext->psShader->aeResourceDims[psInst->asOperands[2].ui32RegisterNumber];
        std::string smpType = GetSamplerType(psContext, eResDim, psInst->asOperands[2].ui32RegisterNumber);
        std::ostringstream oss;
        oss << smpType;
        oss << "(" << texName << ", " << vulkanSamplerName << ")";
        texName = oss.str();
    }

    psContext->AddIndentation();
    AddAssignToDest(&psInst->asOperands[0], TypeFlagsToSVTType(ResourceReturnTypeToFlag(psBinding->ui32ReturnType)), 4, psInst->ui32PreciseMask, &numParenthesis);

    if (hasOffset)
        bcatcstr(glsl, "texelFetchOffset(");
    else
        bcatcstr(glsl, "texelFetch(");

    switch (psBinding->eDimension)
    {
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
        case REFLECT_RESOURCE_DIMENSION_BUFFER:
        {
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            // Buffers don't have LOD or offset
            if (psBinding->eDimension != REFLECT_RESOURCE_DIMENSION_BUFFER)
            {
                bcatcstr(glsl, ", ");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_A);
                if (hasOffset)
                    bformata(glsl, ", %d", psInst->iUAddrOffset);
            }
            bcatcstr(glsl, ")");
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY:
        case REFLECT_RESOURCE_DIMENSION_TEXTURE3D:
        {
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_A);
            if (hasOffset && psBinding->eDimension == REFLECT_RESOURCE_DIMENSION_TEXTURE3D)
                bformata(glsl, ", ivec3(%d, %d, %d)", psInst->iUAddrOffset, psInst->iVAddrOffset, psInst->iWAddrOffset);
            if (hasOffset && psBinding->eDimension == REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY)
                bformata(glsl, ", ivec3(%d, %d)", psInst->iUAddrOffset, psInst->iVAddrOffset);
            bcatcstr(glsl, ")");
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2D:
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_A);
            if (hasOffset && psBinding->eDimension == REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY)
                bformata(glsl, ", %d", psInst->iUAddrOffset);
            if (hasOffset && psBinding->eDimension == REFLECT_RESOURCE_DIMENSION_TEXTURE2D)
                bformata(glsl, ", ivec3(%d, %d)", psInst->iUAddrOffset, psInst->iVAddrOffset);
            bcatcstr(glsl, ")");
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            ASSERT(psInst->eOpcode == OPCODE_LD_MS);
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[3], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bcatcstr(glsl, ")");
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            ASSERT(psInst->eOpcode == OPCODE_LD_MS);
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[3], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bcatcstr(glsl, ")");
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBE:
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        case REFLECT_RESOURCE_DIMENSION_BUFFEREX:
        default:
        {
            // Not possible in either HLSL or GLSL
            ASSERT(0);
            break;
        }
    }

    TranslateOperandSwizzleWithMask(psContext, &psInst->asOperands[2], psInst->asOperands[0].GetAccessMask(), 0);
    AddAssignPrologue(numParenthesis);
}

//Makes sure the texture coordinate swizzle is appropriate for the texture type.
//i.e. vecX for X-dimension texture.
//Currently supports floating point coord only, so not used for texelFetch.
void ToGLSL::TranslateTexCoord(
    const RESOURCE_DIMENSION eResDim,
    Operand* psTexCoordOperand)
{
    uint32_t flags = TO_AUTO_BITCAST_TO_FLOAT;
    uint32_t opMask = OPERAND_4_COMPONENT_MASK_ALL;

    switch (eResDim)
    {
        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            //Vec1 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2D:
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            //Vec2 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
            flags |= TO_AUTO_EXPAND_TO_VEC2;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBE:
        case RESOURCE_DIMENSION_TEXTURE3D:
        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            //Vec3 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
            flags |= TO_AUTO_EXPAND_TO_VEC3;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            flags |= TO_AUTO_EXPAND_TO_VEC4;
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }

    //FIXME detect when integer coords are needed.
    TranslateOperand(psTexCoordOperand, flags, opMask);
}

void ToGLSL::GetResInfoData(Instruction* psInst, int index, int destElem)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    const RESINFO_RETURN_TYPE eResInfoReturnType = psInst->eResInfoReturnType;
    bool isUAV = (psInst->asOperands[2].eType == OPERAND_TYPE_UNORDERED_ACCESS_VIEW);
    bool isMS = psInst->eResDim == RESOURCE_DIMENSION_TEXTURE2DMS || psInst->eResDim == RESOURCE_DIMENSION_TEXTURE2DMSARRAY;

    std::string texName = ResourceName(psContext, isUAV ? RGROUP_UAV : RGROUP_TEXTURE, psInst->asOperands[2].ui32RegisterNumber, 0);

    // On Vulkan wrap the tex name with the sampler constructor
    if (psContext->IsVulkan() && !isUAV)
    {
        std::string vulkanSamplerName = GetVulkanDummySamplerName();

        const RESOURCE_DIMENSION eResDim = psContext->psShader->aeResourceDims[psInst->asOperands[2].ui32RegisterNumber];
        std::string smpType = GetSamplerType(psContext, eResDim, psInst->asOperands[2].ui32RegisterNumber);
        std::ostringstream oss;
        oss << smpType;
        oss << "(" << texName << ", " << vulkanSamplerName << ")";
        texName = oss.str();
    }

    psContext->AddIndentation();
    AddOpAssignToDestWithMask(&psInst->asOperands[0], eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT ? SVT_UINT : SVT_FLOAT, 1, psInst->ui32PreciseMask, &numParenthesis, 1 << destElem);

    //[width, height, depth or array size, total-mip-count]
    if (index < 3)
    {
        int dim = GetNumTextureDimensions(psInst->eResDim);
        bcatcstr(glsl, "(");
        if (dim < (index + 1))
        {
            bcatcstr(glsl, eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT ? (HaveUnsignedTypes(psContext->psShader->eTargetLanguage) ? "uint(0)" : "0") : "0.0");    // Old ES3.0 Adrenos treat 0u as const int.
        }
        else
        {
            if (eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT)
            {
                if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                    bformata(glsl, "uvec%d(", dim);
                else
                    bformata(glsl, "ivec%d(", dim);
            }
            else if (eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_RCPFLOAT)
                bformata(glsl, "vec%d(1.0) / vec%d(", dim, dim);
            else
                bformata(glsl, "vec%d(", dim);

            if (isUAV)
                bcatcstr(glsl, "imageSize(");
            else
                bcatcstr(glsl, "textureSize(");

            bcatcstr(glsl, texName.c_str());

            if (!isUAV && !isMS)
            {
                bcatcstr(glsl, ", ");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER);
            }
            bcatcstr(glsl, "))");

            switch (index)
            {
                case 0:
                    bcatcstr(glsl, ".x");
                    break;
                case 1:
                    bcatcstr(glsl, ".y");
                    break;
                case 2:
                    bcatcstr(glsl, ".z");
                    break;
            }
        }

        bcatcstr(glsl, ")");
    }
    else
    {
        ASSERT(!isUAV);
        if (eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT)
        {
            if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                bcatcstr(glsl, "uint(");
            else
                bcatcstr(glsl, "int(");
        }
        else
            bcatcstr(glsl, "float(");
        bcatcstr(glsl, "textureQueryLevels(");
        bcatcstr(glsl, texName.c_str());
        bcatcstr(glsl, "))");
    }
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::TranslateTextureSample(Instruction* psInst,
    uint32_t ui32Flags)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    int hasParamOffset = (ui32Flags & TEXSMP_FLAG_PARAMOFFSET) ? 1 : 0;

    Operand* psDest = &psInst->asOperands[0];
    Operand* psDestAddr = &psInst->asOperands[1];
    Operand* psSrcOff = (ui32Flags & TEXSMP_FLAG_PARAMOFFSET) ? &psInst->asOperands[2] : 0;
    Operand* psSrcTex = &psInst->asOperands[2 + hasParamOffset];
    Operand* psSrcSamp = &psInst->asOperands[3 + hasParamOffset];
    Operand* psSrcRef = (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) ? &psInst->asOperands[4 + hasParamOffset] : 0;
    Operand* psSrcLOD = (ui32Flags & TEXSMP_FLAG_LOD) ? &psInst->asOperands[4] : 0;
    Operand* psSrcDx = (ui32Flags & TEXSMP_FLAG_GRAD) ? &psInst->asOperands[4] : 0;
    Operand* psSrcDy = (ui32Flags & TEXSMP_FLAG_GRAD) ? &psInst->asOperands[5] : 0;
    Operand* psSrcBias = (ui32Flags & TEXSMP_FLAG_BIAS) ? &psInst->asOperands[4] : 0;

    const char* funcName = "texture";
    const char* offset = "";
    const char* depthCmpCoordType = "";
    const char* gradSwizzle = "";
    const char* ext = "";

    uint32_t ui32NumOffsets = 0;

    const RESOURCE_DIMENSION eResDim = psContext->psShader->aeResourceDims[psSrcTex->ui32RegisterNumber];
    const int iHaveOverloadedTexFuncs = HaveOverloadedTextureFuncs(psContext->psShader->eTargetLanguage);
    const int useCombinedTextureSamplers = (psContext->flags & HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS) ? 1 : 0;

    if (psInst->bAddressOffset)
    {
        offset = "Offset";
    }
    if (psContext->IsSwitch() && psInst->eOpcode == OPCODE_GATHER4_PO)
    {
        // it seems that other GLSLCore compilers accept textureGather(sampler2D sampler, vec2 texCoord, ivec2 texelOffset, int component) with the "texelOffset" parameter,
        // however this is not in the GLSL spec, and Switch's GLSLc compiler requires to use the textureGatherOffset version of the function
        offset = "Offset";
    }

    switch (eResDim)
    {
        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            depthCmpCoordType = "vec2";
            gradSwizzle = ".x";
            ui32NumOffsets = 1;
            if (!iHaveOverloadedTexFuncs)
            {
                funcName = "texture1D";
                if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE)
                {
                    funcName = "shadow1D";
                }
            }
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            depthCmpCoordType = "vec3";
            gradSwizzle = ".xy";
            ui32NumOffsets = 2;
            if (!iHaveOverloadedTexFuncs)
            {
                funcName = "texture2D";
                if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE)
                {
                    funcName = "shadow2D";
                }
            }
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            depthCmpCoordType = "vec4";
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            if (!iHaveOverloadedTexFuncs)
            {
                funcName = "textureCube";
            }
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            depthCmpCoordType = "vec4";
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            if (!iHaveOverloadedTexFuncs)
            {
                funcName = "texture3D";
            }
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            depthCmpCoordType = "vec3";
            gradSwizzle = ".x";
            ui32NumOffsets = 1;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            depthCmpCoordType = "vec4";
            gradSwizzle = ".xy";
            ui32NumOffsets = 2;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }

    if (ui32Flags & TEXSMP_FLAG_GATHER)
        funcName = "textureGather";

    uint32_t uniqueNameCounter = 0;

    // In GLSL, for every texture sampling func overload, except for cubemap arrays, the
    // depth compare reference value is given as the last component of the texture coord vector.
    // Cubemap array sampling as well as all the gather funcs have a separate parameter for it.
    // HLSL always provides the reference as a separate param.
    //
    // Here we create a temp texcoord var with the reference value embedded
    if ((ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) &&
        (eResDim != RESOURCE_DIMENSION_TEXTURECUBEARRAY && !(ui32Flags & TEXSMP_FLAG_GATHER)))
    {
        uniqueNameCounter = psContext->psShader->asPhases[psContext->currentPhase].m_NextTexCoordTemp++;
        psContext->AddIndentation();
        // Create a temp variable for the coordinate as Adrenos hate nonstandard swizzles in the texcoords
        bformata(glsl, "%s txVec%d = ", depthCmpCoordType, uniqueNameCounter);
        bformata(glsl, "%s(", depthCmpCoordType);
        TranslateTexCoord(eResDim, psDestAddr);
        bcatcstr(glsl, ",");
        // Last component is the reference
        TranslateOperand(psSrcRef, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ");\n");
    }

    SHADER_VARIABLE_TYPE dataType = psContext->psShader->sInfo.GetTextureDataType(psSrcTex->ui32RegisterNumber);
    psContext->AddIndentation();
    AddAssignToDest(psDest, dataType, psSrcTex->GetNumSwizzleElements(), psInst->ui32PreciseMask, &numParenthesis);

    // GLSL doesn't have textureLod() for 2d shadow samplers, we'll have to use grad instead. In that case assume LOD 0.
    bool needsLodWorkaround = (eResDim == RESOURCE_DIMENSION_TEXTURE2DARRAY) && (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE);
    const bool needsLodWorkaroundES2 = (psContext->psShader->eTargetLanguage == LANG_ES_100 && psContext->psShader->eShaderType == PIXEL_SHADER && (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE));

    // Workaround for switch for OPCODE_SAMPLE_C_LZ, in particular sampler2dArrayShadow.SampleCmpLevelZero().
    // textureGrad() with shadow samplers is not implemented in HW on switch so the behavior is emulated using shuffles and 4 texture fetches.
    // The code generated is very heavy.
    // Workaround: use standard texture fetch, shadows are currently not mipmapped, so that should work for now.
    if (needsLodWorkaround && psContext->IsSwitch() && ui32Flags == (TEXSMP_FLAG_DEPTHCOMPARE | TEXSMP_FLAG_FIRSTLOD))
    {
        needsLodWorkaround = false;
        ui32Flags &= ~TEXSMP_FLAG_FIRSTLOD;
    }

    if (needsLodWorkaround)
    {
        bformata(glsl, "%sGrad%s(", funcName, offset);
    }
    else
    {
        if (psContext->psShader->eTargetLanguage == LANG_ES_100 &&
            psContext->psShader->eShaderType == PIXEL_SHADER &&
            ui32Flags & (TEXSMP_FLAG_LOD | TEXSMP_FLAG_FIRSTLOD | TEXSMP_FLAG_GRAD))
            ext = "EXT";

        if (ui32Flags & (TEXSMP_FLAG_LOD | TEXSMP_FLAG_FIRSTLOD) && !needsLodWorkaroundES2)
            bformata(glsl, "%sLod%s%s(", funcName, ext, offset);
        else if (ui32Flags & TEXSMP_FLAG_GRAD)
            bformata(glsl, "%sGrad%s%s(", funcName, ext, offset);
        else
            bformata(glsl, "%s%s%s(", funcName, ext, offset);
    }

    if (psContext->IsVulkan())
    {
        // Build the sampler name here
        std::string samplerType = GetSamplerType(psContext, eResDim, psSrcTex->ui32RegisterNumber);
        const ResourceBinding *pSmpRes = NULL;
        psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_SAMPLER, psSrcSamp->ui32RegisterNumber, &pSmpRes);

        if (pSmpRes->m_SamplerMode == D3D10_SB_SAMPLER_MODE_COMPARISON)
            samplerType.append("Shadow");
        std::string texName = ResourceName(psContext, RGROUP_TEXTURE, psSrcTex->ui32RegisterNumber, 0);
        std::string smpName = ResourceName(psContext, RGROUP_SAMPLER, psSrcSamp->ui32RegisterNumber, 0);
        bformata(glsl, "%s(%s, %s)", samplerType.c_str(), texName.c_str(), smpName.c_str());
    }
    else
    {
        // Sampler name
        if (!useCombinedTextureSamplers)
            ResourceName(glsl, psContext, RGROUP_TEXTURE, psSrcTex->ui32RegisterNumber, ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE);
        else
            bcatcstr(glsl, TextureSamplerName(&psContext->psShader->sInfo, psSrcTex->ui32RegisterNumber, psSrcSamp->ui32RegisterNumber, ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE).c_str());
    }
    bcatcstr(glsl, ", ");

    // Texture coordinates, either from previously constructed temp
    // or straight from the psDestAddr operand
    if ((ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) &&
        (eResDim != RESOURCE_DIMENSION_TEXTURECUBEARRAY && !(ui32Flags & TEXSMP_FLAG_GATHER)))
        bformata(glsl, "txVec%d", uniqueNameCounter);
    else
        TranslateTexCoord(eResDim, psDestAddr);

    // If depth compare reference was not embedded to texcoord
    // then insert it here as a separate param
    if ((ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) &&
        (eResDim == RESOURCE_DIMENSION_TEXTURECUBEARRAY || (ui32Flags & TEXSMP_FLAG_GATHER)))
    {
        bcatcstr(glsl, ", ");
        TranslateOperand(psSrcRef, TO_AUTO_BITCAST_TO_FLOAT);
    }

    // Add LOD/grad parameters based on the flags
    if (needsLodWorkaround)
    {
        bcatcstr(glsl, ", vec2(0.0, 0.0), vec2(0.0, 0.0)");
    }
    else if (ui32Flags & TEXSMP_FLAG_LOD)
    {
        if (!needsLodWorkaroundES2)
        {
            bcatcstr(glsl, ", ");
            TranslateOperand(psSrcLOD, TO_AUTO_BITCAST_TO_FLOAT);
            if (psContext->psShader->ui32MajorVersion < 4)
            {
                bcatcstr(glsl, ".w");
            }
        }
    }
    else if (ui32Flags & TEXSMP_FLAG_FIRSTLOD)
    {
        if (!needsLodWorkaroundES2)
            bcatcstr(glsl, ", 0.0");
    }
    else if (ui32Flags & TEXSMP_FLAG_GRAD)
    {
        bcatcstr(glsl, ", vec4(");
        TranslateOperand(psSrcDx, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ")");
        bcatcstr(glsl, gradSwizzle);
        bcatcstr(glsl, ", vec4(");
        TranslateOperand(psSrcDy, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ")");
        bcatcstr(glsl, gradSwizzle);
    }

    // Add offset param
    if (psInst->bAddressOffset)
    {
        if (ui32NumOffsets == 1)
        {
            bformata(glsl, ", %d",
                psInst->iUAddrOffset);
        }
        else if (ui32NumOffsets == 2)
        {
            bformata(glsl, ", ivec2(%d, %d)",
                psInst->iUAddrOffset,
                psInst->iVAddrOffset);
        }
        else if (ui32NumOffsets == 3)
        {
            bformata(glsl, ", ivec3(%d, %d, %d)",
                psInst->iUAddrOffset,
                psInst->iVAddrOffset,
                psInst->iWAddrOffset);
        }
    }
    // HLSL gather has a variant with separate offset operand
    else if (ui32Flags & TEXSMP_FLAG_PARAMOFFSET)
    {
        uint32_t mask = OPERAND_4_COMPONENT_MASK_X;
        if (ui32NumOffsets > 1)
            mask |= OPERAND_4_COMPONENT_MASK_Y;
        if (ui32NumOffsets > 2)
            mask |= OPERAND_4_COMPONENT_MASK_Z;

        bcatcstr(glsl, ",");
        TranslateOperand(psSrcOff, TO_FLAG_INTEGER, mask);
    }

    // Add bias if present
    if (ui32Flags & TEXSMP_FLAG_BIAS)
    {
        bcatcstr(glsl, ", ");
        TranslateOperand(psSrcBias, TO_AUTO_BITCAST_TO_FLOAT);
    }

    // Add texture gather component selection if needed
    if ((ui32Flags & TEXSMP_FLAG_GATHER) && psSrcSamp->GetNumSwizzleElements() > 0)
    {
        ASSERT(psSrcSamp->GetNumSwizzleElements() == 1);
        if (psSrcSamp->aui32Swizzle[0] != OPERAND_4_COMPONENT_X)
        {
            if (!(ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE))
            {
                bformata(glsl, ", %d", psSrcSamp->aui32Swizzle[0]);
            }
            else
            {
                // Component selection not supported with depth compare gather
            }
        }
    }

    bcatcstr(glsl, ")");

    if (!(ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) || (ui32Flags & TEXSMP_FLAG_GATHER))
    {
        // iWriteMaskEnabled is forced off during DecodeOperand because swizzle on sampler uniforms
        // does not make sense. But need to re-enable to correctly swizzle this particular instruction.
        psSrcTex->iWriteMaskEnabled = 1;
        TranslateOperandSwizzleWithMask(psContext, psSrcTex, psDest->GetAccessMask(), 0);
    }
    AddAssignPrologue(numParenthesis);
}

const char* swizzleString[] = { ".x", ".y", ".z", ".w" };

// Handle cases where vector components are accessed with dynamic index ([] notation).
// A bit ugly hack because compiled HLSL uses byte offsets to access data in structs => we are converting
// the offset back to vector component index in runtime => calculating stuff back and forth.
// TODO: Would be better to eliminate the offset calculation ops and use indexes straight on. Could be tricky though...
void ToGLSL::TranslateDynamicComponentSelection(const ShaderVarType* psVarType, const Operand* psByteAddr, uint32_t offset, uint32_t mask)
{
    bstring glsl = *psContext->currentGLSLString;
    ASSERT(psVarType->Class == SVC_VECTOR);

    bcatcstr(glsl, "["); // Access vector component with [] notation
    if (offset > 0)
        bcatcstr(glsl, "(");

    if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
    {
        // The var containing byte address to the requested element
        TranslateOperand(psByteAddr, TO_FLAG_UNSIGNED_INTEGER, mask);

        if (offset > 0)// If the vector is part of a struct, there is an extra offset in our byte address
            bformata(glsl, " - %du)", offset); // Subtract that first

        bcatcstr(glsl, " >> 0x2u"); // Convert byte offset to index: div by four
        bcatcstr(glsl, "]");
    }
    else
    {
        // The var containing byte address to the requested element
        TranslateOperand(psByteAddr, TO_FLAG_INTEGER, mask);

        if (offset > 0)// If the vector is part of a struct, there is an extra offset in our byte address
            bformata(glsl, " - %d)", offset); // Subtract that first

        bcatcstr(glsl, " >> 0x2"); // Convert byte offset to index: div by four
        bcatcstr(glsl, "]");
    }
}

void ToGLSL::TranslateShaderStorageStore(Instruction* psInst)
{
    bstring glsl = *psContext->currentGLSLString;
    int component;
    int srcComponent = 0;

    Operand* psDest = 0;
    Operand* psDestAddr = 0;
    Operand* psDestByteOff = 0;
    Operand* psSrc = 0;

    switch (psInst->eOpcode)
    {
        case OPCODE_STORE_STRUCTURED:
            psDest = &psInst->asOperands[0];
            psDestAddr = &psInst->asOperands[1];
            psDestByteOff = &psInst->asOperands[2];
            psSrc = &psInst->asOperands[3];
            break;
        case OPCODE_STORE_RAW:
            psDest = &psInst->asOperands[0];
            psDestByteOff = &psInst->asOperands[1];
            psSrc = &psInst->asOperands[2];
            break;
        default:
            ASSERT(0);
            break;
    }

    uint32_t dstOffFlag = TO_FLAG_UNSIGNED_INTEGER;
    SHADER_VARIABLE_TYPE dstOffType = psDestByteOff->GetDataType(psContext);
    if (!HaveUnsignedTypes(psContext->psShader->eTargetLanguage) || dstOffType == SVT_INT || dstOffType == SVT_INT16 || dstOffType == SVT_INT12)
        dstOffFlag = TO_FLAG_INTEGER;

    for (component = 0; component < 4; component++)
    {
        ASSERT(psInst->asOperands[0].eSelMode == OPERAND_4_COMPONENT_MASK_MODE);
        if (psInst->asOperands[0].ui32CompMask & (1 << component))
        {
            psContext->AddIndentation();

            TranslateOperand(psDest, TO_FLAG_DESTINATION | TO_FLAG_NAME_ONLY);

            if (psDest->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
                bcatcstr(glsl, "_buf");

            if (psDestAddr)
            {
                bcatcstr(glsl, "[");
                TranslateOperand(psDestAddr, TO_FLAG_INTEGER | TO_FLAG_UNSIGNED_INTEGER);
                bcatcstr(glsl, "].value");
            }

            bcatcstr(glsl, "[(");
            TranslateOperand(psDestByteOff, dstOffFlag);
            bcatcstr(glsl, " >> 2");
            if (dstOffFlag == TO_FLAG_UNSIGNED_INTEGER)
                bcatcstr(glsl, "u");
            bcatcstr(glsl, ")");

            if (component != 0)
            {
                bformata(glsl, " + %d", component);
                if (dstOffFlag == TO_FLAG_UNSIGNED_INTEGER)
                    bcatcstr(glsl, "u");
            }

            bcatcstr(glsl, "]");

            uint32_t srcFlag = TO_FLAG_UNSIGNED_INTEGER;
            if (DeclareRWStructuredBufferTemplateTypeAsInteger(psContext, psDest) &&
                psDest->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY) // group shared is uint
                srcFlag = TO_FLAG_INTEGER;

            bcatcstr(glsl, " = ");
            if (psSrc->GetNumSwizzleElements() > 1)
                TranslateOperand(psSrc, srcFlag, 1 << (srcComponent++));
            else
                TranslateOperand(psSrc, srcFlag, OPERAND_4_COMPONENT_MASK_X);

            bcatcstr(glsl, ";\n");
        }
    }
}

void ToGLSL::TranslateShaderStorageLoad(Instruction* psInst)
{
    bstring glsl = *psContext->currentGLSLString;
    int component;
    Operand* psDest = 0;
    Operand* psSrcAddr = 0;
    Operand* psSrcByteOff = 0;
    Operand* psSrc = 0;

    switch (psInst->eOpcode)
    {
        case OPCODE_LD_STRUCTURED:
            psDest = &psInst->asOperands[0];
            psSrcAddr = &psInst->asOperands[1];
            psSrcByteOff = &psInst->asOperands[2];
            psSrc = &psInst->asOperands[3];
            break;
        case OPCODE_LD_RAW:
            psDest = &psInst->asOperands[0];
            psSrcByteOff = &psInst->asOperands[1];
            psSrc = &psInst->asOperands[2];
            break;
        default:
            ASSERT(0);
            break;
    }

    uint32_t destCount = psDest->GetNumSwizzleElements();
    uint32_t destMask = psDest->GetAccessMask();

    int numParenthesis = 0;
    int firstItemAdded = 0;
    SHADER_VARIABLE_TYPE destDataType = psDest->GetDataType(psContext);
    uint32_t srcOffFlag = TO_FLAG_UNSIGNED_INTEGER;
    SHADER_VARIABLE_TYPE srcOffType = psSrcByteOff->GetDataType(psContext);
    if (!HaveUnsignedTypes(psContext->psShader->eTargetLanguage) || srcOffType == SVT_INT || srcOffType == SVT_INT16 || srcOffType == SVT_INT12)
        srcOffFlag = TO_FLAG_INTEGER;

    psContext->AddIndentation();
    AddAssignToDest(psDest, destDataType, destCount, psInst->ui32PreciseMask, &numParenthesis); //TODO check this out?
    if (destCount > 1 || destDataType == SVT_FLOAT16)
    {
        bformata(glsl, "%s(", GetConstructorForTypeGLSL(psContext, destDataType, destCount, false));
        numParenthesis++;
    }
    for (component = 0; component < 4; component++)
    {
        int addedBitcast = 0;
        if (!(destMask & (1 << component)))
            continue;

        if (firstItemAdded)
            bcatcstr(glsl, ", ");
        else
            firstItemAdded = 1;

        // always uint array atm
        if (destDataType == SVT_FLOAT)
        {
            if (HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
                bcatcstr(glsl, "uintBitsToFloat(");
            else
                bcatcstr(glsl, "float(");
            addedBitcast = 1;
        }
        else if (destDataType == SVT_INT || destDataType == SVT_INT16 || destDataType == SVT_INT12)
        {
            bcatcstr(glsl, "int(");
            addedBitcast = 1;
        }

        TranslateOperand(psSrc, TO_FLAG_NAME_ONLY);

        if (psSrc->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
            bcatcstr(glsl, "_buf");

        if (psSrcAddr)
        {
            bcatcstr(glsl, "[");
            TranslateOperand(psSrcAddr, TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_INTEGER);
            bcatcstr(glsl, "].value");
        }
        bcatcstr(glsl, "[(");
        TranslateOperand(psSrcByteOff, srcOffFlag);
        bcatcstr(glsl, " >> 2");
        if (srcOffFlag == TO_FLAG_UNSIGNED_INTEGER)
            bcatcstr(glsl, "u");

        bformata(glsl, ") + %d", psSrc->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE ? psSrc->aui32Swizzle[component] : component);
        if (srcOffFlag == TO_FLAG_UNSIGNED_INTEGER)
            bcatcstr(glsl, "u");

        bcatcstr(glsl, "]");

        if (addedBitcast)
            bcatcstr(glsl, ")");
    }
    AddAssignPrologue(numParenthesis);
}

void ToGLSL::TranslateAtomicMemOp(Instruction* psInst)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    uint32_t ui32DstDataTypeFlag = TO_FLAG_DESTINATION | TO_FLAG_NAME_ONLY;
    uint32_t ui32DataTypeFlag = TO_FLAG_INTEGER;
    const char* func = "";
    Operand* dest = 0;
    Operand* previousValue = 0;
    Operand* destAddr = 0;
    Operand* src = 0;
    Operand* compare = 0;
    int texDim = 0;
    bool isUint = true;

    switch (psInst->eOpcode)
    {
        case OPCODE_IMM_ATOMIC_IADD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_IADD\n");
            }
            func = "Add";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_IADD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_IADD\n");
            }
            func = "Add";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_AND:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_AND\n");
            }
            func = "And";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_AND:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_AND\n");
            }
            func = "And";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_OR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_OR\n");
            }
            func = "Or";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_OR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_OR\n");
            }
            func = "Or";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_XOR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_XOR\n");
            }
            func = "Xor";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_XOR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_XOR\n");
            }
            func = "Xor";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }

        case OPCODE_IMM_ATOMIC_EXCH:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_EXCH\n");
            }
            func = "Exchange";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_IMM_ATOMIC_CMP_EXCH:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_CMP_EXC\n");
            }
            func = "CompSwap";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            compare = &psInst->asOperands[3];
            src = &psInst->asOperands[4];
            break;
        }
        case OPCODE_ATOMIC_CMP_STORE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_CMP_STORE\n");
            }
            func = "CompSwap";
            previousValue = 0;
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            compare = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_IMM_ATOMIC_UMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_UMIN\n");
            }
            func = "Min";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_UMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_UMIN\n");
            }
            func = "Min";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_IMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_IMIN\n");
            }
            func = "Min";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_IMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_IMIN\n");
            }
            func = "Min";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_UMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_UMAX\n");
            }
            func = "Max";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_UMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_UMAX\n");
            }
            func = "Max";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        case OPCODE_IMM_ATOMIC_IMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_IMAX\n");
            }
            func = "Max";
            previousValue = &psInst->asOperands[0];
            dest = &psInst->asOperands[1];
            destAddr = &psInst->asOperands[2];
            src = &psInst->asOperands[3];
            break;
        }
        case OPCODE_ATOMIC_IMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ATOMIC_IMAX\n");
            }
            func = "Max";
            dest = &psInst->asOperands[0];
            destAddr = &psInst->asOperands[1];
            src = &psInst->asOperands[2];
            break;
        }
        default:
            ASSERT(0);
            break;
    }

    psContext->AddIndentation();

    if (dest->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
    {
        const ResourceBinding* psBinding = 0;
        psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, dest->ui32RegisterNumber, &psBinding);

        if (psBinding->eType == RTYPE_UAV_RWTYPED)
        {
            isUint = (psBinding->ui32ReturnType == RETURN_TYPE_UINT);

            // Find out if it's texture and of what dimension
            switch (psBinding->eDimension)
            {
                case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
                case REFLECT_RESOURCE_DIMENSION_BUFFER:
                    texDim = 1;
                    break;
                case REFLECT_RESOURCE_DIMENSION_TEXTURECUBE:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2D:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS:
                    texDim = 2;
                    break;
                case REFLECT_RESOURCE_DIMENSION_TEXTURE3D:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                    texDim = 3;
                    break;
                default:
                    ASSERT(0);
                    break;
            }
        }
        else if (psBinding->eType == RTYPE_UAV_RWSTRUCTURED)
        {
            if (DeclareRWStructuredBufferTemplateTypeAsInteger(psContext, dest))
            {
                isUint = false;
                ui32DstDataTypeFlag |= TO_FLAG_INTEGER;
            }
        }
    }

    if (isUint && HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
        ui32DataTypeFlag = TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_BITCAST_TO_UINT;
    else
        ui32DataTypeFlag = TO_FLAG_INTEGER | TO_AUTO_BITCAST_TO_INT;

    if (previousValue)
        AddAssignToDest(previousValue, isUint ? SVT_UINT : SVT_INT, 1, psInst->ui32PreciseMask, &numParenthesis);

    if (texDim > 0)
        bcatcstr(glsl, "imageAtomic");
    else
        bcatcstr(glsl, "atomic");

    bcatcstr(glsl, func);
    bcatcstr(glsl, "(");

    TranslateOperand(dest, ui32DstDataTypeFlag);

    if (texDim > 0)
    {
        bcatcstr(glsl, ", ");
        unsigned int compMask = OPERAND_4_COMPONENT_MASK_X;
        if (texDim >= 2)
            compMask |= OPERAND_4_COMPONENT_MASK_Y;
        if (texDim == 3)
            compMask |= OPERAND_4_COMPONENT_MASK_Z;

        TranslateOperand(destAddr, TO_FLAG_INTEGER, compMask);
    }
    else
    {
        if (dest->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
            bcatcstr(glsl, "_buf");

        uint32_t destAddrFlag = TO_FLAG_UNSIGNED_INTEGER;
        SHADER_VARIABLE_TYPE destAddrType = destAddr->GetDataType(psContext);
        if (!HaveUnsignedTypes(psContext->psShader->eTargetLanguage) || destAddrType == SVT_INT || destAddrType == SVT_INT16 || destAddrType == SVT_INT12)
            destAddrFlag = TO_FLAG_INTEGER;

        bcatcstr(glsl, "[");
        TranslateOperand(destAddr, destAddrFlag, OPERAND_4_COMPONENT_MASK_X);

        // Structured buf if we have both x & y swizzles. Raw buf has only x -> no .value[]
        if (destAddr->GetNumSwizzleElements(OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y) == 2)
        {
            bcatcstr(glsl, "]");

            bcatcstr(glsl, ".value[");
            TranslateOperand(destAddr, destAddrFlag, OPERAND_4_COMPONENT_MASK_Y);
        }

        bcatcstr(glsl, " >> 2");//bytes to floats
        if (destAddrFlag == TO_FLAG_UNSIGNED_INTEGER)
            bcatcstr(glsl, "u");

        bcatcstr(glsl, "]");
    }

    bcatcstr(glsl, ", ");

    if (compare)
    {
        TranslateOperand(compare, ui32DataTypeFlag);
        bcatcstr(glsl, ", ");
    }

    TranslateOperand(src, ui32DataTypeFlag);
    bcatcstr(glsl, ")");
    if (previousValue)
    {
        AddAssignPrologue(numParenthesis);
    }
    else
        bcatcstr(glsl, ";\n");
}

void ToGLSL::TranslateConditional(
    Instruction* psInst,
    bstring glsl)
{
    const char* statement = "";
    if (psInst->eOpcode == OPCODE_BREAKC)
    {
        statement = "break";
    }
    else if (psInst->eOpcode == OPCODE_CONTINUEC)
    {
        statement = "continue";
    }
    else if (psInst->eOpcode == OPCODE_RETC) // FIXME! Need to spew out shader epilogue
    {
        statement = "return";
    }

    SHADER_VARIABLE_TYPE argType = psInst->asOperands[0].GetDataType(psContext);
    if (argType == SVT_BOOL)
    {
        bcatcstr(glsl, "if(");
        if (psInst->eBooleanTestType != INSTRUCTION_TEST_NONZERO)
            bcatcstr(glsl, "!");
        TranslateOperand(&psInst->asOperands[0], TO_FLAG_BOOL);
        if (psInst->eOpcode != OPCODE_IF)
        {
            bformata(glsl, "){%s;}\n", statement);
        }
        else
        {
            bcatcstr(glsl, "){\n");
        }
    }
    else
    {
        uint32_t oFlag = TO_FLAG_UNSIGNED_INTEGER;
        bool isInt = false;
        if (!HaveUnsignedTypes(psContext->psShader->eTargetLanguage) || argType == SVT_INT || argType == SVT_INT16 || argType == SVT_INT12)
        {
            isInt = true;
            oFlag = TO_FLAG_INTEGER;
        }

        bcatcstr(glsl, "if(");
        TranslateOperand(&psInst->asOperands[0], oFlag);

        if (psInst->eBooleanTestType == INSTRUCTION_TEST_ZERO)
            bcatcstr(glsl, " == ");
        else
            bcatcstr(glsl, " != ");

        bcatcstr(glsl, isInt ? "0)" : "uint(0))");  // Old ES3.0 Adrenos treat 0u as const int.

        if (psInst->eOpcode != OPCODE_IF)
        {
            bformata(glsl, " {%s;}\n", statement);
        }
        else
        {
            bcatcstr(glsl, " {\n");
        }
    }
}

void ToGLSL::HandleSwitchTransformation(Instruction* psInst, bstring glsl)
{
    SwitchConversion& current = m_SwitchStack.back();
    if (psInst->eOpcode != OPCODE_CASE && current.currentCaseOperands.size() > 0)
    {
        --psContext->indent;
        psContext->AddIndentation();
        bcatcstr(glsl, current.isFirstCase ? "if(" : "} else if(");
        current.isFirstCase = false;
        for (size_t i = 0; i < current.currentCaseOperands.size(); ++i)
        {
            if (i > 0)
                bcatcstr(glsl, " || ");

            bformata(glsl, "%s == %s", current.switchOperand->data, current.currentCaseOperands[i]->data);
            bdestroy(current.currentCaseOperands[i]);
        }
        bcatcstr(glsl, ") {\n");
        ++psContext->indent;
        current.currentCaseOperands.clear();
    }

    if (current.conditionalsInfo.size() > 0)
    {
        SwitchConversion::ConditionalInfo& conditional = current.conditionalsInfo.back();

        if (conditional.breakEncountered)
        {
            // We first check for BREAK ENDIF sequence.
            // If we see ELSE or CASE afterwards, we don't emit our own ELSE.
            if (psInst->eOpcode == OPCODE_ENDIF && !conditional.endifEncountered)
                conditional.endifEncountered = true;
            else
            {
                conditional.endifEncountered = false;
                conditional.breakEncountered = false;
                if (psInst->eOpcode == OPCODE_ELSE)
                {
                    if (conditional.breakCount > 0)
                        --conditional.breakCount;
                }
                else if (psInst->eOpcode != OPCODE_CASE)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "else {\n");
                    ++psContext->indent;
                }
            }
        }

        if (psInst->eOpcode == OPCODE_CASE || psInst->eOpcode == OPCODE_ENDSWITCH || (psInst->eOpcode == OPCODE_ENDIF && !conditional.endifEncountered))
        {
            for (int i = 0; i < conditional.breakCount; ++i)
            {
                --psContext->indent;
                psContext->AddIndentation();
                bcatcstr(glsl, "}\n");
            }
            current.conditionalsInfo.pop_back();
        }
    }
}

void ToGLSL::TranslateInstruction(Instruction* psInst, bool isEmbedded /* = false */)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    const bool isVulkan = ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0);
    const bool avoidAtomicCounter = ((psContext->flags & HLSLCC_FLAG_AVOID_SHADER_ATOMIC_COUNTERS) != 0);

    if (!isEmbedded)
    {
        if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
        {
            // Uncomment to print instruction IDs
            //psContext->AddIndentation();
            //bformata(glsl, "//Instruction %d\n", psInst->id);
            #if 0
            if (psInst->id == 73)
            {
                ASSERT(1); //Set breakpoint here to debug an instruction from its ID.
            }
            #endif
        }
        if (psInst->m_SkipTranslation)
            return;
    }

    if (!m_SwitchStack.empty())
        HandleSwitchTransformation(psInst, glsl);

    switch (psInst->eOpcode)
    {
        case OPCODE_FTOI:
        case OPCODE_FTOU:
        {
            uint32_t dstCount = psInst->asOperands[0].GetNumSwizzleElements();
            uint32_t srcCount = psInst->asOperands[1].GetNumSwizzleElements();
            SHADER_VARIABLE_TYPE castType = psInst->eOpcode == OPCODE_FTOU ? SVT_UINT : SVT_INT;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                if (psInst->eOpcode == OPCODE_FTOU)
                    bcatcstr(glsl, "//FTOU\n");
                else
                    bcatcstr(glsl, "//FTOI\n");
            }
            switch (psInst->asOperands[0].eMinPrecision)
            {
                case OPERAND_MIN_PRECISION_DEFAULT:
                    break;
                case OPERAND_MIN_PRECISION_SINT_16:
                    castType = SVT_INT16;
                    ASSERT(psInst->eOpcode == OPCODE_FTOI);
                    break;
                case OPERAND_MIN_PRECISION_UINT_16:
                    castType = SVT_UINT16;
                    ASSERT(psInst->eOpcode == OPCODE_FTOU);
                    break;
                default:
                    ASSERT(0);         // We'd be doing bitcasts into low/mediump ints, not good.
            }
            psContext->AddIndentation();

            AddAssignToDest(&psInst->asOperands[0], castType, srcCount, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, castType, dstCount, false));
            bcatcstr(glsl, "(");             // 1
            TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_FLOAT, psInst->asOperands[0].GetAccessMask());
            bcatcstr(glsl, ")");             // 1
            AddAssignPrologue(numParenthesis);
            break;
        }

        case OPCODE_MOV:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                if (!isEmbedded)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//MOV\n");
                }
            }
            if (!isEmbedded)
                psContext->AddIndentation();

            // UNITY SPECIFIC: you can check case 1158280
            // This looks like a hack because it is! There is a bug that is quite hard to reproduce.
            // When doing data analysis we assume that immediates are ints and hope it will be promoted later
            //   which is kinda fine unless there is an unfortunate combination happening:
            // We operate on 4-component registers - we need different components to be treated as float/int
            //   but we should not use float operations (as this will mark register as float)
            //   instead "float" components should be used for MOV and friends to other registers
            //   and they, in turn, should be used for float ops
            // In pseudocode it can look like this:
            //   var2.xy = var1.xy; <float magic with var2> var1.xy = var2.xy;  // not marked as float explicitly
            //   bool foo = var1.z | <...>                                      // marked as int
            // Now we have immediate that will be treated as int but NOT promoted because we think we have all ints
            //   var1.w = 1                                                     // var1 is marked int
            // What is important is that this temporary is marked as int by us but DX compiler treats it
            //   as "normal" float (and rightfully so) [or rather - we speak about cases where it does treat it as float]
            // It is also important that we speak about temps (otherwise we have explicit data type to use, so promotion works)
            //
            // At this point we have mov immediate to int temp (which should really be float temp)
            {
                Operand *pDst = &psInst->asOperands[0], *pSrc = &psInst->asOperands[1];
                if (pDst->GetDataType(psContext) == SVT_INT                                 // dst marked as int
                    && pDst->eType == OPERAND_TYPE_TEMP                                     // dst is temp
                    && pSrc->eType == OPERAND_TYPE_IMMEDIATE32                              // src is immediate
                    && psContext->psShader->psIntTempSizes[pDst->ui32RegisterNumber] == 0   // no temp register allocated
                )
                {
                    pDst->aeDataType[0] = pDst->aeDataType[1] = pDst->aeDataType[2] = pDst->aeDataType[3] = SVT_FLOAT;
                }
            }

            AddMOVBinaryOp(&psInst->asOperands[0], &psInst->asOperands[1], psInst->ui32PreciseMask, isEmbedded);
            break;
        }
        case OPCODE_ITOF://signed to float
        case OPCODE_UTOF://unsigned to float
        {
            SHADER_VARIABLE_TYPE castType = SVT_FLOAT;
            uint32_t dstCount = psInst->asOperands[0].GetNumSwizzleElements();
            uint32_t srcCount = psInst->asOperands[1].GetNumSwizzleElements();

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                if (psInst->eOpcode == OPCODE_ITOF)
                    bcatcstr(glsl, "//ITOF\n");
                else
                    bcatcstr(glsl, "//UTOF\n");
            }

            switch (psInst->asOperands[0].eMinPrecision)
            {
                case OPERAND_MIN_PRECISION_DEFAULT:
                    break;
                case OPERAND_MIN_PRECISION_FLOAT_2_8:
                    castType = SVT_FLOAT10;
                    break;
                case OPERAND_MIN_PRECISION_FLOAT_16:
                    castType = SVT_FLOAT16;
                    break;
                default:
                    ASSERT(0);          // We'd be doing bitcasts into low/mediump ints, not good.
            }

            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], castType, srcCount, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, castType, dstCount, false));
            bcatcstr(glsl, "(");              // 1
            TranslateOperand(&psInst->asOperands[1], psInst->eOpcode == OPCODE_UTOF ? TO_AUTO_BITCAST_TO_UINT : TO_AUTO_BITCAST_TO_INT, psInst->asOperands[0].GetAccessMask());
            bcatcstr(glsl, ")");              // 1
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_MAD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MAD\n");
            }
            CallTernaryOp("*", "+", psInst, 0, 1, 2, 3, TO_FLAG_NONE);
            break;
        }
        case OPCODE_IMAD:
        {
            uint32_t ui32Flags = TO_FLAG_INTEGER;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMAD\n");
            }
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_UINT)
            {
                ui32Flags = TO_FLAG_UNSIGNED_INTEGER;
            }

            CallTernaryOp("*", "+", psInst, 0, 1, 2, 3, ui32Flags);
            break;
        }
        case OPCODE_DADD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DADD\n");
            }
            CallBinaryOp("+", psInst, 0, 1, 2, SVT_DOUBLE);
            break;
        }
        case OPCODE_IADD:
        {
            SHADER_VARIABLE_TYPE eType = SVT_INT;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                if (!isEmbedded)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//IADD\n");
                }
            }
            //Is this a signed or unsigned add?
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_UINT)
            {
                eType = SVT_UINT;
            }
            CallBinaryOp("+", psInst, 0, 1, 2, eType, isEmbedded);
            break;
        }
        case OPCODE_ADD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ADD\n");
            }
            CallBinaryOp("+", psInst, 0, 1, 2, SVT_FLOAT);
            break;
        }
        case OPCODE_OR:
        {
            /*Todo: vector version */
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//OR\n");
            }
            uint32_t dstSwizCount = psInst->asOperands[0].GetNumSwizzleElements();
            uint32_t destMask = psInst->asOperands[0].GetAccessMask();
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL)
            {
                if (dstSwizCount == 1)
                {
                    uint32_t destMask = psInst->asOperands[0].GetAccessMask();

                    int needsParenthesis = 0;
                    psContext->AddIndentation();
                    AddAssignToDest(&psInst->asOperands[0], SVT_BOOL, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, &needsParenthesis);
                    TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, " || ");
                    TranslateOperand(&psInst->asOperands[2], TO_FLAG_BOOL, destMask);
                    AddAssignPrologue(needsParenthesis);
                }
                else
                {
                    Operand* pDest = &psInst->asOperands[0];
                    const SHADER_VARIABLE_TYPE eDestType = pDest->GetDataType(psContext);
                    const std::string tempName = "hlslcc_orTemp";

                    psContext->AddIndentation();
                    bcatcstr(glsl, "{\n");
                    ++psContext->indent;
                    psContext->AddIndentation();

                    int numComponents = (pDest->eType == OPERAND_TYPE_TEMP) ?
                        psContext->psShader->GetTempComponentCount(eDestType, pDest->ui32RegisterNumber) :
                        pDest->iNumComponents;
                    const char* constructorStr = HLSLcc::GetConstructorForType(psContext, eDestType, numComponents, false);
                    bformata(glsl, "%s %s = ", constructorStr, tempName.c_str());
                    TranslateOperand(pDest, TO_FLAG_NAME_ONLY);
                    bformata(glsl, ";\n");

                    const_cast<Operand *>(pDest)->specialName.assign(tempName);

                    int srcElem = -1;
                    for (uint32_t destElem = 0; destElem < 4; ++destElem)
                    {
                        int numParenthesis = 0;
                        srcElem++;
                        if (pDest->eSelMode == OPERAND_4_COMPONENT_MASK_MODE && pDest->ui32CompMask != 0 && !(pDest->ui32CompMask & (1 << destElem)))
                            continue;

                        psContext->AddIndentation();
                        AddOpAssignToDestWithMask(pDest, eDestType, 1, psInst->ui32PreciseMask, &numParenthesis, 1 << destElem);
                        TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, 1 << srcElem);
                        bcatcstr(glsl, " || ");
                        TranslateOperand(&psInst->asOperands[2], SVTTypeToFlag(eDestType), 1 << srcElem);
                        AddAssignPrologue(numParenthesis);
                    }

                    const_cast<Operand *>(pDest)->specialName.clear();

                    psContext->AddIndentation();
                    TranslateOperand(glsl, pDest, TO_FLAG_NAME_ONLY);
                    bformata(glsl, " = %s;\n", tempName.c_str());

                    --psContext->indent;
                    psContext->AddIndentation();
                    bcatcstr(glsl, "}\n");
                }
            }
            else
                CallBinaryOp("|", psInst, 0, 1, 2, SVT_UINT);
            break;
        }
        case OPCODE_AND:
        {
            SHADER_VARIABLE_TYPE eA = psInst->asOperands[1].GetDataType(psContext);
            SHADER_VARIABLE_TYPE eB = psInst->asOperands[2].GetDataType(psContext);
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//AND\n");
            }
            uint32_t destMask = psInst->asOperands[0].GetAccessMask();
            const uint32_t dstSwizCount = psInst->asOperands[0].GetNumSwizzleElements();
            SHADER_VARIABLE_TYPE eDataType = psInst->asOperands[0].GetDataType(psContext);
            uint32_t ui32Flags = SVTTypeToFlag(eDataType);
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL)
            {
                if (dstSwizCount == 1)
                {
                    int needsParenthesis = 0;
                    psContext->AddIndentation();
                    AddAssignToDest(&psInst->asOperands[0], SVT_BOOL, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, &needsParenthesis);
                    TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, " && ");
                    TranslateOperand(&psInst->asOperands[2], TO_FLAG_BOOL, destMask);
                    AddAssignPrologue(needsParenthesis);
                }
                else
                {
                    // Do component-wise and, glsl doesn't support && on bvecs
                    for (uint32_t k = 0; k < 4; k++)
                    {
                        if ((destMask & (1 << k)) == 0)
                            continue;

                        int needsParenthesis = 0;
                        psContext->AddIndentation();
                        // Override dest mask temporarily
                        psInst->asOperands[0].ui32CompMask = (1 << k);
                        ASSERT(psInst->asOperands[0].eSelMode == OPERAND_4_COMPONENT_MASK_MODE);
                        AddAssignToDest(&psInst->asOperands[0], SVT_BOOL, 1, psInst->ui32PreciseMask, &needsParenthesis);
                        TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, 1 << k);
                        bcatcstr(glsl, " && ");
                        TranslateOperand(&psInst->asOperands[2], TO_FLAG_BOOL, 1 << k);
                        AddAssignPrologue(needsParenthesis);
                    }
                    // Restore old mask
                    psInst->asOperands[0].ui32CompMask = destMask;
                }
            }
            else if ((eA == SVT_BOOL || eB == SVT_BOOL) && !(eA == SVT_BOOL && eB == SVT_BOOL))
            {
                int boolOp = eA == SVT_BOOL ? 1 : 2;
                int otherOp = eA == SVT_BOOL ? 2 : 1;
                int needsParenthesis = 0;
                uint32_t i;
                psContext->AddIndentation();

                if (dstSwizCount == 1)
                {
                    AddAssignToDest(&psInst->asOperands[0], eDataType, dstSwizCount, psInst->ui32PreciseMask, &needsParenthesis);
                    TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, " ? ");
                    TranslateOperand(&psInst->asOperands[otherOp], ui32Flags, destMask);
                    bcatcstr(glsl, " : ");

                    bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, eDataType, dstSwizCount, false));
                    bcatcstr(glsl, "(");
                    switch (eDataType)
                    {
                        case SVT_FLOAT:
                        case SVT_FLOAT10:
                        case SVT_FLOAT16:
                        case SVT_DOUBLE:
                            bcatcstr(glsl, "0.0");
                            break;
                        default:
                            bcatcstr(glsl, "0");
                    }
                    bcatcstr(glsl, ")");
                }
                else if (eDataType == SVT_FLOAT)
                {
                    // We cannot use mix(), because it propagates NaN from both endpoints, which
                    // is not correct if the AND was used to implement a branch that guards against NaN.
                    // Instead, do either a single ?: select if the bool is a scalar, or component-wise
                    // ?: selects if the bool is a vector.
                    if (psInst->asOperands[boolOp].IsSwizzleReplicated())
                    {
                        // Bool is effectively a scalar, we can just do a single ?:

                        // The swizzle is either xxxx, yyyy, zzzz, or wwww. In each case,
                        // the max component will give us the 1-based index.
                        int boolChannel = psInst->asOperands[boolOp].GetMaxComponent();

                        AddAssignToDest(&psInst->asOperands[0], eDataType, dstSwizCount, psInst->ui32PreciseMask, &needsParenthesis);
                        TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, 1 << (boolChannel - 1));
                        bcatcstr(glsl, " ? ");
                        TranslateOperand(&psInst->asOperands[otherOp], ui32Flags, destMask);
                        bcatcstr(glsl, " : ");
                        bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, eDataType, dstSwizCount, false));
                        bcatcstr(glsl, "(");
                        for (i = 0; i < dstSwizCount; i++)
                        {
                            if (i > 0)
                                bcatcstr(glsl, ", ");
                            bcatcstr(glsl, "0.0");
                        }
                        bcatcstr(glsl, ")");
                    }
                    else
                    {
                        bool needsIndent = false;

                        // Do component-wise select
                        for (uint32_t k = 0; k < 4; k++)
                        {
                            if ((destMask & (1 << k)) == 0)
                                continue;

                            int needsParenthesis = 0;
                            if (needsIndent)
                                psContext->AddIndentation();

                            // Override dest mask temporarily
                            psInst->asOperands[0].ui32CompMask = (1 << k);
                            ASSERT(psInst->asOperands[0].eSelMode == OPERAND_4_COMPONENT_MASK_MODE);
                            AddAssignToDest(&psInst->asOperands[0], eDataType, 1, psInst->ui32PreciseMask, &needsParenthesis);
                            TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, 1 << k);
                            bcatcstr(glsl, " ? ");
                            TranslateOperand(&psInst->asOperands[otherOp], ui32Flags, 1 << k);
                            bcatcstr(glsl, " : 0.0");
                            AddAssignPrologue(needsParenthesis);

                            needsIndent = true;
                        }

                        // Restore old mask
                        psInst->asOperands[0].ui32CompMask = destMask;
                    }
                }
                else
                {
                    AddAssignToDest(&psInst->asOperands[0], SVT_UINT, dstSwizCount, psInst->ui32PreciseMask, &needsParenthesis);
                    const bool haveNativeBitwiseOps = HaveNativeBitwiseOps(psContext->psShader->eTargetLanguage);
                    if (!haveNativeBitwiseOps)
                    {
                        UseExtraFunctionDependency("op_and");
                        bcatcstr(glsl, "op_and");
                    }
                    bcatcstr(glsl, "(");
                    bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, SVT_UINT, dstSwizCount, false));
                    bcatcstr(glsl, "(");
                    TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, destMask);
                    if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                        bcatcstr(glsl, ") * 0xFFFFFFFFu");
                    else
                        bcatcstr(glsl, ") * -1");   // GLSL ES 2 spec: high precision ints are guaranteed to have a range of at least (-2^16, 2^16)

                    if (haveNativeBitwiseOps)
                        bcatcstr(glsl, ") & ");
                    else
                        bcatcstr(glsl, ", ");

                    TranslateOperand(&psInst->asOperands[otherOp], TO_FLAG_UNSIGNED_INTEGER, destMask);
                    if (!haveNativeBitwiseOps)
                        bcatcstr(glsl, ")");
                }

                AddAssignPrologue(needsParenthesis);
            }
            else
            {
                CallBinaryOp("&", psInst, 0, 1, 2, SVT_UINT);
            }

            break;
        }
        case OPCODE_GE:
        {
            /*
                dest = vec4(greaterThanEqual(vec4(srcA), vec4(srcB));
                Caveat: The result is a boolean but HLSL asm returns 0xFFFFFFFF/0x0 instead.
                */
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//GE\n");
            }
            AddComparison(psInst, CMP_GE, TO_FLAG_NONE);
            break;
        }
        case OPCODE_MUL:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MUL\n");
            }
            CallBinaryOp("*", psInst, 0, 1, 2, SVT_FLOAT);
            break;
        }
        case OPCODE_IMUL:
        {
            SHADER_VARIABLE_TYPE eType = SVT_INT;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMUL\n");
            }
            if (psInst->asOperands[1].GetDataType(psContext) == SVT_UINT)
            {
                eType = SVT_UINT;
            }

            ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_NULL);

            CallBinaryOp("*", psInst, 1, 2, 3, eType);
            break;
        }
        case OPCODE_UDIV:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//UDIV\n");
            }
            //destQuotient, destRemainder, src0, src1

            // There are cases where destQuotient is the same variable as src0 or src1. If that happens,
            // we need to compute "%" before the "/" in order to avoid src0 or src1 being overriden first.
            if ((psInst->asOperands[0].eType != psInst->asOperands[2].eType || psInst->asOperands[0].ui32RegisterNumber != psInst->asOperands[2].ui32RegisterNumber)
                && (psInst->asOperands[0].eType != psInst->asOperands[3].eType || psInst->asOperands[0].ui32RegisterNumber != psInst->asOperands[3].ui32RegisterNumber))
            {
                CallBinaryOp("/", psInst, 0, 2, 3, SVT_UINT);
                CallBinaryOp("%", psInst, 1, 2, 3, SVT_UINT);
            }
            else
            {
                CallBinaryOp("%", psInst, 1, 2, 3, SVT_UINT);
                CallBinaryOp("/", psInst, 0, 2, 3, SVT_UINT);
            }
            break;
        }
        case OPCODE_DIV:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DIV\n");
            }
            CallBinaryOp("/", psInst, 0, 1, 2, SVT_FLOAT);
            break;
        }
        case OPCODE_SINCOS:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SINCOS\n");
            }
            // Need careful ordering if src == dest[0], as then the cos() will be reading from wrong value
            if (psInst->asOperands[0].eType == psInst->asOperands[2].eType &&
                psInst->asOperands[0].ui32RegisterNumber == psInst->asOperands[2].ui32RegisterNumber)
            {
                // sin() result overwrites source, do cos() first.
                // The case where both write the src shouldn't really happen anyway.
                if (psInst->asOperands[1].eType != OPERAND_TYPE_NULL)
                {
                    CallHelper1("cos", psInst, 1, 2, 1);
                }

                if (psInst->asOperands[0].eType != OPERAND_TYPE_NULL)
                {
                    CallHelper1(
                        "sin", psInst, 0, 2, 1);
                }
            }
            else
            {
                if (psInst->asOperands[0].eType != OPERAND_TYPE_NULL)
                {
                    CallHelper1("sin", psInst, 0, 2, 1);
                }

                if (psInst->asOperands[1].eType != OPERAND_TYPE_NULL)
                {
                    CallHelper1("cos", psInst, 1, 2, 1);
                }
            }
            break;
        }

        case OPCODE_DP2:
        {
            int numParenthesis = 0;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DP2\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 1, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, "dot(");
            TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[2], TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_DP3:
        {
            int numParenthesis = 0;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DP3\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 1, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, "dot(");
            TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[2], TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_DP4:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DP4\n");
            }
            CallHelper2("dot", psInst, 0, 1, 2, 0);
            break;
        }
        case OPCODE_INE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//INE\n");
            }
            AddComparison(psInst, CMP_NE, TO_FLAG_INTEGER);
            break;
        }
        case OPCODE_NE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//NE\n");
            }
            AddComparison(psInst, CMP_NE, TO_FLAG_NONE);
            break;
        }
        case OPCODE_IGE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IGE\n");
            }
            AddComparison(psInst, CMP_GE, TO_FLAG_INTEGER);
            break;
        }
        case OPCODE_ILT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ILT\n");
            }
            AddComparison(psInst, CMP_LT, TO_FLAG_INTEGER);
            break;
        }
        case OPCODE_LT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LT\n");
            }
            AddComparison(psInst, CMP_LT, TO_FLAG_NONE);
            break;
        }
        case OPCODE_IEQ:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IEQ\n");
            }
            AddComparison(psInst, CMP_EQ, TO_FLAG_INTEGER);
            break;
        }
        case OPCODE_ULT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ULT\n");
            }
            AddComparison(psInst, CMP_LT, TO_FLAG_UNSIGNED_INTEGER);
            break;
        }
        case OPCODE_UGE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//UGE\n");
            }
            AddComparison(psInst, CMP_GE, TO_FLAG_UNSIGNED_INTEGER);
            break;
        }
        case OPCODE_MOVC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MOVC\n");
            }
            AddMOVCBinaryOp(&psInst->asOperands[0], &psInst->asOperands[1], &psInst->asOperands[2], &psInst->asOperands[3], psInst->ui32PreciseMask);
            break;
        }
        case OPCODE_SWAPC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SWAPC\n");
            }
            // TODO needs temps!!
            AddMOVCBinaryOp(&psInst->asOperands[0], &psInst->asOperands[2], &psInst->asOperands[4], &psInst->asOperands[3], psInst->ui32PreciseMask);
            AddMOVCBinaryOp(&psInst->asOperands[1], &psInst->asOperands[2], &psInst->asOperands[3], &psInst->asOperands[4], psInst->ui32PreciseMask);
            break;
        }

        case OPCODE_LOG:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LOG\n");
            }
            CallHelper1("log2", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_RSQ:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RSQ\n");
            }
            CallHelper1("inversesqrt", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_EXP:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EXP\n");
            }
            CallHelper1("exp2", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_SQRT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SQRT\n");
            }
            CallHelper1("sqrt", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_ROUND_PI:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ROUND_PI\n");
            }
            CallHelper1("ceil", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_ROUND_NI:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ROUND_NI\n");
            }
            CallHelper1("floor", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_ROUND_Z:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ROUND_Z\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                UseExtraFunctionDependency("trunc");

            CallHelper1("trunc", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_ROUND_NE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ROUND_NE\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                UseExtraFunctionDependency("roundEven");

            CallHelper1("roundEven", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_FRC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//FRC\n");
            }
            CallHelper1("fract", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_IMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMAX\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                CallHelper2("max", psInst, 0, 1, 2, 1);
            else
                CallHelper2Int("max", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_UMAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//UMAX\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                CallHelper2("max", psInst, 0, 1, 2, 1);
            else
                CallHelper2UInt("max", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_MAX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MAX\n");
            }
            CallHelper2("max", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_IMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMIN\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                CallHelper2("min", psInst, 0, 1, 2, 1);
            else
                CallHelper2Int("min", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_UMIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//UMIN\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                CallHelper2("min", psInst, 0, 1, 2, 1);
            else
                CallHelper2UInt("min", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_MIN:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MIN\n");
            }
            CallHelper2("min", psInst, 0, 1, 2, 1);
            break;
        }
        case OPCODE_GATHER4:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//GATHER4\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_GATHER);
            break;
        }
        case OPCODE_GATHER4_PO_C:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//GATHER4_PO_C\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_GATHER | TEXSMP_FLAG_PARAMOFFSET | TEXSMP_FLAG_DEPTHCOMPARE);
            break;
        }
        case OPCODE_GATHER4_PO:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//GATHER4_PO\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_GATHER | TEXSMP_FLAG_PARAMOFFSET);
            break;
        }
        case OPCODE_GATHER4_C:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//GATHER4_C\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_GATHER | TEXSMP_FLAG_DEPTHCOMPARE);
            break;
        }
        case OPCODE_SAMPLE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_NONE);
            break;
        }
        case OPCODE_SAMPLE_L:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_L\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_LOD);
            break;
        }
        case OPCODE_SAMPLE_C:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_C\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_DEPTHCOMPARE);
            break;
        }
        case OPCODE_SAMPLE_C_LZ:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_C_LZ\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_DEPTHCOMPARE | TEXSMP_FLAG_FIRSTLOD);
            break;
        }
        case OPCODE_SAMPLE_D:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_D\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_GRAD);
            break;
        }
        case OPCODE_SAMPLE_B:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_B\n");
            }
            TranslateTextureSample(psInst, TEXSMP_FLAG_BIAS);
            break;
        }
        case OPCODE_RET:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RET\n");
            }
            if (psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode)
            {
                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- Post shader code ---\n");
                }

                bconcat(glsl, psContext->psShader->asPhases[psContext->currentPhase].postShaderCode);

                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- End post shader code ---\n");
                }
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "return;\n");
            break;
        }
        case OPCODE_INTERFACE_CALL:
        {
            const char* name;
            ShaderVar* psVar;
            uint32_t varFound;

            uint32_t funcPointer;
            uint32_t funcBodyIndex;
            uint32_t ui32NumBodiesPerTable;

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//INTERFACE_CALL\n");
            }

            ASSERT(psInst->asOperands[0].eIndexRep[0] == OPERAND_INDEX_IMMEDIATE32);

            funcPointer = psInst->asOperands[0].aui32ArraySizes[0];
            funcBodyIndex = psInst->ui32FuncIndexWithinInterface;

            ui32NumBodiesPerTable = psContext->psShader->funcPointer[funcPointer].ui32NumBodiesPerTable;

            varFound = psContext->psShader->sInfo.GetInterfaceVarFromOffset(funcPointer, &psVar);

            ASSERT(varFound);

            name = &psVar->name[0];

            psContext->AddIndentation();
            bcatcstr(glsl, name);
            TranslateOperandIndexMAD(&psInst->asOperands[0], 1, ui32NumBodiesPerTable, funcBodyIndex);
            //bformata(glsl, "[%d]", funcBodyIndex);
            bcatcstr(glsl, "();\n");
            break;
        }
        case OPCODE_LABEL:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LABEL\n");
            }
            --psContext->indent;
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");              //Closing brace ends the previous function.
            psContext->AddIndentation();

            bcatcstr(glsl, "subroutine(SubroutineType)\n");
            bcatcstr(glsl, "void ");
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, "(){\n");
            ++psContext->indent;
            break;
        }
        case OPCODE_COUNTBITS:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//COUNTBITS\n");
            }
            psContext->AddIndentation();

            // in glsl bitCount decl is genIType bitCount(genIType), so it is important that input/output types agree
            // enter assembly: when writing swizzle encoding we use 0 to say "source from x"
            // now, say, we generate code o.xy = bitcount(i.xy)
            //   output gets component mask 1,1,0,0 (note that we use bit 1<<i to indicate if we should use component)
            //   input gets swizzle mask (2 bits sectios) 0,2,0,0; which is interpreted as xyxx
            // from the assembly standpoint this is fine, but, as indicated above, in glsl code we should be more careful
            // NOTE: this is pretty general issue for functions like that - i am not sure if we can/should fix it generally
            // anyway here we are. It *seems* that doing bitCount(i.<..>).<..> will still collapse everything into
            //   bitCount(i.<..>) [well, tweaking swizzle, sure]
            // what does that mean is that we can safely take output component count to determine "proper" type
            // note that hlsl compiler already checked that things can work out, so it should be fine doing this magic
            const Operand* dst = &psInst->asOperands[0];
            const int dstCompCount = dst->eSelMode == OPERAND_4_COMPONENT_MASK_MODE ? dst->ui32CompMask : OPERAND_4_COMPONENT_MASK_ALL;

            TranslateOperand(dst, TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = bitCount(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, dstCompCount);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_FIRSTBIT_HI:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//FIRSTBIT_HI\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = findMSB(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_FIRSTBIT_LO:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//FIRSTBIT_LO\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = findLSB(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_FIRSTBIT_SHI: //signed high
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//FIRSTBIT_SHI\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = findMSB(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_BFREV:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BFREV\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = bitfieldReverse(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_BFI:
        {
            uint32_t destMask = psInst->asOperands[0].GetAccessMask();
            uint32_t numelements_width = psInst->asOperands[1].GetNumSwizzleElements();
            uint32_t numelements_offset = psInst->asOperands[2].GetNumSwizzleElements();
            uint32_t numelements_dest = psInst->asOperands[0].GetNumSwizzleElements();
            uint32_t numoverall_elements = std::min(std::min(numelements_width, numelements_offset), numelements_dest);
            uint32_t i, j, k;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BFI\n");
            }
            if (psContext->psShader->eTargetLanguage == LANG_ES_300)
                UseExtraFunctionDependency("int_bitfieldInsert");

            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_INT, numoverall_elements, psInst->ui32PreciseMask, &numParenthesis);

            if (numoverall_elements == 1)
                bformata(glsl, "int(");
            else
                bformata(glsl, "ivec%d(", numoverall_elements);

            k = 0;
            for (i = 0; i < 4; ++i)
            {
                if ((destMask & (1 << i)) == 0)
                    continue;

                k++;
                if (psContext->psShader->eTargetLanguage == LANG_ES_300)
                    bcatcstr(glsl, "int_bitfieldInsert(");
                else
                    bcatcstr(glsl, "bitfieldInsert(");

                for (j = 4; j >= 1; --j)
                {
                    TranslateOperand(&psInst->asOperands[j], TO_FLAG_INTEGER, 1 << i);
                    if (j != 1)
                        bcatcstr(glsl, ",");
                }

                bcatcstr(glsl, ") ");
                if (k != numoverall_elements)
                    bcatcstr(glsl, ", ");
            }
            bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_CUT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//CUT\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "EndPrimitive();\n");
            break;
        }
        case OPCODE_EMIT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EMIT\n");
            }
            if (psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode)
            {
                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- Post shader code ---\n");
                }

                bconcat(glsl, psContext->psShader->asPhases[psContext->currentPhase].postShaderCode);

                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- End post shader code ---\n");
                }
            }

            psContext->AddIndentation();
            bcatcstr(glsl, "EmitVertex();\n");
            break;
        }
        case OPCODE_EMITTHENCUT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EMITTHENCUT\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "EmitVertex();\n");
            psContext->AddIndentation();
            bcatcstr(glsl, "EndPrimitive();\n");
            break;
        }

        case OPCODE_CUT_STREAM:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//CUT_STREAM\n");
            }
            psContext->AddIndentation();
            ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_STREAM);
            if (psContext->psShader->eTargetLanguage < LANG_400 || psInst->asOperands[0].ui32RegisterNumber == 0)
            {
                // ES geom shaders only support one stream.
                bcatcstr(glsl, "EndPrimitive();\n");
            }
            else
            {
                bcatcstr(glsl, "EndStreamPrimitive(");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
                bcatcstr(glsl, ");\n");
            }

            break;
        }
        case OPCODE_EMIT_STREAM:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EMIT_STREAM\n");
            }
            if (psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode)
            {
                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- Post shader code ---\n");
                }

                bconcat(glsl, psContext->psShader->asPhases[psContext->currentPhase].postShaderCode);

                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    psContext->AddIndentation();
                    bcatcstr(glsl, "//--- End post shader code ---\n");
                }
            }

            psContext->AddIndentation();

            ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_STREAM);
            if (psContext->psShader->eTargetLanguage < LANG_400 || psInst->asOperands[0].ui32RegisterNumber == 0)
            {
                // ES geom shaders only support one stream.
                bcatcstr(glsl, "EmitVertex();\n");
            }
            else
            {
                bcatcstr(glsl, "EmitStreamVertex(");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
                bcatcstr(glsl, ");\n");
            }
            break;
        }
        case OPCODE_EMITTHENCUT_STREAM:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EMITTHENCUT\n");
            }
            ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_STREAM);
            if (psContext->psShader->eTargetLanguage < LANG_400 || psInst->asOperands[0].ui32RegisterNumber == 0)
            {
                // ES geom shaders only support one stream.
                bcatcstr(glsl, "EmitVertex();\n");
                psContext->AddIndentation();
                bcatcstr(glsl, "EndPrimitive();\n");
            }
            else
            {
                bcatcstr(glsl, "EmitStreamVertex(");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
                bcatcstr(glsl, ");\n");
                psContext->AddIndentation();
                bcatcstr(glsl, "EndStreamPrimitive(");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
                bcatcstr(glsl, ");\n");
            }
            break;
        }
        case OPCODE_REP:
        {
            if (!m_SwitchStack.empty())
                ++m_SwitchStack.back().isInLoop;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//REP\n");
            }
            //Need to handle nesting.
            //Max of 4 for rep - 'Flow Control Limitations' http://msdn.microsoft.com/en-us/library/windows/desktop/bb219848(v=vs.85).aspx

            psContext->AddIndentation();
            bcatcstr(glsl, "RepCounter = ");
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bcatcstr(glsl, ";\n");

            psContext->AddIndentation();
            bcatcstr(glsl, "while(RepCounter!=0){\n");
            ++psContext->indent;
            break;
        }
        case OPCODE_ENDREP:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ENDREP\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "RepCounter--;\n");

            --psContext->indent;

            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
            if (!m_SwitchStack.empty())
                --m_SwitchStack.back().isInLoop;
            break;
        }
        case OPCODE_LOOP:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LOOP\n");
            }
            if (!m_SwitchStack.empty())
                ++m_SwitchStack.back().isInLoop;
            psContext->AddIndentation();

            if (psInst->ui32NumOperands == 2)
            {
                //DX9 version
                ASSERT(psInst->asOperands[0].eType == OPERAND_TYPE_SPECIAL_LOOPCOUNTER);
                bcatcstr(glsl, "for(");
                bcatcstr(glsl, "LoopCounter = ");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
                bcatcstr(glsl, ".y, ZeroBasedCounter = 0;");
                bcatcstr(glsl, "ZeroBasedCounter < ");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
                bcatcstr(glsl, ".x;");

                bcatcstr(glsl, "LoopCounter += ");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
                bcatcstr(glsl, ".z, ZeroBasedCounter++){\n");
                ++psContext->indent;
            }
            else if (psInst->m_LoopInductors[1] != 0)
            {
                // Can emit as for
                uint32_t typeFlags = TO_FLAG_INTEGER;
                bcatcstr(glsl, "for(");
                if (psInst->m_LoopInductors[0] != 0)
                {
                    if (psInst->m_InductorRegister != 0)
                    {
                        // Do declaration here as well
                        switch (psInst->m_LoopInductors[0]->asOperands[0].GetDataType(psContext))
                        {
                            case SVT_INT:
                                bcatcstr(glsl, "int ");
                                break;
                            case SVT_UINT:
                                bcatcstr(glsl, "uint ");
                                typeFlags = TO_FLAG_UNSIGNED_INTEGER;
                                break;
                            default:
                                ASSERT(0);
                                break;
                        }
                    }
                    TranslateInstruction(psInst->m_LoopInductors[0], true);
                }
                bcatcstr(glsl, " ; ");
                bool negateCondition = psInst->m_LoopInductors[1]->eBooleanTestType
                    != psInst->m_LoopInductors[2]->eBooleanTestType;
                bool negateOrder = false;

                // Yet Another NVidia OSX shader compiler bug workaround (really nvidia, get your s#!t together):
                // For reasons unfathomable to us, this breaks SSAO effect on OSX (case 756028)
                // Broken: for(int ti_loop_1 = int(int(0xFFFFFFFCu)) ; 4 >= ti_loop_1 ; ti_loop_1++)
                // Works: for (int ti_loop_1 = int(int(0xFFFFFFFCu)); ti_loop_1 <= 4; ti_loop_1++)
                //
                // So, check if the first argument is an immediate value, and if so, switch the order or the operands
                // (and adjust condition)
                if (psInst->m_LoopInductors[1]->asOperands[1].eType == OPERAND_TYPE_IMMEDIATE32)
                    negateOrder = true;

                const char *cmpOp = "";
                switch (psInst->m_LoopInductors[1]->eOpcode)
                {
                    case OPCODE_IGE:
                        if (negateOrder)
                            cmpOp = negateCondition ? ">" : "<=";
                        else
                            cmpOp = negateCondition ? "<" : ">=";
                        break;
                    case OPCODE_ILT:
                        if (negateOrder)
                            cmpOp = negateCondition ? "<=" : ">";
                        else
                            cmpOp = negateCondition ? ">=" : "<";
                        break;
                    case OPCODE_IEQ:
                        // No need to change the comparison if negateOrder is true
                        cmpOp = negateCondition ? "!=" : "==";
                        if (psInst->m_LoopInductors[1]->asOperands[0].GetDataType(psContext) == SVT_UINT)
                            typeFlags = TO_FLAG_UNSIGNED_INTEGER;
                        break;
                    case OPCODE_INE:
                        // No need to change the comparison if negateOrder is true
                        cmpOp = negateCondition ? "==" : "!=";
                        if (psInst->m_LoopInductors[1]->asOperands[0].GetDataType(psContext) == SVT_UINT)
                            typeFlags = TO_FLAG_UNSIGNED_INTEGER;
                        break;
                    case OPCODE_UGE:
                        if (negateOrder)
                            cmpOp = negateCondition ? ">" : "<=";
                        else
                            cmpOp = negateCondition ? "<" : ">=";
                        typeFlags = TO_FLAG_UNSIGNED_INTEGER;
                        break;
                    case OPCODE_ULT:
                        if (negateOrder)
                            cmpOp = negateCondition ? "<=" : ">";
                        else
                            cmpOp = negateCondition ? ">=" : "<";
                        typeFlags = TO_FLAG_UNSIGNED_INTEGER;
                        break;

                    default:
                        ASSERT(0);
                }
                TranslateOperand(&psInst->m_LoopInductors[1]->asOperands[negateOrder ? 2 : 1], typeFlags);
                bcatcstr(glsl, cmpOp);
                TranslateOperand(&psInst->m_LoopInductors[1]->asOperands[negateOrder ? 1 : 2], typeFlags);

                bcatcstr(glsl, " ; ");
                // One more shortcut: translate IADD tX, tX, 1 to tX++
                if (HLSLcc::IsAddOneInstruction(psInst->m_LoopInductors[3]))
                {
                    TranslateOperand(&psInst->m_LoopInductors[3]->asOperands[0], TO_FLAG_DESTINATION);
                    bcatcstr(glsl, "++");
                }
                else
                    TranslateInstruction(psInst->m_LoopInductors[3], true);

                bcatcstr(glsl, ")\n");
                psContext->AddIndentation();
                bcatcstr(glsl, "{\n");
                ++psContext->indent;
            }
            else
            {
                if (psContext->psShader->eTargetLanguage == LANG_ES_100)
                {
                    bstring name;
                    name = bformat(HLSLCC_TEMP_PREFIX "i_while_true_%d", m_NumDeclaredWhileTrueLoops++);

                    // Workaround limitation with WebGL 1.0 GLSL, as we're expecting something to break the loop in any case
                    // Fragment shaders on some devices don't like too large integer constants (Adreno 3xx, for example)
                    int hardcoded_iteration_limit = (psContext->psShader->eShaderType == PIXEL_SHADER) ? 0x7FFF : 0x7FFFFFFF;

                    bformata(glsl, "for(int %s = 0 ; %s < 0x%X ; %s++){\n", name->data, name->data, hardcoded_iteration_limit, name->data);
                    bdestroy(name);
                }
                else
                {
                    bcatcstr(glsl, "while(true){\n");
                }
                ++psContext->indent;
            }
            break;
        }
        case OPCODE_ENDLOOP:
        {
            --psContext->indent;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ENDLOOP\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
            if (!m_SwitchStack.empty())
                --m_SwitchStack.back().isInLoop;
            break;
        }
        case OPCODE_BREAK:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BREAK\n");
            }
            if (m_SwitchStack.empty() || m_SwitchStack.back().isInLoop != 0)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "break;\n");
            }
            else
            {
                std::vector<SwitchConversion::ConditionalInfo>& conditionalsInfo = m_SwitchStack.back().conditionalsInfo;
                if (conditionalsInfo.size() > 0)
                {
                    conditionalsInfo.back().breakEncountered = true;
                    ++conditionalsInfo.back().breakCount;
                }
            }
            break;
        }
        case OPCODE_BREAKC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BREAKC\n");
            }
            psContext->AddIndentation();

            if (m_SwitchStack.empty() || m_SwitchStack.back().isInLoop != 0)
            {
                TranslateConditional(psInst, glsl);
            }
            else
            {
                // This way we won't emit a "break" when we're transforming a "switch" into if/else for ES2
                OPCODE_TYPE opcode = psInst->eOpcode;
                psInst->eOpcode = OPCODE_IF;
                TranslateConditional(psInst, glsl);
                psInst->eOpcode = opcode;
                std::vector<SwitchConversion::ConditionalInfo>& conditionalsInfo = m_SwitchStack.back().conditionalsInfo;
                conditionalsInfo.push_back(SwitchConversion::ConditionalInfo(1, true, true));
            }
            break;
        }
        case OPCODE_CONTINUEC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//CONTINUEC\n");
            }
            psContext->AddIndentation();

            TranslateConditional(psInst, glsl);
            break;
        }
        case OPCODE_IF:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IF\n");
            }
            psContext->AddIndentation();

            TranslateConditional(psInst, glsl);
            ++psContext->indent;

            if (!m_SwitchStack.empty() && m_SwitchStack.back().isInLoop == 0)
            {
                std::vector<SwitchConversion::ConditionalInfo>& conditionalsInfo = m_SwitchStack.back().conditionalsInfo;
                conditionalsInfo.push_back(SwitchConversion::ConditionalInfo(0));
            }

            break;
        }
        case OPCODE_RETC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RETC\n");
            }
            psContext->AddIndentation();

            TranslateConditional(psInst, glsl);
            break;
        }
        case OPCODE_ELSE:
        {
            --psContext->indent;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ELSE\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "} else {\n");
            psContext->indent++;

            if (!m_SwitchStack.empty() && m_SwitchStack.back().isInLoop == 0)
            {
                std::vector<SwitchConversion::ConditionalInfo>& conditionalsInfo = m_SwitchStack.back().conditionalsInfo;
                conditionalsInfo.push_back(SwitchConversion::ConditionalInfo(0));
            }
            break;
        }
        case OPCODE_ENDSWITCH:
        {
            const bool endsSwitch = m_SwitchStack.empty();
            --psContext->indent;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ENDSWITCH\n");
            }
            if (endsSwitch)
                --psContext->indent;
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
            if (!endsSwitch)
                m_SwitchStack.pop_back();
            break;
        }
        case OPCODE_ENDIF:
        {
            --psContext->indent;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ENDIF\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");

            if (!m_SwitchStack.empty() && m_SwitchStack.back().isInLoop == 0)
            {
                std::vector<SwitchConversion::ConditionalInfo>& conditionalsInfo = m_SwitchStack.back().conditionalsInfo;
                conditionalsInfo.pop_back();
            }
            break;
        }
        case OPCODE_CONTINUE:
        {
            psContext->AddIndentation();
            bcatcstr(glsl, "continue;\n");
            break;
        }
        case OPCODE_DEFAULT:
        {
            --psContext->indent;
            psContext->AddIndentation();
            if (m_SwitchStack.empty())
                bcatcstr(glsl, "default:\n");
            else
                bcatcstr(glsl, "} else {\n");
            ++psContext->indent;
            break;
        }
        case OPCODE_NOP:
        {
            break;
        }
        case OPCODE_SYNC:
        {
            const uint32_t ui32SyncFlags = psInst->ui32SyncFlags;

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SYNC\n");
            }

            if (ui32SyncFlags & SYNC_THREAD_GROUP_SHARED_MEMORY)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "memoryBarrierShared();\n");
            }
            if (ui32SyncFlags & (SYNC_UNORDERED_ACCESS_VIEW_MEMORY_GROUP | SYNC_UNORDERED_ACCESS_VIEW_MEMORY_GLOBAL))
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "memoryBarrier();\n");
            }
            if (ui32SyncFlags & SYNC_THREADS_IN_GROUP)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "barrier();\n");
            }
            break;
        }
        case OPCODE_SWITCH:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SWITCH\n");
            }
            if (psContext->psShader->eTargetLanguage != LANG_ES_100)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "switch(");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
                bcatcstr(glsl, "){\n");

                psContext->indent += 2;
            }
            else
            {
                // GLSL ES2 doesn't support switch, need to convert to if/else if/else
                SwitchConversion conversion;
                TranslateOperand(conversion.switchOperand, &psInst->asOperands[0], TO_FLAG_INTEGER);
                m_SwitchStack.push_back(conversion);
                ++psContext->indent;
            }
            break;
        }
        case OPCODE_CASE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//case\n");
            }
            if (m_SwitchStack.empty())
            {
                --psContext->indent;
                psContext->AddIndentation();

                bcatcstr(glsl, "case ");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
                bcatcstr(glsl, ":\n");

                ++psContext->indent;
            }
            else
            {
                bstring operand = bfromcstr("");
                TranslateOperand(operand, &psInst->asOperands[0], TO_FLAG_INTEGER);
                m_SwitchStack.back().currentCaseOperands.push_back(operand);
            }
            break;
        }
        case OPCODE_EQ:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EQ\n");
            }
            AddComparison(psInst, CMP_EQ, TO_FLAG_NONE);
            break;
        }
        case OPCODE_USHR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//USHR\n");
            }
            CallBinaryOp(">>", psInst, 0, 1, 2, SVT_UINT);
            break;
        }
        case OPCODE_ISHL:
        {
            SHADER_VARIABLE_TYPE eType = SVT_INT;

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ISHL\n");
            }

            if (psInst->asOperands[0].GetDataType(psContext) == SVT_UINT)
            {
                eType = SVT_UINT;
            }

            CallBinaryOp("<<", psInst, 0, 1, 2, eType);
            break;
        }
        case OPCODE_ISHR:
        {
            SHADER_VARIABLE_TYPE eType = SVT_INT;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//ISHR\n");
            }

            if (psInst->asOperands[0].GetDataType(psContext) == SVT_UINT)
            {
                eType = SVT_UINT;
            }

            CallBinaryOp(">>", psInst, 0, 1, 2, eType);
            break;
        }
        case OPCODE_LD:
        case OPCODE_LD_MS:
        {
            const ResourceBinding* psBinding = 0;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                if (psInst->eOpcode == OPCODE_LD)
                    bcatcstr(glsl, "//LD\n");
                else
                    bcatcstr(glsl, "//LD_MS\n");
            }

            psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, psInst->asOperands[2].ui32RegisterNumber, &psBinding);

            TranslateTexelFetch(psInst, psBinding, glsl);
            break;
        }
        case OPCODE_DISCARD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DISCARD\n");
            }
            psContext->AddIndentation();
            if (psContext->psShader->ui32MajorVersion <= 3)
            {
                bcatcstr(glsl, "if(any(lessThan((");
                TranslateOperand(&psInst->asOperands[0], TO_FLAG_NONE);

                if (psContext->psShader->ui32MajorVersion == 1)
                {
                    /* SM1.X only kills based on the rgb channels */
                    bcatcstr(glsl, ").xyz, vec3(0)))){discard;}\n");
                }
                else
                {
                    bcatcstr(glsl, "), vec4(0)))){discard;}\n");
                }
            }
            else if (psInst->eBooleanTestType == INSTRUCTION_TEST_ZERO)
            {
                const bool isBool = psInst->asOperands[0].GetDataType(psContext, SVT_INT) == SVT_BOOL;
                const bool forceNoBoolUpscale = psContext->psShader->eTargetLanguage >= LANG_ES_FIRST && psContext->psShader->eTargetLanguage <= LANG_ES_LAST;
                const bool useDirectTest = isBool && forceNoBoolUpscale;
                bcatcstr(glsl, "if(");
                bcatcstr(glsl, useDirectTest ? "!" : "(");
                TranslateOperand(&psInst->asOperands[0], useDirectTest ? TO_FLAG_BOOL : TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_ALL, forceNoBoolUpscale);
                if (!useDirectTest)
                    bcatcstr(glsl, ")==0");
                bcatcstr(glsl, "){discard;}\n");
            }
            else
            {
                ASSERT(psInst->eBooleanTestType == INSTRUCTION_TEST_NONZERO);
                const bool isBool = psInst->asOperands[0].GetDataType(psContext, SVT_INT) == SVT_BOOL;
                const bool forceNoBoolUpscale = psContext->psShader->eTargetLanguage >= LANG_ES_FIRST && psContext->psShader->eTargetLanguage <= LANG_ES_LAST;
                const bool useDirectTest = isBool && forceNoBoolUpscale;
                bcatcstr(glsl, "if(");
                if (!useDirectTest)
                    bcatcstr(glsl, "(");
                TranslateOperand(&psInst->asOperands[0], useDirectTest ? TO_FLAG_BOOL : TO_FLAG_INTEGER, OPERAND_4_COMPONENT_MASK_ALL, forceNoBoolUpscale);
                if (!useDirectTest)
                    bcatcstr(glsl, ")!=0");
                bcatcstr(glsl, "){discard;}\n");
            }
            break;
        }
        case OPCODE_LOD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LOD\n");
            }
            //LOD computes the following vector (ClampedLOD, NonClampedLOD, 0, 0)

            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 4, psInst->ui32PreciseMask, &numParenthesis);

            //If the core language does not have query-lod feature,
            //then the extension is used. The name of the function
            //changed between extension and core.
            if (HaveQueryLod(psContext->psShader->eTargetLanguage))
            {
                bcatcstr(glsl, "textureQueryLod(");
            }
            else
            {
                bcatcstr(glsl, "textureQueryLOD(");
            }

            TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
            bcatcstr(glsl, ",");
            TranslateTexCoord(
                psContext->psShader->aeResourceDims[psInst->asOperands[2].ui32RegisterNumber],
                &psInst->asOperands[1]);
            bcatcstr(glsl, ")");

            //The swizzle on srcResource allows the returned values to be swizzled arbitrarily before they are written to the destination.

            // iWriteMaskEnabled is forced off during DecodeOperand because swizzle on sampler uniforms
            // does not make sense. But need to re-enable to correctly swizzle this particular instruction.
            psInst->asOperands[2].iWriteMaskEnabled = 1;
            TranslateOperandSwizzleWithMask(psContext, &psInst->asOperands[2], psInst->asOperands[0].GetAccessMask(), 0);
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_EVAL_CENTROID:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EVAL_CENTROID\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtCentroid(");
            //interpolateAtCentroid accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_EVAL_SAMPLE_INDEX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EVAL_SAMPLE_INDEX\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtSample(");
            //interpolateAtSample accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[2], TO_FLAG_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_EVAL_SNAPPED:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//EVAL_SNAPPED\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtOffset(");
            //interpolateAtOffset accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[2], TO_FLAG_INTEGER);
            bcatcstr(glsl, ".xy);\n");
            break;
        }
        case OPCODE_LD_STRUCTURED:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LD_STRUCTURED\n");
            }
            TranslateShaderStorageLoad(psInst);
            break;
        }
        case OPCODE_LD_UAV_TYPED:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LD_UAV_TYPED\n");
            }
            Operand* psDest = &psInst->asOperands[0];
            Operand* psSrc = &psInst->asOperands[2];
            Operand* psSrcAddr = &psInst->asOperands[1];

            int srcCount = psSrc->GetNumSwizzleElements();
            int numParenthesis = 0;
            uint32_t compMask = 0;

            switch (psInst->eResDim)
            {
                case RESOURCE_DIMENSION_TEXTURE3D:
                case RESOURCE_DIMENSION_TEXTURE2DARRAY:
                case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                    compMask |= (1 << 2);
                case RESOURCE_DIMENSION_TEXTURECUBE:
                case RESOURCE_DIMENSION_TEXTURE1DARRAY:
                case RESOURCE_DIMENSION_TEXTURE2D:
                case RESOURCE_DIMENSION_TEXTURE2DMS:
                    compMask |= (1 << 1);
                case RESOURCE_DIMENSION_TEXTURE1D:
                case RESOURCE_DIMENSION_BUFFER:
                    compMask |= 1;
                    break;
                default:
                    ASSERT(0);
                    break;
            }

            SHADER_VARIABLE_TYPE srcDataType = SVT_FLOAT;
            const ResourceBinding* psBinding = 0;
            psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, psSrc->ui32RegisterNumber, &psBinding);
            switch (psBinding->ui32ReturnType)
            {
                case RETURN_TYPE_FLOAT:
                    srcDataType = SVT_FLOAT;
                    break;
                case RETURN_TYPE_SINT:
                    srcDataType = SVT_INT;
                    break;
                case RETURN_TYPE_UINT:
                    srcDataType = SVT_UINT;
                    break;
                case RETURN_TYPE_SNORM:
                case RETURN_TYPE_UNORM:
                    srcDataType = SVT_FLOAT;
                    break;
                default:
                    ASSERT(0);
                    // Suppress uninitialised variable warning
                    srcDataType = SVT_VOID;
                    break;
            }

            psContext->AddIndentation();
            AddAssignToDest(psDest, srcDataType, srcCount, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, "imageLoad(");
            TranslateOperand(psSrc, TO_FLAG_NAME_ONLY);
            bcatcstr(glsl, ", ");
            TranslateOperand(psSrcAddr, TO_FLAG_INTEGER, compMask);
            bcatcstr(glsl, ")");
            TranslateOperandSwizzleWithMask(psContext, psSrc, psDest->ui32CompMask, 0);
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_STORE_RAW:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//STORE_RAW\n");
            }
            TranslateShaderStorageStore(psInst);
            break;
        }
        case OPCODE_STORE_STRUCTURED:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//STORE_STRUCTURED\n");
            }
            TranslateShaderStorageStore(psInst);
            break;
        }

        case OPCODE_STORE_UAV_TYPED:
        {
            const ResourceBinding* psRes;
            int foundResource;
            uint32_t flags = TO_FLAG_INTEGER;
            uint32_t opMask = OPERAND_4_COMPONENT_MASK_ALL;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//STORE_UAV_TYPED\n");
            }
            psContext->AddIndentation();

            foundResource = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV,
                psInst->asOperands[0].ui32RegisterNumber,
                &psRes);

            ASSERT(foundResource);

            bcatcstr(glsl, "imageStore(");
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_NAME_ONLY);
            bcatcstr(glsl, ", ");

            switch (psRes->eDimension)
            {
                case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
                case REFLECT_RESOURCE_DIMENSION_BUFFER:
                    opMask = OPERAND_4_COMPONENT_MASK_X;
                    break;
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2D:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS:
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
                    flags |= TO_AUTO_EXPAND_TO_VEC2;
                    break;
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE3D:
                case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                case REFLECT_RESOURCE_DIMENSION_TEXTURECUBE:
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
                    flags |= TO_AUTO_EXPAND_TO_VEC3;
                    break;
                case REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                    flags |= TO_AUTO_EXPAND_TO_VEC4;
                    break;
                default:
                    ASSERT(0);
                    break;
            }

            TranslateOperand(&psInst->asOperands[1], flags, opMask);
            bcatcstr(glsl, ", ");
            TranslateOperand(&psInst->asOperands[2], ResourceReturnTypeToFlag(psRes->ui32ReturnType));
            bformata(glsl, ");\n");

            break;
        }
        case OPCODE_LD_RAW:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LD_RAW\n");
            }

            TranslateShaderStorageLoad(psInst);
            break;
        }

        case OPCODE_ATOMIC_AND:
        case OPCODE_ATOMIC_OR:
        case OPCODE_ATOMIC_XOR:
        case OPCODE_ATOMIC_CMP_STORE:
        case OPCODE_ATOMIC_IADD:
        case OPCODE_ATOMIC_IMAX:
        case OPCODE_ATOMIC_IMIN:
        case OPCODE_ATOMIC_UMAX:
        case OPCODE_ATOMIC_UMIN:
        case OPCODE_IMM_ATOMIC_IADD:
        case OPCODE_IMM_ATOMIC_AND:
        case OPCODE_IMM_ATOMIC_OR:
        case OPCODE_IMM_ATOMIC_XOR:
        case OPCODE_IMM_ATOMIC_EXCH:
        case OPCODE_IMM_ATOMIC_CMP_EXCH:
        case OPCODE_IMM_ATOMIC_IMAX:
        case OPCODE_IMM_ATOMIC_IMIN:
        case OPCODE_IMM_ATOMIC_UMAX:
        case OPCODE_IMM_ATOMIC_UMIN:
        {
            TranslateAtomicMemOp(psInst);
            break;
        }
        case OPCODE_UBFE:
        case OPCODE_IBFE:
        {
            int numParenthesis = 0;
            int i;
            uint32_t writeMask = psInst->asOperands[0].GetAccessMask();
            SHADER_VARIABLE_TYPE dataType = psInst->eOpcode == OPCODE_UBFE ? SVT_UINT : SVT_INT;
            uint32_t flags = psInst->eOpcode == OPCODE_UBFE ? TO_AUTO_BITCAST_TO_UINT : TO_AUTO_BITCAST_TO_INT;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                if (psInst->eOpcode == OPCODE_UBFE)
                    bcatcstr(glsl, "//OPCODE_UBFE\n");
                else
                    bcatcstr(glsl, "//OPCODE_IBFE\n");
            }
            // Need to open this up, GLSL bitfieldextract uses same offset and width for all components
            for (i = 0; i < 4; i++)
            {
                if ((writeMask & (1 << i)) == 0)
                    continue;
                psContext->AddIndentation();
                psInst->asOperands[0].ui32CompMask = (1 << i);
                psInst->asOperands[0].eSelMode = OPERAND_4_COMPONENT_MASK_MODE;
                AddAssignToDest(&psInst->asOperands[0], dataType, 1, psInst->ui32PreciseMask, &numParenthesis);

                bcatcstr(glsl, "bitfieldExtract(");
                TranslateOperand(&psInst->asOperands[3], flags, (1 << i));
                bcatcstr(glsl, ", ");
                TranslateOperand(&psInst->asOperands[2], TO_AUTO_BITCAST_TO_INT, (1 << i));
                bcatcstr(glsl, ", ");
                TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_INT, (1 << i));
                bcatcstr(glsl, ")");
                AddAssignPrologue(numParenthesis);
            }
            break;
        }
        case OPCODE_RCP:
        {
            const uint32_t destElemCount = psInst->asOperands[0].GetNumSwizzleElements();
            const uint32_t srcElemCount = psInst->asOperands[1].GetNumSwizzleElements();
            int numParenthesis = 0;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RCP\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, srcElemCount, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, SVT_FLOAT, destElemCount, false));
            bcatcstr(glsl, "(1.0) / ");
            bcatcstr(glsl, GetConstructorForTypeGLSL(psContext, SVT_FLOAT, destElemCount, false));
            bcatcstr(glsl, "(");
            numParenthesis++;
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE, psInst->asOperands[0].GetAccessMask());
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_F32TOF16:
        {
            uint32_t writeMask = psInst->asOperands[0].GetAccessMask();

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//F32TOF16\n");
            }

            for (int i = 0; i < 4; i++)
            {
                if ((writeMask & (1 << i)) == 0)
                    continue;
                psContext->AddIndentation();
                psInst->asOperands[0].ui32CompMask = (1 << i);
                psInst->asOperands[0].eSelMode = OPERAND_4_COMPONENT_MASK_MODE;
                AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, &numParenthesis);

                bcatcstr(glsl, "packHalf2x16(vec2(");
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE, (1 << i));
                bcatcstr(glsl, ", 0.0))");
                AddAssignPrologue(numParenthesis);
            }
            break;
        }
        case OPCODE_F16TOF32:
        {
            uint32_t writeMask = psInst->asOperands[0].GetAccessMask();

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//F16TOF32\n");
            }

            for (int i = 0; i < 4; i++)
            {
                if ((writeMask & (1 << i)) == 0)
                    continue;
                psContext->AddIndentation();
                psInst->asOperands[0].ui32CompMask = (1 << i);
                psInst->asOperands[0].eSelMode = OPERAND_4_COMPONENT_MASK_MODE;
                AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 1, psInst->ui32PreciseMask, &numParenthesis);

                bcatcstr(glsl, "unpackHalf2x16(");
                TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_UINT, (1 << i));
                bcatcstr(glsl, ").x");
                AddAssignPrologue(numParenthesis);
            }
            break;
        }
        case OPCODE_INEG:
        {
            int numParenthesis = 0;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//INEG\n");
            }
            //dest = 0 - src0
            psContext->AddIndentation();

            AddAssignToDest(&psInst->asOperands[0], SVT_INT, psInst->asOperands[1].GetNumSwizzleElements(), psInst->ui32PreciseMask, &numParenthesis);

            bcatcstr(glsl, "0 - ");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, psInst->asOperands[0].GetAccessMask());
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_DERIV_RTX_COARSE:
        case OPCODE_DERIV_RTX_FINE:
        case OPCODE_DERIV_RTX:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DERIV_RTX\n");
            }
            CallHelper1("dFdx", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_DERIV_RTY_COARSE:
        case OPCODE_DERIV_RTY_FINE:
        case OPCODE_DERIV_RTY:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DERIV_RTY\n");
            }
            CallHelper1("dFdy", psInst, 0, 1, 1);
            break;
        }
        case OPCODE_LRP:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LRP\n");
            }
            CallHelper3("mix", psInst, 0, 2, 3, 1, 1);
            break;
        }
        case OPCODE_DP2ADD:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DP2ADD\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = dot(vec2(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
            bcatcstr(glsl, "), vec2(");
            TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
            bcatcstr(glsl, ")) + ");
            TranslateOperand(&psInst->asOperands[3], TO_FLAG_NONE);
            bcatcstr(glsl, ";\n");
            break;
        }
        case OPCODE_POW:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//POW\n");
            }
            psContext->AddIndentation();
            TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = pow(abs(");
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
            bcatcstr(glsl, "), ");
            TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
            bcatcstr(glsl, ");\n");
            break;
        }

        case OPCODE_IMM_ATOMIC_ALLOC:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_ALLOC\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, &numParenthesis);
            if (isVulkan || avoidAtomicCounter)
                bcatcstr(glsl, "atomicAdd(");
            else
                bcatcstr(glsl, "atomicCounterIncrement(");
            ResourceName(glsl, psContext, RGROUP_UAV, psInst->asOperands[1].ui32RegisterNumber, 0);
            bformata(glsl, "_counter");
            if (isVulkan || avoidAtomicCounter)
                bcatcstr(glsl, ", 1u)");
            else
                bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_IMM_ATOMIC_CONSUME:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//IMM_ATOMIC_CONSUME\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, &numParenthesis);
            if (isVulkan || avoidAtomicCounter)
                bcatcstr(glsl, "(atomicAdd(");
            else
                bcatcstr(glsl, "atomicCounterDecrement(");
            ResourceName(glsl, psContext, RGROUP_UAV, psInst->asOperands[1].ui32RegisterNumber, 0);
            bformata(glsl, "_counter");
            if (isVulkan || avoidAtomicCounter)
                bcatcstr(glsl, ", 0xffffffffu) + 0xffffffffu)");
            else
                bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }

        case OPCODE_NOT:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//NOT\n");
            }
            // Adreno 3xx fails on ~a with "Internal compiler error: unexpected operator", use op_not instead
            if (!HaveNativeBitwiseOps(psContext->psShader->eTargetLanguage) || psContext->psShader->eTargetLanguage == LANG_ES_300)
            {
                UseExtraFunctionDependency("op_not");

                psContext->AddIndentation();
                AddAssignToDest(&psInst->asOperands[0], SVT_INT, psInst->asOperands[1].GetNumSwizzleElements(), psInst->ui32PreciseMask, &numParenthesis);
                bcatcstr(glsl, "op_not(");
                numParenthesis++;
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, psInst->asOperands[0].GetAccessMask());
                AddAssignPrologue(numParenthesis);
            }
            else
            {
                psContext->AddIndentation();
                AddAssignToDest(&psInst->asOperands[0], SVT_INT, psInst->asOperands[1].GetNumSwizzleElements(), psInst->ui32PreciseMask, &numParenthesis);

                bcatcstr(glsl, "~(");
                numParenthesis++;
                TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, psInst->asOperands[0].GetAccessMask());
                AddAssignPrologue(numParenthesis);
            }
            break;
        }
        case OPCODE_XOR:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//XOR\n");
            }
            CallBinaryOp("^", psInst, 0, 1, 2, SVT_UINT);
            break;
        }
        case OPCODE_RESINFO:
        {
            uint32_t destElem;
            uint32_t mask = psInst->asOperands[0].GetAccessMask();

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RESINFO\n");
            }

            for (destElem = 0; destElem < 4; ++destElem)
            {
                if (1 << destElem & mask)
                    GetResInfoData(psInst, psInst->asOperands[2].aui32Swizzle[destElem], destElem);
            }

            break;
        }
        case OPCODE_BUFINFO:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BUFINFO\n");
            }
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_INT, 1, psInst->ui32PreciseMask, &numParenthesis);
            TranslateOperand(&psInst->asOperands[1], TO_FLAG_NAME_ONLY);
            bcatcstr(glsl, "_buf.length()");
            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_SAMPLE_INFO:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//SAMPLE_INFO\n");
            }
            const RESINFO_RETURN_TYPE eResInfoReturnType = psInst->eResInfoReturnType;
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_FLOAT ? SVT_FLOAT : SVT_UINT, 1, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, "textureSamples(");
            std::string texName = ResourceName(psContext, RGROUP_TEXTURE, psInst->asOperands[1].ui32RegisterNumber, 0);
            if (psContext->IsVulkan())
            {
                std::string vulkanSamplerName = GetVulkanDummySamplerName();

                const RESOURCE_DIMENSION eResDim = psContext->psShader->aeResourceDims[psInst->asOperands[2].ui32RegisterNumber];
                std::string smpType = GetSamplerType(psContext, eResDim, psInst->asOperands[2].ui32RegisterNumber);
                std::ostringstream oss;
                oss << smpType;
                oss << "(" << texName << ", " << vulkanSamplerName << ")";
                texName = oss.str();
            }
            bcatcstr(glsl, texName.c_str());
            bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
            break;
        }
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
        case OPCODE_DDIV:
        case OPCODE_DFMA:
        case OPCODE_DRCP:
        case OPCODE_MSAD:
        case OPCODE_DTOI:
        case OPCODE_DTOU:
        case OPCODE_ITOD:
        case OPCODE_UTOD:
        default:
        {
            ASSERT(0);
            break;
        }
    }

    if (psInst->bSaturate) //Saturate is only for floating point data (float opcodes or MOV)
    {
        int dstCount = psInst->asOperands[0].GetNumSwizzleElements();

        const bool workaroundAdrenoBugs = psContext->psShader->eTargetLanguage == LANG_ES_300;

        if (workaroundAdrenoBugs)
            bcatcstr(glsl, "#ifdef UNITY_ADRENO_ES3\n");

        for (int i = workaroundAdrenoBugs ? 0 : 1; i < 2; ++i)
        {
            const bool generateWorkaround = (i == 0);
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, dstCount, psInst->ui32PreciseMask, &numParenthesis);
            bcatcstr(glsl, generateWorkaround ? "min(max(" : "clamp(");
            TranslateOperand(&psInst->asOperands[0], TO_AUTO_BITCAST_TO_FLOAT);
            bcatcstr(glsl, generateWorkaround ? ", 0.0), 1.0)" : ", 0.0, 1.0)");
            AddAssignPrologue(numParenthesis);

            if (generateWorkaround)
                bcatcstr(glsl, "#else\n");
        }

        if (workaroundAdrenoBugs)
            bcatcstr(glsl, "#endif\n");
    }
}
