#include <memory>

#include "internal_includes/tokens.h"
#include "internal_includes/decode.h"
#include "stdlib.h"
#include "stdio.h"
#include "bstrlib.h"
#include "internal_includes/toGLSL.h"
#include "internal_includes/toGLSLOperand.h"
#include "internal_includes/Declaration.h"
#include "internal_includes/languages.h"
#include "internal_includes/debug.h"
#include "internal_includes/HLSLccToolkit.h"
#include "internal_includes/UseDefineChains.h"
#include "internal_includes/DataTypeAnalysis.h"
#include "internal_includes/Shader.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/Instruction.h"
#include "internal_includes/LoopTransform.h"
#include "UnityInstancingFlexibleArraySize.h"
#include <algorithm>
#include <sstream>

// In GLSL, the input and output names cannot clash.
// Also, the output name of previous stage must match the input name of the next stage.
// So, do gymnastics depending on which shader we're running on and which other shaders exist in this program.
//
void ToGLSL::SetIOPrefixes()
{
    switch (psContext->psShader->eShaderType)
    {
        case VERTEX_SHADER:
            psContext->inputPrefix = "in_";
            psContext->outputPrefix = "vs_";
            break;

        case HULL_SHADER:
            // Input always coming from vertex shader
            psContext->inputPrefix = "vs_";
            psContext->outputPrefix = "hs_";
            break;

        case DOMAIN_SHADER:
            // There's no domain shader without hull shader
            psContext->inputPrefix = "hs_";
            psContext->outputPrefix = "ds_";
            break;

        case GEOMETRY_SHADER:
            // The input depends on whether there's a tessellation shader before us
            if (psContext->psDependencies && (psContext->psDependencies->ui32ProgramStages & PS_FLAG_DOMAIN_SHADER))
                psContext->inputPrefix = "ds_";
            else
                psContext->inputPrefix = "vs_";

            psContext->outputPrefix = "gs_";
            break;

        case PIXEL_SHADER:
            // The inputs can come from geom shader, domain shader or directly from vertex shader
            if (psContext->psDependencies)
            {
                if (psContext->psDependencies->ui32ProgramStages & PS_FLAG_GEOMETRY_SHADER)
                {
                    psContext->inputPrefix = "gs_";
                }
                else if (psContext->psDependencies->ui32ProgramStages & PS_FLAG_DOMAIN_SHADER)
                {
                    psContext->inputPrefix = "ds_";
                }
                else
                {
                    psContext->inputPrefix = "vs_";
                }
            }
            else
            {
                psContext->inputPrefix = "vs_";
            }
            psContext->outputPrefix = "";
            break;


        case COMPUTE_SHADER:
        default:
            // No prefixes
            psContext->inputPrefix = "";
            psContext->outputPrefix = "";
            break;
    }
}

