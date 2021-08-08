#include "internal_includes/toMetal.h"
#include "internal_includes/debug.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/Declaration.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/languages.h"
#include <algorithm>
#include <sstream>
#include <cmath>

using namespace HLSLcc;

#ifndef fpcheck
#ifdef _MSC_VER
#define fpcheck(x) (_isnan(x) || !_finite(x))
#else
#define fpcheck(x) (std::isnan(x) || std::isinf(x))
#endif
#endif // #ifndef fpcheck


bool ToMetal::TranslateSystemValue(const Operand *psOperand, const ShaderInfo::InOutSignature *sig, std::string &result, uint32_t *pui32IgnoreSwizzle, bool isIndexed, bool isInput, bool *outSkipPrefix, int *iIgnoreRedirect)
{
    if (sig)
    {
        if (psContext->psShader->eShaderType == HULL_SHADER && sig->semanticName == "SV_TessFactor")
        {
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            ASSERT(sig->ui32SemanticIndex <= 3);
            std::ostringstream oss;
            oss << "tessFactor.edgeTessellationFactor[" << sig->ui32SemanticIndex << "]";
            result = oss.str();
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (iIgnoreRedirect != NULL) *iIgnoreRedirect = 1;
            return true;
        }

        if (psContext->psShader->eShaderType == HULL_SHADER && sig->semanticName == "SV_InsideTessFactor")
        {
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            ASSERT(sig->ui32SemanticIndex <= 1);
            std::ostringstream oss;
            oss << "tessFactor.insideTessellationFactor";
            if (psContext->psShader->sInfo.eTessDomain != TESSELLATOR_DOMAIN_TRI)
                oss << "[" << sig->ui32SemanticIndex << "]";
            result = oss.str();
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (iIgnoreRedirect != NULL) *iIgnoreRedirect = 1;
            return true;
        }

        if (sig->semanticName == "SV_InstanceID")
        {
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
        }

        if (((sig->eSystemValueType == NAME_POSITION || sig->semanticName == "POS") && sig->ui32SemanticIndex == 0) &&
            ((psContext->psShader->eShaderType == VERTEX_SHADER && (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) == 0)))
        {
            result = "mtl_Position";
            return true;
        }

        switch (sig->eSystemValueType)
        {
            case NAME_POSITION:
                if (psContext->psShader->eShaderType == PIXEL_SHADER)
                    result = "hlslcc_FragCoord";
                else
                    result = "mtl_Position";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                return true;
            case NAME_RENDER_TARGET_ARRAY_INDEX:
                result = "mtl_Layer";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                return true;
            case NAME_CLIP_DISTANCE:
            {
                // this is temp variable, declaration and redirecting to actual output is handled in DeclareClipPlanes
                char tmpName[128]; sprintf(tmpName, "phase%d_ClipDistance%d", psContext->currentPhase, sig->ui32SemanticIndex);
                result = tmpName;
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (iIgnoreRedirect != NULL) *iIgnoreRedirect = 1;
                return true;
            }
            case NAME_VIEWPORT_ARRAY_INDEX:
                result = "mtl_ViewPortIndex";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                return true;
            case NAME_VERTEX_ID:
                result = "mtl_VertexID";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                return true;
            case NAME_INSTANCE_ID:
                result = "mtl_InstanceID";
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                return true;
            case NAME_IS_FRONT_FACE:
                result = "(mtl_FrontFace ? 0xffffffffu : uint(0))";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                return true;
            case NAME_SAMPLE_INDEX:
                result = "mtl_SampleID";
                if (outSkipPrefix != NULL) *outSkipPrefix = true;
                if (pui32IgnoreSwizzle)
                    *pui32IgnoreSwizzle = 1;
                return true;

            default:
                break;
        }

        if (psContext->psShader->asPhases[psContext->currentPhase].ePhase == HS_CTRL_POINT_PHASE ||
            psContext->psShader->asPhases[psContext->currentPhase].ePhase == HS_FORK_PHASE)
        {
            std::ostringstream oss;
            oss << sig->semanticName << sig->ui32SemanticIndex;
            result = oss.str();
            return true;
        }
    }

    switch (psOperand->eType)
    {
        case OPERAND_TYPE_INPUT_COVERAGE_MASK:
        case OPERAND_TYPE_OUTPUT_COVERAGE_MASK:
            result = "mtl_CoverageMask";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case OPERAND_TYPE_INPUT_THREAD_ID:
            result = "mtl_ThreadID";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            return true;
        case OPERAND_TYPE_INPUT_THREAD_GROUP_ID:
            result = "mtl_ThreadGroupID";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            return true;
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP:
            result = "mtl_ThreadIDInGroup";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            return true;
        case OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED:
            result = "mtl_ThreadIndexInThreadGroup";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case OPERAND_TYPE_INPUT_DOMAIN_POINT:
            result = "mtl_TessCoord";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case OPERAND_TYPE_OUTPUT_DEPTH:
        case OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL:
        case OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL:
            result = "mtl_Depth";
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case OPERAND_TYPE_OUTPUT:
        case OPERAND_TYPE_INPUT:
        {
            std::ostringstream oss;
            ASSERT(sig != nullptr);
            oss << sig->semanticName << sig->ui32SemanticIndex;
            result = oss.str();
            if (HLSLcc::WriteMaskToComponentCount(sig->ui32Mask) == 1 && pui32IgnoreSwizzle != NULL)
                *pui32IgnoreSwizzle = 1;
            return true;
        }
        case OPERAND_TYPE_INPUT_PATCH_CONSTANT:
        {
            std::ostringstream oss;
            ASSERT(sig != nullptr);
            oss << sig->semanticName << sig->ui32SemanticIndex;
            result = oss.str();
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            return true;
        }
        case OPERAND_TYPE_INPUT_CONTROL_POINT:
        {
            std::ostringstream oss;
            ASSERT(sig != nullptr);
            oss << sig->semanticName << sig->ui32SemanticIndex;
            result = oss.str();
            if (outSkipPrefix != NULL) *outSkipPrefix = true;
            return true;
            break;
        }
        default:
            ASSERT(0);
            break;
    }


    return false;
}

void ToMetal::DeclareBuiltinInput(const Declaration *psDecl)
{
    const SPECIAL_NAME eSpecialName = psDecl->asOperands[0].eSpecialName;

    Shader* psShader = psContext->psShader;
    const Operand* psOperand = &psDecl->asOperands[0];
    const int regSpace = psOperand->GetRegisterSpace(psContext);
    ASSERT(regSpace == 0);

    // we need to at least mark if they are scalars or not (as we might need to use vector ctor)
    if (psOperand->GetNumInputElements(psContext) == 1)
        psShader->abScalarInput[regSpace][psOperand->ui32RegisterNumber] |= (int)psOperand->ui32CompMask;

    switch (eSpecialName)
    {
        case NAME_POSITION:
            ASSERT(psContext->psShader->eShaderType == PIXEL_SHADER);
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_FragCoord", "float4 mtl_FragCoord [[ position ]]"));
            bcatcstr(GetEarlyMain(psContext), "float4 hlslcc_FragCoord = float4(mtl_FragCoord.xyz, 1.0/mtl_FragCoord.w);\n");
            break;
        case NAME_RENDER_TARGET_ARRAY_INDEX:
            // Only supported on a Mac
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_Layer", "uint mtl_Layer [[ render_target_array_index ]]"));
            break;
        case NAME_CLIP_DISTANCE:
            ASSERT(0); // Should never be an input
            break;
        case NAME_VIEWPORT_ARRAY_INDEX:
            // Only supported on a Mac
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_ViewPortIndex", "uint mtl_ViewPortIndex [[ viewport_array_index ]]"));
            break;
        case NAME_INSTANCE_ID:
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_InstanceID", "uint mtl_InstanceID [[ instance_id ]]"));
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_BaseInstance", "uint mtl_BaseInstance [[ base_instance ]]")); // Requires Metal runtime 1.1+
            break;
        case NAME_IS_FRONT_FACE:
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_FrontFace", "bool mtl_FrontFace [[ front_facing ]]"));
            break;
        case NAME_SAMPLE_INDEX:
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_SampleID", "uint mtl_SampleID [[ sample_id ]]"));
            break;
        case NAME_VERTEX_ID:
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_VertexID", "uint mtl_VertexID [[ vertex_id ]]"));
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_BaseVertex", "uint mtl_BaseVertex [[ base_vertex ]]")); // Requires Metal runtime 1.1+
            break;
        case NAME_PRIMITIVE_ID:
            // Not on Metal
            ASSERT(0);
            break;
        default:
            m_StructDefinitions[""].m_Members.push_back(std::make_pair(psDecl->asOperands[0].specialName, std::string("float4 ").append(psDecl->asOperands[0].specialName)));
            ASSERT(0); // Catch this to see what's happening
            break;
    }
}

void ToMetal::DeclareClipPlanes(const Declaration* decl, unsigned declCount)
{
    unsigned planeCount = 0;
    for (unsigned i = 0, n = declCount; i < n; ++i)
    {
        const Operand* operand = &decl[i].asOperands[0];
        if (operand->eSpecialName == NAME_CLIP_DISTANCE)
            planeCount += operand->GetMaxComponent();
    }
    if (planeCount == 0) return;

    std::ostringstream oss; oss << "float mtl_ClipDistance [[ clip_distance ]]";
    if (planeCount > 1) oss << "[" << planeCount << "]";
    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(std::string("mtl_ClipDistance"), oss.str()));

    Shader* shader = psContext->psShader;

    unsigned compCount = 1;
    const ShaderInfo::InOutSignature* psFirstClipSignature;
    if (shader->sInfo.GetOutputSignatureFromSystemValue(NAME_CLIP_DISTANCE, 0, &psFirstClipSignature))
    {
        if (psFirstClipSignature->ui32Mask & (1 << 3))       compCount = 4;
        else if (psFirstClipSignature->ui32Mask & (1 << 2))  compCount = 3;
        else if (psFirstClipSignature->ui32Mask & (1 << 1))  compCount = 2;
    }

    for (unsigned i = 0, n = declCount; i < n; ++i)
    {
        const Operand* operand = &decl[i].asOperands[0];
        if (operand->eSpecialName != NAME_CLIP_DISTANCE) continue;

        const ShaderInfo::InOutSignature* signature = 0;
        shader->sInfo.GetOutputSignatureFromRegister(operand->ui32RegisterNumber, operand->ui32CompMask, 0, &signature);
        const int semanticIndex = signature->ui32SemanticIndex;

        bformata(GetEarlyMain(psContext), "float4 phase%d_ClipDistance%d;\n", psContext->currentPhase, signature->ui32SemanticIndex);

        const char* swizzleStr[] = { "x", "y", "z", "w" };
        if (planeCount > 1)
        {
            for (int i = 0; i < compCount; ++i)
            {
                bformata(GetPostShaderCode(psContext), "%s.mtl_ClipDistance[%d] = phase%d_ClipDistance%d.%s;\n", "output", semanticIndex * compCount + i, psContext->currentPhase, semanticIndex, swizzleStr[i]);
            }
        }
        else
        {
            bformata(GetPostShaderCode(psContext), "%s.mtl_ClipDistance = phase%d_ClipDistance%d.x;\n", "output", psContext->currentPhase, semanticIndex);
        }
    }
}

