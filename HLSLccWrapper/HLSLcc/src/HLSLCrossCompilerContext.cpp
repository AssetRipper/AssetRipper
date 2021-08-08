#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/Shader.h"
#include "internal_includes/DataTypeAnalysis.h"
#include "internal_includes/UseDefineChains.h"
#include "internal_includes/Declaration.h"
#include "internal_includes/debug.h"
#include "internal_includes/Translator.h"
#include "internal_includes/ControlFlowGraph.h"
#include "internal_includes/languages.h"
#include "include/hlslcc.h"
#include <sstream>

void HLSLCrossCompilerContext::DoDataTypeAnalysis(ShaderPhase *psPhase)
{
    size_t ui32DeclCount = psPhase->psDecl.size();
    uint32_t i;

    psPhase->psTempDeclaration = NULL;
    psPhase->ui32OrigTemps = 0;
    psPhase->ui32TotalTemps = 0;

    // Retrieve the temp decl count
    for (i = 0; i < ui32DeclCount; ++i)
    {
        if (psPhase->psDecl[i].eOpcode == OPCODE_DCL_TEMPS)
        {
            psPhase->ui32TotalTemps = psPhase->psDecl[i].value.ui32NumTemps;
            psPhase->psTempDeclaration = &psPhase->psDecl[i];
            break;
        }
    }

    if (psPhase->ui32TotalTemps == 0)
        return;

    psPhase->ui32OrigTemps = psPhase->ui32TotalTemps;

    // The split table is a table containing the index of the original register this register was split out from, or 0xffffffff
    // Format: lowest 16 bits: original register. bits 16-23: rebase (eg value of 1 means .yzw was changed to .xyz): bits 24-31: component count
    psPhase->pui32SplitInfo.clear();
    psPhase->pui32SplitInfo.resize(psPhase->ui32TotalTemps * 2, 0xffffffff);

    // Build use-define chains and split temps based on those.
    {
        DefineUseChains duChains;
        UseDefineChains udChains;

        BuildUseDefineChains(psPhase->psInst, psPhase->ui32TotalTemps, duChains, udChains, psPhase->GetCFG());

        CalculateStandaloneDefinitions(duChains, psPhase->ui32TotalTemps);

        // Only do sampler precision downgrade with pixel shaders on mobile targets / Switch
        if (psShader->eShaderType == PIXEL_SHADER && (IsMobileTarget(this) || IsSwitch()))
            UpdateSamplerPrecisions(psShader->sInfo, duChains, psPhase->ui32TotalTemps);

        UDSplitTemps(&psPhase->ui32TotalTemps, duChains, udChains, psPhase->pui32SplitInfo);

        WriteBackUsesAndDefines(duChains);
    }

    HLSLcc::DataTypeAnalysis::SetDataTypes(this, psPhase->psInst, psPhase->ui32TotalTemps, psPhase->peTempTypes);

    if (psPhase->psTempDeclaration && (psPhase->ui32OrigTemps != psPhase->ui32TotalTemps))
        psPhase->psTempDeclaration->value.ui32NumTemps = psPhase->ui32TotalTemps;
}

void HLSLCrossCompilerContext::ReserveFramebufferFetchInputs()
{
    if (psShader->eShaderType != PIXEL_SHADER)
        return;

    if (!psShader->extensions->EXT_shader_framebuffer_fetch)
        return;

    if ((flags & HLSLCC_FLAG_SHADER_FRAMEBUFFER_FETCH) == 0)
        return;

    if (!(psShader->eTargetLanguage >= LANG_ES_300 && psShader->eTargetLanguage <= LANG_ES_LAST))
        return;

    if (!psDependencies)
        return;

    if (!HaveUniformBindingsAndLocations(psShader->eTargetLanguage, psShader->extensions, flags) &&
        ((flags & HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS) == 0 || (flags & HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS) != 0))
        return;

    // The Adreno GLSL compiler fails to compile shaders that use the same location for textures and inout attachments
    // So here we figure out the maximum index of any inout render target and then make sure that we never use those for textures.
    int maxInOutRenderTargetIndex = -1;
    for (const Declaration& decl : psShader->asPhases[0].psDecl)
    {
        if (decl.eOpcode != OPCODE_DCL_INPUT_PS)
            continue;

        const Operand& operand = decl.asOperands[0];
        if (!operand.iPSInOut)
            continue;

        const ShaderInfo::InOutSignature* signature = NULL;
        if (!psShader->sInfo.GetInputSignatureFromRegister(operand.ui32RegisterNumber, operand.ui32CompMask, &signature, true))
            continue;

        const int index = signature->ui32SemanticIndex;
        if (index > maxInOutRenderTargetIndex)
            maxInOutRenderTargetIndex = index;
    }

    if (maxInOutRenderTargetIndex >= 0)
    {
        if (maxInOutRenderTargetIndex >= psDependencies->m_NextAvailableGLSLResourceBinding[GLSLCrossDependencyData::BufferType_Texture])
            psDependencies->m_NextAvailableGLSLResourceBinding[GLSLCrossDependencyData::BufferType_Texture] = maxInOutRenderTargetIndex + 1;
    }
}