static void AddVersionDependentCode(HLSLCrossCompilerContext* psContext)
{
    bstring glsl = *psContext->currentGLSLString;
    bstring extensions = psContext->extensions;
    bool isES = (psContext->psShader->eTargetLanguage >= LANG_ES_100 && psContext->psShader->eTargetLanguage <= LANG_ES_310);
    bool GL_ARB_shader_storage_buffer_object = false;
    bool GL_ARB_shader_image_load_store = false;

    if (psContext->psShader->ui32MajorVersion > 3 && psContext->psShader->eTargetLanguage != LANG_ES_100 && psContext->psShader->eTargetLanguage != LANG_ES_300 && psContext->psShader->eTargetLanguage != LANG_ES_310 && !(psContext->psShader->eTargetLanguage >= LANG_330))
    {
        psContext->EnableExtension("GL_ARB_shader_bit_encoding");
    }

    if (!HaveCompute(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->eShaderType == COMPUTE_SHADER)
        {
            psContext->EnableExtension("GL_ARB_compute_shader");
        }

        if (psContext->psShader->aiOpcodeUsed[OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DCL_RESOURCE_STRUCTURED] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DCL_RESOURCE_RAW])
        {
            GL_ARB_shader_storage_buffer_object = true;
        }
    }

    if (!HaveAtomicMem(psContext->psShader->eTargetLanguage) ||
        !HaveAtomicCounter(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_ALLOC] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_CONSUME] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED])
        {
            psContext->EnableExtension("GL_ARB_shader_atomic_counters");
        }
    }

    if (psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_CMP_STORE] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_AND] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_AND] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_IADD] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_IADD] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_OR] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_XOR] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_IMIN] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_ATOMIC_UMIN] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_IMAX] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_IMIN] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_UMAX] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_UMIN] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_OR] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_XOR] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_EXCH] ||
        psContext->psShader->aiOpcodeUsed[OPCODE_IMM_ATOMIC_CMP_EXCH])
    {
        if (!HaveAtomicMem(psContext->psShader->eTargetLanguage))
            GL_ARB_shader_storage_buffer_object = true;

        if (!HaveImageAtomics(psContext->psShader->eTargetLanguage))
        {
            if (isES)
                psContext->EnableExtension("GL_OES_shader_image_atomic");
            else
                GL_ARB_shader_image_load_store = true;
        }
    }

    if (!HaveGather(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4_PO_C] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4_PO] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4_C])
        {
            psContext->EnableExtension("GL_ARB_texture_gather");
        }
    }

    if (IsESLanguage(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTX_COARSE] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTX_FINE] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTX] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTY_COARSE] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTY_FINE] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_DERIV_RTY])
        {
            if (psContext->psShader->eTargetLanguage < LANG_ES_300)
            {
                psContext->EnableExtension("GL_OES_standard_derivatives");
            }
        }

        if (psContext->psShader->eShaderType == PIXEL_SHADER &&
            (psContext->psShader->aiOpcodeUsed[OPCODE_SAMPLE_L] ||
             psContext->psShader->aiOpcodeUsed[OPCODE_SAMPLE_C_LZ] ||
             psContext->psShader->aiOpcodeUsed[OPCODE_SAMPLE_D]))
        {
            psContext->EnableExtension("GL_EXT_shader_texture_lod");

            static const int tex_sampler_type_count = 4;
            static const char* tex_sampler_dim_name[tex_sampler_type_count] = {
                "1D", "2D", "3D", "Cube",
            };

            if (psContext->psShader->eTargetLanguage == LANG_ES_100)
            {
                bcatcstr(extensions, "#if !defined(GL_EXT_shader_texture_lod)\n");

                for (int dim = 0; dim < tex_sampler_type_count; dim++)
                {
                    bformata(extensions, "#define texture%sLodEXT texture%s\n", tex_sampler_dim_name[dim], tex_sampler_dim_name[dim]);

                    if (dim == 1) // 2D
                        bformata(extensions, "#define texture%sProjLodEXT texture%sProj\n", tex_sampler_dim_name[dim], tex_sampler_dim_name[dim]);
                }
                bcatcstr(extensions, "#endif\n");
            }
        }
    }

    if (!HaveGatherNonConstOffset(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4_PO_C] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_GATHER4_PO])
        {
            psContext->EnableExtension("GL_ARB_gpu_shader5");
        }
    }

    if (!HaveQueryLod(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_LOD])
        {
            psContext->EnableExtension("GL_ARB_texture_query_lod");
        }
    }

    if (!HaveQueryLevels(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_RESINFO])
        {
            psContext->EnableExtension("GL_ARB_texture_query_levels");
            psContext->EnableExtension("GL_ARB_shader_image_size");
        }
    }

    if (psContext->psShader->aiOpcodeUsed[OPCODE_SAMPLE_INFO])
    {
        psContext->EnableExtension("GL_ARB_shader_texture_image_samples");
    }

    if (!HaveImageLoadStore(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->aiOpcodeUsed[OPCODE_STORE_UAV_TYPED] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_STORE_RAW] ||
            psContext->psShader->aiOpcodeUsed[OPCODE_STORE_STRUCTURED])
        {
            GL_ARB_shader_image_load_store = true;
            psContext->EnableExtension("GL_ARB_shader_bit_encoding");
        }
        else if (psContext->psShader->aiOpcodeUsed[OPCODE_LD_UAV_TYPED] ||
                 psContext->psShader->aiOpcodeUsed[OPCODE_LD_RAW] ||
                 psContext->psShader->aiOpcodeUsed[OPCODE_LD_STRUCTURED])
        {
            GL_ARB_shader_image_load_store = true;
        }
    }

    if (!HaveGeometryShaderARB(psContext->psShader->eTargetLanguage))
    {
        if (psContext->psShader->eShaderType == GEOMETRY_SHADER)
        {
            psContext->EnableExtension("GL_ARB_geometry_shader");
        }
    }

    if (psContext->psShader->eTargetLanguage == LANG_ES_300 || psContext->psShader->eTargetLanguage == LANG_ES_310)
    {
        if (psContext->psShader->eShaderType == GEOMETRY_SHADER)
        {
            psContext->EnableExtension("GL_OES_geometry_shader");
            psContext->EnableExtension("GL_EXT_geometry_shader");
        }
    }

    if (psContext->psShader->eTargetLanguage == LANG_ES_300 || psContext->psShader->eTargetLanguage == LANG_ES_310)
    {
        if (psContext->psShader->eShaderType == HULL_SHADER || psContext->psShader->eShaderType == DOMAIN_SHADER)
        {
            psContext->EnableExtension("GL_OES_tessellation_shader");
            psContext->EnableExtension("GL_EXT_tessellation_shader");
        }
    }

    if (GL_ARB_shader_storage_buffer_object)
        psContext->EnableExtension("GL_ARB_shader_storage_buffer_object");

    if (GL_ARB_shader_image_load_store)
        psContext->EnableExtension("GL_ARB_shader_image_load_store");

    if (psContext->psShader->eShaderType == PIXEL_SHADER && psContext->psShader->eTargetLanguage >= LANG_120 && !HaveFragmentCoordConventions(psContext->psShader->eTargetLanguage))
    {
        psContext->RequireExtension("GL_ARB_fragment_coord_conventions");
    }

    if (psContext->psShader->extensions->EXT_shader_framebuffer_fetch && psContext->psShader->eShaderType == PIXEL_SHADER && psContext->flags & HLSLCC_FLAG_SHADER_FRAMEBUFFER_FETCH)
    {
        psContext->EnableExtension("GL_EXT_shader_framebuffer_fetch");
    }

    //Handle fragment shader default precision
    if (psContext->psShader->eShaderType == PIXEL_SHADER &&
        (psContext->psShader->eTargetLanguage == LANG_ES_100 || psContext->psShader->eTargetLanguage == LANG_ES_300 || psContext->psShader->eTargetLanguage == LANG_ES_310 || (psContext->flags & HLSLCC_FLAG_NVN_TARGET)))
    {
        if (psContext->psShader->eTargetLanguage == LANG_ES_100)
        {
            // gles 2.0 shaders can have mediump as default if the GPU doesn't have highp support
            bcatcstr(glsl,
                "#ifdef GL_FRAGMENT_PRECISION_HIGH\n"
                "    precision highp float;\n"
                "#else\n"
                "    precision mediump float;\n"
                "#endif\n");
        }
        else
        {
            bcatcstr(glsl, "precision highp float;\n");
        }

        // Define default int precision to highp to avoid issues on platforms that actually implement mediump
        bcatcstr(glsl, "precision highp int;\n");
    }

    if (psContext->psShader->eShaderType == PIXEL_SHADER && psContext->psShader->eTargetLanguage >= LANG_150)
    {
        if (psContext->flags & HLSLCC_FLAG_ORIGIN_UPPER_LEFT)
            bcatcstr(glsl, "layout(origin_upper_left) in vec4 gl_FragCoord;\n");

        if (psContext->flags & HLSLCC_FLAG_PIXEL_CENTER_INTEGER)
            bcatcstr(glsl, "layout(pixel_center_integer) in vec4 gl_FragCoord;\n");
    }


    /*
        OpenGL 4.1 API spec:
        To use any built-in input or output in the gl_PerVertex block in separable
        program objects, shader code must redeclare that block prior to use.
    */
    /* DISABLED FOR NOW */
