#include "internal_includes/toMetal.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/languages.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "bstrlib.h"
#include "stdio.h"
#include <stdlib.h>
#include <algorithm>
#include <cmath>
#include "internal_includes/debug.h"
#include "internal_includes/Shader.h"
#include "internal_includes/Instruction.h"
#include "hlslcc.h"

using namespace HLSLcc;

bstring operator<<(bstring a, const std::string &b)
{
    bcatcstr(a, b.c_str());
    return a;
}

static void AddOpAssignToDest(bstring glsl, SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, SHADER_VARIABLE_TYPE eDestType, uint32_t ui32DestElementCount, uint32_t precise, int& numParenthesis, bool allowReinterpretCast = true)
{
    numParenthesis = 0;

    // Find out from type the precisions and types without precision
    RESOURCE_RETURN_TYPE srcBareType = SVTTypeToResourceReturnType(eSrcType);
    RESOURCE_RETURN_TYPE dstBareType = SVTTypeToResourceReturnType(eDestType);
    REFLECT_RESOURCE_PRECISION srcPrec = SVTTypeToPrecision(eSrcType);
    REFLECT_RESOURCE_PRECISION dstPrec = SVTTypeToPrecision(eDestType);

    // Add assigment
    bcatcstr(glsl, " = ");

    /* TODO: implement precise for metal
    if (precise)
    {
        bcatcstr(glsl, "u_xlat_precise(");
        numParenthesis++;
    }*/

    // Special reinterpret cast between float<->uint/int if size matches
    // TODO: Handle bools?
    if (srcBareType != dstBareType && (srcBareType == RETURN_TYPE_FLOAT || dstBareType == RETURN_TYPE_FLOAT) && srcPrec == dstPrec && allowReinterpretCast)
    {
        bformata(glsl, "as_type<%s>(", GetConstructorForTypeMetal(eDestType, ui32DestElementCount));
        numParenthesis++;
        if (ui32DestElementCount > ui32SrcElementCount)
        {
            bformata(glsl, "%s(", GetConstructorForTypeMetal(eSrcType, ui32DestElementCount));
            numParenthesis++;
        }
        return;
    }

    // Do cast in case of type missmatch or dimension
    if (eSrcType != eDestType || ui32DestElementCount > ui32SrcElementCount)
    {
        bformata(glsl, "%s(", GetConstructorForTypeMetal(eDestType, ui32DestElementCount));
        numParenthesis++;
        return;
    }
}

// This function prints out the destination name, possible destination writemask, assignment operator
// and any possible conversions needed based on the eSrcType+ui32SrcElementCount (type and size of data expected to be coming in)
// As an output, pNeedsParenthesis will be filled with the amount of closing parenthesis needed
// and pSrcCount will be filled with the number of components expected
// ui32CompMask can be used to only write to 1 or more components (used by MOVC)
void ToMetal::AddOpAssignToDestWithMask(const Operand* psDest,
    SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int& numParenthesis, uint32_t ui32CompMask)
{
    uint32_t ui32DestElementCount = psDest->GetNumSwizzleElements(ui32CompMask);
    bstring glsl = *psContext->currentGLSLString;
    SHADER_VARIABLE_TYPE eDestType = psDest->GetDataType(psContext);
    glsl << TranslateOperand(psDest, TO_FLAG_DESTINATION, ui32CompMask);
    AddOpAssignToDest(glsl, eSrcType, ui32SrcElementCount, eDestType, ui32DestElementCount, precise, numParenthesis, psContext->psShader->ui32MajorVersion > 3);
}

void ToMetal::AddAssignToDest(const Operand* psDest,
    SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int& numParenthesis)
{
    AddOpAssignToDestWithMask(psDest, eSrcType, ui32SrcElementCount, precise, numParenthesis, OPERAND_4_COMPONENT_MASK_ALL);
}

void ToMetal::AddAssignPrologue(int numParenthesis)
{
    bstring glsl = *psContext->currentGLSLString;
    while (numParenthesis != 0)
    {
        bcatcstr(glsl, ")");
        numParenthesis--;
    }
    bcatcstr(glsl, ";\n");
}

void ToMetal::AddComparison(Instruction* psInst, ComparisonType eType,
    uint32_t typeFlag)
{
    // Multiple cases to consider here:
    // OPCODE_LT, _GT, _NE etc: inputs are floats, outputs UINT 0xffffffff or 0. typeflag: TO_FLAG_NONE
    // OPCODE_ILT, _IGT etc: comparisons are signed ints, outputs UINT 0xffffffff or 0 typeflag TO_FLAG_INTEGER
    // _ULT, UGT etc: inputs unsigned ints, outputs UINTs typeflag TO_FLAG_UNSIGNED_INTEGER
    //


    bstring glsl = *psContext->currentGLSLString;
    const uint32_t destElemCount = psInst->asOperands[0].GetNumSwizzleElements();
    const uint32_t s0ElemCount = psInst->asOperands[1].GetNumSwizzleElements();
    const uint32_t s1ElemCount = psInst->asOperands[2].GetNumSwizzleElements();
    int isBoolDest = psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL;
    const uint32_t destMask = psInst->asOperands[0].GetAccessMask();

    int needsParenthesis = 0;
    if (typeFlag == TO_FLAG_NONE
        && CanForceToHalfOperand(&psInst->asOperands[1])
        && CanForceToHalfOperand(&psInst->asOperands[2]))
        typeFlag = TO_FLAG_FORCE_HALF;
    ASSERT(s0ElemCount == s1ElemCount || s1ElemCount == 1 || s0ElemCount == 1);
    if ((s0ElemCount != s1ElemCount) && (destElemCount > 1))
    {
        // Set the proper auto-expand flag is either argument is scalar
        typeFlag |= (TO_AUTO_EXPAND_TO_VEC2 << (std::min(std::max(s0ElemCount, s1ElemCount), destElemCount) - 2));
    }
    if (destElemCount > 1)
    {
        const char* glslOpcode[] = {
            "==",
            "<",
            ">=",
            "!=",
        };
        psContext->AddIndentation();
        if (isBoolDest)
        {
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION | TO_FLAG_BOOL);
            bcatcstr(glsl, " = ");
        }
        else
        {
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, destElemCount, psInst->ui32PreciseMask, needsParenthesis);

            bcatcstr(glsl, GetConstructorForTypeMetal(SVT_UINT, destElemCount));
            bcatcstr(glsl, "(");
        }
        bcatcstr(glsl, "(");
        glsl << TranslateOperand(&psInst->asOperands[1], typeFlag, destMask);
        bformata(glsl, "%s", glslOpcode[eType]);
        glsl << TranslateOperand(&psInst->asOperands[2], typeFlag, destMask);
        bcatcstr(glsl, ")");
        if (!isBoolDest)
        {
            bcatcstr(glsl, ")");
            bcatcstr(glsl, " * 0xFFFFFFFFu");
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

        psContext->AddIndentation();
        if (isBoolDest)
        {
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION | TO_FLAG_BOOL);
            bcatcstr(glsl, " = ");
        }
        else
        {
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, destElemCount, psInst->ui32PreciseMask, needsParenthesis);
            bcatcstr(glsl, "(");
        }
        glsl << TranslateOperand(&psInst->asOperands[1], typeFlag, destMask);
        bformata(glsl, "%s", glslOpcode[eType]);
        glsl << TranslateOperand(&psInst->asOperands[2], typeFlag, destMask);
        if (!isBoolDest)
        {
            bcatcstr(glsl, ") ? 0xFFFFFFFFu : uint(0)");
        }
        AddAssignPrologue(needsParenthesis);
    }
}

bool ToMetal::CanForceToHalfOperand(const Operand *psOperand)
{
    if (psOperand->GetDataType(psContext) == SVT_FLOAT16)
        return true;

    if (psOperand->eType == OPERAND_TYPE_IMMEDIATE32 || psOperand->eType == OPERAND_TYPE_IMMEDIATE_CONSTANT_BUFFER)
    {
        for (int i = 0; i < psOperand->iNumComponents; i++)
        {
            float val = fabs(psOperand->afImmediates[i]);
            // Do not allow forcing immediate value to half if value is beyond half min/max boundaries
            if (val != 0 && (val > 65504 || val < 6.10352e-5))
                return false;
        }
        return true;
    }

    return false;
}

void ToMetal::AddMOVBinaryOp(const Operand *pDest, Operand *pSrc, uint32_t precise)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    int srcSwizzleCount = pSrc->GetNumSwizzleElements();
    uint32_t writeMask = pDest->GetAccessMask();

    const SHADER_VARIABLE_TYPE eSrcType = pSrc->GetDataType(psContext, pDest->GetDataType(psContext));
    uint32_t flags = SVTTypeToFlag(eSrcType);

    AddAssignToDest(pDest, eSrcType, srcSwizzleCount, precise, numParenthesis);
    glsl << TranslateOperand(pSrc, flags, writeMask);

    AddAssignPrologue(numParenthesis);
}

void ToMetal::AddMOVCBinaryOp(const Operand *pDest, const Operand *src0, Operand *src1, Operand *src2, uint32_t precise)
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
        AddAssignToDest(pDest, eDestType, destElemCount, precise, numParenthesis);
        bcatcstr(glsl, "(");
        if (s0Type == SVT_UINT || s0Type == SVT_UINT16)
            glsl << TranslateOperand(src0, TO_AUTO_BITCAST_TO_UINT, OPERAND_4_COMPONENT_MASK_X);
        else if (s0Type == SVT_BOOL)
            glsl << TranslateOperand(src0, TO_FLAG_BOOL, OPERAND_4_COMPONENT_MASK_X);
        else
            glsl << TranslateOperand(src0, TO_AUTO_BITCAST_TO_INT, OPERAND_4_COMPONENT_MASK_X);

        if (psContext->psShader->ui32MajorVersion < 4)
        {
            //cmp opcode uses >= 0
            bcatcstr(glsl, " >= 0) ? ");
        }
        else
        {
            if (s0Type == SVT_UINT || s0Type == SVT_UINT16)
                bcatcstr(glsl, " != uint(0)) ? ");
            else if (s0Type == SVT_BOOL)
                bcatcstr(glsl, ") ? ");
            else
                bcatcstr(glsl, " != 0) ? ");
        }

        if (s1ElemCount == 1 && destElemCount > 1)
            glsl << TranslateOperand(src1, SVTTypeToFlag(eDestType) | ElemCountToAutoExpandFlag(destElemCount));
        else
            glsl << TranslateOperand(src1, SVTTypeToFlag(eDestType), destWriteMask);

        bcatcstr(glsl, " : ");
        if (s2ElemCount == 1 && destElemCount > 1)
            glsl << TranslateOperand(src2, SVTTypeToFlag(eDestType) | ElemCountToAutoExpandFlag(destElemCount));
        else
            glsl << TranslateOperand(src2, SVTTypeToFlag(eDestType), destWriteMask);

        AddAssignPrologue(numParenthesis);
    }
    else
    {
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
            bformata(glsl, "%s %s = %s;\n", HLSLcc::GetConstructorForType(psContext, eDestType, numComponents), tempName.c_str(), TranslateOperand(pDest, TO_FLAG_NAME_ONLY).c_str());

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
            AddOpAssignToDestWithMask(pDest, eDestType, 1, precise, numParenthesis, 1 << destElem);
            bcatcstr(glsl, "(");
            if (s0Type == SVT_BOOL)
            {
                glsl << TranslateOperand(src0, TO_FLAG_BOOL, 1 << srcElem);
                bcatcstr(glsl, ") ? ");
            }
            else
            {
                glsl << TranslateOperand(src0, TO_AUTO_BITCAST_TO_INT, 1 << srcElem);

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

            glsl << TranslateOperand(src1, SVTTypeToFlag(eDestType), 1 << srcElem);
            bcatcstr(glsl, " : ");
            glsl << TranslateOperand(src2, SVTTypeToFlag(eDestType), 1 << srcElem);
            AddAssignPrologue(numParenthesis);
        }

        if (dstIsSrc1 || dstIsSrc2)
        {
            const_cast<Operand *>(pDest)->specialName.clear();

            psContext->AddIndentation();
            glsl << TranslateOperand(pDest, TO_FLAG_NAME_ONLY);
            bformata(glsl, " = %s;\n", tempName.c_str());

            --psContext->indent;
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
        }
    }
}