void HLSLCrossCompilerContext::ClearDependencyData()
{
    switch (psShader->eShaderType)
    {
        case PIXEL_SHADER:
        {
            psDependencies->ClearCrossDependencyData();
            break;
        }
        case HULL_SHADER:
        {
            psDependencies->eTessPartitioning = TESSELLATOR_PARTITIONING_UNDEFINED;
            psDependencies->eTessOutPrim = TESSELLATOR_OUTPUT_UNDEFINED;
            break;
        }
        default:
            break;
    }
}

void HLSLCrossCompilerContext::AddIndentation()
{
    int i;
    bstring glsl = *currentGLSLString;
    for (i = 0; i < indent; ++i)
    {
        bcatcstr(glsl, "    ");
    }
}

bool HLSLCrossCompilerContext::RequireExtension(const std::string &extName)
{
    if (m_EnabledExtensions.find(extName) != m_EnabledExtensions.end())
        return true;

    m_EnabledExtensions.insert(extName);
    bformata(extensions, "#extension %s : require\n", extName.c_str());
    return false;
}

bool HLSLCrossCompilerContext::EnableExtension(const std::string &extName)
{
    if (m_EnabledExtensions.find(extName) != m_EnabledExtensions.end())
        return true;

    m_EnabledExtensions.insert(extName);
    bformata(extensions, "#ifdef %s\n", extName.c_str());
    bformata(extensions, "#extension %s : enable\n", extName.c_str());
    bcatcstr(extensions, "#endif\n");
    return false;
}

std::string HLSLCrossCompilerContext::GetDeclaredInputName(const Operand* psOperand, int *piRebase, int iIgnoreRedirect, uint32_t *puiIgnoreSwizzle) const
{
    std::ostringstream oss;
    const ShaderInfo::InOutSignature* psIn = NULL;
    int regSpace = psOperand->GetRegisterSpace(this);

    if (iIgnoreRedirect == 0)
    {
        if ((regSpace == 0 && psShader->asPhases[currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe)
            ||
            (regSpace == 1 && psShader->asPhases[currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe))
        {
            oss << "phase" << currentPhase << "_Input" << regSpace << "_" << psOperand->ui32RegisterNumber;
            if (piRebase)
                *piRebase = 0;
            return oss.str();
        }
    }

    if (regSpace == 0)
    {
        if (psShader->sInfo.GetInputSignatureFromType(psOperand->eType, &psIn) == false) {
            psShader->sInfo.GetInputSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), &psIn, true);
        }
    }
    else
    {
        psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), &psIn, true);
    }

    if (psIn && piRebase)
        *piRebase = psIn->iRebase;

    const std::string patchPrefix = psShader->eTargetLanguage == LANG_METAL ? "patch." : "patch";
    std::string res = "";

    bool skipPrefix = false;
    if (psTranslator->TranslateSystemValue(psOperand, psIn, res, puiIgnoreSwizzle, psShader->aIndexedInput[regSpace][psOperand->ui32RegisterNumber] != 0, true, &skipPrefix, &iIgnoreRedirect))
    {
        if (psShader->eTargetLanguage == LANG_METAL && (iIgnoreRedirect == 0) && !skipPrefix)
            return inputPrefix + res;
        else
            return res;
    }

    ASSERT(psIn != NULL);
    oss << inputPrefix << (regSpace == 1 ? patchPrefix : "") << psIn->semanticName << psIn->ui32SemanticIndex;
    return oss.str();
}