void ToMetal::GenerateTexturesReflection(HLSLccReflection* refl)
{
    for (unsigned i = 0, n = m_Textures.size(); i < n; ++i)
    {
        // Match CheckSamplerAndTextureNameMatch behavior
        const std::string samplerName1 = m_Textures[i].name, samplerName2 = "sampler" + m_Textures[i].name, samplerName3 = "sampler_" + m_Textures[i].name;
        for (unsigned j = 0, m = m_Samplers.size(); j < m; ++j)
        {
            if (m_Samplers[j].name == samplerName1 || m_Samplers[j].name == samplerName2 || m_Samplers[j].name == samplerName3)
            {
                m_Textures[i].samplerBind = m_Samplers[j].slot;
                break;
            }
        }
    }

    for (unsigned i = 0, n = m_Textures.size(); i < n; ++i)
        refl->OnTextureBinding(m_Textures[i].name, m_Textures[i].textureBind, m_Textures[i].samplerBind, m_Textures[i].isMultisampled, m_Textures[i].dim, m_Textures[i].uav);
}

void ToMetal::DeclareBuiltinOutput(const Declaration *psDecl)
{
    std::string out = GetOutputStructName();

    switch (psDecl->asOperands[0].eSpecialName)
    {
        case NAME_POSITION:
            m_StructDefinitions[out].m_Members.push_back(std::make_pair("mtl_Position", "float4 mtl_Position [[ position ]]"));
            break;
        case NAME_RENDER_TARGET_ARRAY_INDEX:
            m_StructDefinitions[out].m_Members.push_back(std::make_pair("mtl_Layer", "uint mtl_Layer [[ render_target_array_index ]]"));
            break;
        case NAME_CLIP_DISTANCE:
            // it will be done separately in DeclareClipPlanes
            break;
        case NAME_VIEWPORT_ARRAY_INDEX:
            // Only supported on a Mac
            m_StructDefinitions[out].m_Members.push_back(std::make_pair("mtl_ViewPortIndex", "uint mtl_ViewPortIndex [[ viewport_array_index ]]"));
            break;
        case NAME_VERTEX_ID:
            ASSERT(0); //VertexID is not an output
            break;
        case NAME_PRIMITIVE_ID:
            // Not on Metal
            ASSERT(0);
            break;
        case NAME_INSTANCE_ID:
            ASSERT(0); //InstanceID is not an output
            break;
        case NAME_IS_FRONT_FACE:
            ASSERT(0); //FrontFacing is not an output
            break;

        //For the quadrilateral domain, there are 6 factors (4 sides, 2 inner).
        case NAME_FINAL_QUAD_U_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_QUAD_V_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_QUAD_U_EQ_1_EDGE_TESSFACTOR:
        case NAME_FINAL_QUAD_V_EQ_1_EDGE_TESSFACTOR:
        case NAME_FINAL_QUAD_U_INSIDE_TESSFACTOR:
        case NAME_FINAL_QUAD_V_INSIDE_TESSFACTOR:

        //For the triangular domain, there are 4 factors (3 sides, 1 inner)
        case NAME_FINAL_TRI_U_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_V_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_W_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_INSIDE_TESSFACTOR:

        //For the isoline domain, there are 2 factors (detail and density).
        case NAME_FINAL_LINE_DETAIL_TESSFACTOR:
        case NAME_FINAL_LINE_DENSITY_TESSFACTOR:
        {
            // Handled separately
            break;
        }
        default:
            // This might be SV_Position (because d3dcompiler is weird). Get signature and check
            const ShaderInfo::InOutSignature *sig = NULL;
            psContext->psShader->sInfo.GetOutputSignatureFromRegister(psDecl->asOperands[0].ui32RegisterNumber, psDecl->asOperands[0].GetAccessMask(), 0, &sig);
            ASSERT(sig != NULL);
            if (sig->eSystemValueType == NAME_POSITION && sig->ui32SemanticIndex == 0)
            {
                m_StructDefinitions[out].m_Members.push_back(std::make_pair("mtl_Position", "float4 mtl_Position [[ position ]]"));
                break;
            }

            ASSERT(0); // Wut
            break;
    }

    psContext->m_Reflection.OnBuiltinOutput(psDecl->asOperands[0].eSpecialName);
}

static std::string BuildOperandTypeString(OPERAND_MIN_PRECISION ePrec, INOUT_COMPONENT_TYPE eType, int numComponents)
{
    SHADER_VARIABLE_TYPE t = SVT_FLOAT;
    switch (eType)
    {
        case INOUT_COMPONENT_FLOAT32:
            t = SVT_FLOAT;
            break;
        case INOUT_COMPONENT_UINT32:
            t = SVT_UINT;
            break;
        case INOUT_COMPONENT_SINT32:
            t = SVT_INT;
            break;
        default:
            ASSERT(0);
            break;
    }
    // Can be overridden by precision
    switch (ePrec)
    {
        case OPERAND_MIN_PRECISION_DEFAULT:
            break;

        case OPERAND_MIN_PRECISION_FLOAT_16:
            ASSERT(eType == INOUT_COMPONENT_FLOAT32);
            t = SVT_FLOAT16;
            break;

        case OPERAND_MIN_PRECISION_FLOAT_2_8:
            ASSERT(eType == INOUT_COMPONENT_FLOAT32);
            t = SVT_FLOAT10;
            break;

        case OPERAND_MIN_PRECISION_SINT_16:
            ASSERT(eType == INOUT_COMPONENT_SINT32);
            t = SVT_INT16;
            break;
        case OPERAND_MIN_PRECISION_UINT_16:
            ASSERT(eType == INOUT_COMPONENT_UINT32);
            t = SVT_UINT16;
            break;
    }
    return HLSLcc::GetConstructorForTypeMetal(t, numComponents);
}

void ToMetal::DeclareHullShaderPassthrough()
{
    uint32_t i;

    for (i = 0; i < psContext->psShader->sInfo.psInputSignatures.size(); i++)
    {
        ShaderInfo::InOutSignature *psSig = &psContext->psShader->sInfo.psInputSignatures[i];

        std::string name;
        {
            std::ostringstream oss;
            oss << psSig->semanticName << psSig->ui32SemanticIndex;
            name = oss.str();
        }

        if ((psSig->eSystemValueType == NAME_POSITION || psSig->semanticName == "POS") && psSig->ui32SemanticIndex == 0)
            name = "mtl_Position";

        uint32_t ui32NumComponents = HLSLcc::GetNumberBitsSet(psSig->ui32Mask);
        std::string typeName = BuildOperandTypeString(OPERAND_MIN_PRECISION_DEFAULT, psSig->eComponentType, ui32NumComponents);

        std::ostringstream oss;
        oss << typeName << " " << name;
        oss << " [[ user(" << name << ") ]]";

        std::string declString;
        declString = oss.str();

        m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair(name, declString));

        std::string out = GetOutputStructName();
        m_StructDefinitions[out].m_Members.push_back(std::make_pair(name, declString));

        // For preserving data layout, declare output struct as domain shader input, too
        oss.str("");
        out += "In";

        oss << typeName << " " << name;
        // VERTEX_SHADER hardcoded on purpose
        bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);
        uint32_t loc = psContext->psDependencies->GetVaryingLocation(name, VERTEX_SHADER, true, keepLocation, psContext->psShader->maxSemanticIndex);
        oss << " [[ " << "attribute(" << loc << ")" << " ]] ";

        psContext->m_Reflection.OnInputBinding(name, loc);
        m_StructDefinitions[out].m_Members.push_back(std::make_pair(name, oss.str()));
    }
}

void ToMetal::HandleOutputRedirect(const Declaration *psDecl, const std::string &typeName)
{
    const Operand *psOperand = &psDecl->asOperands[0];
    Shader *psShader = psContext->psShader;
    int needsRedirect = 0;
    const ShaderInfo::InOutSignature *psSig = NULL;

    int regSpace = psOperand->GetRegisterSpace(psContext);
    if (regSpace == 0 && psShader->asPhases[psContext->currentPhase].acOutputNeedsRedirect[psOperand->ui32RegisterNumber] == 0xff)
    {
        needsRedirect = 1;
    }
    else if (regSpace == 1 && psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0xff)
    {
        needsRedirect = 1;
    }

    if (needsRedirect == 1)
    {
        // TODO What if this is indexed?
        int comp = 0;
        uint32_t origMask = psOperand->ui32CompMask;

        ASSERT(psContext->psShader->aIndexedOutput[regSpace][psOperand->ui32RegisterNumber] == 0);

        bformata(GetEarlyMain(psContext), "%s phase%d_Output%d_%d;\n", typeName.c_str(), psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);

        while (comp < 4)
        {
            int numComps = 0;
            int hasCast = 0;
            uint32_t mask, i;
            psSig = NULL;
            if (regSpace == 0)
                psContext->psShader->sInfo.GetOutputSignatureFromRegister(psOperand->ui32RegisterNumber, 1 << comp, psContext->psShader->ui32CurrentVertexOutputStream, &psSig, true);
            else
                psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, 1 << comp, &psSig, true);

            // The register isn't necessarily packed full. Continue with the next component.
            if (psSig == NULL)
            {
                comp++;
                continue;
            }

            numComps = HLSLcc::GetNumberBitsSet(psSig->ui32Mask);
            mask = psSig->ui32Mask;

            ((Operand *)psOperand)->ui32CompMask = 1 << comp;
            bstring str = GetPostShaderCode(psContext);
            bcatcstr(str, TranslateOperand(psOperand, TO_FLAG_NAME_ONLY).c_str());
            bcatcstr(str, " = ");

            if (psSig->eComponentType == INOUT_COMPONENT_SINT32)
            {
                bformata(str, "as_type<int>(");
                hasCast = 1;
            }
            else if (psSig->eComponentType == INOUT_COMPONENT_UINT32)
            {
                bformata(str, "as_type<uint>(");
                hasCast = 1;
            }
            bformata(str, "phase%d_Output%d_%d.", psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);
            // Print out mask
            for (i = 0; i < 4; i++)
            {
                if ((mask & (1 << i)) == 0)
                    continue;

                bformata(str, "%c", "xyzw"[i]);
            }

            if (hasCast)
                bcatcstr(str, ")");
            comp += numComps;
            bcatcstr(str, ";\n");
        }

        ((Operand *)psOperand)->ui32CompMask = origMask;
        if (regSpace == 0)
            psShader->asPhases[psContext->currentPhase].acOutputNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
        else
            psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
    }
}