void ToMetal::CallBinaryOp(const char* name, Instruction* psInst,
    int dest, int src0, int src1, SHADER_VARIABLE_TYPE eDataType)
{
    uint32_t ui32Flags = SVTTypeToFlag(eDataType);
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = psInst->asOperands[dest].GetAccessMask();
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int needsParenthesis = 0;

    if (eDataType == SVT_FLOAT
        && CanForceToHalfOperand(&psInst->asOperands[dest])
        && CanForceToHalfOperand(&psInst->asOperands[src0])
        && CanForceToHalfOperand(&psInst->asOperands[src1]))
    {
        ui32Flags = TO_FLAG_FORCE_HALF;
        eDataType = SVT_FLOAT16;
    }

    uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
    if (src1SwizCount != src0SwizCount)
    {
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], eDataType, dstSwizCount, psInst->ui32PreciseMask, needsParenthesis);

/*  bool s0NeedsUpscaling = false, s1NeedsUpscaling = false;
    SHADER_VARIABLE_TYPE s0Type = psInst->asOperands[src0].GetDataType(psContext);
    SHADER_VARIABLE_TYPE s1Type = psInst->asOperands[src1].GetDataType(psContext);

    if((s0Type == SVT_FLOAT10 || s0Type == SVT_FLOAT16) && (s1Type != s)
    */
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bformata(glsl, " %s ", name);
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);

    AddAssignPrologue(needsParenthesis);
}