/*  if(psContext->psShader->eShaderType == VERTEX_SHADER && psContext->psShader->eTargetLanguage >= LANG_410)
    {
        bcatcstr(glsl, "out gl_PerVertex {\n");
        bcatcstr(glsl, "vec4 gl_Position;\n");
        bcatcstr(glsl, "float gl_PointSize;\n");
        bcatcstr(glsl, "float gl_ClipDistance[];");
        bcatcstr(glsl, "};\n");
    }*/
}

GLLang ChooseLanguage(Shader* psShader)
{
    // Depends on the HLSL shader model extracted from bytecode.
    switch (psShader->ui32MajorVersion)
    {
        case 5:
        {
            return LANG_430;
        }
        case 4:
        {
            return LANG_330;
        }
        default:
        {
            return LANG_120;
        }
    }
}

const char* GetVersionString(GLLang language)
{
    switch (language)
    {
        case LANG_ES_100:
        {
            return "#version 100\n";
            break;
        }
        case LANG_ES_300:
        {
            return "#version 300 es\n";
            break;
        }
        case LANG_ES_310:
        {
            return "#version 310 es\n";
            break;
        }
        case LANG_120:
        {
            return "#version 120\n";
            break;
        }
        case LANG_130:
        {
            return "#version 130\n";
            break;
        }
        case LANG_140:
        {
            return "#version 140\n";
            break;
        }
        case LANG_150:
        {
            return "#version 150\n";
            break;
        }
        case LANG_330:
        {
            return "#version 330\n";
            break;
        }
        case LANG_400:
        {
            return "#version 400\n";
            break;
        }
        case LANG_410:
        {
            return "#version 410\n";
            break;
        }
        case LANG_420:
        {
            return "#version 420\n";
            break;
        }
        case LANG_430:
        {
            return "#version 430\n";
            break;
        }
        case LANG_440:
        {
            return "#version 440\n";
            break;
        }
        default:
        {
            return "";
            break;
        }
    }
}

static const char * GetPhaseFuncName(SHADER_PHASE_TYPE eType)
{
    switch (eType)
    {
        default:
        case MAIN_PHASE: return "";
        case HS_GLOBAL_DECL_PHASE: return "hs_global_decls";
        case HS_FORK_PHASE: return "fork_phase";
        case HS_CTRL_POINT_PHASE: return "control_point_phase";
        case HS_JOIN_PHASE: return "join_phase";
    }
}