void ToMetal::HandleInputRedirect(const Declaration *psDecl, const std::string &typeName)
{
    Operand *psOperand = (Operand *)&psDecl->asOperands[0];
    Shader *psShader = psContext->psShader;
    int needsRedirect = 0;
    const ShaderInfo::InOutSignature *psSig = NULL;

    int regSpace = psOperand->GetRegisterSpace(psContext);
    if (regSpace == 0)
    {
        if (psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] == 0xff)
            needsRedirect = 1;
    }
    else if (psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0xff)
    {
        psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->ui32CompMask, &psSig);
        needsRedirect = 1;
    }

    if (needsRedirect == 1)
    {
        // TODO What if this is indexed?
        int needsLooping = 0;
        int i = 0;
        uint32_t origArraySize = 0;
        uint32_t origMask = psOperand->ui32CompMask;

        ASSERT(psContext->psShader->aIndexedInput[regSpace][psOperand->ui32RegisterNumber] == 0);

        ++psContext->indent;

        // Does the input have multiple array components (such as geometry shader input, or domain shader control point input)
        if ((psShader->eShaderType == DOMAIN_SHADER && regSpace == 0) || (psShader->eShaderType == GEOMETRY_SHADER))
        {
            // The count is actually stored in psOperand->aui32ArraySizes[0]
            origArraySize = psOperand->aui32ArraySizes[0];
            // bformata(glsl, "%s vec4 phase%d_Input%d_%d[%d];\n", Precision, psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, origArraySize);
            bformata(GetEarlyMain(psContext), "%s phase%d_Input%d_%d[%d];\n", typeName.c_str(), psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, origArraySize);
            needsLooping = 1;
            i = origArraySize - 1;
        }
        else
            // bformata(glsl, "%s vec4 phase%d_Input%d_%d;\n", Precision, psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);
            bformata(GetEarlyMain(psContext), "%s phase%d_Input%d_%d;\n", typeName.c_str(), psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);

        // Do a conditional loop. In normal cases needsLooping == 0 so this is only run once.
        do
        {
            int comp = 0;
            bstring str = GetEarlyMain(psContext);
            if (needsLooping)
                bformata(str, "phase%d_Input%d_%d[%d] = %s(", psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, i, typeName.c_str());
            else
                bformata(str, "phase%d_Input%d_%d = %s(", psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, typeName.c_str());

            while (comp < 4)
            {
                int numComps = 0;
                int hasCast = 0;
                int hasSig = 0;
                if (regSpace == 0)
                    hasSig = psContext->psShader->sInfo.GetInputSignatureFromRegister(psOperand->ui32RegisterNumber, 1 << comp, &psSig, true);
                else
                    hasSig = psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, 1 << comp, &psSig, true);

                if (hasSig)
                {
                    numComps = HLSLcc::GetNumberBitsSet(psSig->ui32Mask);
                    if (psSig->eComponentType != INOUT_COMPONENT_FLOAT32)
                    {
                        if (numComps > 1)
                            bformata(str, "as_type<float%d>(", numComps);
                        else
                            bformata(str, "as_type<float>(");
                        hasCast = 1;
                    }

                    // Override the array size of the operand so TranslateOperand call below prints the correct index
                    if (needsLooping)
                        psOperand->aui32ArraySizes[0] = i;

                    // And the component mask
                    psOperand->ui32CompMask = 1 << comp;

                    bformata(str, TranslateOperand(psOperand, TO_FLAG_NAME_ONLY).c_str());

                    // Restore the original array size value and mask
                    psOperand->ui32CompMask = origMask;
                    if (needsLooping)
                        psOperand->aui32ArraySizes[0] = origArraySize;

                    if (hasCast)
                        bcatcstr(str, ")");
                    comp += numComps;
                }
                else // no signature found -> fill with zero
                {
                    bcatcstr(str, "0");
                    comp++;
                }

                if (comp < 4)
                    bcatcstr(str, ", ");
            }
            bcatcstr(str, ");\n");
        }
        while ((--i) >= 0);

        --psContext->indent;

        if (regSpace == 0)
            psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
        else
            psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
    }
}

static std::string TranslateResourceDeclaration(HLSLCrossCompilerContext* psContext,
    const Declaration *psDecl, const std::string& textureName,
    bool isDepthSampler, bool isUAV)
{
    std::ostringstream oss;
    const ResourceBinding* psBinding = 0;
    const RESOURCE_DIMENSION eDimension = psDecl->value.eResourceDimension;
    const uint32_t ui32RegisterNumber = psDecl->asOperands[0].ui32RegisterNumber;
    REFLECT_RESOURCE_PRECISION ePrec = REFLECT_RESOURCE_PRECISION_UNKNOWN;
    RESOURCE_RETURN_TYPE eType = RETURN_TYPE_UNORM;
    std::string access = "sample";

    if (isUAV)
    {
        if ((psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_WRITE) != 0)
        {
            access = "write";
            if ((psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_READ) != 0)
            {
                access = "read_write";
            }
        }
        else
        {
            access = "read";
            eType = psDecl->sUAV.Type;
        }
        int found;
        found = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, ui32RegisterNumber, &psBinding);
        if (found)
        {
            ePrec = psBinding->ePrecision;
            eType = (RESOURCE_RETURN_TYPE)psBinding->ui32ReturnType;
            // Figured out by reverse engineering bitcode. flags b00xx means float1, b01xx = float2, b10xx = float3 and b11xx = float4
        }
    }
    else
    {
        int found;
        found = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, ui32RegisterNumber, &psBinding);
        if (found)
        {
            eType = (RESOURCE_RETURN_TYPE)psBinding->ui32ReturnType;
            ePrec = psBinding->ePrecision;

            // TODO: it might make sense to propagate float earlier (as hlslcc might declare other variables depending on sampler prec)
            // metal supports ONLY float32 depth textures
            if (isDepthSampler)
            {
                switch (eDimension)
                {
                    case RESOURCE_DIMENSION_TEXTURE2D: case RESOURCE_DIMENSION_TEXTURE2DMS: case RESOURCE_DIMENSION_TEXTURECUBE:
                    case RESOURCE_DIMENSION_TEXTURE2DARRAY: case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                        ePrec = REFLECT_RESOURCE_PRECISION_HIGHP, eType = RETURN_TYPE_FLOAT; break;
                    default:
                        break;
                }
            }
        }
        switch (eDimension)
        {
            case RESOURCE_DIMENSION_BUFFER:
            case RESOURCE_DIMENSION_TEXTURE2DMS:
            case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                access = "read";
            default:
                break;
        }
    }

    SHADER_VARIABLE_TYPE svtType = HLSLcc::ResourceReturnTypeToSVTType(eType, ePrec);
    std::string typeName = HLSLcc::GetConstructorForTypeMetal(svtType, 1);

    if ((textureName == "_CameraDepthTexture" || textureName == "_LastCameraDepthTexture") && svtType != SVT_FLOAT)
    {
        std::string msg = textureName + " should be float on Metal (use sampler2D or sampler2D_float). Incorrect type "
            "can cause Metal validation failures or undefined results on some devices.";
        psContext->m_Reflection.OnDiagnostics(msg, 0, false);
    }

    switch (eDimension)
    {
        case RESOURCE_DIMENSION_BUFFER:
        {
            oss << "texture1d<" << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            oss << "texture1d<" << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            oss << (isDepthSampler ? "depth2d<" : "texture2d<") << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            oss << (isDepthSampler ? "depth2d_ms<" : "texture2d_ms<") << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            oss << "texture3d<" << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            oss << (isDepthSampler ? "depthcube<" : "texturecube<") << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            oss << "texture1d_array<" << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            oss << (isDepthSampler ? "depth2d_array<" : "texture2d_array<") << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            // Not really supported in Metal but let's print it here anyway
            oss << "texture2d_ms_array<" << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            oss << (isDepthSampler ? "depthcube_array<" : "texturecube_array<") << typeName << ", access::" << access << " >";
            return oss.str();
            break;
        }
        default:
            ASSERT(0);
            oss << "texture2d<" << typeName << ", access::" << access << " >";
            return oss.str();
    }
}

static std::string GetInterpolationString(INTERPOLATION_MODE eMode)
{
    switch (eMode)
    {
        case INTERPOLATION_CONSTANT:
            return " [[ flat ]]";

        case INTERPOLATION_LINEAR:
            return "";

        case INTERPOLATION_LINEAR_CENTROID:
            return " [[ centroid_perspective ]]";

        case INTERPOLATION_LINEAR_NOPERSPECTIVE:
            return " [[ center_no_perspective ]]";

        case INTERPOLATION_LINEAR_NOPERSPECTIVE_CENTROID:
            return " [[ centroid_no_perspective ]]";

        case INTERPOLATION_LINEAR_SAMPLE:
            return " [[ sample_perspective ]]";

        case INTERPOLATION_LINEAR_NOPERSPECTIVE_SAMPLE:
            return " [[ sample_no_perspective ]]";
        default:
            ASSERT(0);
            return "";
    }
}

void ToMetal::DeclareStructVariable(const std::string &parentName, const ShaderVar &var, bool withinCB, uint32_t cumulativeOffset, bool isUsed)
{
    DeclareStructVariable(parentName, var.sType, withinCB, cumulativeOffset + var.ui32StartOffset, isUsed);
}

void ToMetal::DeclareStructVariable(const std::string &parentName, const ShaderVarType &var, bool withinCB, uint32_t cumulativeOffset, bool isUsed)
{
    // CB arrays need to be defined as 4 component vectors to match DX11 data layout
    bool arrayWithinCB = (withinCB && (var.Elements > 1) && (psContext->psShader->eShaderType == COMPUTE_SHADER));
    bool doDeclare = true;

    if (isUsed == false && ((psContext->flags & HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS)) == 0)
        isUsed = true;

    if (var.Class == SVC_STRUCT)
    {
        if (m_StructDefinitions.find(var.name + "_Type") == m_StructDefinitions.end())
            DeclareStructType(var.name + "_Type", var.Members, withinCB, cumulativeOffset + var.Offset);

        // Report Array-of-Struct CB top-level struct var after all members are reported.
        if (var.Parent == NULL && var.Elements > 1 && withinCB)
        {
            // var.Type being SVT_VOID indicates it is a struct in this case.
            psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, var.Rows, var.Columns, false, var.Elements, true);
        }

        std::ostringstream oss;
        oss << var.name << "_Type " << var.name;
        if (var.Elements > 1)
        {
            oss << "[" << var.Elements << "]";
        }
        m_StructDefinitions[parentName].m_Members.push_back(std::make_pair(var.name, oss.str()));
        m_StructDefinitions[parentName].m_Dependencies.push_back(var.name + "_Type");
        return;
    }
    else if (var.Class == SVC_MATRIX_COLUMNS || var.Class == SVC_MATRIX_ROWS)
    {
        std::ostringstream oss;
        if (psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES)
        {
            // Translate matrices into vec4 arrays
            char prefix[256];
            sprintf(prefix, HLSLCC_TRANSLATE_MATRIX_FORMAT_STRING, var.Rows, var.Columns);
            oss << HLSLcc::GetConstructorForType(psContext, var.Type, 4) << " " << prefix << var.name;

            uint32_t elemCount = (var.Class == SVC_MATRIX_COLUMNS ? var.Columns : var.Rows);
            if (var.Elements > 1)
            {
                elemCount *= var.Elements;
            }
            oss << "[" << elemCount << "]";

            if (withinCB)
            {
                // On compute shaders we need to reflect the vec array as it is to support all possible matrix sizes correctly.
                // On non-compute we can fake that we still have a matrix, as CB upload code will fill the data correctly on 4x4 matrices.
                // That way we avoid the issues with mismatching types for builtins etc.
                if (psContext->psShader->eShaderType == COMPUTE_SHADER)
                    doDeclare = psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, 1, 4, false, elemCount, isUsed);
                else
                    doDeclare = psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, var.Rows, var.Columns, true, var.Elements, isUsed);
            }
        }
        else
        {
            oss << HLSLcc::GetMatrixTypeName(psContext, var.Type, var.Columns, var.Rows);
            oss << " " << var.name;
            if (var.Elements > 1)
            {
                oss << "[" << var.Elements << "]";
            }

            // TODO Verify whether the offset is from the beginning of the CB or from the beginning of the struct
            if (withinCB)
                doDeclare = psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, var.Rows, var.Columns, true, var.Elements, isUsed);
        }

        if (doDeclare)
            m_StructDefinitions[parentName].m_Members.push_back(std::make_pair(var.name, oss.str()));
    }
    else if (var.Class == SVC_VECTOR && var.Columns > 1)
    {
        std::ostringstream oss;
        oss << HLSLcc::GetConstructorForTypeMetal(var.Type, arrayWithinCB ? 4 : var.Columns);
        oss << " " << var.name;
        if (var.Elements > 1)
        {
            oss << "[" << var.Elements << "]";
        }

        if (withinCB)
            doDeclare = psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, 1, var.Columns, false, var.Elements, isUsed);

        if (doDeclare)
            m_StructDefinitions[parentName].m_Members.push_back(std::make_pair(var.name, oss.str()));
    }
    else if ((var.Class == SVC_SCALAR) ||
             (var.Class == SVC_VECTOR && var.Columns == 1))
    {
        if (var.Type == SVT_BOOL)
        {
            //Use int instead of bool.
            //Allows implicit conversions to integer and
            //bool consumes 4-bytes in HLSL and GLSL anyway.
            ((ShaderVarType &)var).Type = SVT_INT;
        }

        std::ostringstream oss;
        oss << HLSLcc::GetConstructorForTypeMetal(var.Type, arrayWithinCB ? 4 : 1);
        oss << " " << var.name;
        if (var.Elements > 1)
        {
            oss << "[" << var.Elements << "]";
        }

        if (withinCB)
            doDeclare = psContext->m_Reflection.OnConstant(var.fullName, var.Offset + cumulativeOffset, var.Type, 1, 1, false, var.Elements, isUsed);

        if (doDeclare)
            m_StructDefinitions[parentName].m_Members.push_back(std::make_pair(var.name, oss.str()));
    }
    else
    {
        ASSERT(0);
    }
}

