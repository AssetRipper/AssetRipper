#include "hlslcc.h"
#include "internal_includes/Declaration.h"
#include "internal_includes/toGLSLOperand.h"
#include "internal_includes/toGLSL.h"
#include "internal_includes/languages.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/Shader.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "bstrlib.h"
#include "internal_includes/debug.h"
#include <math.h>
#include <float.h>
#include <sstream>
#include <algorithm>
#include <cmath>
#include "internal_includes/toGLSL.h"
#include "UnityInstancingFlexibleArraySize.h"

using namespace HLSLcc;

#ifndef fpcheck
#ifdef _MSC_VER
#define fpcheck(x) (_isnan(x) || !_finite(x))
#else
#define fpcheck(x) (std::isnan(x) || std::isinf(x))
#endif
#endif // #ifndef fpcheck

static bool UseReflection(HLSLCrossCompilerContext* psContext)
{
    return !psContext->IsSwitch() && psContext->psShader->eShaderType != COMPUTE_SHADER;
}

static SHADER_VARIABLE_TYPE TypeToReport(SHADER_VARIABLE_TYPE type)
{
    switch (type)
    {
        case SVT_BOOL:
        case SVT_INT:
        case SVT_UINT:
        case SVT_UINT8:
        case SVT_FORCED_INT:
        case SVT_INT_AMBIGUOUS:
        case SVT_INT16:
        case SVT_INT12:
        case SVT_UINT16:
            return SVT_UINT;

        case SVT_FLOAT:
        case SVT_FLOAT10:
        case SVT_FLOAT16:
            return SVT_FLOAT;

        default:
            return type;
    }
}

static void GenerateUnsupportedFormatWarning(HLSLccReflection& refl, const char* name)
{
    std::ostringstream oss;
    oss << "The resource '" << name << "' uses an unsupported type/format";
    refl.OnDiagnostics(oss.str(), -1, false);
}

static void GenerateUnsupportedReadWriteFormatWarning(HLSLccReflection& refl, const char* name)
{
    std::ostringstream oss;
    oss << "The resource '" << name << "' uses an unsupported type/format for read/write access";
    refl.OnDiagnostics(oss.str(), -1, false);
}

void ToGLSL::DeclareConstBufferShaderVariable(const char* varName, const struct ShaderVarType* psType, const struct ConstantBuffer* psCBuf, int unsizedArray, bool addUniformPrefix, bool reportInReflection)
{
    bstring glsl = *psContext->currentGLSLString;

    if (reportInReflection && !psContext->IsVulkan() && psType->Class != SVC_STRUCT && UseReflection(psContext))
    {
        const bool isMatrix = psType->Class == SVC_MATRIX_COLUMNS || psType->Class == SVC_MATRIX_ROWS;
        const SHADER_VARIABLE_TYPE type = TypeToReport(psType->Type);
        psContext->m_Reflection.OnConstant(varName, 0, type, psType->Rows, psType->Columns, isMatrix, psType->Elements, true);
    }

    if (psType->Class == SVC_STRUCT)
    {
        bformata(glsl, "\t%s%s_Type %s", addUniformPrefix ? "UNITY_UNIFORM " : "", varName, varName);
        if (psType->Elements > 1)
        {
            if (HLSLcc::IsUnityFlexibleInstancingBuffer(psCBuf))
            {
                bformata(glsl, "[" UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO "]");
                m_NeedUnityInstancingArraySizeDecl = true;
            }
            else
                bformata(glsl, "[%d]", psType->Elements);
        }
    }
    else if (psType->Class == SVC_MATRIX_COLUMNS || psType->Class == SVC_MATRIX_ROWS)
    {
        if (psContext->flags & HLSLCC_FLAG_TRANSLATE_MATRICES)
        {
            // Translate matrices into vec4 arrays
            bformata(glsl, "\t%s%s " HLSLCC_TRANSLATE_MATRIX_FORMAT_STRING "%s", addUniformPrefix ? "UNITY_UNIFORM " : "", HLSLcc::GetConstructorForType(psContext, psType->Type, 4), psType->Rows, psType->Columns, varName);
            uint32_t elemCount = (psType->Class == SVC_MATRIX_COLUMNS ? psType->Columns : psType->Rows);
            if (psType->Elements > 1)
            {
                elemCount *= psType->Elements;
            }
            bformata(glsl, "[%d]", elemCount);
        }
        else
        {
            bformata(glsl, "\t%s%s %s", addUniformPrefix ? "UNITY_UNIFORM " : "", HLSLcc::GetMatrixTypeName(psContext, psType->Type, psType->Columns, psType->Rows).c_str(), varName);
            if (psType->Elements > 1)
            {
                bformata(glsl, "[%d]", psType->Elements);
            }
        }
    }
    else if (psType->Class == SVC_VECTOR && psType->Columns > 1)
    {
        bformata(glsl, "\t%s%s %s", addUniformPrefix ? "UNITY_UNIFORM " : "", HLSLcc::GetConstructorForType(psContext, psType->Type, psType->Columns), varName);

        if (psType->Elements > 1)
        {
            bformata(glsl, "[%d]", psType->Elements);
        }
    }
    else if ((psType->Class == SVC_SCALAR) ||
             (psType->Class == SVC_VECTOR && psType->Columns == 1))
    {
        if (psType->Type == SVT_BOOL)
        {
            //Use int instead of bool.
            //Allows implicit conversions to integer and
            //bool consumes 4-bytes in HLSL and GLSL anyway.
            ((ShaderVarType *)psType)->Type = SVT_INT;
        }

        bformata(glsl, "\t%s%s %s", addUniformPrefix ? "UNITY_UNIFORM " : "", HLSLcc::GetConstructorForType(psContext, psType->Type, 1), varName);

        if (psType->Elements > 1)
        {
            bformata(glsl, "[%d]", psType->Elements);
        }
    }
    if (unsizedArray)
        bformata(glsl, "[]");
    bformata(glsl, ";\n");
}

//In GLSL embedded structure definitions are not supported.
void ToGLSL::PreDeclareStructType(const std::string &name, const struct ShaderVarType* psType)
{
    bstring glsl = *psContext->currentGLSLString;
    uint32_t i;

    for (i = 0; i < psType->MemberCount; ++i)
    {
        if (psType->Members[i].Class == SVC_STRUCT)
        {
            PreDeclareStructType(psType->Members[i].name, &psType->Members[i]);
        }
    }

    if (psType->Class == SVC_STRUCT)
    {
        //Not supported at the moment
        ASSERT(name != "$Element");

        for (size_t i = 0; i < m_DefinedStructs.size(); ++i)
        {
            if (m_DefinedStructs[i] == name)
                return;
        }

        m_DefinedStructs.push_back(name);

        bformata(glsl, "struct %s_Type {\n", name.c_str());

        for (i = 0; i < psType->MemberCount; ++i)
        {
            ASSERT(psType->Members.size() != 0);

            DeclareConstBufferShaderVariable(psType->Members[i].name.c_str(), &psType->Members[i], NULL, 0, false, false);
        }

        bformata(glsl, "};\n");
    }
}

static const char* GetInterpolationString(INTERPOLATION_MODE eMode, GLLang lang)
{
    switch (eMode)
    {
        case INTERPOLATION_CONSTANT:
        {
            return "flat ";
        }
        case INTERPOLATION_LINEAR:
        {
            return "";
        }
        case INTERPOLATION_LINEAR_CENTROID:
        {
            return "centroid ";
        }
        case INTERPOLATION_LINEAR_NOPERSPECTIVE:
        {
            return lang <= LANG_ES_310 ? "" : "noperspective ";
            break;
        }
        case INTERPOLATION_LINEAR_NOPERSPECTIVE_CENTROID:
        {
            return lang <= LANG_ES_310 ? "centroid " : "noperspective centroid ";
        }
        case INTERPOLATION_LINEAR_SAMPLE:
        {
            return "sample ";
        }
        case INTERPOLATION_LINEAR_NOPERSPECTIVE_SAMPLE:
        {
            return lang <= LANG_ES_310 ? "" : "noperspective sample ";
        }
        default:
        {
            return "";
        }
    }
}