static void DoHullShaderPassthrough(HLSLCrossCompilerContext *psContext)
{
    uint32_t i;
    bstring glsl = psContext->glsl;

    for (i = 0; i < psContext->psShader->sInfo.psInputSignatures.size(); i++)
    {
        ShaderInfo::InOutSignature *psSig = &psContext->psShader->sInfo.psInputSignatures[i];
        const char *Type;
        uint32_t ui32NumComponents = HLSLcc::GetNumberBitsSet(psSig->ui32Mask);
        switch (psSig->eComponentType)
        {
            default:
            case INOUT_COMPONENT_FLOAT32:
                Type = ui32NumComponents > 1 ? "vec" : "float";
                break;
            case INOUT_COMPONENT_SINT32:
                Type = ui32NumComponents > 1 ? "ivec" : "int";
                break;
            case INOUT_COMPONENT_UINT32:
                Type = ui32NumComponents > 1 ? "uvec" : "uint";
                break;
        }
        if ((psSig->eSystemValueType == NAME_POSITION || psSig->semanticName == "POS") && psSig->ui32SemanticIndex == 0)
            continue;

        std::string inputName;

        {
            std::ostringstream oss;
            oss << psContext->inputPrefix << psSig->semanticName << psSig->ui32SemanticIndex;
            inputName = oss.str();
        }

        if (psContext->psDependencies->IsHullShaderInputAlreadyDeclared(inputName))
            continue;

        psContext->psDependencies->RecordHullShaderInput(inputName);

        std::string outputName;
        {
            std::ostringstream oss;
            oss << psContext->outputPrefix << psSig->semanticName << psSig->ui32SemanticIndex;
            outputName = oss.str();
        }

        const char * prec = "";
        if (HavePrecisionQualifiers(psContext))
        {
            if (psSig->eMinPrec != MIN_PRECISION_DEFAULT)
                prec = "mediump ";
            else
                prec = "highp ";
        }

        bool keepLocation = ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0);
        int inLoc = psContext->psDependencies->GetVaryingLocation(inputName, HULL_SHADER, true, keepLocation, psContext->psShader->maxSemanticIndex);
        int outLoc = psContext->psDependencies->GetVaryingLocation(outputName, HULL_SHADER, false, keepLocation, psContext->psShader->maxSemanticIndex);

        psContext->AddIndentation();
        if (ui32NumComponents > 1)
            bformata(glsl, "layout(location = %d) in %s%s%d %s%s%d[];\n", inLoc, prec, Type, ui32NumComponents, psContext->inputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);
        else
            bformata(glsl, "layout(location = %d) in %s%s %s%s%d[];\n", inLoc, prec, Type, psContext->inputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);

        psContext->AddIndentation();
        if (ui32NumComponents > 1)
            bformata(glsl, "layout(location = %d) out %s%s%d %s%s%d[];\n", outLoc, prec, Type, ui32NumComponents, psContext->outputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);
        else
            bformata(glsl, "layout(location = %d) out %s%s %s%s%d[];\n", outLoc, prec, Type, psContext->outputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);
    }

    psContext->AddIndentation();
    bcatcstr(glsl, "void passthrough_ctrl_points()\n");
    psContext->AddIndentation();
    bcatcstr(glsl, "{\n");
    psContext->indent++;

    for (i = 0; i < psContext->psShader->sInfo.psInputSignatures.size(); i++)
    {
        const ShaderInfo::InOutSignature *psSig = &psContext->psShader->sInfo.psInputSignatures[i];

        psContext->AddIndentation();

        if ((psSig->eSystemValueType == NAME_POSITION || psSig->semanticName == "POS") && psSig->ui32SemanticIndex == 0)
            bformata(glsl, "gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;\n");
        else
            bformata(glsl, "%s%s%d[gl_InvocationID] = %s%s%d[gl_InvocationID];\n", psContext->outputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex, psContext->inputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);
    }

    psContext->indent--;
    psContext->AddIndentation();
    bcatcstr(glsl, "}\n");
}

GLLang ToGLSL::SetLanguage(GLLang suggestedLanguage)
{
    language = suggestedLanguage;
    if (language == LANG_DEFAULT)
    {
        language = ChooseLanguage(psContext->psShader);
    }
    return language;
}

// Go through all declarations and remove reserve UAV occupied binding points
void ResolveStructuredBufferBindingSlots(ShaderPhase *psPhase, HLSLCrossCompilerContext *psContext, GLSLCrossDependencyData *glslDependencyData)
{
    for (uint32_t p = 0; p < psPhase->psDecl.size(); ++p)
    {
        if (psPhase->psDecl[p].eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW ||
            psPhase->psDecl[p].eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED)
        {
            uint32_t uav = psPhase->psDecl[p].asOperands[0].ui32RegisterNumber; // uav binding point

            bstring BufNamebstr = bfromcstr("");
            ResourceName(BufNamebstr, psContext, RGROUP_UAV, psPhase->psDecl[p].asOperands[0].ui32RegisterNumber, 0);

            char *btmp = bstr2cstr(BufNamebstr, '\0');
            std::string BufName = btmp;
            bcstrfree(btmp);
            bdestroy(BufNamebstr);

            glslDependencyData->ReserveNamedBindPoint(BufName, uav, GLSLCrossDependencyData::BufferType_ReadWrite);
        }
    }
}