void ToMetal::DeclareStructType(const std::string &name, const std::vector<ShaderVar> &contents, bool withinCB, uint32_t cumulativeOffset, bool stripUnused /* = false */)
{
    for (std::vector<ShaderVar>::const_iterator itr = contents.begin(); itr != contents.end(); itr++)
    {
        if (stripUnused && !itr->sType.m_IsUsed)
            continue;

        DeclareStructVariable(name, *itr, withinCB, cumulativeOffset, itr->sType.m_IsUsed);
    }
}

void ToMetal::DeclareStructType(const std::string &name, const std::vector<ShaderVarType> &contents, bool withinCB, uint32_t cumulativeOffset)
{
    for (std::vector<ShaderVarType>::const_iterator itr = contents.begin(); itr != contents.end(); itr++)
    {
        DeclareStructVariable(name, *itr, withinCB, cumulativeOffset);
    }
}

void ToMetal::DeclareConstantBuffer(const ConstantBuffer *psCBuf, uint32_t ui32BindingPoint)
{
    const bool isGlobals = (psCBuf->name == "$Globals");
    const bool stripUnused = isGlobals && (psContext->flags & HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS);
    std::string cbname = GetCBName(psCBuf->name);

    // Note: if we're stripping unused members, both ui32TotalSizeInBytes and individual offsets into reflection will be completely off.
    // However, the reflection layer re-calculates both to match Metal alignment rules anyway, so we're good.
    if (!psContext->m_Reflection.OnConstantBuffer(cbname, psCBuf->ui32TotalSizeInBytes, psCBuf->GetMemberCount(stripUnused)))
        return;

    if (psContext->psDependencies->IsMemberDeclared(cbname))
        return;

    DeclareStructType(cbname + "_Type", psCBuf->asVars, true, 0, stripUnused);

    std::ostringstream oss;
    uint32_t slot = m_BufferSlots.GetBindingSlot(ui32BindingPoint, BindingSlotAllocator::ConstantBuffer);

    if (HLSLcc::IsUnityFlexibleInstancingBuffer(psCBuf))
        oss << "const constant " << psCBuf->asVars[0].name << "_Type* ";
    else
        oss << "constant " << cbname << "_Type& ";
    oss << cbname << " [[ buffer(" << slot << ") ]]";

    m_StructDefinitions[""].m_Members.push_back(std::make_pair(cbname, oss.str()));
    m_StructDefinitions[""].m_Dependencies.push_back(cbname + "_Type");
    psContext->m_Reflection.OnConstantBufferBinding(cbname, slot);
}

void ToMetal::DeclareBufferVariable(const Declaration *psDecl, bool isRaw, bool isUAV)
{
    uint32_t regNo = psDecl->asOperands[0].ui32RegisterNumber;
    std::string BufName, BufType, BufConst;

    BufName = "";
    BufType = "";
    BufConst = "";

    BufName = ResourceName(isUAV ? RGROUP_UAV : RGROUP_TEXTURE, regNo);

    if (!isRaw) // declare struct containing uint array when needed
    {
        std::ostringstream typeoss;
        BufType = BufName + "_Type";
        typeoss << "uint value[";
        typeoss << psDecl->ui32BufferStride / 4 << "]";
        m_StructDefinitions[BufType].m_Members.push_back(std::make_pair("value", typeoss.str()));
        m_StructDefinitions[""].m_Dependencies.push_back(BufType);
    }

    if (!psContext->psDependencies->IsMemberDeclared(BufName))
    {
        std::ostringstream oss;

        if (!isUAV || ((psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_WRITE) == 0))
        {
            BufConst =  "const ";
            oss << BufConst;
        }

        if (isRaw)
            oss << "device uint *" << BufName;
        else
            oss << "device " << BufType << " *" << BufName;

        uint32_t loc = m_BufferSlots.GetBindingSlot(regNo, isUAV ? BindingSlotAllocator::RWBuffer : BindingSlotAllocator::Texture);
        oss << " [[ buffer(" << loc << ") ]]";

        m_StructDefinitions[""].m_Members.push_back(std::make_pair(BufName, oss.str()));

        // We don't do REAL reflection here, we need to collect all data and figure out if we're dealing with counters.
        // And if so - we need to patch counter binding info, add counters to empty slots, etc
        const BufferReflection br = { loc, isUAV, psDecl->sUAV.bCounter != 0 };
        m_BufferReflections.insert(std::make_pair(BufName, br));
    }
}

static int ParseInlineSamplerWrapMode(const std::string& samplerName, const std::string& wrapName)
{
    int res = 0;
    const bool hasWrap = (samplerName.find(wrapName) != std::string::npos);
    if (!hasWrap)
        return res;

    const bool hasU = (samplerName.find(wrapName + 'u') != std::string::npos);
    const bool hasV = (samplerName.find(wrapName + 'v') != std::string::npos);
    const bool hasW = (samplerName.find(wrapName + 'w') != std::string::npos);

    if (hasWrap) res |= 1;
    if (hasU) res |= 2;
    if (hasV) res |= 4;
    if (hasW) res |= 8;
    return res;
}

static bool EmitInlineSampler(HLSLCrossCompilerContext* psContext, const std::string& name)
{
    // See if it's a sampler that goes with the texture, or an "inline" sampler
    // where sampler states are hardcoded in the shader directly.
    //
    // The logic for "inline" samplers below must match what is recognized
    // by other shader platforms in Unity (ParseInlineSamplerName function
    // in the shader compiler).

    std::string samplerName(name); std::transform(samplerName.begin(), samplerName.end(), samplerName.begin(), ::tolower);

    // filter modes
    const bool hasPoint = (samplerName.find("point") != std::string::npos);
    const bool hasTrilinear = (samplerName.find("trilinear") != std::string::npos);
    const bool hasLinear = (samplerName.find("linear") != std::string::npos);
    const bool hasAnyFilter = hasPoint || hasTrilinear || hasLinear;

    // wrap modes
    const int bitsClamp = ParseInlineSamplerWrapMode(samplerName, "clamp");
    const int bitsRepeat = ParseInlineSamplerWrapMode(samplerName, "repeat");
    const int bitsMirror = ParseInlineSamplerWrapMode(samplerName, "mirror");
    const int bitsMirrorOnce = ParseInlineSamplerWrapMode(samplerName, "mirroronce");

    const bool hasAnyWrap = bitsClamp != 0 || bitsRepeat != 0 || bitsMirror != 0 || bitsMirrorOnce != 0;

    // depth comparison
    const bool hasCompare = (samplerName.find("compare") != std::string::npos);

    // name must contain a filter mode and a wrap mode at least
    if (!hasAnyFilter || !hasAnyWrap)
    {
        return false;
    }

    // Starting with macOS 11/iOS 14, the metal compiler will warn about unused inline samplers, that might
    // happen on mobile due to _mtl_xl_shadow_sampler workaround that's required for pre-GPUFamily3.
    if (hasCompare && IsMobileTarget(psContext))
        return true;

    bstring str = GetEarlyMain(psContext);
    bformata(str, "constexpr sampler %s(", name.c_str());

    if (hasCompare)
        bformata(str, "compare_func::greater_equal,");

    if (hasTrilinear)
        bformata(str, "filter::linear,mip_filter::linear,");
    else if (hasLinear)
        bformata(str, "filter::linear,mip_filter::nearest,");
    else
        bformata(str, "filter::nearest,");

    const char* kTexWrapClamp = "clamp_to_edge";
    const char* kTexWrapRepeat = "repeat";
    const char* kTexWrapMirror = "mirrored_repeat";
    const char* kTexWrapMirrorOnce = "mirrored_repeat"; // currently Metal shading language does not have syntax for inline sampler state that would do "mirror clamp to edge"
    const char* wrapU = kTexWrapRepeat;
    const char* wrapV = kTexWrapRepeat;
    const char* wrapW = kTexWrapRepeat;

    if (bitsClamp == 1)             wrapU = wrapV = wrapW = kTexWrapClamp;
    else if (bitsRepeat == 1)       wrapU = wrapV = wrapW = kTexWrapRepeat;
    else if (bitsMirrorOnce == 1)   wrapU = wrapV = wrapW = kTexWrapMirrorOnce;
    else if (bitsMirror == 1)       wrapU = wrapV = wrapW = kTexWrapMirror;

    if ((bitsClamp & 2) != 0)   wrapU = kTexWrapClamp;
    if ((bitsClamp & 4) != 0)   wrapV = kTexWrapClamp;
    if ((bitsClamp & 8) != 0)   wrapW = kTexWrapClamp;

    if ((bitsRepeat & 2) != 0)  wrapU = kTexWrapRepeat;
    if ((bitsRepeat & 4) != 0)  wrapV = kTexWrapRepeat;
    if ((bitsRepeat & 8) != 0)  wrapW = kTexWrapRepeat;

    if ((bitsMirrorOnce & 2) != 0)  wrapU = kTexWrapMirrorOnce;
    if ((bitsMirrorOnce & 4) != 0)  wrapV = kTexWrapMirrorOnce;
    if ((bitsMirrorOnce & 8) != 0)  wrapW = kTexWrapMirrorOnce;

    if ((bitsMirror & 2) != 0)  wrapU = kTexWrapMirror;
    if ((bitsMirror & 4) != 0)  wrapV = kTexWrapMirror;
    if ((bitsMirror & 8) != 0)  wrapW = kTexWrapMirror;

    if (wrapU == wrapV && wrapU == wrapW)
        bformata(str, "address::%s", wrapU);
    else
        bformata(str, "s_address::%s,t_address::%s,r_address::%s", wrapU, wrapV, wrapW);

    bformata(str, ");\n");

    return true;
}