void ToMetal::CallTernaryOp(const char* op1, const char* op2, Instruction* psInst,
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

    if (dataType == TO_FLAG_NONE
        && CanForceToHalfOperand(&psInst->asOperands[dest])
        && CanForceToHalfOperand(&psInst->asOperands[src0])
        && CanForceToHalfOperand(&psInst->asOperands[src1])
        && CanForceToHalfOperand(&psInst->asOperands[src2]))
        ui32Flags = dataType = TO_FLAG_FORCE_HALF;

    if (src1SwizCount != src0SwizCount || src2SwizCount != src0SwizCount)
    {
        uint32_t maxElems = std::max(src2SwizCount, std::max(src1SwizCount, src0SwizCount));
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], TypeFlagsToSVTType(dataType), dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bformata(glsl, " %s ", op1);
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    bformata(glsl, " %s ", op2);
    glsl << TranslateOperand(&psInst->asOperands[src2], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::CallHelper3(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int src2, int paramsShouldFollowWriteMask, uint32_t ui32Flags)
{
    bstring glsl = *psContext->currentGLSLString;
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    uint32_t src2SwizCount = psInst->asOperands[src2].GetNumSwizzleElements(destMask);
    uint32_t src1SwizCount = psInst->asOperands[src1].GetNumSwizzleElements(destMask);
    uint32_t src0SwizCount = psInst->asOperands[src0].GetNumSwizzleElements(destMask);
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    int numParenthesis = 0;

    if (CanForceToHalfOperand(&psInst->asOperands[dest])
        && CanForceToHalfOperand(&psInst->asOperands[src0])
        && CanForceToHalfOperand(&psInst->asOperands[src1])
        && CanForceToHalfOperand(&psInst->asOperands[src2]))
        ui32Flags = TO_FLAG_FORCE_HALF | TO_AUTO_BITCAST_TO_FLOAT;

    if ((src1SwizCount != src0SwizCount || src2SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src2SwizCount, std::max(src1SwizCount, src0SwizCount));
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();

    AddAssignToDest(&psInst->asOperands[dest], ui32Flags & TO_FLAG_FORCE_HALF ? SVT_FLOAT16 : SVT_FLOAT, dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    glsl << TranslateOperand(&psInst->asOperands[src2], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::CallHelper3(const char* name, Instruction* psInst,
    int dest, int src0, int src1, int src2, int paramsShouldFollowWriteMask)
{
    CallHelper3(name, psInst, dest, src0, src1, src2, paramsShouldFollowWriteMask, TO_AUTO_BITCAST_TO_FLOAT);
}

void ToMetal::CallHelper2(const char* name, Instruction* psInst,
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

    if (CanForceToHalfOperand(&psInst->asOperands[dest])
        && CanForceToHalfOperand(&psInst->asOperands[src0])
        && CanForceToHalfOperand(&psInst->asOperands[src1]))
        ui32Flags = TO_FLAG_FORCE_HALF | TO_AUTO_BITCAST_TO_FLOAT;


    if ((src1SwizCount != src0SwizCount) && paramsShouldFollowWriteMask)
    {
        uint32_t maxElems = std::max(src1SwizCount, src0SwizCount);
        ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
    }

    psContext->AddIndentation();
    AddAssignToDest(&psInst->asOperands[dest], ui32Flags & TO_FLAG_FORCE_HALF ? SVT_FLOAT16 : SVT_FLOAT, isDotProduct ? 1 : dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;

    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);

    AddAssignPrologue(numParenthesis);
}

void ToMetal::CallHelper2Int(const char* name, Instruction* psInst,
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

    AddAssignToDest(&psInst->asOperands[dest], SVT_INT, dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::CallHelper2UInt(const char* name, Instruction* psInst,
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

    AddAssignToDest(&psInst->asOperands[dest], SVT_UINT, dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    bcatcstr(glsl, ", ");
    glsl << TranslateOperand(&psInst->asOperands[src1], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::CallHelper1(const char* name, Instruction* psInst,
    int dest, int src0, int paramsShouldFollowWriteMask)
{
    uint32_t ui32Flags = TO_AUTO_BITCAST_TO_FLOAT;
    bstring glsl = *psContext->currentGLSLString;
    uint32_t dstSwizCount = psInst->asOperands[dest].GetNumSwizzleElements();
    uint32_t destMask = paramsShouldFollowWriteMask ? psInst->asOperands[dest].GetAccessMask() : OPERAND_4_COMPONENT_MASK_ALL;
    int numParenthesis = 0;

    psContext->AddIndentation();
    if (CanForceToHalfOperand(&psInst->asOperands[dest])
        && CanForceToHalfOperand(&psInst->asOperands[src0]))
        ui32Flags = TO_FLAG_FORCE_HALF | TO_AUTO_BITCAST_TO_FLOAT;

    AddAssignToDest(&psInst->asOperands[dest], ui32Flags & TO_FLAG_FORCE_HALF ? SVT_FLOAT16 : SVT_FLOAT, dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

//Result is an int.
void ToMetal::CallHelper1Int(
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

    AddAssignToDest(&psInst->asOperands[dest], SVT_INT, dstSwizCount, psInst->ui32PreciseMask, numParenthesis);

    bformata(glsl, "%s(", name);
    numParenthesis++;
    glsl << TranslateOperand(&psInst->asOperands[src0], ui32Flags, destMask);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::TranslateTexelFetch(
    Instruction* psInst,
    const ResourceBinding* psBinding,
    bstring glsl)
{
    int numParenthesis = 0;
    psContext->AddIndentation();
    AddAssignToDest(&psInst->asOperands[0], psContext->psShader->sInfo.GetTextureDataType(psInst->asOperands[2].ui32RegisterNumber), 4, psInst->ui32PreciseMask, numParenthesis);
    glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
    bcatcstr(glsl, ".read(");

    switch (psBinding->eDimension)
    {
        case REFLECT_RESOURCE_DIMENSION_BUFFER:
        {
            psContext->m_Reflection.OnDiagnostics("Buffer resources not supported in Metal (in texel fetch)", 0, true);
            return;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W);
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }

        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_Z); // Array index
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE3D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[3], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X); // Sample index
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            psContext->m_Reflection.OnDiagnostics("Multisampled texture arrays not supported in Metal (in texel fetch)", 0, true);
            return;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBE:
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        case REFLECT_RESOURCE_DIMENSION_BUFFEREX:
        default:
        {
            // Shouldn't happen. Cubemap reads are not supported in HLSL
            ASSERT(0);
            break;
        }
    }
    bcatcstr(glsl, ")");

    glsl << TranslateOperandSwizzle(&psInst->asOperands[2], psInst->asOperands[0].GetAccessMask(), 0);
    AddAssignPrologue(numParenthesis);
}

void ToMetal::TranslateTexelFetchOffset(
    Instruction* psInst,
    const ResourceBinding* psBinding,
    bstring glsl)
{
    int numParenthesis = 0;
    psContext->AddIndentation();
    AddAssignToDest(&psInst->asOperands[0], psContext->psShader->sInfo.GetTextureDataType(psInst->asOperands[2].ui32RegisterNumber), 4, psInst->ui32PreciseMask, numParenthesis);

    glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
    bcatcstr(glsl, ".read(");

    switch (psBinding->eDimension)
    {
        case REFLECT_RESOURCE_DIMENSION_BUFFER:
        {
            psContext->m_Reflection.OnDiagnostics("Buffer resources not supported in Metal (in texel fetch)", 0, true);
            return;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            psContext->m_Reflection.OnDiagnostics("Multisampled texture arrays not supported in Metal (in texel fetch)", 0, true);
            return;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bformata(glsl, " + %d", psInst->iUAddrOffset);
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X);
            bformata(glsl, " + %d, ", psInst->iUAddrOffset);

            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_Y);
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bformata(glsl, "+ ivec2(%d, %d), ", psInst->iUAddrOffset, psInst->iVAddrOffset);
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bformata(glsl, "+ ivec2(%d, %d), ", psInst->iUAddrOffset, psInst->iVAddrOffset);
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_Z); // Array index
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE3D:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC3, 7 /* .xyz */);
            bformata(glsl, "+ ivec3(%d, %d, %d), ", psInst->iUAddrOffset, psInst->iVAddrOffset, psInst->iWAddrOffset);
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_W); // Lod level
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_EXPAND_TO_VEC2, 3 /* .xy */);
            bformata(glsl, "+ ivec2(%d, %d), ", psInst->iUAddrOffset, psInst->iVAddrOffset);
            glsl << TranslateOperand(&psInst->asOperands[3], TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X); // Sample index
            break;
        }
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBE:
        case REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        case REFLECT_RESOURCE_DIMENSION_BUFFEREX:
        default:
        {
            // Shouldn't happen. Cubemap reads are not supported in HLSL
            ASSERT(0);
            break;
        }
    }
    bcatcstr(glsl, ")");

    glsl << TranslateOperandSwizzle(&psInst->asOperands[2], psInst->asOperands[0].GetAccessMask(), 0);
    AddAssignPrologue(numParenthesis);
}

//Makes sure the texture coordinate swizzle is appropriate for the texture type.
//i.e. vecX for X-dimension texture.
//Currently supports floating point coord only, so not used for texelFetch.
void ToMetal::TranslateTexCoord(
    const RESOURCE_DIMENSION eResDim,
    Operand* psTexCoordOperand)
{
    uint32_t flags = TO_AUTO_BITCAST_TO_FLOAT;
    uint32_t opMask = OPERAND_4_COMPONENT_MASK_ALL;
    bool isArray = false;

    switch (eResDim)
    {
        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            //Vec1 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            // x for coord, y for array element
            opMask = OPERAND_4_COMPONENT_MASK_X;
            bstring glsl = *psContext->currentGLSLString;
            glsl << TranslateOperand(psTexCoordOperand, flags, opMask);

            bcatcstr(glsl, ", round(");

            opMask = OPERAND_4_COMPONENT_MASK_Y;
            flags = TO_AUTO_BITCAST_TO_FLOAT;
            isArray = true;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            //Vec2 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
            flags |= TO_AUTO_EXPAND_TO_VEC2;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBE:
        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            //Vec3 texcoord. Mask out the other components.
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
            flags |= TO_AUTO_EXPAND_TO_VEC3;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            // xy for coord, z for array element
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
            flags |= TO_AUTO_EXPAND_TO_VEC2;

            bstring glsl = *psContext->currentGLSLString;
            glsl << TranslateOperand(psTexCoordOperand, flags, opMask);

            bcatcstr(glsl, ", round(");

            opMask = OPERAND_4_COMPONENT_MASK_Z;
            flags = TO_AUTO_BITCAST_TO_FLOAT;
            isArray = true;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            // xyz for coord, w for array element
            opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
            flags |= TO_AUTO_EXPAND_TO_VEC3;

            bstring glsl = *psContext->currentGLSLString;
            glsl << TranslateOperand(psTexCoordOperand, flags, opMask);

            bcatcstr(glsl, ", round(");

            opMask = OPERAND_4_COMPONENT_MASK_W;
            flags = TO_AUTO_BITCAST_TO_FLOAT;
            isArray = true;
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }

    //FIXME detect when integer coords are needed.
    bstring glsl = *psContext->currentGLSLString;
    glsl << TranslateOperand(psTexCoordOperand, flags, opMask);

    if (isArray)
        bcatcstr(glsl, ")");
}

void ToMetal::GetResInfoData(Instruction* psInst, int index, int destElem)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
    const RESINFO_RETURN_TYPE eResInfoReturnType = psInst->eResInfoReturnType;

    psContext->AddIndentation();
    AddOpAssignToDestWithMask(&psInst->asOperands[0], eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT ? SVT_UINT : SVT_FLOAT, 1, psInst->ui32PreciseMask, numParenthesis, 1 << destElem);

    const char *metalGetters[] = { ".get_width(", ".get_height(", ".get_depth(", ".get_num_mip_levels()" };
    int dim = GetNumTextureDimensions(psInst->eResDim);
    if (dim < (index + 1) && index != 3)
    {
        bcatcstr(glsl, eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT ? "uint(0)" : "0.0");
    }
    else
    {
        if (eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_FLOAT)
        {
            bcatcstr(glsl, "float(");
            numParenthesis++;
        }
        else if (eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_RCPFLOAT)
        {
            bcatcstr(glsl, "1.0f / float(");
            numParenthesis++;
        }
        glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_NAME_ONLY);
        if ((index == 1 && psInst->eResDim == RESOURCE_DIMENSION_TEXTURE1DARRAY) ||
            (index == 2 && (psInst->eResDim == RESOURCE_DIMENSION_TEXTURE2DARRAY ||
                            psInst->eResDim == RESOURCE_DIMENSION_TEXTURE2DMSARRAY)))
        {
            bcatcstr(glsl, ".get_array_size()");
        }
        else
        {
            bcatcstr(glsl, metalGetters[index]);

            if (index < 3)
            {
                if (psInst->eResDim != RESOURCE_DIMENSION_TEXTURE2DMS &&
                    psInst->eResDim != RESOURCE_DIMENSION_TEXTURE2DMSARRAY)
                    glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER); //mip level

                bcatcstr(glsl, ")");
            }
        }
    }
    AddAssignPrologue(numParenthesis);
}

void ToMetal::TranslateTextureSample(Instruction* psInst,
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

    const char *funcName = "";
    const char* gradSwizzle = "";
    const char *gradientName = "";

    uint32_t ui32NumOffsets = 0;

    const RESOURCE_DIMENSION eResDim = psContext->psShader->aeResourceDims[psSrcTex->ui32RegisterNumber];

    if (ui32Flags & TEXSMP_FLAG_GATHER)
    {
        if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE)
            funcName = "gather_compare";
        else
            funcName = "gather";
    }
    else
    {
        if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE)
            funcName = "sample_compare";
        else
            funcName = "sample";
    }

    switch (eResDim)
    {
        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            gradSwizzle = ".x";
            ui32NumOffsets = 1;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            gradSwizzle = ".xy";
            gradientName = "gradient2d";
            ui32NumOffsets = 2;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            gradientName = "gradientcube";
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            gradientName = "gradient3d";
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            gradSwizzle = ".x";
            ui32NumOffsets = 1;
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            gradSwizzle = ".xy";
            ui32NumOffsets = 2;
            gradientName = "gradient2d";
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            gradSwizzle = ".xyz";
            ui32NumOffsets = 3;
            gradientName = "gradientcube";
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }


    SHADER_VARIABLE_TYPE dataType = psContext->psShader->sInfo.GetTextureDataType(psSrcTex->ui32RegisterNumber);
    psContext->AddIndentation();
    AddAssignToDest(psDest, dataType, psSrcTex->GetNumSwizzleElements(), psInst->ui32PreciseMask, numParenthesis);

    std::string texName = TranslateOperand(psSrcTex, TO_FLAG_NAME_ONLY);

    // TextureName.FuncName(
    glsl << texName;
    bformata(glsl, ".%s(", funcName);

    bool isDepthSampler = false;
    for (unsigned j = 0, m = m_Textures.size(); j < m; ++j)
    {
        if (m_Textures[j].name == texName)
        {
            isDepthSampler = m_Textures[j].isDepthSampler;
            break;
        }
    }

    // Sampler name
    // on ios pre-GPUFamily3 we MUST have constexpr in shader for a sampler with compare func
    // for now we use fixed shadow sampler in all cases of depth compare (ATM all depth compares are interpreted as shadow usage)
    if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE && IsMobileTarget(psContext))
    {
        bcatcstr(glsl, "_mtl_xl_shadow_sampler");
    }
    else
    {
        std::string sampName = TranslateOperand(psSrcSamp, TO_FLAG_NAME_ONLY);

        // insert the "sampler" prefix if the sampler name is equal to the texture name (default sampler)
        if (texName == sampName)
            sampName.insert(0, "sampler");
        glsl << sampName;
    }

    bcatcstr(glsl, ", ");

    // Texture coordinates
    TranslateTexCoord(eResDim, psDestAddr);

    // Depth compare reference value
    if (ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE)
    {
        bcatcstr(glsl, ", saturate("); // TODO: why the saturate here?
        glsl << TranslateOperand(psSrcRef, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ")");
    }

    // lod_options (LOD/grad/bias) based on the flags
    if (ui32Flags & TEXSMP_FLAG_LOD)
    {
        bcatcstr(glsl, ", level(");
        glsl << TranslateOperand(psSrcLOD, TO_AUTO_BITCAST_TO_FLOAT);
        if (psContext->psShader->ui32MajorVersion < 4)
        {
            bcatcstr(glsl, ".w");
        }
        bcatcstr(glsl, ")");
    }
    else if (ui32Flags & TEXSMP_FLAG_FIRSTLOD)
    {
        bcatcstr(glsl, ", level(0.0)");
    }
    else if (ui32Flags & TEXSMP_FLAG_GRAD)
    {
        glsl << std::string(", ") << std::string(gradientName) << std::string("(float4(");
        glsl << TranslateOperand(psSrcDx, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ")");
        bcatcstr(glsl, gradSwizzle);
        bcatcstr(glsl, ", float4(");
        glsl << TranslateOperand(psSrcDy, TO_AUTO_BITCAST_TO_FLOAT);
        bcatcstr(glsl, ")");
        bcatcstr(glsl, gradSwizzle);
        bcatcstr(glsl, ")");
    }
    else if (ui32Flags & (TEXSMP_FLAG_BIAS))
    {
        glsl << std::string(", bias(") << TranslateOperand(psSrcBias, TO_AUTO_BITCAST_TO_FLOAT) << std::string(")");
    }

    bool hadOffset = false;

    // Add offset param
    if (psInst->bAddressOffset)
    {
        hadOffset = true;
        if (ui32NumOffsets == 1)
        {
            bformata(glsl, ", %d",
                psInst->iUAddrOffset);
        }
        else if (ui32NumOffsets == 2)
        {
            bformata(glsl, ", int2(%d, %d)",
                psInst->iUAddrOffset,
                psInst->iVAddrOffset);
        }
        else if (ui32NumOffsets == 3)
        {
            bformata(glsl, ", int3(%d, %d, %d)",
                psInst->iUAddrOffset,
                psInst->iVAddrOffset,
                psInst->iWAddrOffset);
        }
    }
    // HLSL gather has a variant with separate offset operand
    else if (ui32Flags & TEXSMP_FLAG_PARAMOFFSET)
    {
        hadOffset = true;
        uint32_t mask = OPERAND_4_COMPONENT_MASK_X;
        if (ui32NumOffsets > 1)
            mask |= OPERAND_4_COMPONENT_MASK_Y;
        if (ui32NumOffsets > 2)
            mask |= OPERAND_4_COMPONENT_MASK_Z;

        bcatcstr(glsl, ",");
        glsl << TranslateOperand(psSrcOff, TO_FLAG_INTEGER, mask);
    }

    // Add texture gather component selection if needed
    if ((ui32Flags & TEXSMP_FLAG_GATHER) && psSrcSamp->GetNumSwizzleElements() > 0)
    {
        ASSERT(psSrcSamp->GetNumSwizzleElements() == 1);
        if (psSrcSamp->aui32Swizzle[0] != OPERAND_4_COMPONENT_X)
        {
            if (!(ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE))
            {
                // Need to add offset param to match func overload
                if (!hadOffset)
                {
                    if (ui32NumOffsets == 1)
                        bcatcstr(glsl, ", 0");
                    else
                        bformata(glsl, ", int%d(0)", ui32NumOffsets);
                }

                bcatcstr(glsl, ", component::");
                glsl << TranslateOperandSwizzle(psSrcSamp, OPERAND_4_COMPONENT_MASK_ALL, 0, false);
            }
            else
            {
                psContext->m_Reflection.OnDiagnostics("Metal supports gather compare only for the first component.", 0, true);
            }
        }
    }

    bcatcstr(glsl, ")");

    if (!((ui32Flags & TEXSMP_FLAG_DEPTHCOMPARE) || isDepthSampler) || (ui32Flags & TEXSMP_FLAG_GATHER))
    {
        // iWriteMaskEnabled is forced off during DecodeOperand because swizzle on sampler uniforms
        // does not make sense. But need to re-enable to correctly swizzle this particular instruction.
        psSrcTex->iWriteMaskEnabled = 1;
        glsl << TranslateOperandSwizzle(psSrcTex, psDest->GetAccessMask(), 0);
    }
    AddAssignPrologue(numParenthesis);
}