bool ToGLSL::Translate()
{
    bstring glsl;
    uint32_t i;
    Shader* psShader = psContext->psShader;
    uint32_t ui32Phase;

    psContext->psTranslator = this;

    if (language == LANG_DEFAULT)
        SetLanguage(LANG_DEFAULT);

    SetIOPrefixes();
    psShader->ExpandSWAPCs();
    psShader->ForcePositionToHighp();
    psShader->AnalyzeIOOverlap();
    if ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0)
        psShader->SetMaxSemanticIndex();
    psShader->FindUnusedGlobals(psContext->flags);

    psContext->indent = 0;

    glsl = bfromcstralloc(1024 * 10, "\n");
    bstring extensions = bfromcstralloc(1024 * 10, GetVersionString(language));
    psContext->extensions = extensions;

    psContext->glsl = glsl;
    for (i = 0; i < psShader->asPhases.size(); ++i)
    {
        psShader->asPhases[i].postShaderCode = bfromcstralloc(1024 * 5, "");
        psShader->asPhases[i].earlyMain = bfromcstralloc(1024 * 5, "");
    }
    psContext->currentGLSLString = &glsl;
    psShader->eTargetLanguage = language;
    psContext->currentPhase = MAIN_PHASE;

    if (psShader->extensions)
    {
        if (psContext->flags & HLSLCC_FLAG_NVN_TARGET)
        {
            psContext->EnableExtension("GL_ARB_separate_shader_objects");
            psContext->EnableExtension("GL_NV_desktop_lowp_mediump"); // This flag allow FP16 operations (mediump in GLSL)
        }
        if (psShader->extensions->ARB_explicit_attrib_location)
            psContext->RequireExtension("GL_ARB_explicit_attrib_location");
        if (psShader->extensions->ARB_explicit_uniform_location)
            psContext->RequireExtension("GL_ARB_explicit_uniform_location");
        if (psShader->extensions->ARB_shading_language_420pack)
            psContext->RequireExtension("GL_ARB_shading_language_420pack");
    }

    psContext->ClearDependencyData();

    AddVersionDependentCode(psContext);

    if (psShader->eShaderType == VERTEX_SHADER &&
        HaveLimitedInOutLocationQualifier(language, psShader->extensions) &&
        psContext->flags & HLSLCC_FLAG_NVN_TARGET)
    {
        bcatcstr(glsl, "out gl_PerVertex { vec4 gl_Position; };\n");
    }

    if (!psContext->psDependencies->m_ExtBlendModes.empty() && psShader->eShaderType == PIXEL_SHADER)
    {
        psContext->EnableExtension("GL_KHR_blend_equation_advanced");
        bcatcstr(glsl, "#if GL_KHR_blend_equation_advanced\n");
        for (i = 0; i < psContext->psDependencies->m_ExtBlendModes.size(); i++)
        {
            bformata(glsl, "layout(%s) out;\n", psContext->psDependencies->m_ExtBlendModes[i].c_str());
        }
        bcatcstr(glsl, "#endif\n");
    }

    if (psContext->psShader->eTargetLanguage != LANG_ES_100)
    {
        bool hasConstantBuffers = psContext->psShader->sInfo.psConstantBuffers.size() > 0;
        if (hasConstantBuffers)
        {
            // This value will be replaced at runtime with 0 if we need to disable UBO.
            bcatcstr(glsl, "#define HLSLCC_ENABLE_UNIFORM_BUFFERS 1\n");
            bcatcstr(glsl, "#if HLSLCC_ENABLE_UNIFORM_BUFFERS\n#define UNITY_UNIFORM\n#else\n#define UNITY_UNIFORM uniform\n#endif\n");
        }
        bool hasTextures = false;
        for (i = 0; i < psShader->asPhases[0].psDecl.size(); ++i)
        {
            if (psShader->asPhases[0].psDecl[i].eOpcode == OPCODE_DCL_RESOURCE)
            {
                hasTextures = true;
                break;
            }
        }
        if (hasTextures || hasConstantBuffers)
        {
            // This value will be replaced at runtime with 0 if we need to disable explicit uniform locations.
            bcatcstr(glsl, "#define UNITY_SUPPORTS_UNIFORM_LOCATION 1\n");
            bcatcstr(glsl, "#if UNITY_SUPPORTS_UNIFORM_LOCATION\n#define UNITY_LOCATION(x) layout(location = x)\n#define UNITY_BINDING(x) layout(binding = x, std140)\n#else\n#define UNITY_LOCATION(x)\n#define UNITY_BINDING(x) layout(std140)\n#endif\n");
        }
    }

    for (ui32Phase = 0; ui32Phase < psShader->asPhases.size(); ui32Phase++)
    {
        ShaderPhase &phase = psShader->asPhases[ui32Phase];
        phase.UnvectorizeImmMoves();
        psContext->DoDataTypeAnalysis(&phase);
        phase.ResolveUAVProperties(psShader->sInfo);
        ResolveStructuredBufferBindingSlots(&phase, psContext, psContext->psDependencies);
        if (!psContext->IsVulkan() && !psContext->IsSwitch())
        {
            phase.PruneConstArrays();
            psContext->ReserveFramebufferFetchInputs();
        }
    }

    psShader->PruneTempRegisters();

    for (ui32Phase = 0; ui32Phase < psShader->asPhases.size(); ui32Phase++)
    {
        // Loop transform can only be done after the temps have been pruned
        ShaderPhase &phase = psShader->asPhases[ui32Phase];
        HLSLcc::DoLoopTransform(psContext, phase);
    }

    //Special case. Can have multiple phases.
    if (psShader->eShaderType == HULL_SHADER)
    {
        const SHADER_PHASE_TYPE ePhaseFuncCallOrder[3] = { HS_CTRL_POINT_PHASE, HS_FORK_PHASE, HS_JOIN_PHASE };
        uint32_t ui32PhaseCallIndex;
        int perPatchSectionAdded = 0;
        int hasControlPointPhase = 0;

        psShader->ConsolidateHullTempVars();

        // Find out if we have a passthrough hull shader
        for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
        {
            if (psShader->asPhases[ui32Phase].ePhase == HS_CTRL_POINT_PHASE)
                hasControlPointPhase = 1;
        }

        // Phase 1 is always the global decls phase, no instructions
        for (i = 0; i < psShader->asPhases[1].psDecl.size(); ++i)
        {
            TranslateDeclaration(&psShader->asPhases[1].psDecl[i]);
        }

        if (hasControlPointPhase == 0)
        {
            DoHullShaderPassthrough(psContext);
        }

        for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
        {
            ShaderPhase *psPhase = &psShader->asPhases[ui32Phase];
            psContext->currentPhase = ui32Phase;

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                bformata(glsl, "//%s declarations\n", GetPhaseFuncName(psPhase->ePhase));
            }

            for (i = 0; i < psPhase->psDecl.size(); ++i)
            {
                TranslateDeclaration(&psPhase->psDecl[i]);
            }

            bformata(glsl, "void %s%d(int phaseInstanceID)\n{\n", GetPhaseFuncName(psPhase->ePhase), ui32Phase);
            psContext->indent++;

            if (psPhase->psInst.size() > 0)
            {
                //The minus one here is remove the return statement at end of phases.
                //We don't want to translate that, we'll just end the function body.
                ASSERT(psPhase->psInst[psPhase->psInst.size() - 1].eOpcode == OPCODE_RET);
                for (i = 0; i < psPhase->psInst.size() - 1; ++i)
                {
                    TranslateInstruction(&psPhase->psInst[i]);
                }
            }


            psContext->indent--;
            bcatcstr(glsl, "}\n");
        }

        bcatcstr(glsl, "void main()\n{\n");

        psContext->indent++;

        // There are cases when there are no control point phases and we have to do passthrough
        if (hasControlPointPhase == 0)
        {
            // Passthrough control point phase, run the rest only once per patch
            psContext->AddIndentation();
            bcatcstr(glsl, "passthrough_ctrl_points();\n");
            psContext->AddIndentation();
            bcatcstr(glsl, "barrier();\n");
            psContext->AddIndentation();
            bcatcstr(glsl, "if (gl_InvocationID == 0)\n");
            psContext->AddIndentation();
            bcatcstr(glsl, "{\n");
            psContext->indent++;
            perPatchSectionAdded = 1;
        }

        for (ui32PhaseCallIndex = 0; ui32PhaseCallIndex < 3; ui32PhaseCallIndex++)
        {
            for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
            {
                uint32_t i;
                ShaderPhase *psPhase = &psShader->asPhases[ui32Phase];
                if (psPhase->ePhase != ePhaseFuncCallOrder[ui32PhaseCallIndex])
                    continue;

                if (psPhase->earlyMain->slen > 1)
                {
                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(glsl, "//--- Start Early Main ---\n");
                    }

                    bconcat(glsl, psPhase->earlyMain);

                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(glsl, "//--- End Early Main ---\n");
                    }
                }

                for (i = 0; i < psPhase->ui32InstanceCount; i++)
                {
                    psContext->AddIndentation();
                    bformata(glsl, "%s%d(%d);\n", GetPhaseFuncName(psShader->asPhases[ui32Phase].ePhase), ui32Phase, i);
                }

                if (psPhase->hasPostShaderCode)
                {
                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(glsl, "//--- Post shader code ---\n");
                    }

                    bconcat(glsl, psPhase->postShaderCode);

                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(glsl, "//--- End post shader code ---\n");
                    }
                }


                if (psShader->asPhases[ui32Phase].ePhase == HS_CTRL_POINT_PHASE)
                {
                    // We're done printing control point phase, run the rest only once per patch
                    psContext->AddIndentation();
                    bcatcstr(glsl, "barrier();\n");
                    psContext->AddIndentation();
                    bcatcstr(glsl, "if (gl_InvocationID == 0)\n");
                    psContext->AddIndentation();
                    bcatcstr(glsl, "{\n");
                    psContext->indent++;
                    perPatchSectionAdded = 1;
                }
            }
        }

        if (perPatchSectionAdded != 0)
        {
            psContext->indent--;
            psContext->AddIndentation();
            bcatcstr(glsl, "}\n");
        }

        psContext->indent--;

        bcatcstr(glsl, "}\n");

        // Print out extra functions we generated, in reverse order for potential dependencies
        std::for_each(m_FunctionDefinitions.rbegin(), m_FunctionDefinitions.rend(), [&extensions](const FunctionDefinitions::value_type &p)
        {
            bcatcstr(extensions, p.second.c_str());
            bcatcstr(extensions, "\n");
        });

        // Concat extensions and glsl for the final shader code.
        if (m_NeedUnityInstancingArraySizeDecl)
        {
            if (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS)
            {
                bformata(extensions, "layout(constant_id = %d) const int %s = 2;\n", kArraySizeConstantID, UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO);
            }
            else
            {
                bcatcstr(extensions, "#ifndef " UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO "\n\t#define " UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO " 2\n#endif\n");
            }
        }
        if (m_NeedUnityPreTransformDecl)
        {
            if (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS)
            {
                bformata(extensions, "layout(constant_id = %d) const int %s = 0;\n", kPreTransformConstantID, UNITY_PRETRANSFORM_CONSTANT_NAME);
            }
        }

        bconcat(extensions, glsl);
        bdestroy(glsl);
        psContext->glsl = extensions;
        glsl = NULL;

        if (psContext->psDependencies)
        {
            //Save partitioning and primitive type for use by domain shader.
            psContext->psDependencies->eTessOutPrim = psShader->sInfo.eTessOutPrim;

            psContext->psDependencies->eTessPartitioning = psShader->sInfo.eTessPartitioning;
        }

        return true;
    }

    if (psShader->eShaderType == DOMAIN_SHADER && psContext->psDependencies)
    {
        //Load partitioning and primitive type from hull shader.
        switch (psContext->psDependencies->eTessOutPrim)
        {
            case TESSELLATOR_OUTPUT_TRIANGLE_CCW:
            {
                bcatcstr(glsl, "layout(ccw) in;\n");
                break;
            }
            case TESSELLATOR_OUTPUT_TRIANGLE_CW:
            {
                bcatcstr(glsl, "layout(cw) in;\n");
                break;
            }
            case TESSELLATOR_OUTPUT_POINT:
            {
                bcatcstr(glsl, "layout(point_mode) in;\n");
                break;
            }
            default:
            {
                break;
            }
        }

        switch (psContext->psDependencies->eTessPartitioning)
        {
            case TESSELLATOR_PARTITIONING_FRACTIONAL_ODD:
            {
                bcatcstr(glsl, "layout(fractional_odd_spacing) in;\n");
                break;
            }
            case TESSELLATOR_PARTITIONING_FRACTIONAL_EVEN:
            {
                bcatcstr(glsl, "layout(fractional_even_spacing) in;\n");
                break;
            }
            default:
            {
                break;
            }
        }
    }

    bstring generatedFunctionsKeyword = bfromcstr("\n// Generated functions\n\n");
    bstring beforeMain = NULL;
    bstring beforeMainKeyword = NULL;

    if (!HaveDynamicIndexing(psContext))
    {
        beforeMain = bfromcstr("");
        beforeMainKeyword = bfromcstr("\n// Before Main\n\n");
        psContext->beforeMain = beforeMain;
    }

    for (i = 0; i < psShader->asPhases[0].psDecl.size(); ++i)
    {
        TranslateDeclaration(&psShader->asPhases[0].psDecl[i]);
    }

    // Search and replace string, for injecting generated functions that need to be after default precision declarations
    bconcat(glsl, generatedFunctionsKeyword);

    // Search and replace string, for injecting stuff from translation that need to be after normal declarations and before main
    if (!HaveDynamicIndexing(psContext))
    {
        bconcat(glsl, beforeMainKeyword);
    }

    bcatcstr(glsl, "void main()\n{\n");

    psContext->indent++;

    if (psContext->psShader->asPhases[0].earlyMain->slen > 1)
    {
        if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
        {
            psContext->AddIndentation();
            bcatcstr(glsl, "//--- Start Early Main ---\n");
        }

        bconcat(glsl, psContext->psShader->asPhases[0].earlyMain);

        if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
        {
            psContext->AddIndentation();
            bcatcstr(glsl, "//--- End Early Main ---\n");
        }
    }

    for (i = 0; i < psShader->asPhases[0].psInst.size(); ++i)
    {
        TranslateInstruction(&psShader->asPhases[0].psInst[i]);
    }

    psContext->indent--;

    bcatcstr(glsl, "}\n");

    // Print out extra definitions and functions we generated in generation order to satisfy dependencies
    {
        bstring generatedFunctionsAndDefinitions = bfromcstr("");

        for (size_t i = 0; i < m_AdditionalDefinitions.size(); ++i)
        {
            bcatcstr(generatedFunctionsAndDefinitions, m_AdditionalDefinitions[i].c_str());
            bcatcstr(generatedFunctionsAndDefinitions, "\n");
        }

        for (std::vector<std::string>::const_iterator funcNameIter = m_FunctionDefinitionsOrder.begin(); funcNameIter != m_FunctionDefinitionsOrder.end(); ++funcNameIter)
        {
            const FunctionDefinitions::const_iterator definition = m_FunctionDefinitions.find(*funcNameIter);
            ASSERT(definition != m_FunctionDefinitions.end());
            bcatcstr(generatedFunctionsAndDefinitions, definition->second.c_str());
            bcatcstr(generatedFunctionsAndDefinitions, "\n");
        }
        bfindreplace(glsl, generatedFunctionsKeyword, generatedFunctionsAndDefinitions, 0);
        bdestroy(generatedFunctionsAndDefinitions);
        bdestroy(generatedFunctionsKeyword);
    }

    // Concat extensions and glsl for the final shader code.
    if (m_NeedUnityInstancingArraySizeDecl)
    {
        if (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS)
        {
            bformata(extensions, "layout(constant_id = %d) const int %s = 2;\n", kArraySizeConstantID, UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO);
        }
        else
        {
            bcatcstr(extensions, "#ifndef " UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO "\n\t#define " UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO " 2\n#endif\n");
        }
    }
    if (m_NeedUnityPreTransformDecl)
    {
        if (psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS)
        {
            bformata(extensions, "layout(constant_id = %d) const int %s = 0;\n", kPreTransformConstantID, UNITY_PRETRANSFORM_CONSTANT_NAME);
        }
    }
    bconcat(extensions, glsl);
    bdestroy(glsl);

    if (!HaveDynamicIndexing(psContext))
    {
        bstring empty = bfromcstr("");

        if (beforeMain->slen > 1)
            bfindreplace(extensions, beforeMainKeyword, beforeMain, 0);
        else
            bfindreplace(extensions, beforeMainKeyword, empty, 0);

        psContext->beforeMain = NULL;

        bdestroy(empty);
        bdestroy(beforeMain);
        bdestroy(beforeMainKeyword);
    }

    psContext->glsl = extensions;
    glsl = NULL;

    return true;
}