void ToMetal::TranslateDeclaration(const Declaration* psDecl)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;

    switch (psDecl->eOpcode)
    {
        case OPCODE_DCL_INPUT_SGV:
        case OPCODE_DCL_INPUT_PS_SGV:
            DeclareBuiltinInput(psDecl);
            break;
        case OPCODE_DCL_OUTPUT_SIV:
            DeclareBuiltinOutput(psDecl);
            break;
        case OPCODE_DCL_INPUT:
        case OPCODE_DCL_INPUT_PS_SIV:
        case OPCODE_DCL_INPUT_SIV:
        case OPCODE_DCL_INPUT_PS:
        {
            const Operand* psOperand = &psDecl->asOperands[0];

            if ((psOperand->eType == OPERAND_TYPE_OUTPUT_CONTROL_POINT_ID) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_FORK_INSTANCE_ID))
            {
                break;
            }

            // No need to declare patch constants read again by the hull shader.
            if ((psOperand->eType == OPERAND_TYPE_INPUT_PATCH_CONSTANT) && psContext->psShader->eShaderType == HULL_SHADER)
            {
                break;
            }
            // ...or control points
            if ((psOperand->eType == OPERAND_TYPE_INPUT_CONTROL_POINT) && psContext->psShader->eShaderType == HULL_SHADER)
            {
                break;
            }

            //Already declared as part of an array.
            if (psDecl->eOpcode == OPCODE_DCL_INPUT && psShader->aIndexedInput[psOperand->GetRegisterSpace(psContext)][psDecl->asOperands[0].ui32RegisterNumber] == -1)
            {
                break;
            }

            uint32_t ui32Reg = psDecl->asOperands[0].ui32RegisterNumber;
            uint32_t ui32CompMask = psDecl->asOperands[0].ui32CompMask;

            std::string name = psContext->GetDeclaredInputName(psOperand, nullptr, 1, nullptr);

            // NB: unlike GL we keep arrays of 2-component vectors as is (without collapsing into float4)
            // if(psShader->aIndexedInput[0][psDecl->asOperands[0].ui32RegisterNumber] == -1)
            //     break;

            // Already declared?
            if ((ui32CompMask != 0) && ((ui32CompMask & ~psShader->acInputDeclared[0][ui32Reg]) == 0))
            {
                ASSERT(0); // Catch this
                break;
            }

            if (psOperand->eType == OPERAND_TYPE_INPUT_COVERAGE_MASK)
            {
                std::ostringstream oss;
                oss << "uint " << name << " [[ sample_mask ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }

            if (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID)
            {
                std::ostringstream oss;
                oss << "uint3 " << name << " [[ thread_position_in_grid ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }

            if (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_GROUP_ID)
            {
                std::ostringstream oss;
                oss << "uint3 " << name << " [[ threadgroup_position_in_grid ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }

            if (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP)
            {
                std::ostringstream oss;
                oss << "uint3 " << name << " [[ thread_position_in_threadgroup ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }
            if (psOperand->eSpecialName == NAME_RENDER_TARGET_ARRAY_INDEX)
            {
                std::ostringstream oss;
                oss << "uint " << name << " [[ render_target_array_index ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }
            if (psOperand->eType == OPERAND_TYPE_INPUT_DOMAIN_POINT)
            {
                std::ostringstream oss;
                std::string patchPositionType = psShader->sInfo.eTessDomain == TESSELLATOR_DOMAIN_QUAD ? "float2 " : "float3 ";
                oss << patchPositionType << name << " [[ position_in_patch ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }
            if (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED)
            {
                std::ostringstream oss;
                oss << "uint " << name << " [[ thread_index_in_threadgroup ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }
            if (psOperand->eSpecialName == NAME_VIEWPORT_ARRAY_INDEX)
            {
                std::ostringstream oss;
                oss << "uint " << name << " [[ viewport_array_index ]]";
                m_StructDefinitions[""].m_Members.push_back(std::make_pair(name, oss.str()));
                break;
            }

            if (psDecl->eOpcode == OPCODE_DCL_INPUT_PS_SIV && psOperand->eSpecialName == NAME_POSITION)
            {
                m_StructDefinitions[""].m_Members.push_back(std::make_pair("mtl_FragCoord", "float4 mtl_FragCoord [[ position ]]"));
                bcatcstr(GetEarlyMain(psContext), "float4 hlslcc_FragCoord = float4(mtl_FragCoord.xyz, 1.0/mtl_FragCoord.w);\n");
                break;
            }

            if (psContext->psDependencies)
            {
                if (psShader->eShaderType == PIXEL_SHADER)
                {
                    psContext->psDependencies->SetInterpolationMode(ui32Reg, psDecl->value.eInterpolation);
                }
            }

            int regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);

            const ShaderInfo::InOutSignature *psSig = NULL;

            // This falls within the specified index ranges. The default is 0 if no input range is specified
            if (regSpace == 0)
                psContext->psShader->sInfo.GetInputSignatureFromRegister(ui32Reg, ui32CompMask, &psSig);
            else
                psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Reg, ui32CompMask, &psSig);

            if (!psSig)
                break;

            // fragment shader cannot reference builtins generated by vertex program (with obvious exception of position)
            // TODO: some visible error? handle more builtins?
            if (psContext->psShader->eShaderType == PIXEL_SHADER  && !strncmp(psSig->semanticName.c_str(), "PSIZE", 5))
                break;

            int iNumComponents = psOperand->GetNumInputElements(psContext);
            psShader->acInputDeclared[0][ui32Reg] = (char)psSig->ui32Mask;

            std::string typeName = BuildOperandTypeString(psOperand->eMinPrecision, psSig->eComponentType, iNumComponents);

            std::string semantic;
            if (psContext->psShader->eShaderType == VERTEX_SHADER || psContext->psShader->eShaderType == HULL_SHADER || psContext->psShader->eShaderType == DOMAIN_SHADER)
            {
                std::ostringstream oss;
                // VERTEX_SHADER hardcoded on purpose
                bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);
                uint32_t loc = psContext->psDependencies->GetVaryingLocation(name, VERTEX_SHADER, true, keepLocation, psShader->maxSemanticIndex);
                oss << "attribute(" << loc << ")";
                semantic = oss.str();
                psContext->m_Reflection.OnInputBinding(name, loc);
            }
            else
            {
                std::ostringstream oss;

                // UNITY_FRAMEBUFFER_FETCH_AVAILABLE
                // special case mapping for inout color, see HLSLSupport.cginc
                if (psOperand->iPSInOut && name.size() == 10 && !strncmp(name.c_str(), "SV_Target", 9))
                {
                    // Metal allows color(X) declared in input/output structs
                    oss << "color(xlt_remap_i[" << psSig->ui32SemanticIndex << "])";
                    m_NeedFBInputRemapDecl = true;
                }
                else
                {
                    oss << "user(" << name << ")";
                }
                semantic = oss.str();
            }

            std::string interpolation = "";
            if (psDecl->eOpcode == OPCODE_DCL_INPUT_PS)
            {
                interpolation = GetInterpolationString(psDecl->value.eInterpolation);
            }

            std::string declString;
            if ((OPERAND_INDEX_DIMENSION)psOperand->iIndexDims == INDEX_2D && psOperand->eType != OPERAND_TYPE_INPUT_CONTROL_POINT && psContext->psShader->eShaderType != HULL_SHADER)
            {
                std::ostringstream oss;
                oss << typeName << " " << name << " [ " << psOperand->aui32ArraySizes[0] << " ] ";

                if (psContext->psShader->eShaderType != HULL_SHADER)
                    oss << " [[ " << semantic << " ]] " << interpolation;
                declString = oss.str();
            }
            else
            {
                std::ostringstream oss;
                oss << typeName << " " << name;
                if (psContext->psShader->eShaderType != HULL_SHADER)
                    oss << " [[ " << semantic << " ]] " << interpolation;
                declString = oss.str();
            }

            if (psOperand->eType == OPERAND_TYPE_INPUT_PATCH_CONSTANT && psContext->psShader->eShaderType == DOMAIN_SHADER)
            {
                m_StructDefinitions["Mtl_PatchConstant"].m_Members.push_back(std::make_pair(name, declString));
            }
            else if (psOperand->eType == OPERAND_TYPE_INPUT_CONTROL_POINT && psContext->psShader->eShaderType == DOMAIN_SHADER)
            {
                m_StructDefinitions["Mtl_ControlPoint"].m_Members.push_back(std::make_pair(name, declString));
            }
            else if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair(name, declString));
            }
            else
            {
                m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair(name, declString));
            }

            HandleInputRedirect(psDecl, BuildOperandTypeString(psOperand->eMinPrecision, INOUT_COMPONENT_FLOAT32, 4));
            break;
        }
        case OPCODE_DCL_TEMPS:
        {
            uint32_t i = 0;
            const uint32_t ui32NumTemps = psDecl->value.ui32NumTemps;
            for (i = 0; i < ui32NumTemps; i++)
            {
                if (psShader->psFloatTempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT, psShader->psFloatTempSizes[i]), i);
                if (psShader->psFloat16TempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT16, psShader->psFloat16TempSizes[i]), i);
                if (psShader->psFloat10TempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "10_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT10, psShader->psFloat10TempSizes[i]), i);
                if (psShader->psIntTempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "i%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT, psShader->psIntTempSizes[i]), i);
                if (psShader->psInt16TempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "i16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT16, psShader->psInt16TempSizes[i]), i);
                if (psShader->psInt12TempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "i12_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT12, psShader->psInt12TempSizes[i]), i);
                if (psShader->psUIntTempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "u%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_UINT, psShader->psUIntTempSizes[i]), i);
                if (psShader->psUInt16TempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "u16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_UINT16, psShader->psUInt16TempSizes[i]), i);
                if (psShader->fp64 && (psShader->psDoubleTempSizes[i] != 0))
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "d%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_DOUBLE, psShader->psDoubleTempSizes[i]), i);
                if (psShader->psBoolTempSizes[i] != 0)
                    bformata(GetEarlyMain(psContext), "%s " HLSLCC_TEMP_PREFIX "b%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_BOOL, psShader->psBoolTempSizes[i]), i);
            }
            break;
        }
        case OPCODE_SPECIAL_DCL_IMMCONST:
        {
            ASSERT(0 && "DX9 shaders no longer supported!");
            break;
        }
        case OPCODE_DCL_CONSTANT_BUFFER:
        {
            const ConstantBuffer* psCBuf = NULL;
            psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, psDecl->asOperands[0].aui32ArraySizes[0], &psCBuf);
            ASSERT(psCBuf != NULL);

            if (psCBuf->name.substr(0, 20) == "hlslcc_SubpassInput_" && psCBuf->name.length() >= 23 && !psCBuf->asVars.empty())
            {
                // Special case for framebuffer fetch.
                char ty = psCBuf->name[20];
                int idx = psCBuf->name[22] - '0';

                const ShaderVar &sv = psCBuf->asVars[0];
                if (sv.name.substr(0, 15) == "hlslcc_fbinput_")
                {
                    // Pick up the type and index
                    std::ostringstream oss;
                    m_NeedFBInputRemapDecl = true;
                    switch (ty)
                    {
                        case 'f':
                        case 'F':
                            oss << "float4 " << sv.name << " [[ color(xlt_remap_i[" << idx << "]) ]]";
                            m_StructDefinitions[""].m_Members.push_back(std::make_pair(sv.name, oss.str()));
                            break;
                        case 'h':
                        case 'H':
                            oss << "half4 " << sv.name << " [[ color(xlt_remap_i[" << idx << "]) ]]";
                            m_StructDefinitions[""].m_Members.push_back(std::make_pair(sv.name, oss.str()));
                            break;
                        case 'i':
                        case 'I':
                            oss << "int4 " << sv.name << " [[ color(xlt_remap_i[" << idx << "]) ]]";
                            m_StructDefinitions[""].m_Members.push_back(std::make_pair(sv.name, oss.str()));
                            break;
                        case 'u':
                        case 'U':
                            oss << "uint4 " << sv.name << " [[ color(xlt_remap_i[" << idx << "]) ]]";
                            m_StructDefinitions[""].m_Members.push_back(std::make_pair(sv.name, oss.str()));
                            break;
                        default:
                            break;
                    }
                }
                // Break out so this doesn't get declared.
                break;
            }

            DeclareConstantBuffer(psCBuf, psDecl->asOperands[0].aui32ArraySizes[0]);
            break;
        }
        case OPCODE_DCL_RESOURCE:
        {
            DeclareResource(psDecl);
            break;
        }
        case OPCODE_DCL_OUTPUT:
        {
            DeclareOutput(psDecl);
            break;
        }

        case OPCODE_DCL_GLOBAL_FLAGS:
        {
            uint32_t ui32Flags = psDecl->value.ui32GlobalFlags;

            if (ui32Flags & GLOBAL_FLAG_FORCE_EARLY_DEPTH_STENCIL && psContext->psShader->eShaderType == PIXEL_SHADER)
            {
                psShader->sInfo.bEarlyFragmentTests = true;
            }
            if (!(ui32Flags & GLOBAL_FLAG_REFACTORING_ALLOWED))
            {
                //TODO add precise
                //HLSL precise - http://msdn.microsoft.com/en-us/library/windows/desktop/hh447204(v=vs.85).aspx
            }
            if (ui32Flags & GLOBAL_FLAG_ENABLE_DOUBLE_PRECISION_FLOAT_OPS)
            {
                // Not supported on Metal
//          psShader->fp64 = 1;
            }
            break;
        }
        case OPCODE_DCL_THREAD_GROUP:
        {
            // Send this info to reflecion: Metal gives this at runtime as a param
            psContext->m_Reflection.OnThreadGroupSize(psDecl->value.aui32WorkGroupSize[0],
                psDecl->value.aui32WorkGroupSize[1],
                psDecl->value.aui32WorkGroupSize[2]);
            break;
        }
        case OPCODE_DCL_TESS_OUTPUT_PRIMITIVE:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                psContext->psShader->sInfo.eTessOutPrim = psDecl->value.eTessOutPrim;
                if (psContext->psShader->sInfo.eTessOutPrim == TESSELLATOR_OUTPUT_TRIANGLE_CW)
                    psContext->psShader->sInfo.eTessOutPrim = TESSELLATOR_OUTPUT_TRIANGLE_CCW;
                else if (psContext->psShader->sInfo.eTessOutPrim == TESSELLATOR_OUTPUT_TRIANGLE_CCW)
                    psContext->psShader->sInfo.eTessOutPrim = TESSELLATOR_OUTPUT_TRIANGLE_CW;
            }
            break;
        }
        case OPCODE_DCL_TESS_DOMAIN:
        {
            psContext->psShader->sInfo.eTessDomain = psDecl->value.eTessDomain;

            if (psContext->psShader->sInfo.eTessDomain == TESSELLATOR_DOMAIN_ISOLINE)
                psContext->m_Reflection.OnDiagnostics("Metal Tessellation: domain(\"isoline\") not supported.", 0, true);
            break;
        }
        case OPCODE_DCL_TESS_PARTITIONING:
        {
            psContext->psShader->sInfo.eTessPartitioning = psDecl->value.eTessPartitioning;
            break;
        }
        case OPCODE_DCL_GS_OUTPUT_PRIMITIVE_TOPOLOGY:
        {
            // Not supported
            break;
        }
        case OPCODE_DCL_MAX_OUTPUT_VERTEX_COUNT:
        {
            // Not supported
            break;
        }
        case OPCODE_DCL_GS_INPUT_PRIMITIVE:
        {
            // Not supported
            break;
        }
        case OPCODE_DCL_INTERFACE:
        {
            // Are interfaces ever even used?
            ASSERT(0);
            break;
        }
        case OPCODE_DCL_FUNCTION_BODY:
        {
            ASSERT(0);
            break;
        }
        case OPCODE_DCL_FUNCTION_TABLE:
        {
            ASSERT(0);
            break;
        }
        case OPCODE_CUSTOMDATA:
        {
            // TODO: This is only ever accessed as a float currently. Do trickery if we ever see ints accessed from an array.
            // Walk through all the chunks we've seen in this phase.

            bstring glsl = *psContext->currentGLSLString;
            bformata(glsl, "constant float4 ImmCB_%d[%d] =\n{\n", psContext->currentPhase, psDecl->asImmediateConstBuffer.size());
            bool isFirst = true;
            std::for_each(psDecl->asImmediateConstBuffer.begin(), psDecl->asImmediateConstBuffer.end(), [&](const ICBVec4 &data)
            {
                if (!isFirst)
                {
                    bcatcstr(glsl, ",\n");
                }
                isFirst = false;

                float val[4] = {
                    *(float*)&data.a,
                    *(float*)&data.b,
                    *(float*)&data.c,
                    *(float*)&data.d
                };

                bformata(glsl, "\tfloat4(");
                for (uint32_t k = 0; k < 4; k++)
                {
                    if (k != 0)
                        bcatcstr(glsl, ", ");
                    if (fpcheck(val[k]))
                        bformata(glsl, "as_type<float>(0x%Xu)", *(uint32_t *)&val[k]);
                    else
                        HLSLcc::PrintFloat(glsl, val[k]);
                }
                bcatcstr(glsl, ")");
            });
            bcatcstr(glsl, "\n};\n");
            break;
        }
        case OPCODE_DCL_HS_FORK_PHASE_INSTANCE_COUNT:
        case OPCODE_DCL_HS_JOIN_PHASE_INSTANCE_COUNT:
            break; // Nothing to do

        case OPCODE_DCL_INDEXABLE_TEMP:
        {
            const uint32_t ui32RegIndex = psDecl->sIdxTemp.ui32RegIndex;
            const uint32_t ui32RegCount = psDecl->sIdxTemp.ui32RegCount;
            const uint32_t ui32RegComponentSize = psDecl->sIdxTemp.ui32RegComponentSize;
            bformata(GetEarlyMain(psContext), "float%d TempArray%d[%d];\n", ui32RegComponentSize, ui32RegIndex, ui32RegCount);
            break;
        }
        case OPCODE_DCL_INDEX_RANGE:
        {
            switch (psDecl->asOperands[0].eType)
            {
                case OPERAND_TYPE_OUTPUT:
                case OPERAND_TYPE_INPUT:
                {
                    const ShaderInfo::InOutSignature* psSignature = NULL;
                    const char* type = "float";
                    uint32_t startReg = 0;
                    uint32_t i;
                    bstring *oldString;
                    int regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);
                    int isInput = psDecl->asOperands[0].eType == OPERAND_TYPE_INPUT ? 1 : 0;

                    if (regSpace == 0)
                    {
                        if (isInput)
                            psShader->sInfo.GetInputSignatureFromRegister(
                                psDecl->asOperands[0].ui32RegisterNumber,
                                psDecl->asOperands[0].ui32CompMask,
                                &psSignature);
                        else
                            psShader->sInfo.GetOutputSignatureFromRegister(
                                psDecl->asOperands[0].ui32RegisterNumber,
                                psDecl->asOperands[0].ui32CompMask,
                                psShader->ui32CurrentVertexOutputStream,
                                &psSignature);
                    }
                    else
                        psShader->sInfo.GetPatchConstantSignatureFromRegister(psDecl->asOperands[0].ui32RegisterNumber, psDecl->asOperands[0].ui32CompMask, &psSignature);

                    ASSERT(psSignature != NULL);

                    switch (psSignature->eComponentType)
                    {
                        case INOUT_COMPONENT_UINT32:
                        {
                            type = "uint";
                            break;
                        }
                        case INOUT_COMPONENT_SINT32:
                        {
                            type = "int";
                            break;
                        }
                        case INOUT_COMPONENT_FLOAT32:
                        {
                            break;
                        }
                        default:
                            ASSERT(0);
                            break;
                    }

                    switch (psSignature->eMinPrec) // TODO What if the inputs in the indexed range are of different precisions?
                    {
                        default:
                            break;
                        case MIN_PRECISION_ANY_16:
                            ASSERT(0); // Wut?
                            break;
                        case MIN_PRECISION_FLOAT_16:
                        case MIN_PRECISION_FLOAT_2_8:
                            type = "half";
                            break;
                        case MIN_PRECISION_SINT_16:
                            type = "short";
                            break;
                        case MIN_PRECISION_UINT_16:
                            type = "ushort";
                            break;
                    }

                    startReg = psDecl->asOperands[0].ui32RegisterNumber;
                    oldString = psContext->currentGLSLString;
                    psContext->currentGLSLString = &psContext->psShader->asPhases[psContext->currentPhase].earlyMain;
                    psContext->AddIndentation();
                    bformata(psContext->psShader->asPhases[psContext->currentPhase].earlyMain, "%s4 phase%d_%sput%d_%d[%d];\n", type, psContext->currentPhase, isInput ? "In" : "Out", regSpace, startReg, psDecl->value.ui32IndexRange);
                    glsl = isInput ? psContext->psShader->asPhases[psContext->currentPhase].earlyMain : psContext->psShader->asPhases[psContext->currentPhase].postShaderCode;
                    psContext->currentGLSLString = &glsl;
                    if (isInput == 0)
                        psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode = 1;
                    for (i = 0; i < psDecl->value.ui32IndexRange; i++)
                    {
                        int dummy = 0;
                        std::string realName;
                        uint32_t destMask = psDecl->asOperands[0].ui32CompMask;
                        uint32_t rebase = 0;
                        const ShaderInfo::InOutSignature *psSig = NULL;
                        uint32_t regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);

                        if (regSpace == 0)
                            if (isInput)
                                psContext->psShader->sInfo.GetInputSignatureFromRegister(startReg + i, destMask, &psSig);
                            else
                                psContext->psShader->sInfo.GetOutputSignatureFromRegister(startReg + i, destMask, 0, &psSig);
                        else
                            psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(startReg + i, destMask, &psSig);

                        ASSERT(psSig != NULL);

                        if ((psSig->ui32Mask & destMask) == 0)
                            continue; // Skip dummy writes (vec2 texcoords get filled to vec4 with zeroes etc)

                        while ((psSig->ui32Mask & (1 << rebase)) == 0)
                            rebase++;

                        ((Declaration *)psDecl)->asOperands[0].ui32RegisterNumber = startReg + i;

                        if (isInput)
                        {
                            realName = psContext->GetDeclaredInputName(&psDecl->asOperands[0], &dummy, 1, NULL);

                            psContext->AddIndentation();
                            bformata(glsl, "phase%d_Input%d_%d[%d]", psContext->currentPhase, regSpace, startReg, i);

                            if (destMask != OPERAND_4_COMPONENT_MASK_ALL)
                            {
                                int k;
                                const char *swizzle = "xyzw";
                                bcatcstr(glsl, ".");
                                for (k = 0; k < 4; k++)
                                {
                                    if ((destMask & (1 << k)) && (psSig->ui32Mask & (1 << k)))
                                    {
                                        bformata(glsl, "%c", swizzle[k]);
                                    }
                                }
                            }

                            // for some reason input struct is missed here from GetDeclaredInputName result, so add it manually
                            bformata(glsl, " = input.%s", realName.c_str());
                            if (destMask != OPERAND_4_COMPONENT_MASK_ALL && destMask != psSig->ui32Mask)
                            {
                                int k;
                                const char *swizzle = "xyzw";
                                bcatcstr(glsl, ".");
                                for (k = 0; k < 4; k++)
                                {
                                    if ((destMask & (1 << k)) && (psSig->ui32Mask & (1 << k)))
                                    {
                                        bformata(glsl, "%c", swizzle[k - rebase]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            realName = psContext->GetDeclaredOutputName(&psDecl->asOperands[0], &dummy, NULL, NULL, 0);

                            psContext->AddIndentation();
                            bcatcstr(glsl, realName.c_str());
                            if (destMask != OPERAND_4_COMPONENT_MASK_ALL && destMask != psSig->ui32Mask)
                            {
                                int k;
                                const char *swizzle = "xyzw";
                                bcatcstr(glsl, ".");
                                for (k = 0; k < 4; k++)
                                {
                                    if ((destMask & (1 << k)) && (psSig->ui32Mask & (1 << k)))
                                    {
                                        bformata(glsl, "%c", swizzle[k - rebase]);
                                    }
                                }
                            }

                            bformata(glsl, " = phase%d_Output%d_%d[%d]", psContext->currentPhase, regSpace, startReg, i);

                            if (destMask != OPERAND_4_COMPONENT_MASK_ALL)
                            {
                                int k;
                                const char *swizzle = "xyzw";
                                bcatcstr(glsl, ".");
                                for (k = 0; k < 4; k++)
                                {
                                    if ((destMask & (1 << k)) && (psSig->ui32Mask & (1 << k)))
                                    {
                                        bformata(glsl, "%c", swizzle[k]);
                                    }
                                }
                            }
                        }

                        bcatcstr(glsl, ";\n");
                    }

                    ((Declaration *)psDecl)->asOperands[0].ui32RegisterNumber = startReg;
                    psContext->currentGLSLString = oldString;
                    glsl = *psContext->currentGLSLString;

                    for (i = 0; i < psDecl->value.ui32IndexRange; i++)
                    {
                        if (regSpace == 0)
                        {
                            if (isInput)
                                psShader->sInfo.GetInputSignatureFromRegister(
                                    psDecl->asOperands[0].ui32RegisterNumber + i,
                                    psDecl->asOperands[0].ui32CompMask,
                                    &psSignature);
                            else
                                psShader->sInfo.GetOutputSignatureFromRegister(
                                    psDecl->asOperands[0].ui32RegisterNumber + i,
                                    psDecl->asOperands[0].ui32CompMask,
                                    psShader->ui32CurrentVertexOutputStream,
                                    &psSignature);
                        }
                        else
                            psShader->sInfo.GetPatchConstantSignatureFromRegister(psDecl->asOperands[0].ui32RegisterNumber + i, psDecl->asOperands[0].ui32CompMask, &psSignature);

                        ASSERT(psSignature != NULL);

                        ((ShaderInfo::InOutSignature *)psSignature)->isIndexed.insert(psContext->currentPhase);
                        ((ShaderInfo::InOutSignature *)psSignature)->indexStart[psContext->currentPhase] = startReg;
                        ((ShaderInfo::InOutSignature *)psSignature)->index[psContext->currentPhase] = i;
                    }


                    break;
                }
                default:
                    // TODO Input index ranges.
                    ASSERT(0);
            }
            break;
        }

        case OPCODE_HS_DECLS:
        {
            // Not supported
            break;
        }
        case OPCODE_DCL_INPUT_CONTROL_POINT_COUNT:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
                psShader->sInfo.ui32TessInputControlPointCount = psDecl->value.ui32MaxOutputVertexCount;
            else if (psContext->psShader->eShaderType == DOMAIN_SHADER)
                psShader->sInfo.ui32TessOutputControlPointCount = psDecl->value.ui32MaxOutputVertexCount;
            break;
        }
        case OPCODE_DCL_OUTPUT_CONTROL_POINT_COUNT:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
                psShader->sInfo.ui32TessOutputControlPointCount = psDecl->value.ui32MaxOutputVertexCount;
            break;
        }
        case OPCODE_HS_FORK_PHASE:
        {
            // Not supported
            break;
        }
        case OPCODE_HS_JOIN_PHASE:
        {
            // Not supported
            break;
        }
        case OPCODE_DCL_SAMPLER:
        {
            std::string name = TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY);

            if (!EmitInlineSampler(psContext, name))
            {
                // for some reason we have some samplers start with "sampler" and some not
                const bool startsWithSampler = name.find("sampler") == 0;

                std::ostringstream samplerOss;
                samplerOss << (startsWithSampler ? "" : "sampler") << name;
                std::string samplerName = samplerOss.str();

                if (!psContext->psDependencies->IsMemberDeclared(samplerName))
                {
                    const uint32_t slot = m_SamplerSlots.GetBindingSlot(psDecl->asOperands[0].ui32RegisterNumber, BindingSlotAllocator::Texture);
                    std::ostringstream oss;
                    oss << "sampler " << samplerName << " [[ sampler (" << slot << ") ]]";

                    m_StructDefinitions[""].m_Members.push_back(std::make_pair(samplerName, oss.str()));

                    SamplerDesc desc = { name, psDecl->asOperands[0].ui32RegisterNumber, slot };
                    m_Samplers.push_back(desc);
                }
            }

            break;
        }
        case OPCODE_DCL_HS_MAX_TESSFACTOR:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER && psContext->psDependencies)
                psContext->psDependencies->fMaxTessFactor = psDecl->value.fMaxTessFactor;
            break;
        }
        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED:
        {
            // A hack to support single component 32bit RWBuffers: Declare as raw buffer.
            // TODO: Use textures for RWBuffers when the scripting API has actual format selection etc
            // way to flag the created ComputeBuffer as typed. Even then might want to leave this
            // hack path for 32bit (u)int typed buffers to continue support atomic ops on those formats.
            if (psDecl->value.eResourceDimension == RESOURCE_DIMENSION_BUFFER)
            {
                DeclareBufferVariable(psDecl, true, true);
                break;
            }
            std::string texName = ResourceName(RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber);
            std::string samplerTypeName = TranslateResourceDeclaration(psContext, psDecl, texName, false, true);
            if (!psContext->psDependencies->IsMemberDeclared(texName))
            {
                uint32_t slot = m_TextureSlots.GetBindingSlot(psDecl->asOperands[0].ui32RegisterNumber, BindingSlotAllocator::UAV);

                std::ostringstream oss;
                oss << samplerTypeName << " " << texName << " [[ texture(" << slot << ") ]] ";

                m_StructDefinitions[""].m_Members.push_back(std::make_pair(texName, oss.str()));

                HLSLCC_TEX_DIMENSION texDim = TD_INT;
                switch (psDecl->value.eResourceDimension)
                {
                    default: break;
                    case RESOURCE_DIMENSION_TEXTURE2D:
                    case RESOURCE_DIMENSION_TEXTURE2DMS:
                        texDim = TD_2D;
                        break;
                    case RESOURCE_DIMENSION_TEXTURE2DARRAY:
                    case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                        texDim = TD_2DARRAY;
                        break;
                    case RESOURCE_DIMENSION_TEXTURE3D:
                        texDim = TD_3D;
                        break;
                    case RESOURCE_DIMENSION_TEXTURECUBE:
                        texDim = TD_CUBE;
                        break;
                    case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                        texDim = TD_CUBEARRAY;
                        break;
                }
                TextureSamplerDesc desc = {texName, (int)slot, -1, texDim, false, false, true};
                m_Textures.push_back(desc);
            }
            break;
        }

        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED:
        {
            DeclareBufferVariable(psDecl, false, true);
            break;
        }
        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW:
        {
            DeclareBufferVariable(psDecl, true, true);
            break;
        }
        case OPCODE_DCL_RESOURCE_STRUCTURED:
        {
            DeclareBufferVariable(psDecl, false, false);
            break;
        }
        case OPCODE_DCL_RESOURCE_RAW:
        {
            DeclareBufferVariable(psDecl, true, false);
            break;
        }
        case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_STRUCTURED:
        {
            ShaderVarType* psVarType = &psShader->sInfo.sGroupSharedVarType[psDecl->asOperands[0].ui32RegisterNumber];
            std::ostringstream oss;
            oss << "uint value[" << psDecl->sTGSM.ui32Stride / 4 << "]";
            m_StructDefinitions[TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY) + "_Type"].m_Members.push_back(std::make_pair("value", oss.str()));
            m_StructDefinitions[""].m_Dependencies.push_back(TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY) + "_Type");
            oss.str("");
            oss << "threadgroup " << TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY)
            << "_Type " << TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY)
            << "[" << psDecl->sTGSM.ui32Count << "]";

            bformata(GetEarlyMain(psContext), "%s;\n", oss.str().c_str());
            psVarType->name = "$Element";

            psVarType->Columns = psDecl->sTGSM.ui32Stride / 4;
            psVarType->Elements = psDecl->sTGSM.ui32Count;
            break;
        }
        case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_RAW:
        {
            ShaderVarType* psVarType = &psShader->sInfo.sGroupSharedVarType[psDecl->asOperands[0].ui32RegisterNumber];

            std::ostringstream oss;
            oss << "threadgroup uint " << TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY)
            << "[" << (psDecl->sTGSM.ui32Count / psDecl->sTGSM.ui32Stride) << "]";

            bformata(GetEarlyMain(psContext), "%s;\n", oss.str().c_str());
            psVarType->name = "$Element";

            psVarType->Columns = 1;
            psVarType->Elements = psDecl->sTGSM.ui32Count / psDecl->sTGSM.ui32Stride;
            break;
        }

        case OPCODE_DCL_STREAM:
        {
            // Not supported on Metal
            break;
        }
        case OPCODE_DCL_GS_INSTANCE_COUNT:
        {
            // Not supported on Metal
            break;
        }

        default:
            ASSERT(0);
            break;
    }
}