// Handle cases where vector components are accessed with dynamic index ([] notation).
// A bit ugly hack because compiled HLSL uses byte offsets to access data in structs => we are converting
// the offset back to vector component index in runtime => calculating stuff back and forth.
// TODO: Would be better to eliminate the offset calculation ops and use indexes straight on. Could be tricky though...
void ToMetal::TranslateDynamicComponentSelection(const ShaderVarType* psVarType, const Operand* psByteAddr, uint32_t offset, uint32_t mask)
{
    bstring glsl = *psContext->currentGLSLString;
    ASSERT(psVarType->Class == SVC_VECTOR);

    bcatcstr(glsl, "["); // Access vector component with [] notation
    if (offset > 0)
        bcatcstr(glsl, "(");

    // The var containing byte address to the requested element
    glsl << TranslateOperand(psByteAddr, TO_FLAG_UNSIGNED_INTEGER, mask);

    if (offset > 0)// If the vector is part of a struct, there is an extra offset in our byte address
        bformata(glsl, " - %du)", offset); // Subtract that first

    bcatcstr(glsl, " >> 0x2u"); // Convert byte offset to index: div by four
    bcatcstr(glsl, "]");
}

void ToMetal::TranslateShaderStorageStore(Instruction* psInst)
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
        case OPCODE_STORE_UAV_TYPED: // Hack typed buffer as raw buf
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
    if (dstOffType == SVT_INT || dstOffType == SVT_INT16 || dstOffType == SVT_INT12)
        dstOffFlag = TO_FLAG_INTEGER;

    for (component = 0; component < 4; component++)
    {
        ASSERT(psInst->asOperands[0].eSelMode == OPERAND_4_COMPONENT_MASK_MODE);
        if (psInst->asOperands[0].ui32CompMask & (1 << component))
        {
            psContext->AddIndentation();
            glsl << TranslateOperand(psDest, TO_FLAG_DESTINATION | TO_FLAG_NAME_ONLY);

            if (psDestAddr)
            {
                bcatcstr(glsl, "[");
                glsl << TranslateOperand(psDestAddr, TO_FLAG_INTEGER | TO_FLAG_UNSIGNED_INTEGER);
                bcatcstr(glsl, "].value");
            }

            bcatcstr(glsl, "[(");
            glsl << TranslateOperand(psDestByteOff, dstOffFlag);
            if (psInst->eOpcode == OPCODE_STORE_UAV_TYPED)
            {
                bcatcstr(glsl, ")");
            }
            else
            {
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
            }
            bcatcstr(glsl, "]");

            //Dest type is currently always a uint array.
            bcatcstr(glsl, " = ");
            if (psSrc->GetNumSwizzleElements() > 1)
                glsl << TranslateOperand(psSrc, TO_FLAG_UNSIGNED_INTEGER, 1 << (srcComponent++));
            else
                glsl << TranslateOperand(psSrc, TO_FLAG_UNSIGNED_INTEGER, OPERAND_4_COMPONENT_MASK_X);

            bformata(glsl, ";\n");
        }
    }
}

void ToMetal::TranslateShaderStorageLoad(Instruction* psInst)
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
        case OPCODE_LD_UAV_TYPED: // Hack typed buffer as raw buf
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
    if (srcOffType == SVT_INT || srcOffType == SVT_INT16 || srcOffType == SVT_INT12)
        srcOffFlag = TO_FLAG_INTEGER;

    psContext->AddIndentation();
    AddAssignToDest(psDest, destDataType, destCount, psInst->ui32PreciseMask, numParenthesis);
    if (destCount > 1)
    {
        bformata(glsl, "%s(", GetConstructorForTypeMetal(destDataType, destCount));
        numParenthesis++;
    }
    for (component = 0; component < 4; component++)
    {
        bool addedBitcast = false;
        if (!(destMask & (1 << component)))
            continue;

        if (firstItemAdded)
            bcatcstr(glsl, ", ");
        else
            firstItemAdded = 1;

        // always uint array atm
        if (destDataType == SVT_FLOAT)
        {
            // input already in uints, need bitcast
            bcatcstr(glsl, "as_type<float>(");
            addedBitcast = true;
        }
        else if (destDataType == SVT_INT || destDataType == SVT_INT16 || destDataType == SVT_INT12)
        {
            bcatcstr(glsl, "int(");
            addedBitcast = true;
        }

        glsl << TranslateOperand(psSrc, TO_FLAG_NAME_ONLY);

        if (psSrcAddr)
        {
            bcatcstr(glsl, "[");
            glsl << TranslateOperand(psSrcAddr, TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_INTEGER);
            bcatcstr(glsl, "].value");
        }
        bcatcstr(glsl, "[(");
        glsl << TranslateOperand(psSrcByteOff, srcOffFlag);
        if (psInst->eOpcode == OPCODE_LD_UAV_TYPED)
        {
            bcatcstr(glsl, ")");
        }
        else
        {
            bcatcstr(glsl, " >> 2");
            if (srcOffFlag == TO_FLAG_UNSIGNED_INTEGER)
                bcatcstr(glsl, "u");

            bformata(glsl, ") + %d", psSrc->eSelMode == OPERAND_4_COMPONENT_SWIZZLE_MODE ? psSrc->aui32Swizzle[component] : component);
            if (srcOffFlag == TO_FLAG_UNSIGNED_INTEGER)
                bcatcstr(glsl, "u");
        }
        bcatcstr(glsl, "]");

        if (addedBitcast)
            bcatcstr(glsl, ")");
    }
    AddAssignPrologue(numParenthesis);
}

void ToMetal::TranslateAtomicMemOp(Instruction* psInst)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;
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
            func = "atomic_fetch_add_explicit";
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
            func = "atomic_fetch_add_explicit";
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
            func = "atomic_fetch_and_explicit";
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
            func = "atomic_fetch_and_explicit";
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
            func = "atomic_fetch_or_explicit";
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
            func = "atomic_fetch_or_explicit";
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
            func = "atomic_fetch_xor_explicit";
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
            func = "atomic_fetch_xor_explicit";
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
            func = "atomic_exchange_explicit";
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
            func = "atomic_compare_exchange_weak_explicit";
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
            func = "atomic_compare_exchange_weak_explicit";
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
            func = "atomic_fetch_min_explicit";
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
            func = "atomic_fetch_min_explicit";
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
            func = "atomic_fetch_min_explicit";
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
            func = "atomic_fetch_min_explicit";
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
            func = "atomic_fetch_max_explicit";
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
            func = "atomic_fetch_max_explicit";
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
            func = "atomic_fetch_max_explicit";
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
            func = "atomic_fetch_max_explicit";
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

    const ResourceBinding* psBinding = 0;
    if (dest->eType != OPERAND_TYPE_THREAD_GROUP_SHARED_MEMORY)
    {
        psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, dest->ui32RegisterNumber, &psBinding);

        if (psBinding->eType == RTYPE_UAV_RWTYPED)
        {
            isUint = (psBinding->ui32ReturnType == RETURN_TYPE_UINT);

            // Find out if it's texture and of what dimension
            switch (psBinding->eDimension)
            {
                case REFLECT_RESOURCE_DIMENSION_TEXTURE1D:
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
                case REFLECT_RESOURCE_DIMENSION_BUFFER: // Hack typed buffer as raw buf
                    break;
                default:
                    ASSERT(0);
                    break;
            }
        }
    }

    if (texDim > 0)
    {
        psContext->m_Reflection.OnDiagnostics("Texture atomics are not supported in Metal", 0, true);
        return;
    }

    if (isUint)
        ui32DataTypeFlag = TO_FLAG_UNSIGNED_INTEGER | TO_AUTO_BITCAST_TO_UINT;
    else
        ui32DataTypeFlag = TO_FLAG_INTEGER | TO_AUTO_BITCAST_TO_INT;

    if (compare)
    {
        bcatcstr(glsl, "{\n");
        ++psContext->indent;
        psContext->AddIndentation();
        bcatcstr(glsl, "uint compare_value = ");
        glsl << TranslateOperand(compare, ui32DataTypeFlag);
        bcatcstr(glsl, ";\n");
        psContext->AddIndentation();
    }
    else if (previousValue)
        AddAssignToDest(previousValue, isUint ? SVT_UINT : SVT_INT, 1, psInst->ui32PreciseMask, numParenthesis);

    bcatcstr(glsl, func);
    bcatcstr(glsl, "(");

    uint32_t destAddrFlag = TO_FLAG_UNSIGNED_INTEGER;
    SHADER_VARIABLE_TYPE destAddrType = destAddr->GetDataType(psContext);
    if (destAddrType == SVT_INT || destAddrType == SVT_INT16 || destAddrType == SVT_INT12)
        destAddrFlag = TO_FLAG_INTEGER;

    if (dest->eType == OPERAND_TYPE_UNORDERED_ACCESS_VIEW)
        bcatcstr(glsl, "reinterpret_cast<device atomic_uint *>(&");
    else
        bcatcstr(glsl, "reinterpret_cast<threadgroup atomic_uint *>(&");
    glsl << TranslateOperand(dest, TO_FLAG_DESTINATION | TO_FLAG_NAME_ONLY);
    bcatcstr(glsl, "[");
    glsl << TranslateOperand(destAddr, destAddrFlag, OPERAND_4_COMPONENT_MASK_X);

    if (!psBinding || psBinding->eType != RTYPE_UAV_RWTYPED)
    {
        // Structured buf if we have both x & y swizzles. Raw buf has only x -> no .value[]
        if (destAddr->GetNumSwizzleElements(OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y) == 2)
        {
            bcatcstr(glsl, "]");
            bcatcstr(glsl, ".value[");
            glsl << TranslateOperand(destAddr, destAddrFlag, OPERAND_4_COMPONENT_MASK_Y);
        }

        bcatcstr(glsl, " >> 2");//bytes to floats
        if (destAddrFlag == TO_FLAG_UNSIGNED_INTEGER)
            bcatcstr(glsl, "u");
    }
    bcatcstr(glsl, "]), ");

    if (compare)
        bcatcstr(glsl, "&compare_value, ");

    glsl << TranslateOperand(src, ui32DataTypeFlag);
    bcatcstr(glsl, ", memory_order::memory_order_relaxed");
    if (compare)
        bcatcstr(glsl, ", memory_order::memory_order_relaxed");
    bcatcstr(glsl, ")");
    if (previousValue)
    {
        AddAssignPrologue(numParenthesis);
    }
    else
        bcatcstr(glsl, ";\n");

    if (compare)
    {
        if (previousValue)
        {
            psContext->AddIndentation();
            AddAssignToDest(previousValue, SVT_UINT, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "compare_value");
            AddAssignPrologue(numParenthesis);
        }
        --psContext->indent;
        psContext->AddIndentation();
        bcatcstr(glsl, "}\n");
    }
}