bool ToGLSL::DeclareExtraFunction(const std::string &name, bstring body)
{
    if (m_FunctionDefinitions.find(name) != m_FunctionDefinitions.end())
        return true;
    m_FunctionDefinitions.insert(std::make_pair(name, (const char *)body->data));
    m_FunctionDefinitionsOrder.push_back(name);
    return false;
}

static void PrintComponentWrapper1(bstring code, const char *func, const char *type2, const char *type3, const char *type4)
{
    bformata(code, "%s %s(%s a) { a.x = %s(a.x); a.y = %s(a.y); return a; }\n", type2, func, type2, func, func);
    bformata(code, "%s %s(%s a) { a.x = %s(a.x); a.y = %s(a.y); a.z = %s(a.z); return a; }\n", type3, func, type3, func, func, func);
    bformata(code, "%s %s(%s a) { a.x = %s(a.x); a.y = %s(a.y); a.z = %s(a.z); a.w = %s(a.w); return a; }\n", type4, func, type4, func, func, func, func);
}

static void PrintComponentWrapper2(bstring code, const char *func, const char *type2, const char *type3, const char *type4)
{
    bformata(code, "%s %s(%s a, %s b) { a.x = %s(a.x, b.x); a.y = %s(a.y, b.y); return a; }\n", type2, func, type2, type2, func, func);
    bformata(code, "%s %s(%s a, %s b) { a.x = %s(a.x, b.x); a.y = %s(a.y, b.y); a.z = %s(a.z, b.z); return a; }\n", type3, func, type3, type3, func, func, func);
    bformata(code, "%s %s(%s a, %s b) { a.x = %s(a.x, b.x); a.y = %s(a.y, b.y); a.z = %s(a.z, b.z); a.w = %s(a.w, b.w); return a; }\n", type4, func, type4, type4, func, func, func, func);
}