std::string ToMetal::ResourceName(ResourceGroup group, const uint32_t ui32RegisterNumber)
{
    const ResourceBinding* psBinding = 0;
    std::ostringstream oss;
    int found;

    found = psContext->psShader->sInfo.GetResourceFromBindingPoint(group, ui32RegisterNumber, &psBinding);

    if (found)
    {
        size_t i = 0;
        std::string name = psBinding->name;
        uint32_t ui32ArrayOffset = ui32RegisterNumber - psBinding->ui32BindPoint;

        while (i < name.length())
        {
            //array syntax [X] becomes _0_
            //Otherwise declarations could end up as:
            //uniform sampler2D SomeTextures[0];
            //uniform sampler2D SomeTextures[1];
            if (name[i] == '[' || name[i] == ']')
                name[i] = '_';

            ++i;
        }

        if (ui32ArrayOffset)
        {
            oss << name << ui32ArrayOffset;
            return oss.str();
        }
        else
        {
            return name;
        }
    }
    else
    {
        oss << "UnknownResource" << ui32RegisterNumber;
        return oss.str();
    }
}

void ToMetal::TranslateResourceTexture(const Declaration* psDecl, uint32_t samplerCanDoShadowCmp, HLSLCC_TEX_DIMENSION texDim)
{
    std::string texName = ResourceName(RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber);
    const bool isDepthSampler = (samplerCanDoShadowCmp && psDecl->ui32IsShadowTex);
    std::string samplerTypeName = TranslateResourceDeclaration(psContext, psDecl, texName, isDepthSampler, false);

    bool isMS = false;
    switch (psDecl->value.eResourceDimension)
    {
        default:
            break;
        case RESOURCE_DIMENSION_TEXTURE2DMS:
        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
            isMS = true;
            break;
    }

    if (!psContext->psDependencies->IsMemberDeclared(texName))
    {
        uint32_t slot = m_TextureSlots.GetBindingSlot(psDecl->asOperands[0].ui32RegisterNumber, BindingSlotAllocator::Texture);

        std::ostringstream oss;
        oss << samplerTypeName << " " << texName << " [[ texture(" << slot << ") ]] ";

        m_StructDefinitions[""].m_Members.push_back(std::make_pair(texName, oss.str()));

        TextureSamplerDesc desc = {texName, (int)slot, -1, texDim, isMS, isDepthSampler, false};
        m_Textures.push_back(desc);

        if (isDepthSampler)
            EnsureShadowSamplerDeclared();
    }
}