void ToMetal::TranslateConditional(
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
        if (psContext->psShader->eShaderType == COMPUTE_SHADER || (psContext->psShader->eShaderType == PIXEL_SHADER && m_StructDefinitions[GetOutputStructName()].m_Members.size() == 0))
            statement = "return";
        else
            statement = "return output";
    }


    int isBool = psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL;

    if (isBool)
    {
        bcatcstr(glsl, "if(");
        if (psInst->eBooleanTestType != INSTRUCTION_TEST_NONZERO)
            bcatcstr(glsl, "!");
        glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_BOOL);
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
        if (psInst->eBooleanTestType == INSTRUCTION_TEST_ZERO)
        {
            bcatcstr(glsl, "if((");
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER);

            if (psInst->eOpcode != OPCODE_IF)
            {
                bformata(glsl, ")==uint(0)){%s;}\n", statement);
            }
            else
            {
                bcatcstr(glsl, ")==uint(0)){\n");
            }
        }
        else
        {
            ASSERT(psInst->eBooleanTestType == INSTRUCTION_TEST_NONZERO);
            bcatcstr(glsl, "if((");
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER);

            if (psInst->eOpcode != OPCODE_IF)
            {
                bformata(glsl, ")!=uint(0)){%s;}\n", statement);
            }
            else
            {
                bcatcstr(glsl, ")!=uint(0)){\n");
            }
        }
    }
}