static void PrintTrunc(bstring code, const char *type)
{
    bformata(code, "%s trunc(%s x) { return sign(x)*floor(abs(x)); }\n", type, type);
}

void ToGLSL::UseExtraFunctionDependency(const std::string &name)
{
    if (m_FunctionDefinitions.find(name) != m_FunctionDefinitions.end())
        return;

    bstring code = bfromcstr("");
    bool match = true;

    if (name == "trunc")
    {
        PrintTrunc(code, "float");
        PrintTrunc(code, "vec2");
        PrintTrunc(code, "vec3");
        PrintTrunc(code, "vec4");
    }
    else if (name == "roundEven")
    {
        bformata(code, "float roundEven(float x) { float y = floor(x + 0.5); return (y - x == 0.5) ? floor(0.5*y) * 2.0 : y; }\n");
        PrintComponentWrapper1(code, "roundEven", "vec2", "vec3", "vec4");
    }
    else if (name == "op_modi")
    {
        bformata(code, "const int BITWISE_BIT_COUNT = 32;\nint op_modi(int x, int y) { return x - y * (x / y); }\n");
        PrintComponentWrapper2(code, "op_modi", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_and")
    {
        UseExtraFunctionDependency("op_modi");

        bformata(code, "int op_and(int a, int b) { int result = 0; int n = 1; for (int i = 0; i < BITWISE_BIT_COUNT; i++) { if ((op_modi(a, 2) != 0) && (op_modi(b, 2) != 0)) { result += n; } a = a / 2; b = b / 2; n = n * 2; if (!(a > 0 && b > 0)) { break; } } return result; }\n");
        PrintComponentWrapper2(code, "op_and", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_or")
    {
        UseExtraFunctionDependency("op_modi");

        bformata(code, "int op_or(int a, int b) { int result = 0; int n = 1; for (int i = 0; i < BITWISE_BIT_COUNT; i++) { if ((op_modi(a, 2) != 0) || (op_modi(b, 2) != 0)) { result += n; } a = a / 2; b = b / 2; n = n * 2; if (!(a > 0 || b > 0)) { break; } } return result; }\n");
        PrintComponentWrapper2(code, "op_or", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_xor")
    {
        UseExtraFunctionDependency("op_and");

        bformata(code, "int op_xor(int a, int b) { return (a + b - 2 * op_and(a, b)); }\n");
        PrintComponentWrapper2(code, "op_xor", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_shr")
    {
        bformata(code, "int op_shr(int a, int b) { return int(floor(float(a) / pow(2.0, float(b)))); }\n");
        PrintComponentWrapper2(code, "op_shr", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_shl")
    {
        bformata(code, "int op_shl(int a, int b) { return int(floor(float(a) * pow(2.0, float(b)))); }\n");
        PrintComponentWrapper2(code, "op_shl", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "op_not")
    {
        bformata(code, "int op_not(int value) { return -value - 1; }\n");
        PrintComponentWrapper1(code, "op_not", "ivec2", "ivec3", "ivec4");
    }
    else if (name == "int_bitfieldInsert")
    {
        // Can't use the name 'bitfieldInsert' because Adreno fails with "can't redefine/overload built-in functions!"
        bcatcstr(code,
            "int int_bitfieldInsert(int base, int insert, int offset, int bits) {\n"
            "    uint mask = ~(uint(0xffffffff) << uint(bits)) << uint(offset);\n"
            "    return int((uint(base) & ~mask) | ((uint(insert) << uint(offset)) & mask));\n"
            "}\n");
    }
    else
    {
        match = false;
    }

    if (match)
        DeclareExtraFunction(name, code);

    bdestroy(code);
}