static void DeclareInput(
    HLSLCrossCompilerContext* psContext,
    const Declaration* psDecl,
    const char* Interpolation, const char* StorageQualifier, const char* Precision, int iNumComponents, OPERAND_INDEX_DIMENSION eIndexDim, const char* InputName, const uint32_t ui32CompMask)
{
    Shader* psShader = psContext->psShader;
    bstring glsl = *psContext->currentGLSLString;
    int regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);
    uint32_t ui32Reg = psDecl->asOperands[0].ui32RegisterNumber;
    const ShaderInfo::InOutSignature *psSig = NULL;

    // This falls within the specified index ranges. The default is 0 if no input range is specified

    if (regSpace == 0)
        psContext->psShader->sInfo.GetInputSignatureFromRegister(ui32Reg, ui32CompMask, &psSig);
    else
        psContext->psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Reg, ui32CompMask, &psSig);

    ASSERT(psSig != NULL);

    // No need to declare input pos 0 on HS control point phases, it's always position
    // Also no point in declaring the builtins
    if (psShader->eShaderType == HULL_SHADER && psShader->asPhases[psContext->currentPhase].ePhase == HS_CTRL_POINT_PHASE)
    {
        if (regSpace == 0)
        {
            if ((psSig->semanticName == "POS" || psSig->semanticName == "SV_Position") && psSig->ui32SemanticIndex == 0)
                return;
        }
    }

    if ((ui32CompMask & ~psShader->acInputDeclared[regSpace][ui32Reg]) != 0)
    {
        const char* vecType = "vec";
        const char* scalarType = "float";

        switch (psSig->eComponentType)
        {
            case INOUT_COMPONENT_UINT32:
            {
                vecType = "uvec";
                scalarType = "uint";
                break;
            }
            case INOUT_COMPONENT_SINT32:
            {
                vecType = "ivec";
                scalarType = "int";
                break;
            }
            case INOUT_COMPONENT_FLOAT32:
            {
                break;
            }
            default:
            {
                ASSERT(0);
                break;
            }
        }

        if (psContext->psDependencies)
        {
            if (psShader->eShaderType == PIXEL_SHADER)
            {
                psContext->psDependencies->SetInterpolationMode(ui32Reg, psDecl->value.eInterpolation);
            }
        }

        std::string locationQualifier = "";

        bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);

        if (HaveInOutLocationQualifier(psContext->psShader->eTargetLanguage) ||
            ((psContext->flags & HLSLCC_FLAG_NVN_TARGET) && HaveLimitedInOutLocationQualifier(psContext->psShader->eTargetLanguage, psContext->psShader->extensions)))
        {
            bool addLocation = false;

            // Add locations to vertex shader inputs unless disabled in flags
            if (psShader->eShaderType == VERTEX_SHADER && !(psContext->flags & HLSLCC_FLAG_DISABLE_EXPLICIT_LOCATIONS))
                addLocation = true;

            // Add intra-shader locations if supported
            if (psShader->eShaderType != VERTEX_SHADER)
                addLocation = true;

            if (addLocation)
            {
                std::ostringstream oss;
                oss << "layout(location = " << psContext->psDependencies->GetVaryingLocation(std::string(InputName), psShader->eShaderType, true, keepLocation, psShader->maxSemanticIndex) << ") ";
                locationQualifier = oss.str();
            }
        }

        psShader->acInputDeclared[regSpace][ui32Reg] = (char)psSig->ui32Mask;

        // Do the reflection report on vertex shader inputs
        if (psShader->eShaderType == VERTEX_SHADER)
        {
            psContext->m_Reflection.OnInputBinding(std::string(InputName), psContext->psDependencies->GetVaryingLocation(std::string(InputName), VERTEX_SHADER, true, keepLocation, psShader->maxSemanticIndex));
        }

        switch (eIndexDim)
        {
            case INDEX_2D:
            {
                if (iNumComponents == 1)
                {
                    const uint32_t regNum =  psDecl->asOperands[0].ui32RegisterNumber;
                    const uint32_t arraySize = psDecl->asOperands[0].aui32ArraySizes[0];

                    psContext->psShader->abScalarInput[regSpace][regNum] |= (int)ui32CompMask;

                    if (psShader->eShaderType == HULL_SHADER || psDecl->asOperands[0].eType == OPERAND_TYPE_INPUT_CONTROL_POINT)
                        bformata(glsl, "%s%s%s %s %s %s [];\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, scalarType, InputName);
                    else
                        bformata(glsl, "%s%s%s %s %s %s [%d];\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, scalarType, InputName, arraySize);
                }
                else
                {
                    if (psShader->eShaderType == HULL_SHADER || psDecl->asOperands[0].eType == OPERAND_TYPE_INPUT_CONTROL_POINT)
                        bformata(glsl, "%s%s%s %s %s%d %s [];\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, vecType, iNumComponents, InputName);
                    else
                        bformata(glsl, "%s%s%s %s %s%d %s [%d];\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, vecType, iNumComponents, InputName,
                            psDecl->asOperands[0].aui32ArraySizes[0]);
                }
                break;
            }
            default:
            {
                if (iNumComponents == 1)
                {
                    psContext->psShader->abScalarInput[regSpace][ui32Reg] |= (int)ui32CompMask;

                    bformata(glsl, "%s%s%s %s %s %s;\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, scalarType, InputName);
                }
                else
                {
                    if (psShader->aIndexedInput[regSpace][ui32Reg] > 0)
                    {
                        bformata(glsl, "%s%s%s %s %s%d %s", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, vecType, iNumComponents, InputName);
                        if (psShader->eShaderType == HULL_SHADER)
                            bcatcstr(glsl, "[];\n");
                        else
                            bcatcstr(glsl, ";\n");
                    }
                    else
                    {
                        if (psShader->eShaderType == HULL_SHADER)
                            bformata(glsl, "%s%s%s %s %s%d %s[];\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, vecType, iNumComponents, InputName);
                        else
                            bformata(glsl, "%s%s%s %s %s%d %s;\n", locationQualifier.c_str(), Interpolation, StorageQualifier, Precision, vecType, iNumComponents, InputName);
                    }
                }
                break;
            }
        }
    }
}

bool ToGLSL::RenderTargetDeclared(uint32_t input)
{
    if (m_DeclaredRenderTarget.find(input) != m_DeclaredRenderTarget.end())
        return true;

    m_DeclaredRenderTarget.insert(input);
    return false;
}

void ToGLSL::AddBuiltinInput(const Declaration* psDecl, const char* builtinName)
{
    Shader* psShader = psContext->psShader;
    const Operand* psOperand = &psDecl->asOperands[0];
    const int regSpace = psOperand->GetRegisterSpace(psContext);
    ASSERT(regSpace == 0);

    // we need to at least mark if they are scalars or not (as we might need to use vector ctor)
    if (psOperand->GetNumInputElements(psContext) == 1)
        psShader->abScalarInput[regSpace][psOperand->ui32RegisterNumber] |= (int)psOperand->ui32CompMask;
}

void ToGLSL::AddBuiltinOutput(const Declaration* psDecl, int arrayElements, const char* builtinName)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;
    const SPECIAL_NAME eSpecialName = psDecl->asOperands[0].eSpecialName;

    if (eSpecialName != NAME_CLIP_DISTANCE && eSpecialName != NAME_CULL_DISTANCE)
        return;

    psContext->psShader->asPhases[psContext->currentPhase].hasPostShaderCode = 1;

    if (psContext->OutputNeedsDeclaring(&psDecl->asOperands[0], arrayElements ? arrayElements : 1))
    {
        const ShaderInfo::InOutSignature* psSignature = NULL;

        psShader->sInfo.GetOutputSignatureFromRegister(
            psDecl->asOperands[0].ui32RegisterNumber,
            psDecl->asOperands[0].ui32CompMask,
            0,
            &psSignature);
        psContext->currentGLSLString = &psContext->psShader->asPhases[psContext->currentPhase].postShaderCode;
        glsl = *psContext->currentGLSLString;
        psContext->indent++;
        if (arrayElements)
        {
        }
        else if ((eSpecialName == NAME_CLIP_DISTANCE || eSpecialName == NAME_CULL_DISTANCE) && psContext->psShader->eShaderType != HULL_SHADER)
        {
            // Case 828454 : For some reason DX compiler seems to inject clip/cull distance declaration to the hull shader sometimes
            // even though it's not used at all, and overlaps some completely unrelated patch constant declarations. We'll just ignore this now.
            // Revisit this if this actually pops up elsewhere.

            // cull/clip distance are pretty similar (the only real difference is extension name (and functionality, but we dont care here))
            int max = psDecl->asOperands[0].GetMaxComponent();

            if (IsESLanguage(psShader->eTargetLanguage))
                psContext->RequireExtension("GL_EXT_clip_cull_distance");
            else if (eSpecialName == NAME_CULL_DISTANCE)
                psContext->RequireExtension("GL_ARB_cull_distance");
            const char* glName = eSpecialName == NAME_CLIP_DISTANCE ? "Clip" : "Cull";

            int applySwizzle = psDecl->asOperands[0].GetNumSwizzleElements() > 1 ? 1 : 0;
            const char* swizzle[] = {".x", ".y", ".z", ".w"};

            ASSERT(psSignature != NULL);
            const int index = psSignature->ui32SemanticIndex;

            //Clip/Cull distance can be spread across 1 or 2 outputs (each no more than a vec4).
            //Some examples:
            //float4 clip[2] : SV_ClipDistance; //8 clip distances
            //float3 clip[2] : SV_ClipDistance; //6 clip distances
            //float4 clip : SV_ClipDistance; //4 clip distances
            //float clip : SV_ClipDistance; //1 clip distance.

            //In GLSL the clip/cull distance built-in is an array of up to 8 floats.
            //So vector to array conversion needs to be done here.
            int multiplier = 1;
            if (index == 1)
            {
                const ShaderInfo::InOutSignature* psFirstClipSignature;
                if (psShader->sInfo.GetOutputSignatureFromSystemValue(eSpecialName, 1, &psFirstClipSignature))
                {
                    if (psFirstClipSignature->ui32Mask & (1 << 3))       multiplier = 4;
                    else if (psFirstClipSignature->ui32Mask & (1 << 2))  multiplier = 3;
                    else if (psFirstClipSignature->ui32Mask & (1 << 1))  multiplier = 2;
                }
            }

            // Add a specially crafted comment so runtime knows to enable clip planes.
            // We may end up doing 2 of these, so at runtime OR the results
            uint32_t clipmask = psDecl->asOperands[0].GetAccessMask();
            if (index != 0)
                clipmask <<= multiplier;
            bformata(psContext->glsl, "// HLSLcc_%sDistances_%x\n", glName, clipmask);

            psContext->psShader->asPhases[psContext->currentPhase].acOutputNeedsRedirect[psSignature->ui32Register] = 0xff;
            bformata(psContext->glsl, "vec4 phase%d_gl%sDistance%d;\n", psContext->currentPhase, glName, index);

            for (int i = 0; i < max; ++i)
            {
                psContext->AddIndentation();
                bformata(glsl, "%s[%d] = (", builtinName, i + multiplier * index);
                TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NONE);
                if (applySwizzle)    bformata(glsl, ")%s;\n", swizzle[i]);
                else                bformata(glsl, ");\n");
            }
        }
        psContext->indent--;
        psContext->currentGLSLString = &psContext->glsl;
    }
}

void ToGLSL::HandleOutputRedirect(const Declaration *psDecl, const char *Precision)
{
    const Operand *psOperand = &psDecl->asOperands[0];
    Shader *psShader = psContext->psShader;
    bstring glsl = *psContext->currentGLSLString;
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

        psContext->AddIndentation();
        bformata(glsl, "%s vec4 phase%d_Output%d_%d;\n", Precision, psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);

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

            numComps = GetNumberBitsSet(psSig->ui32Mask);
            mask = psSig->ui32Mask;

            ((Operand *)psOperand)->ui32CompMask = 1 << comp;
            bstring str = GetPostShaderCode(psContext);
            TranslateOperand(str, psOperand, TO_FLAG_NAME_ONLY);
            bcatcstr(str, " = ");

            if (psSig->eComponentType == INOUT_COMPONENT_SINT32)
            {
                bformata(str, HaveBitEncodingOps(psContext->psShader->eTargetLanguage) ? "floatBitsToInt(" : "int(");
                hasCast = 1;
            }
            else if (psSig->eComponentType == INOUT_COMPONENT_UINT32)
            {
                bformata(str, HaveBitEncodingOps(psContext->psShader->eTargetLanguage) ? "floatBitsToUint(" : "int(");
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

void ToGLSL::AddUserOutput(const Declaration* psDecl)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;

    if (psContext->OutputNeedsDeclaring(&psDecl->asOperands[0], 1))
    {
        const Operand* psOperand = &psDecl->asOperands[0];
        const char* Precision = "";
        int iNumComponents;
        bstring type = NULL;
        int regSpace = psDecl->asOperands[0].GetRegisterSpace(psContext);
        uint32_t ui32Reg = psDecl->asOperands[0].ui32RegisterNumber;

        const ShaderInfo::InOutSignature* psSignature = NULL;

        if (regSpace == 0)
            psShader->sInfo.GetOutputSignatureFromRegister(
                ui32Reg,
                psDecl->asOperands[0].ui32CompMask,
                psShader->ui32CurrentVertexOutputStream,
                &psSignature);
        else
            psShader->sInfo.GetPatchConstantSignatureFromRegister(ui32Reg, psDecl->asOperands[0].ui32CompMask, &psSignature);

        if (psSignature->semanticName == "POS" && psOperand->ui32RegisterNumber == 0 && psContext->psShader->eShaderType == VERTEX_SHADER)
            return;

        iNumComponents = GetNumberBitsSet(psSignature->ui32Mask);
        if (iNumComponents == 1)
            psContext->psShader->abScalarOutput[regSpace][ui32Reg] |= (int)psDecl->asOperands[0].ui32CompMask;

        switch (psSignature->eComponentType)
        {
            case INOUT_COMPONENT_UINT32:
            {
                if (iNumComponents > 1)
                    type = bformat("uvec%d", iNumComponents);
                else
                    type = bformat("uint");
                break;
            }
            case INOUT_COMPONENT_SINT32:
            {
                if (iNumComponents > 1)
                    type = bformat("ivec%d", iNumComponents);
                else
                    type = bformat("int");
                break;
            }
            case INOUT_COMPONENT_FLOAT32:
            {
                if (iNumComponents > 1)
                    type = bformat("vec%d", iNumComponents);
                else
                    type = bformat("float");
                break;
            }
            default:
                ASSERT(0);
                break;
        }

        if (HavePrecisionQualifiers(psContext))
        {
            switch (psOperand->eMinPrecision)
            {
                case OPERAND_MIN_PRECISION_DEFAULT:
                {
                    Precision = "highp ";
                    break;
                }
                case OPERAND_MIN_PRECISION_FLOAT_16:
                {
                    Precision = "mediump ";
                    break;
                }
                case OPERAND_MIN_PRECISION_FLOAT_2_8:
                {
                    Precision = EmitLowp(psContext) ? "lowp " : "mediump ";
                    break;
                }
                case OPERAND_MIN_PRECISION_SINT_16:
                {
                    Precision = "mediump ";
                    //type = "ivec";
                    break;
                }
                case OPERAND_MIN_PRECISION_UINT_16:
                {
                    Precision = "mediump ";
                    //type = "uvec";
                    break;
                }
            }
        }

        switch (psShader->eShaderType)
        {
            case PIXEL_SHADER:
            {
                switch (psDecl->asOperands[0].eType)
                {
                    case OPERAND_TYPE_OUTPUT_COVERAGE_MASK:
                    {
                        break;
                    }
                    case OPERAND_TYPE_OUTPUT_DEPTH:
                    {
                        if (psShader->eTargetLanguage == LANG_ES_100 && !psContext->EnableExtension("GL_EXT_frag_depth"))
                        {
                            bcatcstr(psContext->extensions, "#define gl_FragDepth gl_FragDepthEXT\n");
                        }
                        break;
                    }
                    case OPERAND_TYPE_OUTPUT_DEPTH_GREATER_EQUAL:
                    {
                        psContext->EnableExtension("GL_ARB_conservative_depth");
                        bcatcstr(glsl, "#ifdef GL_ARB_conservative_depth\n");
                        bcatcstr(glsl, "layout (depth_greater) out float gl_FragDepth;\n");
                        bcatcstr(glsl, "#endif\n");
                        break;
                    }
                    case OPERAND_TYPE_OUTPUT_DEPTH_LESS_EQUAL:
                    {
                        psContext->EnableExtension("GL_ARB_conservative_depth");
                        bcatcstr(glsl, "#ifdef GL_ARB_conservative_depth\n");
                        bcatcstr(glsl, "layout (depth_less) out float gl_FragDepth;\n");
                        bcatcstr(glsl, "#endif\n");
                        break;
                    }
                    default:
                    {
                        uint32_t renderTarget = psDecl->asOperands[0].ui32RegisterNumber;

                        char OutputName[512];
                        bstring oname;
                        oname = bformat("%s%s%d", psContext->outputPrefix, psSignature->semanticName.c_str(), renderTarget);
                        strncpy(OutputName, (char *)oname->data, 512);
                        bdestroy(oname);

                        if (psShader->eTargetLanguage == LANG_ES_100 && renderTarget > 0)
                            psContext->EnableExtension("GL_EXT_draw_buffers");

                        bool haveFramebufferFetch = (psShader->extensions->EXT_shader_framebuffer_fetch &&
                            psShader->eShaderType == PIXEL_SHADER &&
                            psContext->flags & HLSLCC_FLAG_SHADER_FRAMEBUFFER_FETCH);

                        if (WriteToFragData(psContext->psShader->eTargetLanguage))
                        {
                            bformata(glsl, "#define %s gl_FragData[%d]\n", OutputName, renderTarget);
                        }
                        else
                        {
                            if (!RenderTargetDeclared(renderTarget))
                            {
                                bstring layoutQualifier = bformat("");

                                if (HaveInOutLocationQualifier(psContext->psShader->eTargetLanguage) ||
                                    HaveLimitedInOutLocationQualifier(psContext->psShader->eTargetLanguage, psContext->psShader->extensions))
                                {
                                    uint32_t index = 0;

                                    if ((psContext->flags & HLSLCC_FLAG_DUAL_SOURCE_BLENDING) && DualSourceBlendSupported(psContext->psShader->eTargetLanguage))
                                    {
                                        if (renderTarget > 0)
                                        {
                                            renderTarget = 0;
                                            index = 1;
                                        }
                                        bdestroy(layoutQualifier);
                                        layoutQualifier = bformat("layout(location = %d, index = %d) ", renderTarget, index);
                                    }
                                    else
                                    {
                                        bdestroy(layoutQualifier);
                                        layoutQualifier = bformat("layout(location = %d) ", renderTarget);
                                    }
                                }

                                auto lq = bstr2cstr(layoutQualifier, '\0');

                                if (haveFramebufferFetch)
                                {
                                    bcatcstr(glsl, "#ifdef GL_EXT_shader_framebuffer_fetch\n");
                                    bformata(glsl, "%sinout %s%s %s;\n", lq, Precision, type->data, OutputName);
                                    bcatcstr(glsl, "#else\n");
                                    bformata(glsl, "%sout %s%s %s;\n", lq, Precision, type->data, OutputName);
                                    bcatcstr(glsl, "#endif\n");
                                }
                                else
                                    bformata(glsl, "%sout %s%s %s;\n", lq, Precision, type->data, OutputName);

                                bcstrfree(lq);
                                bdestroy(layoutQualifier);
                            }
                        }
                        break;
                    }
                }
                break;
            }
            case VERTEX_SHADER:
            case GEOMETRY_SHADER:
            case DOMAIN_SHADER:
            case HULL_SHADER:
            {
                const char* Interpolation = "";
                char OutputName[512];
                bstring oname;
                oname = bformat("%s%s%s%d", psContext->outputPrefix, regSpace == 0 ? "" : "patch", psSignature->semanticName.c_str(), psSignature->ui32SemanticIndex);
                strncpy(OutputName, (char *)oname->data, 512);
                bdestroy(oname);

                if (psShader->eShaderType == VERTEX_SHADER || psShader->eShaderType == GEOMETRY_SHADER)
                {
                    if (psSignature->eComponentType == INOUT_COMPONENT_UINT32 ||
                        psSignature->eComponentType == INOUT_COMPONENT_SINT32) // GLSL spec requires that integer vertex outputs always have "flat" interpolation
                    {
                        Interpolation = GetInterpolationString(INTERPOLATION_CONSTANT, psContext->psShader->eTargetLanguage);
                    }
                    else if (psContext->psDependencies) // For floats we get the interpolation that was resolved from the fragment shader input
                    {
                        Interpolation = GetInterpolationString(psContext->psDependencies->GetInterpolationMode(psDecl->asOperands[0].ui32RegisterNumber), psContext->psShader->eTargetLanguage);
                    }
                }

                if (HaveInOutLocationQualifier(psContext->psShader->eTargetLanguage))
                {
                    bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);
                    bformata(glsl, "layout(location = %d) ", psContext->psDependencies->GetVaryingLocation(std::string(OutputName), psShader->eShaderType, false, keepLocation, psShader->maxSemanticIndex));
                }

                if (InOutSupported(psContext->psShader->eTargetLanguage))
                {
                    if (psContext->psShader->eShaderType == HULL_SHADER)
                    {
                        // In Hull shaders outputs are either per-vertex (and need []) or per-patch (need 'out patch')
                        if (regSpace == 0)
                            bformata(glsl, "%sout %s%s %s[];\n", Interpolation, Precision, type->data, OutputName);
                        else
                            bformata(glsl, "patch %sout %s%s %s;\n", Interpolation, Precision, type->data, OutputName);
                    }
                    else
                        bformata(glsl, "%sout %s%s %s;\n", Interpolation, Precision, type->data, OutputName);
                }
                else
                {
                    bformata(glsl, "%svarying %s%s %s;\n", Interpolation, Precision, type->data, OutputName);
                }

                break;
            }
            default:
                ASSERT(0);
                break;
        }
        HandleOutputRedirect(psDecl, Precision);
        bdestroy(type);
    }
}

void ToGLSL::ReportStruct(const std::string &name, const struct ShaderVarType* psType)
{
    if (psContext->IsVulkan() || psContext->IsSwitch() || psType->Class != SVC_STRUCT)
        return;

    for (uint32_t i = 0; i < psType->MemberCount; ++i)
    {
        if (psType->Members[i].Class == SVC_STRUCT)
            ReportStruct(psType->Members[i].name, &psType->Members[i]);
    }

    for (uint32_t i = 0; i < psType->MemberCount; ++i)
    {
        const bool isMatrix = psType->Members[i].Class == SVC_MATRIX_COLUMNS || psType->Members[i].Class == SVC_MATRIX_ROWS;
        const SHADER_VARIABLE_TYPE type = TypeToReport(psType->Members[i].Type);
        psContext->m_Reflection.OnConstant(psType->Members[i].fullName.c_str(), 0, type, psType->Members[i].Rows, psType->Members[i].Columns, isMatrix, psType->Members[i].Elements, true);
    }

    psContext->m_Reflection.OnConstant(psType->fullName.c_str(), 0, SVT_VOID, psType->Rows, psType->Columns, false, psType->Elements, true);
}

void ToGLSL::DeclareUBOConstants(const uint32_t ui32BindingPoint, const ConstantBuffer* psCBuf, bstring glsl)
{
    uint32_t i;

    bool skipUnused = false;

    if ((psContext->flags & HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS) && psCBuf->name == "$Globals")
        skipUnused = true;


    std::string cbName = psCBuf->name;
    if (cbName == "$Globals")
    {
        // Need to tweak Globals struct name to prevent clashes between shader stages
        char prefix = 'A';
        switch (psContext->psShader->eShaderType)
        {
            default:
                ASSERT(0);
                break;
            case COMPUTE_SHADER:
                prefix = 'C';
                break;
            case VERTEX_SHADER:
                prefix = 'V';
                break;
            case PIXEL_SHADER:
                prefix = 'P';
                break;
            case GEOMETRY_SHADER:
                prefix = 'G';
                break;
            case HULL_SHADER:
                prefix = 'H';
                break;
            case DOMAIN_SHADER:
                prefix = 'D';
                break;
        }

        cbName[0] = prefix;
    }

    for (i = 0; i < psCBuf->asVars.size(); ++i)
    {
        if (skipUnused && !psCBuf->asVars[i].sType.m_IsUsed)
            continue;

        PreDeclareStructType(psCBuf->asVars[i].name, &psCBuf->asVars[i].sType);
    }

    if (psContext->flags & HLSLCC_FLAG_WRAP_UBO)
        bformata(glsl, "#if HLSLCC_ENABLE_UNIFORM_BUFFERS\n");

    uint32_t slot = 0xffffffff;
    bool isKnown = true;

    /* [layout (location = X)] uniform vec4 HLSLConstantBufferName[numConsts]; */
    if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0)
    {
        GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(cbName, false, 1);
        bformata(glsl, "layout(set = %d, binding = %d, std140) ", binding.set, binding.binding);
    }
    else
    {
        if (HaveUniformBindingsAndLocations(psContext->psShader->eTargetLanguage, psContext->psShader->extensions, psContext->flags) || (psContext->flags & HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS))
        {
            GLSLCrossDependencyData::GLSLBufferBindPointInfo bindPointInfo = psContext->psDependencies->GetGLSLResourceBinding(cbName, GLSLCrossDependencyData::BufferType_UBO);
            isKnown = bindPointInfo.known;
            slot = bindPointInfo.slot;
            bformata(glsl, "UNITY_BINDING(%d) ", slot);
        }
        else
            bcatcstr(glsl, "layout(std140) ");

        if (slot != 0xffffffff && !isKnown && UseReflection(psContext))
        {
            psContext->m_Reflection.OnConstantBuffer(cbName, psCBuf->ui32TotalSizeInBytes, psCBuf->GetMemberCount(skipUnused));
            for (i = 0; i < psCBuf->asVars.size(); ++i)
            {
                if (skipUnused && !psCBuf->asVars[i].sType.m_IsUsed)
                    continue;

                ReportStruct(psCBuf->asVars[i].name, &psCBuf->asVars[i].sType);
            }
        }
    }

    const bool reportInReflection = slot != 0xffffffff && !isKnown && UseReflection(psContext);

    bformata(glsl, "uniform %s {\n", cbName.c_str());

    if (psContext->flags & HLSLCC_FLAG_WRAP_UBO)
        bformata(glsl, "#endif\n");

    for (i = 0; i < psCBuf->asVars.size(); ++i)
    {
        if (skipUnused && !psCBuf->asVars[i].sType.m_IsUsed)
            continue;

        DeclareConstBufferShaderVariable(psCBuf->asVars[i].name.c_str(),
            &psCBuf->asVars[i].sType, psCBuf, 0, psContext->flags & HLSLCC_FLAG_WRAP_UBO ? true : false, reportInReflection);
    }

    if (psContext->flags & HLSLCC_FLAG_WRAP_UBO)
        bformata(glsl, "#if HLSLCC_ENABLE_UNIFORM_BUFFERS\n");

    if (psContext->flags & HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT_WITH_INSTANCE_NAME)
    {
        std::string instanceName = UniformBufferInstanceName(psContext, psCBuf->name);
        bformata(glsl, "} %s;\n", instanceName.c_str());
    }
    else
        bcatcstr(glsl, "};\n");

    if (psContext->flags & HLSLCC_FLAG_WRAP_UBO)
        bformata(glsl, "#endif\n");

    if (reportInReflection)
        psContext->m_Reflection.OnConstantBufferBinding(cbName, slot);
}