void ToMetal::TranslateInstruction(Instruction* psInst)
{
    bstring glsl = *psContext->currentGLSLString;
    int numParenthesis = 0;

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
                    ASSERT(0); // We'd be doing bitcasts into low/mediump ints, not good.
            }
            psContext->AddIndentation();

            AddAssignToDest(&psInst->asOperands[0], castType, srcCount, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeMetal(castType, dstCount));
            bcatcstr(glsl, "("); // 1
            glsl << TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_FLOAT, psInst->asOperands[0].GetAccessMask());
            bcatcstr(glsl, ")"); // 1
            AddAssignPrologue(numParenthesis);
            break;
        }

        case OPCODE_MOV:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//MOV\n");
            }
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

            AddMOVBinaryOp(&psInst->asOperands[0], &psInst->asOperands[1], psInst->ui32PreciseMask);
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
                    ASSERT(0); // We'd be doing bitcasts into low/mediump ints, not good.
            }

            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], castType, srcCount, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeMetal(castType, dstCount));
            bcatcstr(glsl, "("); // 1
            glsl << TranslateOperand(&psInst->asOperands[1], psInst->eOpcode == OPCODE_UTOF ? TO_AUTO_BITCAST_TO_UINT : TO_AUTO_BITCAST_TO_INT, psInst->asOperands[0].GetAccessMask());
            bcatcstr(glsl, ")"); // 1
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
            CallHelper3("fma", psInst, 0, 1, 2, 3, 1);
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
        case OPCODE_DFMA:
        {
            uint32_t ui32Flags = TO_FLAG_DOUBLE;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//DFMA\n");
            }
            CallHelper3("fma", psInst, 0, 1, 2, 3, 1, ui32Flags);
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
                psContext->AddIndentation();
                bcatcstr(glsl, "//IADD\n");
            }
            //Is this a signed or unsigned add?
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_UINT)
            {
                eType = SVT_UINT;
            }
            CallBinaryOp("+", psInst, 0, 1, 2, eType);
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
            if (psInst->asOperands[0].GetDataType(psContext) == SVT_BOOL)
            {
                uint32_t destMask = psInst->asOperands[0].GetAccessMask();

                int needsParenthesis = 0;
                psContext->AddIndentation();
                AddAssignToDest(&psInst->asOperands[0], SVT_BOOL, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, needsParenthesis);
                glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, destMask);
                bcatcstr(glsl, " || ");
                glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_BOOL, destMask);
                AddAssignPrologue(needsParenthesis);
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
                int needsParenthesis = 0;
                psContext->AddIndentation();
                AddAssignToDest(&psInst->asOperands[0], SVT_BOOL, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, needsParenthesis);
                glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_BOOL, destMask);
                bcatcstr(glsl, " && ");
                glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_BOOL, destMask);
                AddAssignPrologue(needsParenthesis);
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
                    AddAssignToDest(&psInst->asOperands[0], eDataType, dstSwizCount, psInst->ui32PreciseMask, needsParenthesis);
                    glsl << TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, " ? ");
                    glsl << TranslateOperand(&psInst->asOperands[otherOp], ui32Flags, destMask);
                    bcatcstr(glsl, " : ");

                    bcatcstr(glsl, GetConstructorForTypeMetal(eDataType, dstSwizCount));
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
                    // We can use select()
                    AddAssignToDest(&psInst->asOperands[0], eDataType, dstSwizCount, psInst->ui32PreciseMask, needsParenthesis);
                    bcatcstr(glsl, "select(");
                    bcatcstr(glsl, GetConstructorForTypeMetal(eDataType, dstSwizCount));
                    bcatcstr(glsl, "(");
                    for (i = 0; i < dstSwizCount; i++)
                    {
                        if (i > 0)
                            bcatcstr(glsl, ", ");
                        bcatcstr(glsl, "0.0");
                    }
                    bcatcstr(glsl, "), ");
                    glsl << TranslateOperand(&psInst->asOperands[otherOp], ui32Flags, destMask);
                    bcatcstr(glsl, ", ");
                    bcatcstr(glsl, GetConstructorForTypeMetal(SVT_BOOL, dstSwizCount));
                    bcatcstr(glsl, "(");
                    glsl << TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, ")");
                    bcatcstr(glsl, ")");
                }
                else
                {
                    AddAssignToDest(&psInst->asOperands[0], SVT_UINT, dstSwizCount, psInst->ui32PreciseMask, needsParenthesis);
                    bcatcstr(glsl, "(");
                    bcatcstr(glsl, GetConstructorForTypeMetal(SVT_UINT, dstSwizCount));
                    bcatcstr(glsl, "(");
                    glsl << TranslateOperand(&psInst->asOperands[boolOp], TO_FLAG_BOOL, destMask);
                    bcatcstr(glsl, ") * 0xffffffffu) & ");
                    glsl << TranslateOperand(&psInst->asOperands[otherOp], TO_FLAG_UNSIGNED_INTEGER, destMask);
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
            SHADER_VARIABLE_TYPE dstType = psInst->asOperands[0].GetDataType(psContext);
            uint32_t typeFlags = TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC2;
            if (CanForceToHalfOperand(&psInst->asOperands[1])
                && CanForceToHalfOperand(&psInst->asOperands[2]))
                typeFlags = TO_FLAG_FORCE_HALF | TO_AUTO_EXPAND_TO_VEC2;

            if (dstType != SVT_FLOAT16)
                dstType = SVT_FLOAT;

            AddAssignToDest(&psInst->asOperands[0], dstType, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "dot(");
            glsl << TranslateOperand(&psInst->asOperands[1], typeFlags, 3 /* .xy */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], typeFlags, 3 /* .xy */);
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
            SHADER_VARIABLE_TYPE dstType = psInst->asOperands[0].GetDataType(psContext);
            uint32_t typeFlags = TO_AUTO_BITCAST_TO_FLOAT | TO_AUTO_EXPAND_TO_VEC3;
            if (CanForceToHalfOperand(&psInst->asOperands[1])
                && CanForceToHalfOperand(&psInst->asOperands[2]))
                typeFlags = TO_FLAG_FORCE_HALF | TO_AUTO_EXPAND_TO_VEC3;

            if (dstType != SVT_FLOAT16)
                dstType = SVT_FLOAT;

            AddAssignToDest(&psInst->asOperands[0], dstType, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "dot(");
            glsl << TranslateOperand(&psInst->asOperands[1], typeFlags, 7 /* .xyz */);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], typeFlags, 7 /* .xyz */);
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
            ASSERT(0);
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
            CallHelper1("rsqrt", psInst, 0, 1, 1);
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
            CallHelper1("rint", psInst, 0, 1, 1);
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
            if (psContext->psShader->eShaderType == COMPUTE_SHADER || (psContext->psShader->eShaderType == PIXEL_SHADER && m_StructDefinitions[GetOutputStructName()].m_Members.size() == 0))
                bcatcstr(glsl, "return;\n");
            else
                bcatcstr(glsl, "return output;\n");

            break;
        }
        case OPCODE_INTERFACE_CALL:
        {
            ASSERT(0);
        }
        case OPCODE_LABEL:
        {
            ASSERT(0); // Never seen this
        }
        case OPCODE_COUNTBITS:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//COUNTBITS\n");
            }
            psContext->AddIndentation();

            // in metal popcount decl is T popcount(T), so it is important that input/output types agree
            // enter assembly: when writing swizzle encoding we use 0 to say "source from x"
            // now, say, we generate code o.xy = bitcount(i.xy)
            //   output gets component mask 1,1,0,0 (note that we use bit 1<<i to indicate if we should use component)
            //   input gets swizzle mask (2 bits sectios) 0,2,0,0; which is interpreted as xyxx
            // from the assembly standpoint this is fine, but, as indicated above, in metal code we should be more careful
            // NOTE: this is pretty general issue for functions like that - i am not sure if we can/should fix it generally
            // anyway here we are. It *seems* that doing popcount(i.<..>).<..> will still collapse everything into
            //   popcount(i.<..>) [well, tweaking swizzle, sure]
            // what does that mean is that we can safely take output component count to determine "proper" type
            // note that hlsl compiler already checked that things can work out, so it should be fine doing this magic

            const Operand* dst = &psInst->asOperands[0];
            const int dstCompCount = dst->eSelMode == OPERAND_4_COMPONENT_MASK_MODE ? dst->ui32CompMask : OPERAND_4_COMPONENT_MASK_ALL;

            glsl << TranslateOperand(dst, TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = popcount(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, dstCompCount);
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
            DeclareExtraFunction("firstBit_hi", "template <typename UVecType> UVecType firstBit_hi(const UVecType input) { UVecType res = clz(input); return res; };");
            // TODO implement the 0-case (must return 0xffffffff)
            psContext->AddIndentation();
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = firstBit_hi(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER);
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
            // TODO implement the 0-case (must return 0xffffffff)
            DeclareExtraFunction("firstBit_lo", "template <typename UVecType> UVecType firstBit_lo(const UVecType input) { UVecType res = ctz(input); return res; };");
            psContext->AddIndentation();
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_UNSIGNED_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = firstBit_lo(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER);
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
            // TODO Not at all correct for negative values yet.
            DeclareExtraFunction("firstBit_shi", "template <typename IVecType> IVecType firstBit_shi(const IVecType input) { IVecType res = clz(input); return res; };");
            psContext->AddIndentation();
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = firstBit_shi(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER);
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
            DeclareExtraFunction("bitReverse", "template <typename UVecType> UVecType bitReverse(const UVecType input)\n\
\t\t{ UVecType x = input;\n\
\t\t\tx = (((x & 0xaaaaaaaa) >> 1) | ((x & 0x55555555) << 1));\n\
\t\t\tx = (((x & 0xcccccccc) >> 2) | ((x & 0x33333333) << 2));\n\
\t\t\tx = (((x & 0xf0f0f0f0) >> 4) | ((x & 0x0f0f0f0f) << 4));\n\
\t\t\tx = (((x & 0xff00ff00) >> 8) | ((x & 0x00ff00ff) << 8));\n\
\t\t\treturn((x >> 16) | (x << 16));\n\
\t\t}; ");
            psContext->AddIndentation();
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER | TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = bitReverse(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER);
            bcatcstr(glsl, ");\n");
            break;
        }
        case OPCODE_BFI:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BFI\n");
            }
            DeclareExtraFunction("BFI", "\
\t\ttemplate <typename UVecType> UVecType bitFieldInsert(const UVecType width, const UVecType offset, const UVecType src2, const UVecType src3)\n\
\t\t{\n\
\t\t\tUVecType bitmask = (((UVecType(1) << width)-1) << offset) & 0xffffffff;\n\
\t\t\treturn ((src2 << offset) & bitmask) | (src3 & ~bitmask);\n\
\t\t}; ");
            psContext->AddIndentation();

            uint32_t destMask = psInst->asOperands[0].GetAccessMask();
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "bitFieldInsert(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[3], TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[4], TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ")");

            AddAssignPrologue(numParenthesis);
            break;
        }
        case OPCODE_CUT:
        case OPCODE_EMITTHENCUT_STREAM:
        case OPCODE_EMIT:
        case OPCODE_EMITTHENCUT:
        case OPCODE_CUT_STREAM:
        case OPCODE_EMIT_STREAM:
        {
            ASSERT(0); // Not on metal
        }
        case OPCODE_REP:
        case OPCODE_ENDREP:
        {
            ASSERT(0); // Shouldn't see these anymore
        }
        case OPCODE_LOOP:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//LOOP\n");
            }
            psContext->AddIndentation();

            bcatcstr(glsl, "while(true){\n");
            ++psContext->indent;
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
            break;
        }
        case OPCODE_BREAK:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//BREAK\n");
            }
            psContext->AddIndentation();
            bcatcstr(glsl, "break;\n");
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

            TranslateConditional(psInst, glsl);
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
            break;
        }
        case OPCODE_ENDSWITCH:
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
            bcatcstr(glsl, "default:\n");
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
            const bool sync_threadgroup = (ui32SyncFlags & SYNC_THREAD_GROUP_SHARED_MEMORY) != 0;
            const bool sync_device = (ui32SyncFlags & (SYNC_UNORDERED_ACCESS_VIEW_MEMORY_GROUP | SYNC_UNORDERED_ACCESS_VIEW_MEMORY_GLOBAL)) != 0;

            const char* barrierFlags = "mem_flags::mem_none";
            if (sync_threadgroup && sync_device) barrierFlags = "mem_flags::mem_threadgroup | mem_flags::mem_device";
            else if (sync_threadgroup)           barrierFlags = "mem_flags::mem_threadgroup";
            else if (sync_device)                barrierFlags = "mem_flags::mem_device";

            if (ui32SyncFlags & SYNC_THREADS_IN_GROUP)
            {
                psContext->AddIndentation();
                bformata(glsl, "threadgroup_barrier(%s);\n", barrierFlags);
            }
            else
            {
                psContext->AddIndentation(); bformata(glsl, "#if __HAVE_SIMDGROUP_BARRIER__\n");
                psContext->AddIndentation(); bformata(glsl, "simdgroup_barrier(%s);\n", barrierFlags);
                psContext->AddIndentation(); bformata(glsl, "#else\n");
                psContext->AddIndentation(); bformata(glsl, "threadgroup_barrier(%s);\n", barrierFlags);
                psContext->AddIndentation(); bformata(glsl, "#endif\n");
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
            psContext->AddIndentation();
            bcatcstr(glsl, "switch(int(");
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
            bcatcstr(glsl, ")){\n");

            psContext->indent += 2;
            break;
        }
        case OPCODE_CASE:
        {
            --psContext->indent;
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//case\n");
            }
            psContext->AddIndentation();

            bcatcstr(glsl, "case ");
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
            bcatcstr(glsl, ":\n");

            ++psContext->indent;
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

            if (psInst->eResDim == RESOURCE_DIMENSION_BUFFER) // Hack typed buffer as raw buf
            {
                psInst->eOpcode = OPCODE_LD_UAV_TYPED;
                psInst->asOperands[1].eSelMode = OPERAND_4_COMPONENT_SELECT_1_MODE;
                if (psInst->asOperands[1].eType == OPERAND_TYPE_IMMEDIATE32)
                    psInst->asOperands[1].iNumComponents = 1;
                TranslateShaderStorageLoad(psInst);
                break;
            }

            if (psInst->bAddressOffset)
            {
                TranslateTexelFetchOffset(psInst, psBinding, glsl);
            }
            else
            {
                TranslateTexelFetch(psInst, psBinding, glsl);
            }
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
            if (psInst->eBooleanTestType == INSTRUCTION_TEST_ZERO)
            {
                bcatcstr(glsl, "if((");
                glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
                bcatcstr(glsl, ")==0){discard_fragment();}\n");
            }
            else
            {
                ASSERT(psInst->eBooleanTestType == INSTRUCTION_TEST_NONZERO);
                bcatcstr(glsl, "if((");
                glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_INTEGER);
                bcatcstr(glsl, ")!=0){discard_fragment();}\n");
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
            AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 4, psInst->ui32PreciseMask, numParenthesis);

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

            glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
            bcatcstr(glsl, ",");
            TranslateTexCoord(
                psContext->psShader->aeResourceDims[psInst->asOperands[2].ui32RegisterNumber],
                &psInst->asOperands[1]);
            bcatcstr(glsl, ")");

            //The swizzle on srcResource allows the returned values to be swizzled arbitrarily before they are written to the destination.

            // iWriteMaskEnabled is forced off during DecodeOperand because swizzle on sampler uniforms
            // does not make sense. But need to re-enable to correctly swizzle this particular instruction.
            psInst->asOperands[2].iWriteMaskEnabled = 1;
            glsl << TranslateOperandSwizzle(&psInst->asOperands[2], psInst->asOperands[0].GetAccessMask(), 0);
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
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtCentroid(");
            //interpolateAtCentroid accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
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
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtSample(");
            //interpolateAtSample accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_INTEGER);
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
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = interpolateAtOffset(");
            //interpolateAtOffset accepts in-qualified variables.
            //As long as bytecode only writes vX registers in declarations
            //we should be able to use the declared name directly.
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_DECLARATION_NAME);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_INTEGER);
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

            const ResourceBinding* psRes = 0;
            psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, psSrc->ui32RegisterNumber, &psRes);
            SHADER_VARIABLE_TYPE srcDataType = ResourceReturnTypeToSVTType(psRes->ui32ReturnType, psRes->ePrecision);

            if (psInst->eResDim == RESOURCE_DIMENSION_BUFFER) // Hack typed buffer as raw buf
            {
                psSrc->aeDataType[0] = srcDataType;
                psSrcAddr->eSelMode = OPERAND_4_COMPONENT_SELECT_1_MODE;
                if (psSrcAddr->eType == OPERAND_TYPE_IMMEDIATE32)
                    psSrcAddr->iNumComponents = 1;
                TranslateShaderStorageLoad(psInst);
                break;
            }

#define RRD(n) REFLECT_RESOURCE_DIMENSION_ ## n

            // unlike glsl, texture arrays will have index in separate argument
            const bool isArray = psRes->eDimension == RRD(TEXTURE1DARRAY) || psRes->eDimension == RRD(TEXTURE2DARRAY)
                || psRes->eDimension == RRD(TEXTURE2DMSARRAY) || psRes->eDimension == RRD(TEXTURECUBEARRAY);

            uint32_t flags = TO_FLAG_UNSIGNED_INTEGER, opMask = OPERAND_4_COMPONENT_MASK_ALL;
            switch (psRes->eDimension)
            {
                case RRD(TEXTURE3D):
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
                    flags |= TO_AUTO_EXPAND_TO_VEC3;
                    break;
                case RRD(TEXTURECUBE): case RRD(TEXTURECUBEARRAY):
                case RRD(TEXTURE2DARRAY): case RRD(TEXTURE2DMSARRAY): case RRD(TEXTURE2D): case RRD(TEXTURE2DMS):
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
                    flags |= TO_AUTO_EXPAND_TO_VEC2;
                    break;
                case RRD(TEXTURE1D): case RRD(TEXTURE1DARRAY):
                    opMask = OPERAND_4_COMPONENT_MASK_X;
                    break;
                default:
                    ASSERT(0); break;
            }

            int srcCount = psSrc->GetNumSwizzleElements(), numParenthesis = 0;
            psContext->AddIndentation();
            AddAssignToDest(psDest, srcDataType, srcCount, psInst->ui32PreciseMask, numParenthesis);
            glsl << TranslateOperand(psSrc, TO_FLAG_NAME_ONLY);
            bcatcstr(glsl, ".read(");
            glsl << TranslateOperand(psSrcAddr, flags, opMask);
            if (isArray)
            {
                // NB cube array is handled incorrectly - it needs extra "face" arg
                switch (psRes->eDimension)
                {
                    case RRD(TEXTURE1DARRAY): opMask = OPERAND_4_COMPONENT_MASK_Y; break;
                    case RRD(TEXTURE2DARRAY): case RRD(TEXTURE2DMSARRAY): opMask = OPERAND_4_COMPONENT_MASK_Z; break;
                    case RRD(TEXTURECUBEARRAY): opMask = OPERAND_4_COMPONENT_MASK_W; break;
                    default: ASSERT(0); break;
                }

                bcatcstr(glsl, ", ");
                glsl << TranslateOperand(psSrcAddr, TO_FLAG_UNSIGNED_INTEGER, opMask);
            }
            bcatcstr(glsl, ")");
            glsl << TranslateOperandSwizzle(psSrc, psDest->ui32CompMask, 0);
            AddAssignPrologue(numParenthesis);