void ToMetal::DeclareResource(const Declaration *psDecl)
{
    switch (psDecl->value.eResourceDimension)
    {
        case RESOURCE_DIMENSION_BUFFER:
        {
            // Fake single comp 32bit texel buffers by using raw buffer
            DeclareBufferVariable(psDecl, true, false);
            break;

            // TODO: re-enable this code for buffer textures when sripting API has proper support for it
#if 0
            if (!psContext->psDependencies->IsMemberDeclared(texName))
            {
                uint32_t slot = m_TextureSlots.GetBindingSlot(psDecl->asOperands[0].ui32RegisterNumber, BindingSlotAllocator::Texture);
                std::string texName = TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NAME_ONLY);
                std::ostringstream oss;
                oss << "device " << TranslateResourceDeclaration(psContext, psDecl, texName, false, false);

                oss << texName << " [[ texture(" << slot << ") ]]";

                m_StructDefinitions[""].m_Members.push_back(std::make_pair(texName, oss.str()));
                psContext->m_Reflection.OnTextureBinding(texName, slot, TD_2D, false); //TODO: correct HLSLCC_TEX_DIMENSION?
            }
            break;
#endif
        }
        default:
            ASSERT(0);
            break;

        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            TranslateResourceTexture(psDecl, 1, TD_2D); //TODO: correct HLSLCC_TEX_DIMENSION?
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            TranslateResourceTexture(psDecl, 1, TD_2D);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            TranslateResourceTexture(psDecl, 0, TD_2D);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            TranslateResourceTexture(psDecl, 0, TD_3D);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            TranslateResourceTexture(psDecl, 1, TD_CUBE);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            TranslateResourceTexture(psDecl, 1, TD_2DARRAY); //TODO: correct HLSLCC_TEX_DIMENSION?
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            TranslateResourceTexture(psDecl, 1, TD_2DARRAY);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            TranslateResourceTexture(psDecl, 0, TD_2DARRAY);
            break;
        }
        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            TranslateResourceTexture(psDecl, 1, TD_CUBEARRAY);
            break;
        }
    }
    psContext->psShader->aeResourceDims[psDecl->asOperands[0].ui32RegisterNumber] = psDecl->value.eResourceDimension;
}