bool DeclareRWStructuredBufferTemplateTypeAsInteger(HLSLCrossCompilerContext* psContext, const Operand* psOperand)
{
    // with cases like: RWStructuredBuffer<int4> myBuffer; /*...*/ AtomicMin(myBuffer[0].x , myInt);
    // if we translate RWStructuredBuffer template type to uint, incorrect version of the function might be called ( AtomicMin(uint..) instead of AtomicMin(int..) )
    // we try to avoid this case by using integer type in those cases
    if (psContext && psOperand)
    {
        const bool isVulkan = (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;
        if (!isVulkan)
        {
            if (psContext->psShader && HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
            {
                uint32_t ui32BindingPoint = psOperand->ui32RegisterNumber;
                const ResourceBinding* psBinding = NULL;
                psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, ui32BindingPoint, &psBinding);
                if (psBinding)
                {
                    const ConstantBuffer* psBuffer = NULL;
                    psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_UAV, psBinding->ui32BindPoint, &psBuffer);
                    if (psBuffer && psBuffer->asVars.size() == 1 && psBuffer->asVars[0].sType.Type == SVT_INT /*&& psContext->IsSwitch()*/)
                        return true;
                }
            }
        }
    }
    return false;
}

static void DeclareBufferVariable(HLSLCrossCompilerContext* psContext, uint32_t ui32BindingPoint,
    const Operand* psOperand, const uint32_t ui32GloballyCoherentAccess,
    const uint32_t isRaw, const uint32_t isUAV, const uint32_t hasEmbeddedCounter, const uint32_t stride, bstring glsl)
{
    const bool isVulkan = (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;
    bstring BufNamebstr = bfromcstr("");
    // Use original HLSL bindings for UAVs only. For non-UAV buffers we have resolved new binding points from the same register space.

    ResourceName(BufNamebstr, psContext, isUAV ? RGROUP_UAV : RGROUP_TEXTURE, psOperand->ui32RegisterNumber, 0);

    char *btmp = bstr2cstr(BufNamebstr, '\0');
    std::string BufName = btmp;
    bcstrfree(btmp);
    bdestroy(BufNamebstr);

    // Declare the struct type for structured buffers
    if (!isRaw)
    {
        const char* typeStr = "uint";
        if (isUAV && DeclareRWStructuredBufferTemplateTypeAsInteger(psContext, psOperand))
            typeStr = "int";
        bformata(glsl, " struct %s_type {\n\t%s[%d] value;\n};\n\n", BufName.c_str(), typeStr, stride / 4);
    }

    uint32_t slot = 0xffffffff;
    bool isKnown = true;
    if (isVulkan)
    {
        GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(BufName);
        bformata(glsl, "layout(set = %d, binding = %d, std430) ", binding.set, binding.binding);
    }
    else
    {
        GLSLCrossDependencyData::GLSLBufferBindPointInfo bindPointInfo = psContext->psDependencies->GetGLSLResourceBinding(BufName, isUAV ? GLSLCrossDependencyData::BufferType_ReadWrite : GLSLCrossDependencyData::BufferType_SSBO);
        slot = bindPointInfo.slot;
        isKnown = bindPointInfo.known;
        bformata(glsl, "layout(std430, binding = %d) ", slot);
    }

    if (ui32GloballyCoherentAccess & GLOBALLY_COHERENT_ACCESS)
        bcatcstr(glsl, "coherent ");

    if (!isUAV)
        bcatcstr(glsl, "readonly ");

    // For Nintendo Switch, adds a "decoration" to get around not being able to detect readonly modifier on the SSBO via the platform shader reflection API.
    bformata(glsl, "buffer %s%s {\n\t", psContext->IsSwitch() && !isUAV ? "hlslcc_readonly" : "", BufName.c_str());

    if (hasEmbeddedCounter)
        bformata(glsl, "coherent uint %s_counter;\n\t", BufName.c_str());

    if (isRaw)
    {
        if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
            bcatcstr(glsl, "uint");
        else
            bcatcstr(glsl, "int");
    }
    else
        bformata(glsl, "%s_type", BufName.c_str());

    bformata(glsl, " %s_buf[];\n};\n", BufName.c_str());

    if (!isKnown && slot != 0xffffffff && UseReflection(psContext))
        psContext->m_Reflection.OnBufferBinding(BufName, slot, isUAV);
}

void ToGLSL::DeclareStructConstants(const uint32_t ui32BindingPoint,
    const ConstantBuffer* psCBuf, const Operand* psOperand,
    bstring glsl)
{
    uint32_t i;
    int useGlobalsStruct = 1;
    bool skipUnused = false;

    if ((psContext->flags & HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT) && psCBuf->name[0] == '$')
        useGlobalsStruct = 0;

    if ((psContext->flags & HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS) && psCBuf->name == "$Globals")
        skipUnused = true;

    if ((psContext->flags & HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT) == 0)
        useGlobalsStruct = 0;


    for (i = 0; i < psCBuf->asVars.size(); ++i)
    {
        if (skipUnused && !psCBuf->asVars[i].sType.m_IsUsed)
            continue;

        PreDeclareStructType(psCBuf->asVars[i].name, &psCBuf->asVars[i].sType);
    }

    /* [layout (location = X)] uniform vec4 HLSLConstantBufferName[numConsts]; */
    if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0)
    {
        ASSERT(0); // Catch this to see what's going on
        std::string bname = "wut";
        GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(bname);
        bformata(glsl, "layout(set = %d, binding = %d) ", binding.set, binding.binding);
    }
    else
    {
        if (HaveUniformBindingsAndLocations(psContext->psShader->eTargetLanguage, psContext->psShader->extensions, psContext->flags))
            bformata(glsl, "layout(location = %d) ", ui32BindingPoint);
    }
    if (useGlobalsStruct)
    {
        bcatcstr(glsl, "uniform struct ");
        TranslateOperand(psOperand, TO_FLAG_DECLARATION_NAME);

        bcatcstr(glsl, "_Type {\n");
    }
    else
    {
        if (psCBuf->name == "$Globals")
        {
            // GLSL needs to report $Globals in reflection so that SRP batcher can properly determine if the shader is compatible with it or not.
            if (UseReflection(psContext) && !psContext->IsVulkan())
            {
                size_t memberCount = 0;
                for (i = 0; i < psCBuf->asVars.size(); ++i)
                {
                    if (!psCBuf->asVars[i].sType.m_IsUsed)
                        continue;

                    memberCount += psCBuf->asVars[i].sType.GetMemberCount();
                }

                psContext->m_Reflection.OnConstantBuffer(psCBuf->name, 0, memberCount);
            }
        }
    }

    for (i = 0; i < psCBuf->asVars.size(); ++i)
    {
        if (skipUnused && !psCBuf->asVars[i].sType.m_IsUsed)
            continue;

        if (!useGlobalsStruct)
            bcatcstr(glsl, "uniform ");

        DeclareConstBufferShaderVariable(psCBuf->asVars[i].name.c_str(), &psCBuf->asVars[i].sType, psCBuf, 0, false, true);
    }

    if (useGlobalsStruct)
    {
        bcatcstr(glsl, "} ");

        TranslateOperand(psOperand, TO_FLAG_DECLARATION_NAME);

        bcatcstr(glsl, ";\n");
    }
}

static const char* GetVulkanTextureType(HLSLCrossCompilerContext* psContext,
    const RESOURCE_DIMENSION eDimension,
    const uint32_t ui32RegisterNumber)
{
    const ResourceBinding* psBinding = 0;
    RESOURCE_RETURN_TYPE eType = RETURN_TYPE_UNORM;
    int found;
    found = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, ui32RegisterNumber, &psBinding);
    if (found)
    {
        eType = (RESOURCE_RETURN_TYPE)psBinding->ui32ReturnType;
    }
    switch (eDimension)
    {
        case RESOURCE_DIMENSION_BUFFER:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itextureBuffer";
                case RETURN_TYPE_UINT:
                    return "utextureBuffer";
                default:
                    return "textureBuffer";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture1D";
                case RETURN_TYPE_UINT:
                    return "utexture1D";
                default:
                    return "texture1D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture2D";
                case RETURN_TYPE_UINT:
                    return "utexture2D";
                default:
                    return "texture2D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture2DMS";
                case RETURN_TYPE_UINT:
                    return "utexture2DMS";
                default:
                    return "texture2DMS";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture3D";
                case RETURN_TYPE_UINT:
                    return "utexture3D";
                default:
                    return "texture3D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itextureCube";
                case RETURN_TYPE_UINT:
                    return "utextureCube";
                default:
                    return "textureCube";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture1DArray";
                case RETURN_TYPE_UINT:
                    return "utexture1DArray";
                default:
                    return "texture1DArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture2DArray";
                case RETURN_TYPE_UINT:
                    return "utexture2DArray";
                default:
                    return "texture2DArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itexture2DMSArray";
                case RETURN_TYPE_UINT:
                    return "utexture2DMSArray";
                default:
                    return "texture2DMSArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "itextureCubeArray";
                case RETURN_TYPE_UINT:
                    return "utextureCubeArray";
                default:
                    return "textureCubeArray";
            }
            break;
        }
        default:
            ASSERT(0);
            break;
    }

    return "texture2D";
}

static HLSLCC_TEX_DIMENSION GetTextureDimension(HLSLCrossCompilerContext* psContext,
    const RESOURCE_DIMENSION eDimension,
    const uint32_t ui32RegisterNumber)
{
    const ResourceBinding* psBinding = 0;
    RESOURCE_RETURN_TYPE eType = RETURN_TYPE_UNORM;
    int found;
    found = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, ui32RegisterNumber, &psBinding);
    if (found)
    {
        eType = (RESOURCE_RETURN_TYPE)psBinding->ui32ReturnType;
    }

    switch (eDimension)
    {
        case RESOURCE_DIMENSION_BUFFER:
        case RESOURCE_DIMENSION_TEXTURE1D:
            return eType == RETURN_TYPE_SINT || eType == RETURN_TYPE_UINT ? TD_INT : TD_FLOAT;

        case RESOURCE_DIMENSION_TEXTURE2D:
        case RESOURCE_DIMENSION_TEXTURE2DMS:
        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
            return TD_2D;

        case RESOURCE_DIMENSION_TEXTURE3D:
            return TD_3D;

        case RESOURCE_DIMENSION_TEXTURECUBE:
            return TD_CUBE;

        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
            return TD_2DARRAY;

        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
            return TD_CUBEARRAY;
        default:
            ASSERT(0);
            break;
    }

    return TD_2D;
}

// Not static because this is used in toGLSLInstruction.cpp when sampling Vulkan textures
const char* GetSamplerType(HLSLCrossCompilerContext* psContext,
    const RESOURCE_DIMENSION eDimension,
    const uint32_t ui32RegisterNumber)
{
    const ResourceBinding* psBinding = 0;
    RESOURCE_RETURN_TYPE eType = RETURN_TYPE_UNORM;
    int found;
    found = psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, ui32RegisterNumber, &psBinding);
    if (found)
    {
        eType = (RESOURCE_RETURN_TYPE)psBinding->ui32ReturnType;
    }
    switch (eDimension)
    {
        case RESOURCE_DIMENSION_BUFFER:
        {
            if (IsESLanguage(psContext->psShader->eTargetLanguage))
                psContext->RequireExtension("GL_EXT_texture_buffer");
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isamplerBuffer";
                case RETURN_TYPE_UINT:
                    return "usamplerBuffer";
                default:
                    return "samplerBuffer";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler1D";
                case RETURN_TYPE_UINT:
                    return "usampler1D";
                default:
                    return "sampler1D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler2D";
                case RETURN_TYPE_UINT:
                    return "usampler2D";
                default:
                    return "sampler2D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMS:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler2DMS";
                case RETURN_TYPE_UINT:
                    return "usampler2DMS";
                default:
                    return "sampler2DMS";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE3D:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler3D";
                case RETURN_TYPE_UINT:
                    return "usampler3D";
                default:
                    return "sampler3D";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBE:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isamplerCube";
                case RETURN_TYPE_UINT:
                    return "usamplerCube";
                default:
                    return "samplerCube";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE1DARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler1DArray";
                case RETURN_TYPE_UINT:
                    return "usampler1DArray";
                default:
                    return "sampler1DArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler2DArray";
                case RETURN_TYPE_UINT:
                    return "usampler2DArray";
                default:
                    return "sampler2DArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
        {
            if (IsESLanguage(psContext->psShader->eTargetLanguage))
                psContext->RequireExtension("GL_OES_texture_storage_multisample_2d_array");
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isampler2DMSArray";
                case RETURN_TYPE_UINT:
                    return "usampler2DMSArray";
                default:
                    return "sampler2DMSArray";
            }
            break;
        }

        case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
        {
            switch (eType)
            {
                case RETURN_TYPE_SINT:
                    return "isamplerCubeArray";
                case RETURN_TYPE_UINT:
                    return "usamplerCubeArray";
                default:
                    return "samplerCubeArray";
            }
            break;
        }
        default:
            ASSERT(0);
            break;
    }

    return "sampler2D";
}