#undef RRD

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

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//STORE_UAV_TYPED\n");
            }
            foundResource = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV,
                psInst->asOperands[0].ui32RegisterNumber,
                &psRes);
            ASSERT(foundResource);

            if (psRes->eDimension == REFLECT_RESOURCE_DIMENSION_BUFFER) // Hack typed buffer as raw buf
            {
                psInst->asOperands[0].aeDataType[0] = ResourceReturnTypeToSVTType(psRes->ui32ReturnType, psRes->ePrecision);
                psInst->asOperands[1].eSelMode = OPERAND_4_COMPONENT_SELECT_1_MODE;
                if (psInst->asOperands[1].eType == OPERAND_TYPE_IMMEDIATE32)
                    psInst->asOperands[1].iNumComponents = 1;
                TranslateShaderStorageStore(psInst);
                break;
            }

            psContext->AddIndentation();

            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_NAME_ONLY);
            bcatcstr(glsl, ".write(");

    #define RRD(n) REFLECT_RESOURCE_DIMENSION_ ## n

            // unlike glsl, texture arrays will have index in separate argument
            const bool isArray = psRes->eDimension == RRD(TEXTURE1DARRAY) || psRes->eDimension == RRD(TEXTURE2DARRAY)
                || psRes->eDimension == RRD(TEXTURE2DMSARRAY) || psRes->eDimension == RRD(TEXTURECUBEARRAY);

            uint32_t flags = TO_FLAG_UNSIGNED_INTEGER, opMask = OPERAND_4_COMPONENT_MASK_ALL;
            switch (psRes->eDimension)
            {
                case RRD(TEXTURE1D): case RRD(TEXTURE1DARRAY):
                    opMask = OPERAND_4_COMPONENT_MASK_X;
                    break;
                case RRD(TEXTURE2D): case RRD(TEXTURE2DMS): case RRD(TEXTURE2DARRAY): case RRD(TEXTURE2DMSARRAY):
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y;
                    flags |= TO_AUTO_EXPAND_TO_VEC2;
                    break;
                case RRD(TEXTURE3D): case RRD(TEXTURECUBE): case RRD(TEXTURECUBEARRAY):
                    opMask = OPERAND_4_COMPONENT_MASK_X | OPERAND_4_COMPONENT_MASK_Y | OPERAND_4_COMPONENT_MASK_Z;
                    flags |= TO_AUTO_EXPAND_TO_VEC3;
                    break;
                default:
                    ASSERT(0);
                    break;
            }


            glsl << TranslateOperand(&psInst->asOperands[2], ResourceReturnTypeToFlag(psRes->ui32ReturnType));
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[1], flags, opMask);
            if (isArray)
            {
                // NB cube array is handled incorrectly - it needs extra "face" arg
                flags = TO_FLAG_UNSIGNED_INTEGER;
                switch (psRes->eDimension)
                {
                    case RRD(TEXTURE1DARRAY): opMask = OPERAND_4_COMPONENT_MASK_Y; break;
                    case RRD(TEXTURE2DARRAY): case RRD(TEXTURE2DMSARRAY): opMask = OPERAND_4_COMPONENT_MASK_Z; break;
                    case RRD(TEXTURECUBEARRAY): opMask = OPERAND_4_COMPONENT_MASK_Z; break;
                    default: ASSERT(0); break;
                }

                bcatcstr(glsl, ", ");
                glsl << TranslateOperand(&psInst->asOperands[1], flags, opMask);
            }
            bformata(glsl, ");\n");

#undef RRD

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

        case OPCODE_ATOMIC_CMP_STORE:
        case OPCODE_IMM_ATOMIC_AND:
        case OPCODE_ATOMIC_AND:
        case OPCODE_IMM_ATOMIC_IADD:
        case OPCODE_ATOMIC_IADD:
        case OPCODE_ATOMIC_OR:
        case OPCODE_ATOMIC_XOR:
        case OPCODE_ATOMIC_IMAX:
        case OPCODE_ATOMIC_IMIN:
        case OPCODE_ATOMIC_UMAX:
        case OPCODE_ATOMIC_UMIN:
        case OPCODE_IMM_ATOMIC_IMAX:
        case OPCODE_IMM_ATOMIC_IMIN:
        case OPCODE_IMM_ATOMIC_UMAX:
        case OPCODE_IMM_ATOMIC_UMIN:
        case OPCODE_IMM_ATOMIC_OR:
        case OPCODE_IMM_ATOMIC_XOR:
        case OPCODE_IMM_ATOMIC_EXCH:
        case OPCODE_IMM_ATOMIC_CMP_EXCH:
        {
            TranslateAtomicMemOp(psInst);
            break;
        }
        case OPCODE_UBFE:
        case OPCODE_IBFE:
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                if (psInst->eOpcode == OPCODE_UBFE)
                    bcatcstr(glsl, "//OPCODE_UBFE\n");
                else
                    bcatcstr(glsl, "//OPCODE_IBFE\n");
            }

            bool isUBFE = psInst->eOpcode == OPCODE_UBFE;
            bool isScalar = psInst->asOperands[0].GetNumSwizzleElements() == 1;

            if (isUBFE)
            {
                if (isScalar)
                {
                    DeclareExtraFunction("UBFE", "\
uint bitFieldExtractU(uint width, uint offset, uint src);\n\
uint bitFieldExtractU(uint width, uint offset, uint src)\n\
{\n\
\tbool isWidthZero = (width == 0);\n\
\tbool needsClamp = ((width + offset) < 32);\n\
\tuint clampVersion = src << (32-(width+offset));\n\
\tclampVersion = clampVersion >> (32 - width);\n\
\tuint simpleVersion = src >> offset;\n\
\tuint res = select(simpleVersion, clampVersion, needsClamp);\n\
\treturn select(res, (uint)0, isWidthZero);\n\
}; ");
                }
                else
                {
                    DeclareExtraFunction("UBFEV", "\
template <int N> vec<uint, N> bitFieldExtractU(const vec<uint, N> width, const vec<uint, N> offset, const vec<uint, N> src)\n\
{\n\
\tvec<bool, N> isWidthZero = (width == 0);\n\
\tvec<bool, N> needsClamp = ((width + offset) < 32);\n\
\tvec<uint, N> clampVersion = src << (32-(width+offset));\n\
\tclampVersion = clampVersion >> (32 - width);\n\
\tvec<uint, N> simpleVersion = src >> offset;\n\
\tvec<uint, N> res = select(simpleVersion, clampVersion, needsClamp);\n\
\treturn select(res, vec<uint, N>(0), isWidthZero);\n\
}; ");
                }
            }
            else
            {
                if (isScalar)
                {
                    DeclareExtraFunction("IBFE", "\
template int bitFieldExtractI(uint width, uint offset, int src)\n\
{\n\
\tbool isWidthZero = (width == 0);\n\
\tbool needsClamp = ((width + offset) < 32);\n\
\tint clampVersion = src << (32-(width+offset));\n\
\tclampVersion = clampVersion >> (32 - width);\n\
\tint simpleVersion = src >> offset;\n\
\tint res = select(simpleVersion, clampVersion, needsClamp);\n\
\treturn select(res, (int)0, isWidthZero);\n\
}; ");
                }
                else
                {
                    DeclareExtraFunction("IBFEV", "\
template <int N> vec<int, N> bitFieldExtractI(const vec<uint, N> width, const vec<uint, N> offset, const vec<int, N> src)\n\
{\n\
\tvec<bool, N> isWidthZero = (width == 0);\n\
\tvec<bool, N> needsClamp = ((width + offset) < 32);\n\
\tvec<int, N> clampVersion = src << (32-(width+offset));\n\
\tclampVersion = clampVersion >> (32 - width);\n\
\tvec<int, N> simpleVersion = src >> offset;\n\
\tvec<int, N> res = select(simpleVersion, clampVersion, needsClamp);\n\
\treturn select(res, vec<int, N>(0), isWidthZero);\n\
}; ");
                }
            }
            psContext->AddIndentation();

            uint32_t destMask = psInst->asOperands[0].GetAccessMask();
            uint32_t src2SwizCount = psInst->asOperands[3].GetNumSwizzleElements(destMask);
            uint32_t src1SwizCount = psInst->asOperands[2].GetNumSwizzleElements(destMask);
            uint32_t src0SwizCount = psInst->asOperands[1].GetNumSwizzleElements(destMask);
            uint32_t ui32Flags = 0;

            if (src1SwizCount != src0SwizCount || src2SwizCount != src0SwizCount)
            {
                uint32_t maxElems = std::max(src2SwizCount, std::max(src1SwizCount, src0SwizCount));
                ui32Flags |= (TO_AUTO_EXPAND_TO_VEC2 << (maxElems - 2));
            }

            AddAssignToDest(&psInst->asOperands[0], isUBFE ? SVT_UINT : SVT_INT, psInst->asOperands[0].GetNumSwizzleElements(), psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "bitFieldExtract");
            bcatcstr(glsl, isUBFE ? "U" : "I");
            bcatcstr(glsl, "(");
            glsl << TranslateOperand(&psInst->asOperands[1], ui32Flags | TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], ui32Flags | TO_FLAG_UNSIGNED_INTEGER, destMask);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[3], ui32Flags | (isUBFE ? TO_FLAG_UNSIGNED_INTEGER : TO_FLAG_INTEGER), destMask);
            bcatcstr(glsl, ")");
            AddAssignPrologue(numParenthesis);
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

            SHADER_VARIABLE_TYPE dstType = psInst->asOperands[0].GetDataType(psContext);
            SHADER_VARIABLE_TYPE srcType = psInst->asOperands[1].GetDataType(psContext);

            uint32_t typeFlags = TO_FLAG_NONE;
            if (dstType == SVT_FLOAT16 && srcType == SVT_FLOAT16)
            {
                typeFlags = TO_FLAG_FORCE_HALF;
            }
            else
                srcType = SVT_FLOAT;

            AddAssignToDest(&psInst->asOperands[0], srcType, srcElemCount, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, GetConstructorForTypeMetal(srcType, destElemCount));
            bcatcstr(glsl, "(1.0) / ");
            bcatcstr(glsl, GetConstructorForTypeMetal(srcType, destElemCount));
            bcatcstr(glsl, "(");
            numParenthesis++;
            glsl << TranslateOperand(&psInst->asOperands[1], typeFlags, psInst->asOperands[0].GetAccessMask());
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
                AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, numParenthesis);

                bcatcstr(glsl, "as_type<uint>(half2(");
                glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE, (1 << i));
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
                AddAssignToDest(&psInst->asOperands[0], SVT_FLOAT, 1, psInst->ui32PreciseMask, numParenthesis);

                bcatcstr(glsl, "as_type<half2>(");
                glsl << TranslateOperand(&psInst->asOperands[1], TO_AUTO_BITCAST_TO_UINT, (1 << i));
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

            AddAssignToDest(&psInst->asOperands[0], SVT_INT, psInst->asOperands[1].GetNumSwizzleElements(), psInst->ui32PreciseMask, numParenthesis);

            bcatcstr(glsl, "0 - ");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, psInst->asOperands[0].GetAccessMask());
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
            CallHelper1("dfdx", psInst, 0, 1, 1);
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
            CallHelper1("dfdy", psInst, 0, 1, 1);
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
            bool isFP16 = false;
            if (CanForceToHalfOperand(&psInst->asOperands[0])
                && CanForceToHalfOperand(&psInst->asOperands[1])
                && CanForceToHalfOperand(&psInst->asOperands[2])
                && CanForceToHalfOperand(&psInst->asOperands[2]))
                isFP16 = true;
            int parenthesis = 0;
            AddAssignToDest(&psInst->asOperands[0], isFP16 ? SVT_FLOAT16 : SVT_FLOAT, 2, psInst->ui32PreciseMask, parenthesis);

            uint32_t flags = TO_AUTO_EXPAND_TO_VEC2;
            flags |= isFP16 ? TO_FLAG_FORCE_HALF : TO_AUTO_BITCAST_TO_FLOAT;

            bcatcstr(glsl, "dot(");
            glsl << TranslateOperand(&psInst->asOperands[1], flags);
            bcatcstr(glsl, ", ");
            glsl << TranslateOperand(&psInst->asOperands[2], flags);
            bcatcstr(glsl, ") + ");
            glsl << TranslateOperand(&psInst->asOperands[3], flags);
            AddAssignPrologue(parenthesis);
            break;
        }
        case OPCODE_POW:
        {
            // TODO Check POW opcode whether it actually needs the abs
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//POW\n");
            }
            psContext->AddIndentation();
            glsl << TranslateOperand(&psInst->asOperands[0], TO_FLAG_DESTINATION);
            bcatcstr(glsl, " = powr(abs(");
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_NONE);
            bcatcstr(glsl, "), ");
            glsl << TranslateOperand(&psInst->asOperands[2], TO_FLAG_NONE);
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
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "atomic_fetch_add_explicit(");
            glsl << ResourceName(RGROUP_UAV, psInst->asOperands[1].ui32RegisterNumber);
            bcatcstr(glsl, "_counter, 1, memory_order::memory_order_relaxed)");
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
            AddAssignToDest(&psInst->asOperands[0], SVT_UINT, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, "atomic_fetch_sub_explicit(");
            glsl << ResourceName(RGROUP_UAV, psInst->asOperands[1].ui32RegisterNumber);
            // Metal atomic sub returns previous value. Therefore minus one here to get the correct data index.
            bcatcstr(glsl, "_counter, 1, memory_order::memory_order_relaxed) - 1");
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
            psContext->AddIndentation();
            AddAssignToDest(&psInst->asOperands[0], SVT_INT, psInst->asOperands[1].GetNumSwizzleElements(), psInst->ui32PreciseMask, numParenthesis);

            bcatcstr(glsl, "~(");
            numParenthesis++;
            glsl << TranslateOperand(&psInst->asOperands[1], TO_FLAG_INTEGER, psInst->asOperands[0].GetAccessMask());
            AddAssignPrologue(numParenthesis);
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
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(glsl, "//RESINFO\n");
            }

            const uint32_t mask = psInst->asOperands[0].GetAccessMask();
            for (int i = 0; i < 4; ++i)
            {
                if ((1 << i) & mask)
                    GetResInfoData(psInst, psInst->asOperands[2].aui32Swizzle[i], i);
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
            psContext->m_Reflection.OnDiagnostics("Metal shading language does not support buffer size query from shader. Pass the size to shader as const instead.\n", 0, true);
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
            AddAssignToDest(&psInst->asOperands[0], eResInfoReturnType == RESINFO_INSTRUCTION_RETURN_UINT ? SVT_UINT : SVT_FLOAT, 1, psInst->ui32PreciseMask, numParenthesis);
            bcatcstr(glsl, TranslateOperand(&psInst->asOperands[1], TO_FLAG_NAME_ONLY).c_str());
            bcatcstr(glsl, ".get_num_samples()");
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
        psContext->AddIndentation();
        bool isFP16 = false;
        if (psInst->asOperands[0].GetDataType(psContext) == SVT_FLOAT16)
            isFP16 = true;
        AddAssignToDest(&psInst->asOperands[0], isFP16 ? SVT_FLOAT16 : SVT_FLOAT, dstCount, psInst->ui32PreciseMask, numParenthesis);
        bcatcstr(glsl, "clamp(");

        glsl << TranslateOperand(&psInst->asOperands[0], isFP16 ? TO_FLAG_FORCE_HALF : TO_AUTO_BITCAST_TO_FLOAT);
        if (isFP16)
            bcatcstr(glsl, ", 0.0h, 1.0h)");
        else
            bcatcstr(glsl, ", 0.0f, 1.0f)");
        AddAssignPrologue(numParenthesis);
    }
}