void ToMetal::DeclareOutput(const Declaration *psDecl)
{
    Shader* psShader = psContext->psShader;

    if (!psContext->OutputNeedsDeclaring(&psDecl->asOperands[0], 1))
        return;

    const Operand* psOperand = &psDecl->asOperands[0];
    int iNumComponents;
    int regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);
    uint32_t ui32Reg = psDecl->asOperands[0].ui32RegisterNumber;

    const ShaderInfo::InOutSignature* psSignature = NULL;
    SHADER_VARIABLE_TYPE cType = SVT_VOID;

    if (psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH ||
        psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL ||
        psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL)
    {
        iNumComponents = 1;
        cType = SVT_FLOAT;
    }
    else
    {
        if (regSpace == 0)
            psShader->sInfo.GetOutputSignatureFromRegister(
                ui32Reg,
                psDecl->asOperands[0].ui32CompMask,
                psShader->ui32CurrentVertexOutputStream,
                &psSignature);
        else
            psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Reg, psDecl->asOperands[0].ui32CompMask, &psSignature);

        iNumComponents = HLSLcc::GetNumberBitsSet(psSignature->ui32Mask);

        switch (psSignature->eComponentType)
        {
            case INOUT_COMPONENT_UINT32:
            {
                cType = SVT_UINT;
                break;
            }
            case INOUT_COMPONENT_SINT32:
            {
                cType = SVT_INT;
                break;
            }
            case INOUT_COMPONENT_FLOAT32:
            {
                cType = SVT_FLOAT;
                break;
            }
            default:
                ASSERT(0);
                break;
        }
        // Don't set this for oDepth (or variants), because depth output register is in separate space from other outputs (regno 0, but others may overlap with that)
        if (iNumComponents == 1)
            psContext->psShader->abScalarOutput[regSpace][ui32Reg] |= (int)psDecl->asOperands[0].ui32CompMask;

        switch (psOperand->eMinPrecision)
        {
            case OPERAND_MIN_PRECISION_DEFAULT:
                break;
            case OPERAND_MIN_PRECISION_FLOAT_16:
                cType = SVT_FLOAT16;
                break;
            case OPERAND_MIN_PRECISION_FLOAT_2_8:
                cType = SVT_FLOAT10;
                break;
            case OPERAND_MIN_PRECISION_SINT_16:
                cType = SVT_INT16;
                break;
            case OPERAND_MIN_PRECISION_UINT_16:
                cType = SVT_UINT16;
                break;
        }
    }

    std::string type = HLSLcc::GetConstructorForTypeMetal(cType, iNumComponents);
    std::string name = psContext->GetDeclaredOutputName(&psDecl->asOperands[0], nullptr, nullptr, nullptr, 1);

    switch (psShader->eShaderType)
    {
        case PIXEL_SHADER:
        {
            switch (psDecl->asOperands[0].eType)
            {
                case OPERAND_TYPE_OUTPUT_COVERAGE_MASK:
                {
                    std::ostringstream oss;
                    oss << type << " " << name << " [[ sample_mask ]]";
                    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(name, oss.str()));
                    break;
                }
                case OPERAND_TYPE_OUTPUT_DEPTH:
                {
                    std::ostringstream oss;
                    oss << type << " " << name << " [[ depth(any) ]]";
                    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(name, oss.str()));
                    break;
                }
                case OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL:
                {
                    std::ostringstream oss;
                    oss << type << " " << name << " [[ depth(greater) ]]";
                    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(name, oss.str()));
                    break;
                }
                case OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL:
                {
                    std::ostringstream oss;
                    oss << type << " " << name << " [[ depth(less) ]]";
                    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(name, oss.str()));
                    break;
                }
                default:
                {
                    std::ostringstream oss;
                    oss << type << " " << name << " [[ color(xlt_remap_o[" << psSignature->ui32SemanticIndex << "]) ]]";
                    m_NeedFBOutputRemapDecl = true;
                    m_StructDefinitions[GetOutputStructName()].m_Members.push_back(std::make_pair(name, oss.str()));
                    psContext->m_Reflection.OnFragmentOutputDeclaration(iNumComponents, psSignature->ui32SemanticIndex);
                }
            }
            break;
        }
        case VERTEX_SHADER:
        case DOMAIN_SHADER:
        case HULL_SHADER:
        {
            std::string out = GetOutputStructName();
            bool isTessKernel = (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) != 0 && (psContext->psShader->eShaderType == HULL_SHADER || psContext->psShader->eShaderType == VERTEX_SHADER);

            std::ostringstream oss;
            oss << type << " " << name;
            if (!isTessKernel && (psSignature->eSystemValueType == NAME_POSITION || psSignature->semanticName == "POS") && psOperand->ui32RegisterNumber == 0)
                oss << " [[ position ]]";
            else if (!isTessKernel && psSignature->eSystemValueType == NAME_UNDEFINED && psSignature->semanticName == "PSIZE" && psSignature->ui32SemanticIndex == 0)
                oss << " [[ point_size ]]";
            else
                oss << " [[ user(" << name << ") ]]";
            m_StructDefinitions[out].m_Members.push_back(std::make_pair(name, oss.str()));

            if (psContext->psShader->eShaderType == VERTEX_SHADER)
                psContext->m_Reflection.OnVertexProgramOutput(name, psSignature->semanticName, psSignature->ui32SemanticIndex);

            // For preserving data layout, declare output struct as domain shader input, too
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                out += "In";

                std::ostringstream oss;
                oss << type << " " << name;

                // VERTEX_SHADER hardcoded on purpose
                bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);
                uint32_t loc = psContext->psDependencies->GetVaryingLocation(name, VERTEX_SHADER, true, keepLocation, psContext->psShader->maxSemanticIndex);
                oss << " [[ " << "attribute(" << loc << ")" << " ]] ";

                psContext->m_Reflection.OnInputBinding(name, loc);
                m_StructDefinitions[out].m_Members.push_back(std::make_pair(name, oss.str()));
            }
            break;
        }
        case GEOMETRY_SHADER:
        default:
            ASSERT(0);
            break;
    }
    HandleOutputRedirect(psDecl, HLSLcc::GetConstructorForTypeMetal(cType, 4));
}

void ToMetal::EnsureShadowSamplerDeclared()
{
    // on macos we will set comparison func from the app side
    if (m_ShadowSamplerDeclared || !IsMobileTarget(psContext))
        return;

    if ((psContext->flags & HLSLCC_FLAG_METAL_SHADOW_SAMPLER_LINEAR) != 0 || (psContext->psShader->eShaderType == COMPUTE_SHADER))
        m_ExtraGlobalDefinitions += "constexpr sampler _mtl_xl_shadow_sampler(address::clamp_to_edge, filter::linear, compare_func::greater_equal);\n";
    else
        m_ExtraGlobalDefinitions += "constexpr sampler _mtl_xl_shadow_sampler(address::clamp_to_edge, filter::nearest, compare_func::greater_equal);\n";
    m_ShadowSamplerDeclared = true;
}