static const char *GetSamplerPrecision(const HLSLCrossCompilerContext *psContext, REFLECT_RESOURCE_PRECISION ePrec)
{
    if (!HavePrecisionQualifiers(psContext))
        return " ";

    switch (ePrec)
    {
        default:
        case REFLECT_RESOURCE_PRECISION_UNKNOWN:
        case REFLECT_RESOURCE_PRECISION_LOWP:
            return EmitLowp(psContext) ? "lowp " : "mediump ";
        case REFLECT_RESOURCE_PRECISION_HIGHP:
            return "highp ";
        case REFLECT_RESOURCE_PRECISION_MEDIUMP:
            return "mediump ";
    }
}

static void TranslateVulkanResource(HLSLCrossCompilerContext* psContext, const Declaration* psDecl)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;

    const ResourceBinding *psBinding = NULL;
    psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, &psBinding);
    ASSERT(psBinding != NULL);

    const char *samplerPrecision = GetSamplerPrecision(psContext, psBinding ? psBinding->ePrecision : REFLECT_RESOURCE_PRECISION_UNKNOWN);
    std::string tname = ResourceName(psContext, RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, 0);

    const char* samplerTypeName = GetVulkanTextureType(psContext,
        psDecl->value.eResourceDimension,
        psDecl->asOperands[0].ui32RegisterNumber);

    GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(tname);
    bformata(glsl, "layout(set = %d, binding = %d) ", binding.set, binding.binding);
    bcatcstr(glsl, "uniform ");
    bcatcstr(glsl, samplerPrecision);
    bcatcstr(glsl, samplerTypeName);
    bcatcstr(glsl, " ");
    bcatcstr(glsl, tname.c_str());
    bcatcstr(glsl, ";\n");
}

static void TranslateResourceTexture(HLSLCrossCompilerContext* psContext, const Declaration* psDecl, uint32_t samplerCanDoShadowCmp)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;
    const char *samplerPrecision = NULL;
    std::set<uint32_t>::iterator i;

    const char* samplerTypeName = GetSamplerType(psContext,
        psDecl->value.eResourceDimension,
        psDecl->asOperands[0].ui32RegisterNumber);

    if (psDecl->value.eResourceDimension == RESOURCE_DIMENSION_TEXTURECUBEARRAY
        && !HaveCubemapArray(psContext->psShader->eTargetLanguage))
    {
        // Need to enable extension (either OES or ARB), but we only need to add it once
        if (IsESLanguage(psContext->psShader->eTargetLanguage))
        {
            psContext->EnableExtension("GL_OES_texture_cube_map_array");
            psContext->EnableExtension("GL_EXT_texture_cube_map_array");
        }
        else
            psContext->RequireExtension("GL_ARB_texture_cube_map_array");
    }

    if (psContext->psShader->eTargetLanguage == LANG_ES_100 && samplerCanDoShadowCmp && psDecl->ui32IsShadowTex)
    {
        psContext->EnableExtension("GL_EXT_shadow_samplers");
    }

    const ResourceBinding *psBinding = NULL;
    psShader->sInfo.GetResourceFromBindingPoint(RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, &psBinding);
    ASSERT(psBinding != NULL);

    samplerPrecision = GetSamplerPrecision(psContext, psBinding ? psBinding->ePrecision : REFLECT_RESOURCE_PRECISION_UNKNOWN);

    if (psContext->flags & HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS)
    {
        if (samplerCanDoShadowCmp && psDecl->ui32IsShadowTex)
        {
            for (i = psDecl->samplersUsed.begin(); i != psDecl->samplersUsed.end(); i++)
            {
                std::string tname = TextureSamplerName(&psShader->sInfo, psDecl->asOperands[0].ui32RegisterNumber, *i, 1);
                bcatcstr(glsl, "uniform ");
                bcatcstr(glsl, samplerPrecision);
                bcatcstr(glsl, samplerTypeName);
                bcatcstr(glsl, "Shadow ");
                bcatcstr(glsl, tname.c_str());
                bcatcstr(glsl, ";\n");
            }
        }
        for (i = psDecl->samplersUsed.begin(); i != psDecl->samplersUsed.end(); i++)
        {
            std::string tname = TextureSamplerName(&psShader->sInfo, psDecl->asOperands[0].ui32RegisterNumber, *i, 0);
            bcatcstr(glsl, "uniform ");
            bcatcstr(glsl, samplerPrecision);
            bcatcstr(glsl, samplerTypeName);
            bcatcstr(glsl, " ");
            bcatcstr(glsl, tname.c_str());
            bcatcstr(glsl, ";\n");
        }
    }

    std::string tname = ResourceName(psContext, RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, 0);

    bcatcstr(glsl, "uniform ");
    bcatcstr(glsl, samplerPrecision);
    bcatcstr(glsl, samplerTypeName);
    bcatcstr(glsl, " ");
    bcatcstr(glsl, tname.c_str());
    bcatcstr(glsl, ";\n");

    if (samplerCanDoShadowCmp && psDecl->ui32IsShadowTex)
    {
        //Create shadow and non-shadow sampler.
        //HLSL does not have separate types for depth compare, just different functions.
        std::string tname = ResourceName(psContext, RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, 1);

        if (HaveUniformBindingsAndLocations(psContext->psShader->eTargetLanguage, psContext->psShader->extensions, psContext->flags) ||
            ((psContext->flags & HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS) && ((psContext->flags & HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS) != HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS)))
        {
            GLSLCrossDependencyData::GLSLBufferBindPointInfo slotInfo = psContext->psDependencies->GetGLSLResourceBinding(tname, GLSLCrossDependencyData::BufferType_Texture);
            bformata(glsl, "UNITY_LOCATION(%d) ", slotInfo.slot);
        }
        bcatcstr(glsl, "uniform ");
        bcatcstr(glsl, samplerPrecision);
        bcatcstr(glsl, samplerTypeName);
        bcatcstr(glsl, "Shadow ");
        bcatcstr(glsl, tname.c_str());
        bcatcstr(glsl, ";\n");
    }
}

void ToGLSL::HandleInputRedirect(const Declaration *psDecl, const char *Precision)
{
    Operand *psOperand = (Operand *)&psDecl->asOperands[0];
    Shader *psShader = psContext->psShader;
    bstring glsl = *psContext->currentGLSLString;
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

        psContext->AddIndentation();
        // Does the input have multiple array components (such as geometry shader input, or domain shader control point input)
        if ((psShader->eShaderType == DOMAIN_SHADER && regSpace == 0) || (psShader->eShaderType == GEOMETRY_SHADER))
        {
            // The count is actually stored in psOperand->aui32ArraySizes[0]
            origArraySize = psOperand->aui32ArraySizes[0];
            bformata(glsl, "%s vec4 phase%d_Input%d_%d[%d];\n", Precision, psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, origArraySize);
            needsLooping = 1;
            i = origArraySize - 1;
        }
        else
            bformata(glsl, "%s vec4 phase%d_Input%d_%d;\n", Precision, psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);

        psContext->indent++;

        // Do a conditional loop. In normal cases needsLooping == 0 so this is only run once.
        do
        {
            int comp = 0;
            bstring str = GetEarlyMain(psContext);
            if (needsLooping)
                bformata(str, "phase%d_Input%d_%d[%d] = vec4(", psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber, i);
            else
                bformata(str, "phase%d_Input%d_%d = vec4(", psContext->currentPhase, regSpace, psOperand->ui32RegisterNumber);

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
                    numComps = GetNumberBitsSet(psSig->ui32Mask);
                    if (psSig->eComponentType == INOUT_COMPONENT_SINT32)
                    {
                        bformata(str, HaveBitEncodingOps(psContext->psShader->eTargetLanguage) ? "intBitsToFloat(" : "float(");
                        hasCast = 1;
                    }
                    else if (psSig->eComponentType == INOUT_COMPONENT_UINT32)
                    {
                        bformata(str, HaveBitEncodingOps(psContext->psShader->eTargetLanguage) ? "uintBitsToFloat(" : "float(");
                        hasCast = 1;
                    }

                    // Override the array size of the operand so TranslateOperand call below prints the correct index
                    if (needsLooping)
                        psOperand->aui32ArraySizes[0] = i;

                    // And the component mask
                    psOperand->ui32CompMask = 1 << comp;

                    TranslateOperand(str, psOperand, TO_FLAG_NAME_ONLY);

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

        psContext->indent--;

        if (regSpace == 0)
            psShader->asPhases[psContext->currentPhase].acInputNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
        else
            psShader->asPhases[psContext->currentPhase].acPatchConstantsNeedsRedirect[psOperand->ui32RegisterNumber] = 0xfe;
    }
}