#if ENABLE_UNIT_TESTS

#define UNITY_EXTERNAL_TOOL 1
#include "Projects/PrecompiledHeaders/UnityPrefix.h" // Needed for defines such as ENABLE_CPP_EXCEPTIONS
#include "Runtime/Testing/Testing.h"

UNIT_TEST_SUITE(ToMetalInstructionTests)
{
    static void TestAddOpAssignToDest(const char* expect, SHADER_VARIABLE_TYPE srcType, uint32_t srcDim, SHADER_VARIABLE_TYPE dstType, uint32_t dstDim)
    {
        bstring actual = bfromcstralloc(20, "");
        bstring expected = bfromcstralloc(20, expect);
        int parenthesis = 0;
        AddOpAssignToDest(actual, srcType, srcDim, dstType, dstDim, 0, parenthesis);
        CHECK(bstrcmp(actual, expected) == 0);
        bdestroy(actual);
        bdestroy(expected);
    }

    TEST(AddOpAssignToDest_Works)
    {
        // Different Type
        TestAddOpAssignToDest(" = as_type<float>(", SVT_INT, 1, SVT_FLOAT, 1);
        TestAddOpAssignToDest(" = uint(", SVT_INT, 1, SVT_UINT, 1);
        TestAddOpAssignToDest(" = as_type<int>(", SVT_FLOAT, 1, SVT_INT, 1);
        TestAddOpAssignToDest(" = as_type<uint>(", SVT_FLOAT, 1, SVT_UINT, 1);

        TestAddOpAssignToDest(" = as_type<half>(", SVT_INT16, 1, SVT_FLOAT16, 1);
        TestAddOpAssignToDest(" = ushort(", SVT_INT16, 1, SVT_UINT16, 1);
        TestAddOpAssignToDest(" = as_type<short>(", SVT_FLOAT16, 1, SVT_INT16, 1);
        TestAddOpAssignToDest(" = as_type<ushort>(", SVT_FLOAT16, 1, SVT_UINT16, 1);

        // Simply assign
        TestAddOpAssignToDest(" = ", SVT_UINT16, 1, SVT_UINT16, 1);
        TestAddOpAssignToDest(" = ", SVT_INT, 4, SVT_INT, 2);

        // Up cast
        TestAddOpAssignToDest(" = uint(", SVT_UINT16, 1, SVT_UINT, 1);
        TestAddOpAssignToDest(" = float(", SVT_FLOAT16, 1, SVT_FLOAT, 1);
        TestAddOpAssignToDest(" = int(", SVT_INT16, 1, SVT_INT, 1);

        // Down cast
        TestAddOpAssignToDest(" = ushort(", SVT_UINT, 1, SVT_UINT16, 1);
        TestAddOpAssignToDest(" = half(", SVT_FLOAT, 1, SVT_FLOAT16, 1);
        TestAddOpAssignToDest(" = short(", SVT_INT, 1, SVT_INT16, 1);

        // Increase dimensions
        TestAddOpAssignToDest(" = float4(", SVT_FLOAT, 1, SVT_FLOAT, 4);
        TestAddOpAssignToDest(" = uint3(", SVT_UINT, 1, SVT_UINT, 3);
        TestAddOpAssignToDest(" = uint2(", SVT_UINT, 1, SVT_UINT, 2);

        // Decrease dimensions
        TestAddOpAssignToDest(" = ", SVT_FLOAT, 4, SVT_FLOAT, 1);
        TestAddOpAssignToDest(" = ", SVT_UINT, 3, SVT_UINT, 1);
        TestAddOpAssignToDest(" = ", SVT_UINT, 2, SVT_UINT, 1);

        // Reinterop cast + Increase dimensions
        TestAddOpAssignToDest(" = as_type<float4>(int4(", SVT_INT, 1, SVT_FLOAT, 4);
        TestAddOpAssignToDest(" = uint4(", SVT_INT, 1, SVT_UINT, 4);
        TestAddOpAssignToDest(" = as_type<int4>(float4(", SVT_FLOAT, 1, SVT_INT, 4);
        TestAddOpAssignToDest(" = as_type<uint4>(float4(", SVT_FLOAT, 1, SVT_UINT, 4);

        // Reinterop cast + Decrease dimensions
        TestAddOpAssignToDest(" = as_type<float>(", SVT_INT, 4, SVT_FLOAT, 1);
        TestAddOpAssignToDest(" = uint(", SVT_INT, 4, SVT_UINT, 1);
        TestAddOpAssignToDest(" = as_type<int>(", SVT_FLOAT, 4, SVT_INT, 1);
        TestAddOpAssignToDest(" = as_type<uint>(", SVT_FLOAT, 4, SVT_UINT, 1);

        // Different precision + Different Type
        TestAddOpAssignToDest(" = float4(", SVT_INT16, 4, SVT_FLOAT, 4);
        TestAddOpAssignToDest(" = short4(", SVT_FLOAT, 4, SVT_INT16, 4);

        // Sanity check as low precision not used in metal they should fall back
        TestAddOpAssignToDest(" = short4(", SVT_FLOAT, 4, SVT_INT12, 4);
        TestAddOpAssignToDest(" = half4(", SVT_INT, 4, SVT_FLOAT10, 4);
    }
}
#endif