std::string HLSLCrossCompilerContext::GetDeclaredOutputName(const Operand* psOperand,
    int* piStream,
    uint32_t *puiIgnoreSwizzle,
    int *piRebase,
    int iIgnoreRedirect) const
{
    std::ostringstream oss;
    const ShaderInfo::InOutSignature* psOut = NULL;
    int regSpace = psOperand->GetRegisterSpace(this);

    if (iIgnoreRedirect == 0)
    {
        if ((regSpace == 0 && psShader->asPhases[currentPhase].acOutputNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe)
            || (regSpace == 1 && psShader->asPhases[currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] == 0xfe))
        {
            oss << "phase" << currentPhase << "_Output" << regSpace << "_" << psOperand->ui32RegisterNumber;
            if (piRebase)
                *piRebase = 0;
            return oss.str();
        }
    }

    if (regSpace == 0)
        psShader->sInfo.GetOutputSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), psShader->ui32CurrentVertexOutputStream, &psOut, true);
    else
        psShader->sInfo.GetPatchConstantSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), &psOut, true);


    if (psOut && piRebase)
        *piRebase = psOut->iRebase;

    if (psOut && (psOut->isIndexed.find(currentPhase) != psOut->isIndexed.end()))
    {
        // Need to route through temp output variable
        oss << "phase" << currentPhase << "_Output" << regSpace << "_" << psOut->indexStart.find(currentPhase)->second;
        if (!psOperand->m_SubOperands[0].get())
        {
            oss << "[" << psOperand->ui32RegisterNumber << "]";
        }
        if (piRebase)
            *piRebase = 0;
        return oss.str();
    }

    const std::string patchPrefix = psShader->eTargetLanguage == LANG_METAL ? "patch." : "patch";
    std::string res = "";

    if (psTranslator->TranslateSystemValue(psOperand, psOut, res, puiIgnoreSwizzle, psShader->aIndexedOutput[regSpace][psOperand->ui32RegisterNumber], false, NULL, &iIgnoreRedirect))
    {
        // clip/cull planes will always have interim variable, as HLSL operates on float4 but we need to size output accordingly with actual planes count
        // with tessellation factor buffers, a separate buffer from output is used. for some reason TranslateSystemValue return *outSkipPrefix = true
        // for ALL system vars and then we simply ignore it here, so opt to modify iIgnoreRedirect for these special cases

        if (psShader->eTargetLanguage == LANG_METAL && regSpace == 0 && (iIgnoreRedirect == 0))
            return outputPrefix + res;
        else if (psShader->eTargetLanguage == LANG_METAL && (iIgnoreRedirect == 0))
            return patchPrefix + res;
        else
            return res;
    }
    ASSERT(psOut != NULL);

    oss << outputPrefix << (regSpace == 1 ? patchPrefix : "") << psOut->semanticName << psOut->ui32SemanticIndex;
    return oss.str();
}

bool HLSLCrossCompilerContext::OutputNeedsDeclaring(const Operand* psOperand, const int count)
{
    char compMask = (char)psOperand->ui32CompMask;
    int regSpace = psOperand->GetRegisterSpace(this);
    uint32_t startIndex = psOperand->ui32RegisterNumber + (psShader->ui32CurrentVertexOutputStream * 1024); // Assume less than 1K input streams
    ASSERT(psShader->ui32CurrentVertexOutputStream < 4);

    // First check for various builtins, mostly depth-output ones.
    if (psShader->eShaderType == PIXEL_SHADER)
    {
        if (psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL ||
            psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL)
        {
            return true;
        }

        if (psOperand->eType == OPERAND_TYPE_OUTPUT_DEPTH)
        {
            // GL doesn't need declaration, Metal does.
            return psShader->eTargetLanguage == LANG_METAL;
        }
    }

    // Needs declaring if any of the components hasn't been already declared
    if ((compMask & ~psShader->acOutputDeclared[regSpace][startIndex]) != 0)
    {
        int offset;
        const ShaderInfo::InOutSignature* psSignature = NULL;

        if (psOperand->eSpecialName == NAME_UNDEFINED)
        {
            // Need to fetch the actual comp mask
            if (regSpace == 0)
                psShader->sInfo.GetOutputSignatureFromRegister(
                    psOperand->ui32RegisterNumber,
                    psOperand->ui32CompMask,
                    psShader->ui32CurrentVertexOutputStream,
                    &psSignature);
            else
                psShader->sInfo.GetPatchConstantSignatureFromRegister(
                    psOperand->ui32RegisterNumber,
                    psOperand->ui32CompMask,
                    &psSignature);

            compMask = (char)psSignature->ui32Mask;
        }
        for (offset = 0; offset < count; offset++)
        {
            psShader->acOutputDeclared[regSpace][startIndex + offset] |= compMask;
        }

        if (psSignature && (psSignature->semanticName == "PSIZE") && (psShader->eTargetLanguage != LANG_METAL))
        {
            // gl_PointSize, doesn't need declaring. TODO: Metal doesn't have pointsize at all?
            return false;
        }

        return true;
    }

    return false;
}

bool HLSLCrossCompilerContext::IsVulkan() const
{
    return (flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;
}

bool HLSLCrossCompilerContext::IsSwitch() const
{
    return (flags & HLSLCC_FLAG_NVN_TARGET) != 0;
}