void ToGLSL::TranslateDeclaration(const Declaration* psDecl)
{
    bstring glsl = *psContext->currentGLSLString;
    Shader* psShader = psContext->psShader;

    switch (psDecl->eOpcode)
    {
        case OPCODE_DCL_INPUT_SGV:
        case OPCODE_DCL_INPUT_PS_SGV:
        {
            const SPECIAL_NAME eSpecialName = psDecl->asOperands[0].eSpecialName;
            switch (eSpecialName)
            {
                case NAME_POSITION:
                {
                    AddBuiltinInput(psDecl, "gl_Position");
                    break;
                }
                case NAME_RENDER_TARGET_ARRAY_INDEX:
                {
                    AddBuiltinInput(psDecl, "gl_Layer");
                    if (psShader->eShaderType == VERTEX_SHADER)
                    {
                        psContext->RequireExtension("GL_AMD_vertex_shader_layer");
                    }

                    break;
                }
                case NAME_CLIP_DISTANCE:
                {
                    AddBuiltinInput(psDecl, "gl_ClipDistance");
                    break;
                }
                case NAME_CULL_DISTANCE:
                {
                    AddBuiltinInput(psDecl, "gl_CullDistance");
                    break;
                }
                case NAME_VIEWPORT_ARRAY_INDEX:
                {
                    AddBuiltinInput(psDecl, "gl_ViewportIndex");
                    break;
                }
                case NAME_INSTANCE_ID:
                {
                    AddBuiltinInput(psDecl, "gl_InstanceID");
                    break;
                }
                case NAME_IS_FRONT_FACE:
                {
                    /*
                        Cast to int used because
                        if(gl_FrontFacing != 0) failed to compiled on Intel HD 4000.
                        Suggests no implicit conversion for bool<->int.
                    */

                    if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                        AddBuiltinInput(psDecl, "(gl_FrontFacing ? 0xffffffffu : uint(0))");    // Old ES3.0 Adrenos treat 0u as const int
                    else
                        AddBuiltinInput(psDecl, "(gl_FrontFacing ? 1 : 0)");
                    break;
                }
                case NAME_SAMPLE_INDEX:
                {
                    // Using gl_SampleID requires either GL_OES_sample_variables or #version 320 es
                    if (IsESLanguage(psContext->psShader->eTargetLanguage))
                        psContext->RequireExtension("GL_OES_sample_variables");
                    AddBuiltinInput(psDecl, "gl_SampleID");
                    break;
                }
                case NAME_VERTEX_ID:
                {
                    AddBuiltinInput(psDecl, "gl_VertexID");
                    break;
                }
                case NAME_PRIMITIVE_ID:
                {
                    if (psShader->eShaderType == GEOMETRY_SHADER)
                        AddBuiltinInput(psDecl, "gl_PrimitiveIDIn"); // LOL opengl.
                    else
                        AddBuiltinInput(psDecl, "gl_PrimitiveID");
                    break;
                }
                default:
                {
                    bformata(glsl, "in vec4 %s;\n", psDecl->asOperands[0].specialName.c_str());
                }
            }
            break;
        }

        case OPCODE_DCL_OUTPUT_SIV:
        {
            switch (psDecl->asOperands[0].eSpecialName)
            {
                case NAME_POSITION:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_Position");
                    break;
                }
                case NAME_RENDER_TARGET_ARRAY_INDEX:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_Layer");
                    if (psShader->eShaderType == VERTEX_SHADER || psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER)
                    {
                        if (psContext->IsVulkan())
                        {
                            psContext->RequireExtension("GL_ARB_shader_viewport_layer_array");
                        }
                        else if (psContext->IsSwitch())
                        {
                            psContext->RequireExtension("GL_NV_viewport_array2");
                        }
                        else if (psShader->eShaderType == VERTEX_SHADER) // case 1261150
                        {
                            psContext->RequireExtension("GL_AMD_vertex_shader_layer");
                        }
                    }

                    break;
                }
                case NAME_CLIP_DISTANCE:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_ClipDistance");
                    break;
                }
                case NAME_CULL_DISTANCE:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_CullDistance");
                    break;
                }
                case NAME_VIEWPORT_ARRAY_INDEX:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_ViewportIndex");
                    break;
                }
                case NAME_VERTEX_ID:
                {
                    ASSERT(0); //VertexID is not an output
                    break;
                }
                case NAME_PRIMITIVE_ID:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_PrimitiveID");
                    break;
                }
                case NAME_INSTANCE_ID:
                {
                    ASSERT(0); //InstanceID is not an output
                    break;
                }
                case NAME_IS_FRONT_FACE:
                {
                    ASSERT(0); //FrontFacing is not an output
                    break;
                }
                case NAME_FINAL_QUAD_U_EQ_0_EDGE_TESSFACTOR:
                {
                    if (psContext->psShader->aIndexedOutput[1][psDecl->asOperands[0].ui32RegisterNumber])
                    {
                        AddBuiltinOutput(psDecl, 4, "gl_TessLevelOuter");
                    }
                    else
                    {
                        AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[0]");
                    }
                    break;
                }
                case NAME_FINAL_QUAD_V_EQ_0_EDGE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[1]");
                    break;
                }
                case NAME_FINAL_QUAD_U_EQ_1_EDGE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[2]");
                    break;
                }
                case NAME_FINAL_QUAD_V_EQ_1_EDGE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[3]");
                    break;
                }
                case NAME_FINAL_TRI_U_EQ_0_EDGE_TESSFACTOR:
                {
                    if (psContext->psShader->aIndexedOutput[1][psDecl->asOperands[0].ui32RegisterNumber])
                    {
                        AddBuiltinOutput(psDecl, 3, "gl_TessLevelOuter");
                    }
                    else
                    {
                        AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[0]");
                    }
                    break;
                }
                case NAME_FINAL_TRI_V_EQ_0_EDGE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[1]");
                    break;
                }
                case NAME_FINAL_TRI_W_EQ_0_EDGE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[2]");
                    break;
                }
                case NAME_FINAL_LINE_DENSITY_TESSFACTOR:
                {
                    if (psContext->psShader->aIndexedOutput[1][psDecl->asOperands[0].ui32RegisterNumber])
                    {
                        AddBuiltinOutput(psDecl, 2, "gl_TessLevelOuter");
                    }
                    else
                    {
                        AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[0]");
                    }
                    break;
                }
                case NAME_FINAL_LINE_DETAIL_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelOuter[1]");
                    break;
                }
                case NAME_FINAL_TRI_INSIDE_TESSFACTOR:
                case NAME_FINAL_QUAD_U_INSIDE_TESSFACTOR:
                {
                    if (psContext->psShader->aIndexedOutput[1][psDecl->asOperands[0].ui32RegisterNumber])
                    {
                        AddBuiltinOutput(psDecl, 2, "gl_TessLevelInner");
                    }
                    else
                    {
                        AddBuiltinOutput(psDecl, 0, "gl_TessLevelInner[0]");
                    }
                    break;
                }
                case NAME_FINAL_QUAD_V_INSIDE_TESSFACTOR:
                {
                    AddBuiltinOutput(psDecl, 0, "gl_TessLevelInner[1]");
                    break;
                }
                default:
                {
                    // Sometimes DX compiler seems to declare patch constant outputs like this. Anyway, nothing for us to do.
//                  bformata(glsl, "out vec4 %s;\n", psDecl->asOperands[0].specialName.c_str());

/*                    bcatcstr(glsl, "#define ");
                    TranslateOperand(psContext, &psDecl->asOperands[0], TO_FLAG_NONE);
                    bformata(glsl, " %s\n", psDecl->asOperands[0].pszSpecialName);
                    break;*/
                }
            }
            break;
        }
        case OPCODE_DCL_INPUT:
        {
            const Operand* psOperand = &psDecl->asOperands[0];

            int iNumComponents = psOperand->GetNumInputElements(psContext);
            const char* StorageQualifier = "attribute";
            std::string inputName;
            const char* Precision = "";

            if ((psOperand->eType == OPERAND_TYPE_INPUT_DOMAIN_POINT) ||
                (psOperand->eType == OPERAND_TYPE_OUTPUT_CONTROL_POINT_ID) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_COVERAGE_MASK) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_GROUP_ID) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_THREAD_ID_IN_GROUP_FLATTENED) ||
                (psOperand->eType == OPERAND_TYPE_INPUT_FORK_INSTANCE_ID))
            {
                break;
            }

            // No need to declare patch constants read again by the hull shader.
            if ((psOperand->eType == OPERAND_TYPE_INPUT_PATCH_CONSTANT) && psContext->psShader->eShaderType == HULL_SHADER)
            {
                break;
            }

            // Also skip position input to hull and domain shader
            if ((psOperand->eType == OPERAND_TYPE_INPUT_CONTROL_POINT) &&
                (psContext->psShader->eShaderType == HULL_SHADER || psContext->psShader->eShaderType == DOMAIN_SHADER))
            {
                const ShaderInfo::InOutSignature *psIn = NULL;
                psContext->psShader->sInfo.GetInputSignatureFromRegister(psOperand->ui32RegisterNumber, psOperand->GetAccessMask(), &psIn);
                ASSERT(psIn != NULL);

                if ((psIn->semanticName == "SV_POSITION" || psIn->semanticName == "SV_Position"
                     || psIn->semanticName == "POS" || psIn->semanticName == "POSITION") && psIn->ui32SemanticIndex == 0)
                    break;
            }

            //Already declared as part of an array.
            if (psShader->aIndexedInput[psOperand->GetRegisterSpace(psContext)][psDecl->asOperands[0].ui32RegisterNumber] == -1)
            {
                break;
            }

            inputName = psContext->GetDeclaredInputName(psOperand, NULL, 1, NULL);

            // In the case of the Hull Shader, due to the different phases, we might have already delcared this input
            // so check to see if that is the case, and if not record it
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                if (psContext->psDependencies->IsHullShaderInputAlreadyDeclared(inputName))
                {
                    return;
                }

                psContext->psDependencies->RecordHullShaderInput(inputName);
            }

            if (InOutSupported(psContext->psShader->eTargetLanguage))
            {
                if (psOperand->eType == OPERAND_TYPE_INPUT_PATCH_CONSTANT && psContext->psShader->eShaderType == DOMAIN_SHADER)
                    StorageQualifier = "patch in";
                else
                    StorageQualifier = "in";
            }

            if (HavePrecisionQualifiers(psContext))
            {
                switch (psOperand->eMinPrecision)
                {
                    case OPERAND_MIN_PRECISION_DEFAULT:
                    {
                        Precision = "highp";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_FLOAT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_FLOAT_2_8:
                    {
                        Precision = EmitLowp(psContext) ? "lowp " : "mediump ";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_SINT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_UINT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                }
            }

            const char * Interpolation = "";

            if (psShader->eShaderType == GEOMETRY_SHADER || psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER)
            {
                const ShaderInfo::InOutSignature* psSignature = NULL;

                psShader->sInfo.GetInputSignatureFromRegister(psDecl->asOperands[0].ui32RegisterNumber,
                    psDecl->asOperands[0].ui32CompMask,
                    &psSignature, true);

                if ((psSignature != NULL) && (psSignature->eComponentType == INOUT_COMPONENT_UINT32 ||
                                              psSignature->eComponentType == INOUT_COMPONENT_SINT32)) // GLSL spec requires that integer inputs always have "flat" interpolation
                {
                    Interpolation = GetInterpolationString(INTERPOLATION_CONSTANT, psContext->psShader->eTargetLanguage);
                }
                else if (psContext->psDependencies) // For floats we get the interpolation that was resolved from the fragment shader input
                {
                    Interpolation = GetInterpolationString(psContext->psDependencies->GetInterpolationMode(psDecl->asOperands[0].ui32RegisterNumber), psContext->psShader->eTargetLanguage);
                }
            }

            DeclareInput(psContext, psDecl,
                Interpolation, StorageQualifier, Precision, iNumComponents, (OPERAND_INDEX_DIMENSION)psOperand->iIndexDims, inputName.c_str(), psOperand->ui32CompMask);

            HandleInputRedirect(psDecl, Precision);
            break;
        }
        case OPCODE_DCL_INPUT_PS_SIV:
        {
            switch (psDecl->asOperands[0].eSpecialName)
            {
                case NAME_POSITION:
                {
                    AddBuiltinInput(psDecl, "gl_FragCoord");
                    bcatcstr(GetEarlyMain(psContext), "vec4 hlslcc_FragCoord = vec4(gl_FragCoord.xyz, 1.0/gl_FragCoord.w);\n");
                    break;
                }
                case NAME_RENDER_TARGET_ARRAY_INDEX:
                {
                    AddBuiltinInput(psDecl, "gl_Layer");
                    break;
                }
                default:
                    ASSERT(0);
                    break;
            }
            break;
        }
        case OPCODE_DCL_INPUT_SIV:
        {
            if (psShader->eShaderType == PIXEL_SHADER && psContext->psDependencies)
            {
                psContext->psDependencies->SetInterpolationMode(psDecl->asOperands[0].ui32RegisterNumber, psDecl->value.eInterpolation);
            }
            break;
        }
        case OPCODE_DCL_INPUT_PS:
        {
            const Operand* psOperand = &psDecl->asOperands[0];
            int iNumComponents = psOperand->GetNumInputElements(psContext);
            const char* StorageQualifier = "varying";
            const char* Precision = "";
            std::string inputName;
            const char* Interpolation = "";
            int hasNoPerspective = psContext->psShader->eTargetLanguage <= LANG_ES_310 ? 0 : 1;
            inputName = psContext->GetDeclaredInputName(psOperand, NULL, 1, NULL);

            if (InOutSupported(psContext->psShader->eTargetLanguage))
            {
                StorageQualifier = "in";
            }
            const ShaderInfo::InOutSignature* psSignature = NULL;

            psShader->sInfo.GetInputSignatureFromRegister(psDecl->asOperands[0].ui32RegisterNumber,
                psDecl->asOperands[0].ui32CompMask,
                &psSignature);

            if (psSignature->eComponentType == INOUT_COMPONENT_UINT32 ||
                psSignature->eComponentType == INOUT_COMPONENT_SINT32) // GLSL spec requires that integer inputs always have "flat" interpolation
            {
                Interpolation = GetInterpolationString(INTERPOLATION_CONSTANT, psContext->psShader->eTargetLanguage);
            }
            else
            {
                switch (psDecl->value.eInterpolation)
                {
                    case INTERPOLATION_CONSTANT:
                    {
                        Interpolation = "flat ";
                        break;
                    }
                    case INTERPOLATION_LINEAR:
                    {
                        break;
                    }
                    case INTERPOLATION_LINEAR_CENTROID:
                    {
                        Interpolation = "centroid ";
                        break;
                    }
                    case INTERPOLATION_LINEAR_NOPERSPECTIVE:
                    {
                        Interpolation = hasNoPerspective ? "noperspective " : "";
                        break;
                    }
                    case INTERPOLATION_LINEAR_NOPERSPECTIVE_CENTROID:
                    {
                        Interpolation = hasNoPerspective ? "noperspective centroid " : "centroid";
                        break;
                    }
                    case INTERPOLATION_LINEAR_SAMPLE:
                    {
                        Interpolation = hasNoPerspective ? "sample " : "";
                        break;
                    }
                    case INTERPOLATION_LINEAR_NOPERSPECTIVE_SAMPLE:
                    {
                        Interpolation = hasNoPerspective ? "noperspective sample " : "";
                        break;
                    }
                    default:
                        ASSERT(0);
                        break;
                }
            }

            if (HavePrecisionQualifiers(psContext))
            {
                switch (psOperand->eMinPrecision)
                {
                    case OPERAND_MIN_PRECISION_DEFAULT:
                    {
                        Precision = "highp";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_FLOAT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_FLOAT_2_8:
                    {
                        Precision = EmitLowp(psContext) ? "lowp " : "mediump ";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_SINT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                    case OPERAND_MIN_PRECISION_UINT_16:
                    {
                        Precision = "mediump";
                        break;
                    }
                }
            }

            bool haveFramebufferFetch = (psShader->extensions->EXT_shader_framebuffer_fetch &&
                psShader->eShaderType == PIXEL_SHADER &&
                psContext->flags & HLSLCC_FLAG_SHADER_FRAMEBUFFER_FETCH);

            // If this is a SV_Target input and framebuffer fetch is enabled, do special input declaration unless output is declared later
            if (haveFramebufferFetch && psOperand->iPSInOut && inputName.size() == 13 && !strncmp(inputName.c_str(), "vs_SV_Target", 12))
            {
                bstring type = NULL;

                switch (psSignature->eComponentType)
                {
                    case INOUT_COMPONENT_UINT32:
                    {
                        if (iNumComponents > 1)
                            type = bformat("uvec%d", iNumComponents);
                        else
                            type = bformat("uint");
                        break;
                    }
                    case INOUT_COMPONENT_SINT32:
                    {
                        if (iNumComponents > 1)
                            type = bformat("ivec%d", iNumComponents);
                        else
                            type = bformat("int");
                        break;
                    }
                    case INOUT_COMPONENT_FLOAT32:
                    {
                        if (iNumComponents > 1)
                            type = bformat("vec%d", iNumComponents);
                        else
                            type = bformat("float");
                        break;
                    }
                    default:
                        ASSERT(0);
                        break;
                }

                uint32_t renderTarget = psSignature->ui32SemanticIndex;

                char OutputName[512];
                bstring oname;
                oname = bformat("%s%s%d", psContext->outputPrefix, psSignature->semanticName.c_str(), renderTarget);
                strncpy(OutputName, (char *)oname->data, 512);
                bdestroy(oname);

                if (WriteToFragData(psContext->psShader->eTargetLanguage))
                {
                    bcatcstr(glsl, "#ifdef GL_EXT_shader_framebuffer_fetch\n");
                    bformata(glsl, "#define vs_%s gl_LastFragData[%d]\n", OutputName, renderTarget);
                    bcatcstr(glsl, "#else\n");
                    bformata(glsl, "#define vs_%s gl_FragData[%d]\n", OutputName, renderTarget);
                    bcatcstr(glsl, "#endif\n");
                }
                else
                {
                    if (!RenderTargetDeclared(renderTarget))
                    {
                        bstring layoutQualifier = bformat("");

                        if (HaveInOutLocationQualifier(psContext->psShader->eTargetLanguage) ||
                            HaveLimitedInOutLocationQualifier(psContext->psShader->eTargetLanguage, psContext->psShader->extensions))
                        {
                            uint32_t index = 0;

                            if ((psContext->flags & HLSLCC_FLAG_DUAL_SOURCE_BLENDING) && DualSourceBlendSupported(psContext->psShader->eTargetLanguage))
                            {
                                if (renderTarget > 0)
                                {
                                    renderTarget = 0;
                                    index = 1;
                                }
                                bdestroy(layoutQualifier);
                                layoutQualifier = bformat("layout(location = %d, index = %d) ", renderTarget, index);
                            }
                            else
                            {
                                bdestroy(layoutQualifier);
                                layoutQualifier = bformat("layout(location = %d) ", renderTarget);
                            }
                        }

                        auto lq = bstr2cstr(layoutQualifier, '\0');

                        bcatcstr(glsl, "#ifdef GL_EXT_shader_framebuffer_fetch\n");
                        bformata(glsl, "%sinout %s %s %s;\n", lq, Precision, type->data, OutputName);
                        bcatcstr(glsl, "#else\n");
                        bformata(glsl, "%sout %s %s %s;\n", lq, Precision, type->data, OutputName);
                        bcatcstr(glsl, "#endif\n");

                        bcstrfree(lq);
                        bdestroy(layoutQualifier);
                    }
                }
                break;
            }

            DeclareInput(psContext, psDecl,
                Interpolation, StorageQualifier, Precision, iNumComponents, INDEX_1D, inputName.c_str(), psOperand->ui32CompMask);

            HandleInputRedirect(psDecl, Precision);

            break;
        }
        case OPCODE_DCL_TEMPS:
        {
            uint32_t i = 0;
            const uint32_t ui32NumTemps = psDecl->value.ui32NumTemps;
            bool usePrecision = (HavePrecisionQualifiers(psContext) != 0);
            // Default values for temp variables allow avoiding Switch shader compiler incorrect warnings
            // related to potential use of uninitialized variables (false-positives from compiler).
            bool useDefaultInit = psContext->IsSwitch();

            for (i = 0; i < ui32NumTemps; i++)
            {
                if (useDefaultInit)
                {
                    if (psShader->psFloatTempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT, psShader->psFloatTempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT, psShader->psFloatTempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psFloat16TempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT16, psShader->psFloat16TempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT16, psShader->psFloat16TempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "16_%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psFloat10TempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT10, psShader->psFloat10TempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_FLOAT10, psShader->psFloat10TempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "10_%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psIntTempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_INT, psShader->psIntTempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_INT, psShader->psIntTempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psInt16TempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_INT16, psShader->psInt16TempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_INT16, psShader->psInt16TempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i16_%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psInt12TempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_INT12, psShader->psInt12TempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_INT12, psShader->psInt12TempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i12_%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psUIntTempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_UINT, psShader->psUIntTempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_UINT, psShader->psUIntTempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "u%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psUInt16TempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_UINT16, psShader->psUInt16TempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_UINT16, psShader->psUInt16TempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "u16_%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->fp64 && (psShader->psDoubleTempSizes[i] != 0))
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_DOUBLE, psShader->psDoubleTempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_DOUBLE, psShader->psDoubleTempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "d%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                    if (psShader->psBoolTempSizes[i] != 0)
                    {
                        const char* constructor = HLSLcc::GetConstructorForType(psContext, SVT_BOOL, psShader->psBoolTempSizes[i], usePrecision);
                        const char* constructorNoPrecision = HLSLcc::GetConstructorForType(psContext, SVT_BOOL, psShader->psBoolTempSizes[i], false);
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "b%d = %s(0);\n", constructor, i, constructorNoPrecision);
                    }
                }
                else
                {
                    if (psShader->psFloatTempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT, psShader->psFloatTempSizes[i], usePrecision), i);
                    if (psShader->psFloat16TempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT16, psShader->psFloat16TempSizes[i], usePrecision), i);
                    if (psShader->psFloat10TempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "10_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_FLOAT10, psShader->psFloat10TempSizes[i], usePrecision), i);
                    if (psShader->psIntTempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT, psShader->psIntTempSizes[i], usePrecision), i);
                    if (psShader->psInt16TempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT16, psShader->psInt16TempSizes[i], usePrecision), i);
                    if (psShader->psInt12TempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "i12_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_INT12, psShader->psInt12TempSizes[i], usePrecision), i);
                    if (psShader->psUIntTempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "u%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_UINT, psShader->psUIntTempSizes[i], usePrecision), i);
                    if (psShader->psUInt16TempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "u16_%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_UINT16, psShader->psUInt16TempSizes[i], usePrecision), i);
                    if (psShader->fp64 && (psShader->psDoubleTempSizes[i] != 0))
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "d%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_DOUBLE, psShader->psDoubleTempSizes[i], usePrecision), i);
                    if (psShader->psBoolTempSizes[i] != 0)
                        bformata(glsl, "%s " HLSLCC_TEMP_PREFIX "b%d;\n", HLSLcc::GetConstructorForType(psContext, SVT_BOOL, psShader->psBoolTempSizes[i], usePrecision), i);
                }
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
            const Operand* psOperand = &psDecl->asOperands[0];
            const uint32_t ui32BindingPoint = psOperand->aui32ArraySizes[0];

            const ConstantBuffer* psCBuf = NULL;
            psContext->psShader->sInfo.GetConstantBufferFromBindingPoint(RGROUP_CBUFFER, ui32BindingPoint, &psCBuf);

            // We don't have a original resource name, maybe generate one???
            if (!psCBuf)
            {
                char name[24];
                sprintf(name, "ConstantBuffer%d", ui32BindingPoint);

                GLSLCrossDependencyData::GLSLBufferBindPointInfo bindPointInfo = psContext->IsVulkan() ?
                    GLSLCrossDependencyData::GLSLBufferBindPointInfo{ ui32BindingPoint, true } : psContext->psDependencies->GetGLSLResourceBinding(name, GLSLCrossDependencyData::BufferType_Constant);

                bool isKnown = bindPointInfo.known;
                uint32_t actualBindingPoint = bindPointInfo.slot;

                if (HaveUniformBindingsAndLocations(psContext->psShader->eTargetLanguage, psContext->psShader->extensions, psContext->flags) || (psContext->flags & HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS))
                {
                    if (!psContext->IsVulkan() && !isKnown && UseReflection(psContext))
                        psContext->m_Reflection.OnConstantBufferBinding(name, actualBindingPoint);
                    bformata(glsl, "UNITY_LOCATION(%d) ", actualBindingPoint);
                }

                bformata(glsl, "layout(std140) uniform %s {\n\tvec4 data[%d];\n} cb%d;\n", name, psOperand->aui32ArraySizes[1], ui32BindingPoint);
                break;
            }

            if (psCBuf->name.substr(0, 20) == "hlslcc_SubpassInput_" && psCBuf->name.length() >= 23 && !psCBuf->asVars.empty())
            {
                // Special case for vulkan subpass input.

                // The multisample versions have multiple members in the cbuffer, but we must only declare once.
                // We still need to loop through all the variables and adjust names

                // Pick up the type and index
                char ty = psCBuf->name[20];
                int idx = psCBuf->name[22] - '0';
                bool isMS = false;
                GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding((std::string &)psCBuf->name, false, 2);

                bool declared = false;
                for (std::vector<ShaderVar>::const_iterator itr = psCBuf->asVars.begin(); itr != psCBuf->asVars.end(); itr++)
                {
                    ShaderVar &sv = (ShaderVar &)*itr;
                    if (sv.name.substr(0, 15) == "hlslcc_fbinput_")
                    {
                        if (!declared)
                        {
                            switch (ty)
                            {
                                case 'f':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform highp subpassInput %s;\n", idx, binding.set, binding.binding, sv.name.c_str());
                                    break;
                                case 'h':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform mediump subpassInput %s;\n", idx, binding.set, binding.binding, sv.name.c_str());
                                    break;
                                case 'i':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform isubpassInput %s;\n", idx, binding.set, binding.binding, sv.name.c_str());
                                    break;
                                case 'u':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform usubpassInput %s;\n", idx, binding.set, binding.binding, sv.name.c_str());
                                    break;
                                case 'F':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform highp subpassInputMS %s;\n", idx, binding.set, binding.binding, sv.name.substr(0, 16).c_str());
                                    isMS = true;
                                    break;
                                case 'H':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform mediump subpassInputMS %s;\n", idx, binding.set, binding.binding, sv.name.substr(0, 16).c_str());
                                    isMS = true;
                                    break;
                                case 'I':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform isubpassInputMS %s;\n", idx, binding.set, binding.binding, sv.name.substr(0, 16).c_str());
                                    isMS = true;
                                    break;
                                case 'U':
                                    bformata(glsl, "layout(input_attachment_index = %d, set = %d, binding = %d) uniform usubpassInputMS %s;\n", idx, binding.set, binding.binding, sv.name.substr(0, 16).c_str());
                                    isMS = true;
                                    break;
                                default:
                                    break;
                            }
                            declared = true;
                        }
                        else
                        {
                            if (ty == 'F' || ty == 'I' || ty == 'U')
                                isMS = true;
                        }
                        // Munge the name so it'll get the correct function call in GLSL directly
                        sv.name.insert(0, "subpassLoad(");
                        if (isMS)
                            sv.name.append(",");
                        else
                            sv.name.append(")");
                        // Also update the type name
                        sv.sType.name = sv.name;
                        sv.sType.fullName = sv.name;
                    }
                }

                // Break out so this doesn't get declared.
                break;
            }

            if (psCBuf->name == "OVR_multiview")
            {
                // Special case for piggy-backing multiview info out
                // This is not really a cbuffer, but if we see this being accessed, we know we need viewID

                // Extract numViews
                uint32_t numViews = 0;
                for (std::vector<ShaderVar>::const_iterator itr = psCBuf->asVars.begin(); itr != psCBuf->asVars.end(); itr++)
                {
                    if (strncmp(itr->name.c_str(), "numViews_", 9) == 0)
                    {
                        // I really don't think we'll ever have more than 9 multiviews
                        numViews = itr->name[9] - '0';
                        break;
                    }
                }
                if (numViews > 0 && numViews < 10)
                {
                    // multiview2 is required because we have built-in shaders that do eye-dependent work other than just position
                    psContext->RequireExtension("GL_OVR_multiview2");

                    if (psShader->eShaderType == VERTEX_SHADER)
                        bformata(glsl, "layout(num_views = %d) in;\n", numViews);

                    break; // Break out so we don't actually declare this cbuffer
                }
            }

            if (IsPreTransformConstantBufferName(psCBuf->name.c_str()))
            {
                m_NeedUnityPreTransformDecl = true;
                break; // Break out so we don't actually declare this cbuffer
            }

            if (psContext->flags & HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT)
            {
                if (psContext->flags & HLSLCC_FLAG_GLOBAL_CONSTS_NEVER_IN_UBO && psCBuf->name[0] == '$')
                {
                    DeclareStructConstants(ui32BindingPoint, psCBuf, psOperand, glsl);
                }
                else
                {
                    DeclareUBOConstants(ui32BindingPoint, psCBuf, glsl);
                }
            }
            else
            {
                DeclareStructConstants(ui32BindingPoint, psCBuf, psOperand, glsl);
            }
            break;
        }
        case OPCODE_DCL_RESOURCE:
        {
            psShader->aeResourceDims[psDecl->asOperands[0].ui32RegisterNumber] = psDecl->value.eResourceDimension;

            // Vulkan doesn't use combined textures+samplers, so do own handling in a separate func
            if (psContext->IsVulkan())
            {
                TranslateVulkanResource(psContext, psDecl);
                break;
            }

            if (HaveUniformBindingsAndLocations(psContext->psShader->eTargetLanguage, psContext->psShader->extensions, psContext->flags) ||
                ((psContext->flags & HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS) && ((psContext->flags & HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS) != HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS)))
            {
                std::string tname = ResourceName(psContext, RGROUP_TEXTURE, psDecl->asOperands[0].ui32RegisterNumber, 0);
                GLSLCrossDependencyData::GLSLBufferBindPointInfo slotInfo = psContext->psDependencies->GetGLSLResourceBinding(tname, GLSLCrossDependencyData::BufferType_Texture);

                bformata(glsl, "UNITY_LOCATION(%d) ", slotInfo.slot);
                if (!slotInfo.known && UseReflection(psContext))
                {
                    const RESOURCE_DIMENSION dim = psDecl->value.eResourceDimension;
                    if (dim == RESOURCE_DIMENSION_BUFFER)
                        psContext->m_Reflection.OnBufferBinding(tname, slotInfo.slot, false);
                    else
                    {
                        bool isMSAATex = (dim == RESOURCE_DIMENSION_TEXTURE2DMS) || (dim == RESOURCE_DIMENSION_TEXTURE2DMSARRAY);
                        psContext->m_Reflection.OnTextureBinding(tname, slotInfo.slot, slotInfo.slot, isMSAATex, GetTextureDimension(psContext, dim, psDecl->asOperands[0].ui32RegisterNumber), false);
                    }
                }
            }

            switch (psDecl->value.eResourceDimension)
            {
                case RESOURCE_DIMENSION_BUFFER:
                {
                    bcatcstr(glsl, "uniform ");
                    if (IsESLanguage(psContext->psShader->eTargetLanguage))
                        bcatcstr(glsl, "highp ");
                    bformata(glsl, "%s ", GetSamplerType(psContext,
                        RESOURCE_DIMENSION_BUFFER,
                        psDecl->asOperands[0].ui32RegisterNumber));
                    TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NONE);
                    bcatcstr(glsl, ";\n");
                    break;
                }

                case RESOURCE_DIMENSION_TEXTURE1D:
                case RESOURCE_DIMENSION_TEXTURE2D:
                case RESOURCE_DIMENSION_TEXTURECUBE:
                case RESOURCE_DIMENSION_TEXTURE1DARRAY:
                case RESOURCE_DIMENSION_TEXTURE2DARRAY:
                case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                {
                    TranslateResourceTexture(psContext, psDecl, 1);
                    break;
                }

                case RESOURCE_DIMENSION_TEXTURE2DMS:
                case RESOURCE_DIMENSION_TEXTURE3D:
                case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                {
                    TranslateResourceTexture(psContext, psDecl, 0);
                    break;
                }

                default:
                    ASSERT(0);
                    break;
            }
            break;
        }
        case OPCODE_DCL_OUTPUT:
        {
            bool needsDeclare = true;
            if (psShader->eShaderType == HULL_SHADER && psShader->asPhases[psContext->currentPhase].ePhase == HS_CTRL_POINT_PHASE && psDecl->asOperands[0].ui32RegisterNumber == 0)
            {
                // Need extra check from signature:
                const ShaderInfo::InOutSignature *sig = NULL;
                psShader->sInfo.GetOutputSignatureFromRegister(0, psDecl->asOperands->GetAccessMask(), 0, &sig, true);
                if (!sig || sig->semanticName == "POSITION" || sig->semanticName == "POS" || sig->semanticName == "SV_Position")
                {
                    needsDeclare = false;
                    AddBuiltinOutput(psDecl, 0, "gl_out[gl_InvocationID].gl_Position");
                }
            }

            if (needsDeclare)
            {
                AddUserOutput(psDecl);
            }
            break;
        }
        case OPCODE_DCL_GLOBAL_FLAGS:
        {
            uint32_t ui32Flags = psDecl->value.ui32GlobalFlags;

            if (ui32Flags & GLOBAL_FLAG_FORCE_EARLY_DEPTH_STENCIL && psContext->psShader->eShaderType == PIXEL_SHADER)
            {
                bcatcstr(glsl, "layout(early_fragment_tests) in;\n");
                psShader->sInfo.bEarlyFragmentTests = true;
            }
            if ((ui32Flags & GLOBAL_FLAG_REFACTORING_ALLOWED) && HavePreciseQualifier(psContext->psShader->eTargetLanguage))
            {
                static const char * const types[] =
                {
                    "vec4", "ivec4", "bvec4", "uvec4"
                };

                for (int i = 0; i < sizeof(types) / sizeof(types[0]); ++i)
                {
                    char const * t = types[i];
                    bformata(glsl, "precise %s u_xlat_precise_%s;\n", t, t);
                }
            }
            if (ui32Flags & GLOBAL_FLAG_ENABLE_DOUBLE_PRECISION_FLOAT_OPS)
            {
                psContext->EnableExtension("GL_ARB_gpu_shader_fp64");
                psShader->fp64 = 1;
            }
            break;
        }

        case OPCODE_DCL_THREAD_GROUP:
        {
            bformata(glsl, "layout(local_size_x = %d, local_size_y = %d, local_size_z = %d) in;\n",
                psDecl->value.aui32WorkGroupSize[0],
                psDecl->value.aui32WorkGroupSize[1],
                psDecl->value.aui32WorkGroupSize[2]);
            break;
        }
        case OPCODE_DCL_TESS_OUTPUT_PRIMITIVE:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                psContext->psShader->sInfo.eTessOutPrim = psDecl->value.eTessOutPrim;
                // Invert triangle winding order to match glsl better, except on vulkan
                if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) == 0)
                {
                    if (psContext->psShader->sInfo.eTessOutPrim == TESSELLATOR_OUTPUT_TRIANGLE_CW)
                        psContext->psShader->sInfo.eTessOutPrim = TESSELLATOR_OUTPUT_TRIANGLE_CCW;
                    else if (psContext->psShader->sInfo.eTessOutPrim == TESSELLATOR_OUTPUT_TRIANGLE_CCW)
                        psContext->psShader->sInfo.eTessOutPrim = TESSELLATOR_OUTPUT_TRIANGLE_CW;
                }
            }
            break;
        }
        case OPCODE_DCL_TESS_DOMAIN:
        {
            if (psContext->psShader->eShaderType == DOMAIN_SHADER)
            {
                switch (psDecl->value.eTessDomain)
                {
                    case TESSELLATOR_DOMAIN_ISOLINE:
                    {
                        bcatcstr(glsl, "layout(isolines) in;\n");
                        break;
                    }
                    case TESSELLATOR_DOMAIN_TRI:
                    {
                        bcatcstr(glsl, "layout(triangles) in;\n");
                        break;
                    }
                    case TESSELLATOR_DOMAIN_QUAD:
                    {
                        bcatcstr(glsl, "layout(quads) in;\n");
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            break;
        }
        case OPCODE_DCL_TESS_PARTITIONING:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                psContext->psShader->sInfo.eTessPartitioning = psDecl->value.eTessPartitioning;
            }
            break;
        }
        case OPCODE_DCL_GS_OUTPUT_PRIMITIVE_TOPOLOGY:
        {
            switch (psDecl->value.eOutputPrimitiveTopology)
            {
                case PRIMITIVE_TOPOLOGY_POINTLIST:
                {
                    bcatcstr(glsl, "layout(points) out;\n");
                    break;
                }
                case PRIMITIVE_TOPOLOGY_LINELIST_ADJ:
                case PRIMITIVE_TOPOLOGY_LINESTRIP_ADJ:
                case PRIMITIVE_TOPOLOGY_LINELIST:
                case PRIMITIVE_TOPOLOGY_LINESTRIP:
                {
                    bcatcstr(glsl, "layout(line_strip) out;\n");
                    break;
                }

                case PRIMITIVE_TOPOLOGY_TRIANGLELIST_ADJ:
                case PRIMITIVE_TOPOLOGY_TRIANGLESTRIP_ADJ:
                case PRIMITIVE_TOPOLOGY_TRIANGLESTRIP:
                case PRIMITIVE_TOPOLOGY_TRIANGLELIST:
                {
                    bcatcstr(glsl, "layout(triangle_strip) out;\n");
                    break;
                }
                default:
                {
                    break;
                }
            }
            break;
        }
        case OPCODE_DCL_MAX_OUTPUT_VERTEX_COUNT:
        {
            bformata(glsl, "layout(max_vertices = %d) out;\n", psDecl->value.ui32MaxOutputVertexCount);
            break;
        }
        case OPCODE_DCL_GS_INPUT_PRIMITIVE:
        {
            switch (psDecl->value.eInputPrimitive)
            {
                case PRIMITIVE_POINT:
                {
                    bcatcstr(glsl, "layout(points) in;\n");
                    break;
                }
                case PRIMITIVE_LINE:
                {
                    bcatcstr(glsl, "layout(lines) in;\n");
                    break;
                }
                case PRIMITIVE_LINE_ADJ:
                {
                    bcatcstr(glsl, "layout(lines_adjacency) in;\n");
                    break;
                }
                case PRIMITIVE_TRIANGLE:
                {
                    bcatcstr(glsl, "layout(triangles) in;\n");
                    break;
                }
                case PRIMITIVE_TRIANGLE_ADJ:
                {
                    bcatcstr(glsl, "layout(triangles_adjacency) in;\n");
                    break;
                }
                default:
                {
                    break;
                }
            }
            break;
        }
        case OPCODE_DCL_INTERFACE:
        {
            const uint32_t interfaceID = psDecl->value.iface.ui32InterfaceID;
            const uint32_t numUniforms = psDecl->value.iface.ui32ArraySize;
            const uint32_t ui32NumBodiesPerTable = psContext->psShader->funcPointer[interfaceID].ui32NumBodiesPerTable;
            ShaderVar* psVar;
            uint32_t varFound;

            const char* uniformName;

            varFound = psContext->psShader->sInfo.GetInterfaceVarFromOffset(interfaceID, &psVar);
            ASSERT(varFound);
            uniformName = &psVar->name[0];

            bformata(glsl, "subroutine uniform SubroutineType %s[%d*%d];\n", uniformName, numUniforms, ui32NumBodiesPerTable);
            break;
        }
        case OPCODE_DCL_FUNCTION_BODY:
        {
            //bformata(glsl, "void Func%d();//%d\n", psDecl->asOperands[0].ui32RegisterNumber, psDecl->asOperands[0].eType);
            break;
        }
        case OPCODE_DCL_FUNCTION_TABLE:
        {
            break;
        }
        case OPCODE_CUSTOMDATA:
        {
            // On Vulkan we just spew the data in uints as-is
            if (psContext->IsVulkan())
            {
                bstring glsl = *psContext->currentGLSLString;
                bformata(glsl, "const uvec4 ImmCB_%d[] = uvec4[%d] (\n", psContext->currentPhase, psDecl->asImmediateConstBuffer.size());
                bool isFirst = true;
                std::for_each(psDecl->asImmediateConstBuffer.begin(), psDecl->asImmediateConstBuffer.end(), [&](const ICBVec4 &data)
                {
                    if (!isFirst)
                    {
                        bcatcstr(glsl, ",\n");
                    }
                    isFirst = false;
                    bformata(glsl, "\tuvec4(0x%X, 0x%X, 0x%X, 0x%X)", data.a, data.b, data.c, data.d);
                });
                bcatcstr(glsl, ");\n");
            }
            else if (psContext->IsSwitch())
            {
                bstring glsl = *psContext->currentGLSLString;
                bformata(glsl, "const vec4 ImmCB_%d[] = vec4[%d] (\n", psContext->currentPhase, psDecl->asImmediateConstBuffer.size());
                bool isFirst = true;
                std::for_each(psDecl->asImmediateConstBuffer.begin(), psDecl->asImmediateConstBuffer.end(), [&](const ICBVec4 &data)
                {
                    if (!isFirst)
                    {
                        bcatcstr(glsl, ",\n");
                    }
                    isFirst = false;
                    bformata(glsl, "vec4(uintBitsToFloat(uint(0x%Xu)), uintBitsToFloat(uint(0x%Xu)), uintBitsToFloat(uint(0x%Xu)), uintBitsToFloat(uint(0x%Xu)))", data.a, data.b, data.c, data.d);
                });
                bcatcstr(glsl, ");\n");
            }
            else
            {
                // TODO: This is only ever accessed as a float currently. Do trickery if we ever see ints accessed from an array.
                // Walk through all the chunks we've seen in this phase.
                ShaderPhase &sp = psShader->asPhases[psContext->currentPhase];
                std::for_each(sp.m_ConstantArrayInfo.m_Chunks.begin(), sp.m_ConstantArrayInfo.m_Chunks.end(), [this](const std::pair<uint32_t, ConstantArrayChunk> &chunk)
                {
                    bstring glsl = *psContext->currentGLSLString;
                    uint32_t componentCount = chunk.second.m_ComponentCount;
                    // Just do the declaration here and contents to earlyMain.
                    if (componentCount == 1)
                        bformata(glsl, "float ImmCB_%d_%d_%d[%d];\n", psContext->currentPhase, chunk.first, chunk.second.m_Rebase, chunk.second.m_Size);
                    else
                        bformata(glsl, "vec%d ImmCB_%d_%d_%d[%d];\n", componentCount, psContext->currentPhase, chunk.first, chunk.second.m_Rebase, chunk.second.m_Size);

                    if (!HaveDynamicIndexing(psContext))
                    {
                        bstring name = bfromcstr("");
                        bformata(name, "ImmCB_%d_%d_%d", psContext->currentPhase, chunk.first, chunk.second.m_Rebase);
                        SHADER_VARIABLE_CLASS eClass = componentCount > 1 ? SVC_VECTOR : SVC_SCALAR;

                        DeclareDynamicIndexWrapper((const char *)name->data, eClass, SVT_FLOAT, 1, componentCount, chunk.second.m_Size);
                        bdestroy(name);
                    }

                    bstring tgt = psContext->psShader->asPhases[psContext->currentPhase].earlyMain;
                    Declaration *psDecl = psContext->psShader->asPhases[psContext->currentPhase].m_ConstantArrayInfo.m_OrigDeclaration;
                    if (componentCount == 1)
                    {
                        for (uint32_t i = 0; i < chunk.second.m_Size; i++)
                        {
                            float val[4] = {
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].a,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].b,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].c,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].d
                            };
                            bformata(tgt, "\tImmCB_%d_%d_%d[%d] = ", psContext->currentPhase, chunk.first, chunk.second.m_Rebase, i);
                            if (fpcheck(val[chunk.second.m_Rebase]) && HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
                                bformata(tgt, "uintBitsToFloat(uint(0x%Xu))", *(uint32_t *)&val[chunk.second.m_Rebase]);
                            else
                                HLSLcc::PrintFloat(tgt, val[chunk.second.m_Rebase]);
                            bcatcstr(tgt, ";\n");
                        }
                    }
                    else
                    {
                        for (uint32_t i = 0; i < chunk.second.m_Size; i++)
                        {
                            float val[4] = {
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].a,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].b,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].c,
                                *(float*)&psDecl->asImmediateConstBuffer[i + chunk.first].d
                            };
                            bformata(tgt, "\tImmCB_%d_%d_%d[%d] = vec%d(", psContext->currentPhase, chunk.first, chunk.second.m_Rebase, i, componentCount);
                            for (uint32_t k = 0; k < componentCount; k++)
                            {
                                if (k != 0)
                                    bcatcstr(tgt, ", ");
                                if (fpcheck(val[k]) && HaveBitEncodingOps(psContext->psShader->eTargetLanguage))
                                    bformata(tgt, "uintBitsToFloat(uint(0x%Xu))", *(uint32_t *)&val[k + chunk.second.m_Rebase]);
                                else
                                    HLSLcc::PrintFloat(tgt, val[k + chunk.second.m_Rebase]);
                            }
                            bcatcstr(tgt, ");\n");
                        }
                    }
                });
            }


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
            bformata(glsl, "vec%d TempArray%d[%d];\n", ui32RegComponentSize, ui32RegIndex, ui32RegCount);
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
                    const char* type = "vec";
                    const char* Precision = "";
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
                            type = "uvec";
                            break;
                        }
                        case INOUT_COMPONENT_SINT32:
                        {
                            type = "ivec";
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

                    if (HavePrecisionQualifiers(psContext))
                    {
                        switch (psSignature->eMinPrec) // TODO What if the inputs in the indexed range are of different precisions?
                        {
                            default:
                            {
                                Precision = "highp ";
                                break;
                            }
                            case MIN_PRECISION_ANY_16:
                            case MIN_PRECISION_FLOAT_16:
                            case MIN_PRECISION_SINT_16:
                            case MIN_PRECISION_UINT_16:
                            {
                                Precision = "mediump ";
                                break;
                            }
                            case MIN_PRECISION_FLOAT_2_8:
                            {
                                Precision = EmitLowp(psContext) ? "lowp " : "mediump ";
                                break;
                            }
                        }
                    }

                    startReg = psDecl->asOperands[0].ui32RegisterNumber;
                    bformata(glsl, "%s%s4 phase%d_%sput%d_%d[%d];\n", Precision, type, psContext->currentPhase, isInput ? "In" : "Out", regSpace, startReg, psDecl->value.ui32IndexRange);
                    oldString = psContext->currentGLSLString;
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
                            bcatcstr(glsl, " = ");
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
                        }
                        else
                        {
                            realName = psContext->GetDeclaredOutputName(&psDecl->asOperands[0], &dummy, NULL, NULL, 1);

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
            break;
        }
        case OPCODE_DCL_INPUT_CONTROL_POINT_COUNT:
        {
            break;
        }
        case OPCODE_DCL_OUTPUT_CONTROL_POINT_COUNT:
        {
            if (psContext->psShader->eShaderType == HULL_SHADER)
            {
                bformata(glsl, "layout(vertices=%d) out;\n", psDecl->value.ui32MaxOutputVertexCount);
            }
            break;
        }
        case OPCODE_HS_FORK_PHASE:
        {
            break;
        }
        case OPCODE_HS_JOIN_PHASE:
        {
            break;
        }
        case OPCODE_DCL_SAMPLER:
        {
            if (psContext->IsVulkan())
            {
                ResourceBinding *pRes = NULL;
                psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_SAMPLER, psDecl->asOperands[0].ui32RegisterNumber, (const ResourceBinding **)&pRes);
                ASSERT(pRes != NULL);
                std::string name = ResourceName(psContext, RGROUP_SAMPLER, psDecl->asOperands[0].ui32RegisterNumber, 0);
                const char *samplerPrecision = GetSamplerPrecision(psContext, pRes->ePrecision);

                GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(name);
                const char *samplerType = psDecl->value.eSamplerMode == D3D10_SB_SAMPLER_MODE_COMPARISON ? "samplerShadow" : "sampler";
                bformata(glsl, "layout(set = %d, binding = %d) uniform %s %s %s;\n", binding.set, binding.binding, samplerPrecision, samplerType, name.c_str());
                // Store the sampler mode to ShaderInfo, it's needed when we use the sampler
                pRes->m_SamplerMode = psDecl->value.eSamplerMode;
            }
            break;
        }
        case OPCODE_DCL_HS_MAX_TESSFACTOR:
        {
            //For GLSL the max tessellation factor is fixed to the value of gl_MaxTessGenLevel.
            break;
        }
        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED:
        {
            // non-float images need either 'i' or 'u' prefix.
            char imageTypePrefix[2] = { 0, 0 };
            uint32_t bindpoint = psDecl->asOperands[0].ui32RegisterNumber;
            const bool isVulkan = (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;

            if (psDecl->sUAV.ui32GloballyCoherentAccess & GLOBALLY_COHERENT_ACCESS)
            {
                bcatcstr(glsl, "coherent ");
            }

            // Use 4 component format as a fallback if no instruction defines it
            const uint32_t numComponents = psDecl->sUAV.ui32NumComponents > 0 ? psDecl->sUAV.ui32NumComponents : 4;
            REFLECT_RESOURCE_PRECISION precision = REFLECT_RESOURCE_PRECISION_UNKNOWN;

            if (!(psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_READ) &&
                !(psContext->flags & HLSLCC_FLAG_GLES31_IMAGE_QUALIFIERS) && !isVulkan)
            { //Special case on desktop glsl: writeonly image does not need format qualifier
                bformata(glsl, "writeonly layout(binding=%d) ", bindpoint);
            }
            else
            {
                if (!(psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_READ))
                    bcatcstr(glsl, "writeonly ");
                else if (!(psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_WRITE))
                    bcatcstr(glsl, "readonly ");

                if ((psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_WRITE) && IsESLanguage(psShader->eTargetLanguage))
                {
                    // Need to require the extension
                    psContext->RequireExtension("GL_EXT_texture_buffer");
                }

                if (psContext->IsSwitch() && !(psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_ATOMIC))
                {
                    // Switch supports the GL_EXT_shader_image_load_formatted extension but it does require being enabled.
                    // Allows imageLoad() to do formatted reads and match the ld_uav_typed_indexable instruction.
                    // GL_EXT_shader_image_load_formatted doesn't provide support for imageAtomic*() functions. These still require format layout qualifier
                    psContext->RequireExtension("GL_EXT_shader_image_load_formatted");
                    bformata(glsl, "layout(binding=%d) ", bindpoint);
                    switch (psDecl->sUAV.Type)
                    {
                        case RETURN_TYPE_FLOAT:
                        case RETURN_TYPE_UINT:
                        case RETURN_TYPE_SINT:
                            bcatcstr(glsl, "highp "); //TODO: half case?
                            break;
                        case RETURN_TYPE_UNORM:
                        case RETURN_TYPE_SNORM:
                            bcatcstr(glsl, "lowp ");
                            break;
                        default:
                            ASSERT(0);
                    }
                }
                else
                {
                    if (isVulkan)
                    {
                        std::string name = ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0);
                        GLSLCrossDependencyData::VulkanResourceBinding binding = psContext->psDependencies->GetVulkanResourceBinding(name);
                        bformata(glsl, "layout(set = %d, binding = %d, ", binding.set, binding.binding);
                    }
                    else
                        bformata(glsl, "layout(binding=%d, ", bindpoint);

                    const ResourceBinding* psBinding = 0;
                    if (psContext->psShader->sInfo.GetResourceFromBindingPoint(RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, &psBinding))
                        precision = psBinding->ePrecision;

                    if (psDecl->sUAV.Type == RETURN_TYPE_FLOAT && numComponents == 3 && precision == REFLECT_RESOURCE_PRECISION_LOWP)
                    {
                        if (IsESLanguage(psContext->psShader->eTargetLanguage))
                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                        bcatcstr(glsl, "r11f_g11f_b10f) mediump ");
                    }
                    else if (psDecl->sUAV.Type == RETURN_TYPE_UNORM && numComponents == 4 && precision == REFLECT_RESOURCE_PRECISION_LOWP)
                    {
                        if (IsESLanguage(psContext->psShader->eTargetLanguage))
                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                        bcatcstr(glsl, "rgb10_a2) mediump ");
                    }
                    else if (psDecl->sUAV.Type == RETURN_TYPE_UINT && numComponents == 4 && precision == REFLECT_RESOURCE_PRECISION_LOWP)
                    {
                        if (IsESLanguage(psContext->psShader->eTargetLanguage))
                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                        bcatcstr(glsl, "rgb10_a2ui) mediump ");
                    }
                    else
                    {
                        if (numComponents >= 1)
                            bcatcstr(glsl, "r");
                        if (numComponents >= 2)
                            bcatcstr(glsl, "g");
                        if (numComponents >= 3)
                            bcatcstr(glsl, "ba");

                        switch (psDecl->sUAV.Type)
                        {
                            case RETURN_TYPE_FLOAT:
                            {
                                switch (precision)
                                {
                                    case REFLECT_RESOURCE_PRECISION_LOWP:
                                    case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                                        if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4)
                                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                        bcatcstr(glsl, "16f) mediump "); break;
                                    default:
                                        if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4 && numComponents != 1)
                                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                        bcatcstr(glsl, "32f) highp "); break;
                                }
                            } break;
                            case RETURN_TYPE_UNORM:
                            case RETURN_TYPE_SNORM:
                            {
                                if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4)
                                    GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                bformata(glsl, "8%s) lowp ", psDecl->sUAV.Type == RETURN_TYPE_SNORM ? "_snorm" : "");
                            } break;
                            case RETURN_TYPE_UINT:
                            case RETURN_TYPE_SINT:
                            {
                                const char* fmt = psDecl->sUAV.Type == RETURN_TYPE_UINT ? "ui" : "i";
                                switch (precision)
                                {
                                    case REFLECT_RESOURCE_PRECISION_LOWP:
                                        if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4)
                                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                        bformata(glsl, "8%s) lowp ", fmt); break;
                                    case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                                        if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4)
                                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                        bformata(glsl, "16%s) mediump ", fmt); break;
                                    default:
                                        if (IsESLanguage(psContext->psShader->eTargetLanguage) && numComponents != 4 && numComponents != 1)
                                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                                        bformata(glsl, "32%s) highp ", fmt); break;
                                }
                            } break;
                            default:
                                ASSERT(0);
                        }
                    }
                }
            }

            if (psDecl->sUAV.Type == RETURN_TYPE_UINT)
                imageTypePrefix[0] = 'u';
            else if (psDecl->sUAV.Type == RETURN_TYPE_SINT)
                imageTypePrefix[0] = 'i';

            // GLSL requires images to be always explicitly defined as uniforms
            switch (psDecl->value.eResourceDimension)
            {
                case RESOURCE_DIMENSION_BUFFER:
                {
                    if (IsESLanguage(psShader->eTargetLanguage) || psContext->IsVulkan())
                    {
                        psContext->RequireExtension("GL_EXT_texture_buffer");
                        if (numComponents != 1 || precision == REFLECT_RESOURCE_PRECISION_LOWP || precision == REFLECT_RESOURCE_PRECISION_MEDIUMP)
                            GenerateUnsupportedFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
                    }

                    bformata(glsl, "uniform %simageBuffer ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE1D:
                {
                    bformata(glsl, "uniform %simage1D ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE2D:
                {
                    bformata(glsl, "uniform %simage2D ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE2DMS:
                {
                    bformata(glsl, "uniform %simage2DMS ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE3D:
                {
                    bformata(glsl, "uniform %simage3D ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURECUBE:
                {
                    bformata(glsl, "uniform %simageCube ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE1DARRAY:
                {
                    bformata(glsl, "uniform %simage1DArray ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE2DARRAY:
                {
                    bformata(glsl, "uniform %simage2DArray ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURE2DMSARRAY:
                {
                    bformata(glsl, "uniform %simage3DArray ", imageTypePrefix);
                    break;
                }
                case RESOURCE_DIMENSION_TEXTURECUBEARRAY:
                {
                    bformata(glsl, "uniform %simageCubeArray ", imageTypePrefix);
                    break;
                }
                default:
                    ASSERT(0);
                    break;
            }
            TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NONE);
            bcatcstr(glsl, ";\n");

            unsigned int accessFlags = 0;
            if (psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_READ)
                accessFlags |= HLSLccReflection::ReadAccess;
            if (psDecl->sUAV.ui32AccessFlags & ACCESS_FLAG_WRITE)
                accessFlags |= HLSLccReflection::WriteAccess;

            if (IsESLanguage(psContext->psShader->eTargetLanguage) && accessFlags == (HLSLccReflection::ReadAccess | HLSLccReflection::WriteAccess))
            {
                if (numComponents != 1 || precision == REFLECT_RESOURCE_PRECISION_LOWP || precision == REFLECT_RESOURCE_PRECISION_MEDIUMP)
                    GenerateUnsupportedReadWriteFormatWarning(psContext->m_Reflection, ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0).c_str());
            }

            psContext->m_Reflection.OnStorageImage(bindpoint, accessFlags);

            break;
        }
        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED:
        {
            const bool isVulkan = (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;
            const bool avoidAtomicCounter = (psContext->flags & HLSLCC_FLAG_AVOID_SHADER_ATOMIC_COUNTERS) != 0;
            if (psDecl->sUAV.bCounter)
            {
                if (isVulkan)
                {
                    std::string uavname = ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0);
                    GLSLCrossDependencyData::VulkanResourceBinding uavBinding = psContext->psDependencies->GetVulkanResourceBinding(uavname, true);
                    GLSLCrossDependencyData::VulkanResourceBinding counterBinding = { uavBinding.set, uavBinding.binding + 1 };
                    bformata(glsl, "layout(set = %d, binding = %d) buffer %s_counterBuf { highp uint %s_counter; };\n", counterBinding.set, counterBinding.binding, uavname.c_str(), uavname.c_str());

                    DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                        psDecl->sUAV.ui32GloballyCoherentAccess, 0, 1, 0, psDecl->ui32BufferStride, glsl);
                }
                else if (avoidAtomicCounter) // no support for atomic counter. We must use atomic functions in SSBO instead.
                {
                    DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                        psDecl->sUAV.ui32GloballyCoherentAccess, 0, 1, 1, psDecl->ui32BufferStride, glsl);
                }
                else
                {
                    std::string name = ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0);
                    name += "_counter";
                    bcatcstr(glsl, "layout (binding = 0) uniform ");

                    if (HavePrecisionQualifiers(psContext))
                        bcatcstr(glsl, "highp ");
                    bformata(glsl, "atomic_uint %s;\n", name.c_str());

                    DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                        psDecl->sUAV.ui32GloballyCoherentAccess, 0, 1, 0, psDecl->ui32BufferStride, glsl);
                }
            }
            else
            {
                DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                    psDecl->sUAV.ui32GloballyCoherentAccess, 0, 1, 0, psDecl->ui32BufferStride, glsl);
            }

            break;
        }
        case OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW:
        {
            const bool isVulkan = (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0;
            if (psDecl->sUAV.bCounter)
            {
                if (isVulkan)
                {
                    std::string uavname = ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0);
                    GLSLCrossDependencyData::VulkanResourceBinding uavBinding = psContext->psDependencies->GetVulkanResourceBinding(uavname, true);
                    GLSLCrossDependencyData::VulkanResourceBinding counterBinding = { uavBinding.set, uavBinding.binding + 1 };
                    bformata(glsl, "layout(set = %d, binding = %d) buffer %s_counterBuf { highp uint %s_counter; };\n", counterBinding.set, counterBinding.binding, uavname.c_str(), uavname.c_str());
                }
                else
                {
                    std::string name = ResourceName(psContext, RGROUP_UAV, psDecl->asOperands[0].ui32RegisterNumber, 0);
                    name += "_counter";
                    bcatcstr(glsl, "layout (binding = 0) uniform ");

                    if (HavePrecisionQualifiers(psContext))
                        bcatcstr(glsl, "highp ");
                    bformata(glsl, "atomic_uint %s;\n", name.c_str());
                }
            }

            DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                psDecl->sUAV.ui32GloballyCoherentAccess, 1, 1, 0, psDecl->ui32BufferStride, glsl);

            break;
        }
        case OPCODE_DCL_RESOURCE_STRUCTURED:
        {
            DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                psDecl->sUAV.ui32GloballyCoherentAccess, 0, 0, 0, psDecl->ui32BufferStride, glsl);
            break;
        }
        case OPCODE_DCL_RESOURCE_RAW:
        {
            DeclareBufferVariable(psContext, psDecl->asOperands[0].ui32RegisterNumber, &psDecl->asOperands[0],
                psDecl->sUAV.ui32GloballyCoherentAccess, 1, 0, 0, psDecl->ui32BufferStride, glsl);
            break;
        }
        case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_STRUCTURED:
        {
            ShaderVarType* psVarType = &psShader->sInfo.sGroupSharedVarType[psDecl->asOperands[0].ui32RegisterNumber];

            bcatcstr(glsl, "shared struct {\n");
            bformata(glsl, "\tuint value[%d];\n", psDecl->sTGSM.ui32Stride / 4);
            bcatcstr(glsl, "} ");
            TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NONE);
            bformata(glsl, "[%d];\n",
                psDecl->sTGSM.ui32Count);
            psVarType->name = "value";

            psVarType->Columns = psDecl->sTGSM.ui32Stride / 4;
            psVarType->Elements = psDecl->sTGSM.ui32Count;
            break;
        }
        case OPCODE_DCL_THREAD_GROUP_SHARED_MEMORY_RAW:
        {
            ShaderVarType* psVarType = &psShader->sInfo.sGroupSharedVarType[psDecl->asOperands[0].ui32RegisterNumber];

            bcatcstr(glsl, "shared uint ");
            TranslateOperand(&psDecl->asOperands[0], TO_FLAG_NONE);
            bformata(glsl, "[%d];\n", psDecl->sTGSM.ui32Count / psDecl->sTGSM.ui32Stride);

            psVarType->name = "$Element";

            psVarType->Columns = 1;
            psVarType->Elements = psDecl->sTGSM.ui32Count / psDecl->sTGSM.ui32Stride;
            break;
        }
        case OPCODE_DCL_STREAM:
        {
            ASSERT(psDecl->asOperands[0].eType == OPERAND_TYPE_STREAM);


            if (psShader->eTargetLanguage >= LANG_400 && (psShader->ui32CurrentVertexOutputStream != psDecl->asOperands[0].ui32RegisterNumber))
            {
                // Only emit stream declaration for desktop GL >= 4.0, and only if we're declaring something else than the default 0
                bformata(glsl, "layout(stream = %d) out;\n", psShader->ui32CurrentVertexOutputStream);
            }
            psShader->ui32CurrentVertexOutputStream = psDecl->asOperands[0].ui32RegisterNumber;

            break;
        }
        case OPCODE_DCL_GS_INSTANCE_COUNT:
        {
            bformata(glsl, "layout(invocations = %d) in;\n", psDecl->value.ui32GSInstanceCount);
            break;
        }
        default:
        {
            ASSERT(0);
            break;
        }
    }
}

bool ToGLSL::TranslateSystemValue(const Operand *psOperand, const ShaderInfo::InOutSignature *sig, std::string &result, uint32_t *pui32IgnoreSwizzle, bool isIndexed, bool isInput, bool *outSkipPrefix, int *iIgnoreRedirect)
{
    ASSERT(sig != NULL);
    if (psContext->psShader->eShaderType == HULL_SHADER && sig->semanticName == "SV_TessFactor")
    {
        if (pui32IgnoreSwizzle)
            *pui32IgnoreSwizzle = 1;
        ASSERT(sig->ui32SemanticIndex <= 3);
        std::ostringstream oss;
        oss << "gl_TessLevelOuter[" << sig->ui32SemanticIndex << "]";
        result = oss.str();
        return true;
    }

    if (psContext->psShader->eShaderType == HULL_SHADER && sig->semanticName == "SV_InsideTessFactor")
    {
        if (pui32IgnoreSwizzle)
            *pui32IgnoreSwizzle = 1;
        ASSERT(sig->ui32SemanticIndex <= 1);
        std::ostringstream oss;
        oss << "gl_TessLevelInner[" << sig->ui32SemanticIndex << "]";
        result = oss.str();
        return true;
    }

    switch (sig->eSystemValueType)
    {
        case NAME_POSITION:
            if (psContext->psShader->eShaderType == PIXEL_SHADER)
                result = "hlslcc_FragCoord";
            else
                result = "gl_Position";
            return true;
        case NAME_RENDER_TARGET_ARRAY_INDEX:
            result = "gl_Layer";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_CLIP_DISTANCE:
        case NAME_CULL_DISTANCE:
        {
            const char* glName = sig->eSystemValueType == NAME_CLIP_DISTANCE ? "Clip" : "Cull";
            // This is always routed through temp
            std::ostringstream oss;
            oss << "phase" << psContext->currentPhase << "_gl" << glName << "Distance" << sig->ui32SemanticIndex;
            result = oss.str();
            return true;
        }
        case NAME_VIEWPORT_ARRAY_INDEX:
            result = "gl_ViewportIndex";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_VERTEX_ID:
            if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0)
                result = "gl_VertexIndex";
            else
                result = "gl_VertexID";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_INSTANCE_ID:
            if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0)
                result = "gl_InstanceIndex";
            else
                result = "gl_InstanceID";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_IS_FRONT_FACE:
            if (HaveUnsignedTypes(psContext->psShader->eTargetLanguage))
                result = "(gl_FrontFacing ? 0xffffffffu : uint(0))";    // Old ES3.0 Adrenos treat 0u as const int
            else
                result = "(gl_FrontFacing ? 1 : 0)";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_PRIMITIVE_ID:
            if (isInput && psContext->psShader->eShaderType == GEOMETRY_SHADER)
                result = "gl_PrimitiveIDIn"; // LOL opengl
            else
                result = "gl_PrimitiveID";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_SAMPLE_INDEX:
            result = "gl_SampleID";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_FINAL_QUAD_U_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_U_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_LINE_DENSITY_TESSFACTOR:
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            if (isIndexed)
            {
                result = "gl_TessLevelOuter";
                return true;
            }
            else
            {
                result = "gl_TessLevelOuter[0]";
                return true;
            }
        case NAME_FINAL_QUAD_V_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_V_EQ_0_EDGE_TESSFACTOR:
        case NAME_FINAL_LINE_DETAIL_TESSFACTOR:
            result = "gl_TessLevelOuter[1]";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_FINAL_QUAD_U_EQ_1_EDGE_TESSFACTOR:
        case NAME_FINAL_TRI_W_EQ_0_EDGE_TESSFACTOR:
            result = "gl_TessLevelOuter[2]";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        case NAME_FINAL_QUAD_V_EQ_1_EDGE_TESSFACTOR:
            result = "gl_TessLevelOuter[3]";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;

        case NAME_FINAL_TRI_INSIDE_TESSFACTOR:
        case NAME_FINAL_QUAD_U_INSIDE_TESSFACTOR:
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            if (isIndexed)
            {
                result = "gl_TessLevelInner";
                return true;
            }
            else
            {
                result = "gl_TessLevelInner[0]";
                return true;
            }
        case NAME_FINAL_QUAD_V_INSIDE_TESSFACTOR:
            result = "gl_TessLevelInner[3]";
            if (pui32IgnoreSwizzle)
                *pui32IgnoreSwizzle = 1;
            return true;
        default:
            break;
    }

    if (psContext->psShader->asPhases[psContext->currentPhase].ePhase == HS_CTRL_POINT_PHASE)
    {
        if ((sig->semanticName == "POS" || sig->semanticName == "POSITION" || sig->semanticName == "SV_POSITION" || sig->semanticName == "SV_Position")
            && sig->ui32SemanticIndex == 0)
        {
            result = "gl_out[gl_InvocationID].gl_Position";
            return true;
        }
        std::ostringstream oss;
        if (isInput)
            oss << psContext->inputPrefix << sig->semanticName << sig->ui32SemanticIndex;
        else
            oss << psContext->outputPrefix << sig->semanticName << sig->ui32SemanticIndex << "[gl_InvocationID]";
        result = oss.str();
        return true;
    }

    if ((psOperand->eType == OPERAND_TYPE_OUTPUT || psOperand->eType == OPERAND_TYPE_INPUT)
        && HLSLcc::WriteMaskToComponentCount(sig->ui32Mask) == 1 && pui32IgnoreSwizzle)
        *pui32IgnoreSwizzle = 1;

    // TODO: Add other builtins here.
    if (sig->eSystemValueType == NAME_POSITION || (sig->semanticName == "POS" && sig->ui32SemanticIndex == 0 && psContext->psShader->eShaderType == VERTEX_SHADER))
    {
        result = "gl_Position";
        return true;
    }

    if (sig->semanticName == "PSIZE")
    {
        result = "gl_PointSize";
        if (pui32IgnoreSwizzle)
            *pui32IgnoreSwizzle = 1;
        return true;
    }

    return false;
}
