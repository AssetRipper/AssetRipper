#include "internal_includes/toMetal.h"
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/Shader.h"
#include "internal_includes/debug.h"

#include "internal_includes/Declaration.h"
#include "internal_includes/toGLSL.h"
#include "internal_includes/LoopTransform.h"
#include "internal_includes/HLSLccToolkit.h"
#include <algorithm>

static void PrintStructDeclaration(HLSLCrossCompilerContext *psContext, bstring glsl, std::string &sname, StructDefinitions &defs)
{
    StructDefinition &d = defs[sname];
    if (d.m_IsPrinted)
        return;
    d.m_IsPrinted = true;


    std::for_each(d.m_Dependencies.begin(), d.m_Dependencies.end(), [&psContext, &glsl, &defs](std::string &depName)
    {
        PrintStructDeclaration(psContext, glsl, depName, defs);
    });

    bformata(glsl, "struct %s\n{\n", sname.c_str());
    psContext->indent++;
    std::for_each(d.m_Members.begin(), d.m_Members.end(), [&psContext, &glsl](const MemberDefinitions::value_type &mem)
    {
        psContext->AddIndentation();
        bcatcstr(glsl, mem.second.c_str());
        bcatcstr(glsl, ";\n");
    });

    psContext->indent--;
    bcatcstr(glsl, "};\n\n");
}

void ToMetal::PrintStructDeclarations(StructDefinitions &defs, const char *name)
{
    bstring glsl = *psContext->currentGLSLString;
    StructDefinition &args = defs[name];
    std::for_each(args.m_Dependencies.begin(), args.m_Dependencies.end(), [this, glsl, &defs](std::string &sname)
    {
        PrintStructDeclaration(psContext, glsl, sname, defs);
    });
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
    bstring glsl = *psContext->currentGLSLString;

    for (i = 0; i < psContext->psShader->sInfo.psInputSignatures.size(); i++)
    {
        const ShaderInfo::InOutSignature *psSig = &psContext->psShader->sInfo.psInputSignatures[i];

        psContext->AddIndentation();
        if ((psSig->eSystemValueType == NAME_POSITION || psSig->semanticName == "POS") && psSig->ui32SemanticIndex == 0)
            bformata(glsl, "%s%s = %scp[controlPointID].%s;\n", psContext->outputPrefix, "mtl_Position", psContext->inputPrefix, "mtl_Position");
        else
            bformata(glsl, "%s%s%d = %scp[controlPointID].%s%d;\n", psContext->outputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex, psContext->inputPrefix, psSig->semanticName.c_str(), psSig->ui32SemanticIndex);
    }
}

bool ToMetal::Translate()
{
    bstring glsl;
    uint32_t i;
    Shader* psShader = psContext->psShader;
    uint32_t ui32Phase;

    psContext->psTranslator = this;

    SetIOPrefixes();
    psShader->ExpandSWAPCs();
    psShader->ForcePositionToHighp();
    psShader->AnalyzeIOOverlap();
    if ((psContext->flags & HLSLCC_FLAG_KEEP_VARYING_LOCATIONS) != 0)
        psShader->SetMaxSemanticIndex();
    psShader->FindUnusedGlobals(psContext->flags);

    psContext->indent = 0;

    glsl = bfromcstralloc(1024 * 10, "");
    bstring bodyglsl = bfromcstralloc(1024 * 10, "");

    psContext->glsl = glsl;
    for (i = 0; i < psShader->asPhases.size(); ++i)
    {
        psShader->asPhases[i].postShaderCode = bfromcstralloc(1024 * 5, "");
        psShader->asPhases[i].earlyMain = bfromcstralloc(1024 * 5, "");
    }

    psContext->currentGLSLString = &glsl;
    psShader->eTargetLanguage = LANG_METAL;
    psShader->extensions = NULL;
    psContext->currentPhase = MAIN_PHASE;

    psContext->ClearDependencyData();

    const SHADER_PHASE_TYPE ePhaseFuncCallOrder[3] = { HS_CTRL_POINT_PHASE, HS_FORK_PHASE, HS_JOIN_PHASE };
    uint32_t ui32PhaseCallIndex;
    int hasControlPointPhase = 0;

    const int maxThreadsPerThreadGroup = 32;
    int numPatchesInThreadGroup = 0;
    bool hasControlPoint = false;
    bool hasPatchConstant = false;
    std::string tessVertexFunctionArguments;

    if ((psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER) && (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) != 0)
    {
        if (psContext->psDependencies)
        {
            m_StructDefinitions[""].m_Members = psContext->psDependencies->m_SharedFunctionMembers;
            m_TextureSlots = psContext->psDependencies->m_SharedTextureSlots;
            m_SamplerSlots = psContext->psDependencies->m_SharedSamplerSlots;
            m_BufferSlots = psContext->psDependencies->m_SharedBufferSlots;
            hasControlPoint = psContext->psDependencies->hasControlPoint;
            hasPatchConstant = psContext->psDependencies->hasPatchConstant;
        }
    }

    ClampPartialPrecisions();

    for (ui32Phase = 0; ui32Phase < psShader->asPhases.size(); ui32Phase++)
    {
        ShaderPhase &phase = psShader->asPhases[ui32Phase];
        phase.UnvectorizeImmMoves();
        psContext->DoDataTypeAnalysis(&phase);
        phase.ResolveUAVProperties(psShader->sInfo);
        ReserveUAVBindingSlots(&phase); // TODO: unify slot allocation code between gl/metal/vulkan
        HLSLcc::DoLoopTransform(psContext, phase);
    }

    psShader->PruneTempRegisters();

    //Special case. Can have multiple phases.
    if (psShader->eShaderType == HULL_SHADER)
    {
        psShader->ConsolidateHullTempVars();

        // Find out if we have a passthrough hull shader
        for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
        {
            if (psShader->asPhases[ui32Phase].ePhase == HS_CTRL_POINT_PHASE)
                hasControlPointPhase = 1;
        }
    }

    // Hull and Domain shaders get merged into vertex shader output
    if (!(psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER))
    {
        if (psContext->flags & HLSLCC_FLAG_DISABLE_FASTMATH)
            bcatcstr(glsl, "#define UNITY_DISABLE_FASTMATH\n");
        bcatcstr(glsl, "#include <metal_stdlib>\n#include <metal_texture>\nusing namespace metal;\n");
        bcatcstr(glsl, "\n#if !(__HAVE_FMA__)\n#define fma(a,b,c) ((a) * (b) + (c))\n#endif\n\n");
    }

    if (psShader->eShaderType == HULL_SHADER)
    {
        psContext->indent++;

        // Phase 1 is always the global decls phase, no instructions
        for (i = 0; i < psShader->asPhases[1].psDecl.size(); ++i)
        {
            TranslateDeclaration(&psShader->asPhases[1].psDecl[i]);
        }

        if (hasControlPointPhase == 0)
        {
            DeclareHullShaderPassthrough();
        }

        for (ui32PhaseCallIndex = 0; ui32PhaseCallIndex < 3; ui32PhaseCallIndex++)
        {
            for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
            {
                ShaderPhase *psPhase = &psShader->asPhases[ui32Phase];
                if (psPhase->ePhase != ePhaseFuncCallOrder[ui32PhaseCallIndex])
                    continue;
                psContext->currentPhase = ui32Phase;

                if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                {
                    // bformata(glsl, "//%s declarations\n", GetPhaseFuncName(psPhase->ePhase));
                }
                for (i = 0; i < psPhase->psDecl.size(); ++i)
                {
                    TranslateDeclaration(&psPhase->psDecl[i]);
                }
            }
        }

        psContext->indent--;

        numPatchesInThreadGroup = maxThreadsPerThreadGroup / std::max(psShader->sInfo.ui32TessInputControlPointCount, psShader->sInfo.ui32TessOutputControlPointCount);
    }
    else
    {
        psContext->indent++;

        for (i = 0; i < psShader->asPhases[0].psDecl.size(); ++i)
            TranslateDeclaration(&psShader->asPhases[0].psDecl[i]);

        psContext->indent--;

        // Output default implementations for framebuffer index remap if needed
        if (m_NeedFBOutputRemapDecl)
            bcatcstr(glsl, "#ifndef XLT_REMAP_O\n\t#define XLT_REMAP_O {0, 1, 2, 3, 4, 5, 6, 7}\n#endif\nconstexpr constant uint xlt_remap_o[] = XLT_REMAP_O;\n");
        if (m_NeedFBInputRemapDecl)
            bcatcstr(glsl, "#ifndef XLT_REMAP_I\n\t#define XLT_REMAP_I {0, 1, 2, 3, 4, 5, 6, 7}\n#endif\nconstexpr constant uint xlt_remap_i[] = XLT_REMAP_I;\n");

        DeclareClipPlanes(&psShader->asPhases[0].psDecl[0], psShader->asPhases[0].psDecl.size());
        GenerateTexturesReflection(&psContext->m_Reflection);
    }

    if (psShader->eShaderType == HULL_SHADER)
    {
        psContext->currentPhase = MAIN_PHASE;

        if (m_StructDefinitions["Mtl_ControlPoint"].m_Members.size() > 0)
        {
            hasControlPoint = true;

            m_StructDefinitions["Mtl_ControlPoint"].m_Dependencies.push_back("Mtl_ControlPoint");
            m_StructDefinitions["Mtl_ControlPointIn"].m_Dependencies.push_back("Mtl_ControlPointIn");
            PrintStructDeclarations(m_StructDefinitions, "Mtl_ControlPoint");
            PrintStructDeclarations(m_StructDefinitions, "Mtl_ControlPointIn");
        }

        if (m_StructDefinitions["Mtl_PatchConstant"].m_Members.size() > 0)
        {
            hasPatchConstant = true;

            m_StructDefinitions["Mtl_PatchConstant"].m_Dependencies.push_back("Mtl_PatchConstant");
            m_StructDefinitions["Mtl_PatchConstantIn"].m_Dependencies.push_back("Mtl_PatchConstantIn");
            PrintStructDeclarations(m_StructDefinitions, "Mtl_PatchConstant");
            PrintStructDeclarations(m_StructDefinitions, "Mtl_PatchConstantIn");
        }

        m_StructDefinitions["Mtl_KernelPatchInfo"].m_Members.push_back(std::make_pair("numPatches", "uint numPatches"));
        m_StructDefinitions["Mtl_KernelPatchInfo"].m_Members.push_back(std::make_pair("numControlPointsPerPatch", "ushort numControlPointsPerPatch"));

        if (m_StructDefinitions["Mtl_KernelPatchInfo"].m_Members.size() > 0)
        {
            m_StructDefinitions["Mtl_KernelPatchInfo"].m_Dependencies.push_back("Mtl_KernelPatchInfo");
            PrintStructDeclarations(m_StructDefinitions, "Mtl_KernelPatchInfo");
        }

        if (m_StructDefinitions[GetInputStructName()].m_Members.size() > 0)
        {
            m_StructDefinitions[GetInputStructName()].m_Dependencies.push_back(GetInputStructName());
            if (psContext->psDependencies)
                psContext->psDependencies->m_SharedDependencies.push_back(GetInputStructName());

            // Hack, we're reusing Mtl_VertexOut as an hull shader input array, so no need to declare original contents
            m_StructDefinitions[GetInputStructName()].m_Members.clear();

            bstring vertexOut = bfromcstr("");
            bformata(vertexOut, "Mtl_VertexOut cp[%d]", psShader->sInfo.ui32TessOutputControlPointCount);
            m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair("cp", (const char *)vertexOut->data));
            bdestroy(vertexOut);
        }

        if (psContext->psDependencies)
        {
            for (auto i = psContext->psDependencies->m_SharedFunctionMembers.begin(), in = psContext->psDependencies->m_SharedFunctionMembers.end(); i != in;)
            {
                tessVertexFunctionArguments += i->first.c_str();
                ++i;

                // we want to avoid trailing comma
                if (i != in)
                    tessVertexFunctionArguments += ", ";
            }
        }
    }

    if (psShader->eShaderType == DOMAIN_SHADER)
    {
        // For preserving data layout, reuse Mtl_ControlPoint/Mtl_PatchConstant from hull shader
        if (hasControlPoint)
            m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair("cp", "patch_control_point<Mtl_ControlPointIn> cp"));
        if (hasPatchConstant)
            m_StructDefinitions[GetInputStructName()].m_Members.push_back(std::make_pair("patch", "Mtl_PatchConstantIn patch"));
    }

    if ((psShader->eShaderType == VERTEX_SHADER || psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER) && (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) != 0)
    {
        if (psContext->psDependencies)
        {
            psContext->psDependencies->m_SharedFunctionMembers = m_StructDefinitions[""].m_Members;
            psContext->psDependencies->m_SharedTextureSlots = m_TextureSlots;
            psContext->psDependencies->m_SharedTextureSlots.SaveTotalShaderStageAllocationsCount();
            psContext->psDependencies->m_SharedSamplerSlots = m_SamplerSlots;
            psContext->psDependencies->m_SharedSamplerSlots.SaveTotalShaderStageAllocationsCount();
            psContext->psDependencies->m_SharedBufferSlots = m_BufferSlots;
            psContext->psDependencies->m_SharedBufferSlots.SaveTotalShaderStageAllocationsCount();
        }
    }

    if (m_StructDefinitions[GetInputStructName()].m_Members.size() > 0)
    {
        if (psShader->eShaderType == HULL_SHADER)
        {
            if (psContext->psDependencies)
            {
                // if we go for fully procedural geometry we might end up without Mtl_VertexIn
                for (std::vector<std::string>::const_iterator itr = psContext->psDependencies->m_SharedDependencies.begin(); itr != psContext->psDependencies->m_SharedDependencies.end(); itr++)
                {
                    if (*itr == "Mtl_VertexIn")
                    {
                        m_StructDefinitions[""].m_Members.push_back(std::make_pair("vertexInput", "Mtl_VertexIn vertexInput [[ stage_in ]]"));
                        if (tessVertexFunctionArguments.length())
                            tessVertexFunctionArguments += ", ";
                        tessVertexFunctionArguments += "vertexInput";
                        break;
                    }
                }
            }

            m_StructDefinitions[""].m_Members.push_back(std::make_pair("tID", "uint2 tID [[ thread_position_in_grid ]]"));
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("groupID", "ushort2 groupID [[ threadgroup_position_in_grid ]]"));

            bstring buffer = bfromcstr("");
            uint32_t slot = 0;

            if (hasControlPoint)
            {
                slot = m_BufferSlots.GetBindingSlot(0xffff - 1, BindingSlotAllocator::ConstantBuffer);
                bformata(buffer, "device Mtl_ControlPoint *controlPoints [[ buffer(%d) ]]", slot);
                m_StructDefinitions[""].m_Members.push_back(std::make_pair("controlPoints", (const char *)buffer->data));
                btrunc(buffer, 0);
            }

            if (hasPatchConstant)
            {
                slot = m_BufferSlots.GetBindingSlot(0xffff - 2, BindingSlotAllocator::ConstantBuffer);
                bformata(buffer, "device Mtl_PatchConstant *patchConstants [[ buffer(%d) ]]", slot);
                m_StructDefinitions[""].m_Members.push_back(std::make_pair("patchConstants", (const char *)buffer->data));
                btrunc(buffer, 0);
            }

            slot = m_BufferSlots.GetBindingSlot(0xffff - 3, BindingSlotAllocator::ConstantBuffer);
            bformata(buffer, "device %s *tessFactors [[ buffer(%d) ]]", psShader->sInfo.eTessDomain == TESSELLATOR_DOMAIN_QUAD ? "MTLQuadTessellationFactorsHalf" : "MTLTriangleTessellationFactorsHalf", slot);
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("tessFactors", (const char *)buffer->data));
            btrunc(buffer, 0);

            slot = m_BufferSlots.GetBindingSlot(0xffff - 4, BindingSlotAllocator::ConstantBuffer);
            bformata(buffer, "constant Mtl_KernelPatchInfo &patchInfo [[ buffer(%d) ]]", slot);
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("patchInfo", (const char *)buffer->data));
            btrunc(buffer, 0);

            bdestroy(buffer);
        }
        else if (psShader->eShaderType == VERTEX_SHADER && (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) != 0)
        {
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("input", GetInputStructName() + " input"));
        }
        else
        {
            m_StructDefinitions[""].m_Members.push_back(std::make_pair("input", GetInputStructName() + " input [[ stage_in ]]"));
        }

        m_StructDefinitions[""].m_Dependencies.push_back(GetInputStructName());
        if (psContext->psDependencies)
            psContext->psDependencies->m_SharedDependencies.push_back(GetInputStructName());
    }

    if ((psShader->eShaderType == VERTEX_SHADER || psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER) && (psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) != 0)
    {
        // m_StructDefinitions is inherited between tessellation shader stages but some builtins need exceptions
        std::for_each(m_StructDefinitions[""].m_Members.begin(), m_StructDefinitions[""].m_Members.end(), [&psShader](MemberDefinitions::value_type &mem)
        {
            if (mem.first == "mtl_InstanceID")
            {
                if (psShader->eShaderType == VERTEX_SHADER)
                    mem.second.assign("uint mtl_InstanceID");
                else if (psShader->eShaderType == HULL_SHADER)
                    mem.second.assign("// mtl_InstanceID passed through groupID");
            }
            else if (mem.first == "mtl_BaseInstance")
            {
                if (psShader->eShaderType == VERTEX_SHADER)
                    mem.second.assign("uint mtl_BaseInstance");
                else if (psShader->eShaderType == HULL_SHADER)
                    mem.second.assign("// mtl_BaseInstance ignored");
            }
            else if (mem.first == "mtl_VertexID")
            {
                if (psShader->eShaderType == VERTEX_SHADER)
                    mem.second.assign("uint mtl_VertexID");
                else if (psShader->eShaderType == HULL_SHADER)
                    mem.second.assign("// mtl_VertexID generated in compute kernel");
                else if (psShader->eShaderType == DOMAIN_SHADER)
                    mem.second.assign("// mtl_VertexID unused");
            }
            else if (mem.first == "mtl_BaseVertex")
            {
                if (psShader->eShaderType == VERTEX_SHADER)
                    mem.second.assign("uint mtl_BaseVertex");
                else if (psShader->eShaderType == HULL_SHADER)
                    mem.second.assign("// mtl_BaseVertex generated in compute kernel");
                else if (psShader->eShaderType == DOMAIN_SHADER)
                    mem.second.assign("// mtl_BaseVertex unused");
            }
        });
    }

    if (psShader->eShaderType != COMPUTE_SHADER)
    {
        if (m_StructDefinitions[GetOutputStructName()].m_Members.size() > 0)
        {
            m_StructDefinitions[""].m_Dependencies.push_back(GetOutputStructName());
            if (psContext->psDependencies)
                psContext->psDependencies->m_SharedDependencies.push_back(GetOutputStructName());
        }
    }

    PrintStructDeclarations(m_StructDefinitions);

    psContext->currentGLSLString = &bodyglsl;

    bool popPragmaDiagnostic = false;
    if (psShader->eShaderType == HULL_SHADER || psShader->eShaderType == DOMAIN_SHADER)
    {
        popPragmaDiagnostic = true;

        bcatcstr(bodyglsl, "#pragma clang diagnostic push\n");
        bcatcstr(bodyglsl, "#pragma clang diagnostic ignored \"-Wunused-parameter\"\n");
    }

    switch (psShader->eShaderType)
    {
        case VERTEX_SHADER:
            if ((psContext->flags & HLSLCC_FLAG_METAL_TESSELLATION) == 0)
                bcatcstr(bodyglsl, "vertex Mtl_VertexOut xlatMtlMain(\n");
            else
                bcatcstr(bodyglsl, "static Mtl_VertexOut vertexFunction(\n");
            break;
        case PIXEL_SHADER:
            if (psShader->sInfo.bEarlyFragmentTests)
                bcatcstr(bodyglsl, "[[early_fragment_tests]]\n");
            if (m_StructDefinitions[GetOutputStructName()].m_Members.size() > 0)
                bcatcstr(bodyglsl, "fragment Mtl_FragmentOut xlatMtlMain(\n");
            else
                bcatcstr(bodyglsl, "fragment void xlatMtlMain(\n");
            break;
        case COMPUTE_SHADER:
            bcatcstr(bodyglsl, "kernel void computeMain(\n");
            break;
        case HULL_SHADER:
            bcatcstr(bodyglsl, "kernel void patchKernel(\n");
            break;
        case DOMAIN_SHADER:
        {
            const char *patchType = psShader->sInfo.eTessDomain == TESSELLATOR_DOMAIN_QUAD ? "quad" : "triangle";
            uint32_t patchCount = psShader->sInfo.ui32TessOutputControlPointCount;
            bformata(bodyglsl, "[[patch(%s, %d)]] vertex Mtl_VertexOutPostTess xlatMtlMain(\n", patchType, patchCount);
            break;
        }
        default:
            // Not supported
            ASSERT(0);
            return false;
    }

    psContext->indent++;
    for (auto itr = m_StructDefinitions[""].m_Members.begin();;)
    {
        if (itr == m_StructDefinitions[""].m_Members.end())
            break;

        psContext->AddIndentation();
        bcatcstr(bodyglsl, itr->second.c_str());

        itr++;
        if (itr != m_StructDefinitions[""].m_Members.end())
            bcatcstr(bodyglsl, ",\n");
    }

    // Figure and declare counters and their binds (we also postponed buffer reflection until now)
    for (auto it = m_BufferReflections.begin(); it != m_BufferReflections.end(); ++it)
    {
        uint32_t bind = it->second.bind;
        if (it->second.hasCounter)
        {
            const uint32_t counterBind = m_BufferSlots.PeekFirstFreeSlot();
            m_BufferSlots.ReserveBindingSlot(counterBind, BindingSlotAllocator::UAV);

            bformata(bodyglsl, ",\n\t\tdevice atomic_uint* %s_counter [[ buffer(%d) ]]", it->first.c_str(), counterBind);

            // Offset with 1 so we can capture counters that are bound to slot 0 (if, say, user decides to start buffers at register 1 or higher)
            bind |= ((counterBind + 1) << 16);
        }
        psContext->m_Reflection.OnBufferBinding(it->first, bind, it->second.isUAV);
    }

    bcatcstr(bodyglsl, ")\n{\n");

    if (popPragmaDiagnostic)
        bcatcstr(bodyglsl, "#pragma clang diagnostic pop\n");

    if (psShader->eShaderType != COMPUTE_SHADER)
    {
        if (psShader->eShaderType == VERTEX_SHADER)
        {
            // Fix HLSL compatibility with DrawProceduralIndirect, SV_InstanceID always starts at 0 but with Metal, a base instance was not subtracted for equal behavior
            // Base semantics available everywhere starting with iOS9 (except hardware limitation exists with the original Apple A7/A8 GPUs, causing UNITY_SUPPORT_INDIRECT_BUFFERS=0)
            std::for_each(m_StructDefinitions[""].m_Members.begin(), m_StructDefinitions[""].m_Members.end(), [&](MemberDefinitions::value_type &mem)
            {
                if (mem.first == "mtl_InstanceID")
                {
                    bcatcstr(bodyglsl, "#if !UNITY_SUPPORT_INDIRECT_BUFFERS\n");
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "mtl_BaseInstance = 0;\n");
                    bcatcstr(bodyglsl, "#endif\n");
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "mtl_InstanceID = mtl_InstanceID - mtl_BaseInstance;\n");
                }
                else if (mem.first == "mtl_VertexID")
                {
                    bcatcstr(bodyglsl, "#if !UNITY_SUPPORT_INDIRECT_BUFFERS\n");
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "mtl_BaseVertex = 0;\n");
                    bcatcstr(bodyglsl, "#endif\n");
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "mtl_VertexID = mtl_VertexID - mtl_BaseVertex;\n");
                }
            });
        }

        if (m_StructDefinitions[GetOutputStructName().c_str()].m_Members.size() > 0)
        {
            psContext->AddIndentation();
            bcatcstr(bodyglsl, GetOutputStructName().c_str());
            bcatcstr(bodyglsl, " output;\n");
        }
    }

    if (psShader->eShaderType == HULL_SHADER)
    {
        if (hasPatchConstant)
        {
            psContext->AddIndentation();
            bcatcstr(bodyglsl, "Mtl_PatchConstant patch;\n");
        }

        psContext->AddIndentation();
        bformata(bodyglsl, "const uint numPatchesInThreadGroup = %d;\n", numPatchesInThreadGroup);  // Hardcoded because of threadgroup array below
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint patchID = (tID.x / patchInfo.numControlPointsPerPatch);\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const bool patchValid = (patchID < patchInfo.numPatches);\n");

        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint mtl_BaseInstance = 0;\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint mtl_InstanceID = groupID.y - mtl_BaseInstance;\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint internalPatchID = mtl_InstanceID * patchInfo.numPatches + patchID;\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint patchIDInThreadGroup = (patchID % numPatchesInThreadGroup);\n");

        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint controlPointID = (tID.x % patchInfo.numControlPointsPerPatch);\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint mtl_BaseVertex = 0;\n");
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "const uint mtl_VertexID = ((mtl_InstanceID * (patchInfo.numControlPointsPerPatch * patchInfo.numPatches)) + tID.x) - mtl_BaseVertex;\n");

        psContext->AddIndentation();
        bformata(bodyglsl, "threadgroup %s inputGroup[numPatchesInThreadGroup];\n", GetInputStructName().c_str());
        psContext->AddIndentation();
        bformata(bodyglsl, "threadgroup %s &input = inputGroup[patchIDInThreadGroup];\n", GetInputStructName().c_str());

        psContext->AddIndentation();
        std::string tessFactorBufferType = psShader->sInfo.eTessDomain == TESSELLATOR_DOMAIN_QUAD ? "MTLQuadTessellationFactorsHalf" : "MTLTriangleTessellationFactorsHalf";
        bformata(bodyglsl, "%s tessFactor;\n", tessFactorBufferType.c_str());
    }

    // There are cases when there are no control point phases and we have to do passthrough
    if (psShader->eShaderType == HULL_SHADER && hasControlPointPhase == 0)
    {
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "if (patchValid) {\n");
        psContext->indent++;

        // Passthrough control point phase, run the rest only once per patch
        psContext->AddIndentation();
        bformata(bodyglsl, "input.cp[controlPointID] = vertexFunction(%s);\n", tessVertexFunctionArguments.c_str());

        DoHullShaderPassthrough(psContext);

        psContext->indent--;
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "}\n");

        psContext->AddIndentation();
        bcatcstr(bodyglsl, "threadgroup_barrier(mem_flags::mem_threadgroup);\n");

        psContext->AddIndentation();
        bcatcstr(bodyglsl, "if (!patchValid) {\n");
        psContext->indent++;
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "return;\n");
        psContext->indent--;
        psContext->AddIndentation();
        bcatcstr(bodyglsl, "}\n");
    }

    if (psShader->eShaderType == HULL_SHADER)
    {
        for (ui32PhaseCallIndex = 0; ui32PhaseCallIndex < 3; ui32PhaseCallIndex++)
        {
            for (ui32Phase = 2; ui32Phase < psShader->asPhases.size(); ui32Phase++)
            {
                uint32_t i;
                ShaderPhase *psPhase = &psShader->asPhases[ui32Phase];
                if (psPhase->ePhase != ePhaseFuncCallOrder[ui32PhaseCallIndex])
                    continue;
                psContext->currentPhase = ui32Phase;

                if (psPhase->earlyMain->slen > 1)
                {
                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "//--- Start Early Main ---\n");
                    }

                    bconcat(bodyglsl, psPhase->earlyMain);

                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "//--- End Early Main ---\n");
                    }
                }

                psContext->AddIndentation();
                bformata(bodyglsl, "// %s%d\n", GetPhaseFuncName(psShader->asPhases[ui32Phase].ePhase), ui32Phase);
                if (psPhase->ui32InstanceCount > 1)
                {
                    psContext->AddIndentation();
                    bformata(bodyglsl, "for (int phaseInstanceID = 0; phaseInstanceID < %d; phaseInstanceID++) {\n", psPhase->ui32InstanceCount);
                    psContext->indent++;
                }
                else
                {
                    if (psContext->currentPhase == HS_CTRL_POINT_PHASE && hasControlPointPhase == 1)
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "if (patchValid) {\n");
                        psContext->indent++;

                        psContext->AddIndentation();
                        bformata(bodyglsl, "input.cp[controlPointID] = vertexFunction(%s);\n", tessVertexFunctionArguments.c_str());
                    }
                    else
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "{\n");
                        psContext->indent++;
                    }
                }

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
                psContext->AddIndentation();
                bformata(bodyglsl, "}\n");

                if (psPhase->hasPostShaderCode)
                {
                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "//--- Post shader code ---\n");
                    }

                    bconcat(bodyglsl, psPhase->postShaderCode);

                    if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
                    {
                        psContext->AddIndentation();
                        bcatcstr(bodyglsl, "//--- End post shader code ---\n");
                    }
                }

                if (psShader->asPhases[ui32Phase].ePhase == HS_CTRL_POINT_PHASE)
                {
                    // We're done printing control point phase, run the rest only once per patch
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "threadgroup_barrier(mem_flags::mem_threadgroup);\n");

                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "if (!patchValid) {\n");
                    psContext->indent++;
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "return;\n");
                    psContext->indent--;
                    psContext->AddIndentation();
                    bcatcstr(bodyglsl, "}\n");
                }
            }
        }

        if (hasControlPoint)
        {
            psContext->AddIndentation();
            bcatcstr(bodyglsl, "controlPoints[mtl_VertexID] = output;\n");
        }

        psContext->AddIndentation();
        bcatcstr(bodyglsl, "tessFactors[internalPatchID] = tessFactor;\n");

        if (hasPatchConstant)
        {
            psContext->AddIndentation();
            bcatcstr(bodyglsl, "patchConstants[internalPatchID] = patch;\n");
        }

        if (psContext->psDependencies)
        {
            //Save partitioning and primitive type for use by domain shader.
            psContext->psDependencies->eTessOutPrim = psShader->sInfo.eTessOutPrim;
            psContext->psDependencies->eTessPartitioning = psShader->sInfo.eTessPartitioning;
            psContext->psDependencies->numPatchesInThreadGroup = numPatchesInThreadGroup;
            psContext->psDependencies->hasControlPoint = hasControlPoint;
            psContext->psDependencies->hasPatchConstant = hasPatchConstant;
        }
    }
    else
    {
        if (psContext->psShader->asPhases[0].earlyMain->slen > 1)
        {
            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(bodyglsl, "//--- Start Early Main ---\n");
            }

            bconcat(bodyglsl, psContext->psShader->asPhases[0].earlyMain);

            if (psContext->flags & HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS)
            {
                psContext->AddIndentation();
                bcatcstr(bodyglsl, "//--- End Early Main ---\n");
            }
        }

        for (i = 0; i < psShader->asPhases[0].psInst.size(); ++i)
        {
            TranslateInstruction(&psShader->asPhases[0].psInst[i]);
        }
    }

    psContext->indent--;

    bcatcstr(bodyglsl, "}\n");

    psContext->currentGLSLString = &glsl;

    if (psShader->eShaderType == HULL_SHADER && psContext->psDependencies)
    {
        psContext->m_Reflection.OnTessellationKernelInfo(psContext->psDependencies->m_SharedBufferSlots.SaveTotalShaderStageAllocationsCount());
    }

    if (psShader->eShaderType == DOMAIN_SHADER && psContext->psDependencies)
    {
        int mtlTessellationPartitionMode = -1;
        int mtlWinding = -1;

        switch (psContext->psDependencies->eTessPartitioning)
        {
            case TESSELLATOR_PARTITIONING_INTEGER:
                mtlTessellationPartitionMode = 1; // MTLTessellationPartitionModeInteger
                break;
            case TESSELLATOR_PARTITIONING_POW2:
                mtlTessellationPartitionMode = 0; // MTLTessellationPartitionModePow2
                break;
            case TESSELLATOR_PARTITIONING_FRACTIONAL_ODD:
                mtlTessellationPartitionMode = 2; // MTLTessellationPartitionModeFractionalOdd
                break;
            case TESSELLATOR_PARTITIONING_FRACTIONAL_EVEN:
                mtlTessellationPartitionMode = 3; // MTLTessellationPartitionModeFractionalEven
                break;
            case TESSELLATOR_PARTITIONING_UNDEFINED:
            default:
                ASSERT(0);
                break;
        }

        switch (psContext->psDependencies->eTessOutPrim)
        {
            case TESSELLATOR_OUTPUT_TRIANGLE_CW:
                mtlWinding = 0; // MTLWindingClockwise
                break;
            case TESSELLATOR_OUTPUT_TRIANGLE_CCW:
                mtlWinding = 1; // MTLWindingCounterClockwise
                break;
            case TESSELLATOR_OUTPUT_POINT:
                psContext->m_Reflection.OnDiagnostics("Metal Tessellation: outputtopology(\"point\") not supported.", 0, true);
                break;
            case TESSELLATOR_OUTPUT_LINE:
                psContext->m_Reflection.OnDiagnostics("Metal Tessellation: outputtopology(\"line\") not supported.", 0, true);
                break;
            case TESSELLATOR_OUTPUT_UNDEFINED:
            default:
                ASSERT(0);
                break;
        }

        psContext->m_Reflection.OnTessellationInfo(mtlTessellationPartitionMode, mtlWinding, (uint32_t)psContext->psDependencies->fMaxTessFactor, psContext->psDependencies->numPatchesInThreadGroup);
    }

    bcatcstr(glsl, m_ExtraGlobalDefinitions.c_str());

    // Print out extra functions we generated
    std::for_each(m_FunctionDefinitions.begin(), m_FunctionDefinitions.end(), [&glsl](const FunctionDefinitions::value_type &p)
    {
        bcatcstr(glsl, p.second.c_str());
        bcatcstr(glsl, "\n");
    });

    // And then the actual function body
    bconcat(glsl, bodyglsl);
    bdestroy(bodyglsl);

    return true;
}

void ToMetal::DeclareExtraFunction(const std::string &name, const std::string &body)
{
    if (m_FunctionDefinitions.find(name) != m_FunctionDefinitions.end())
        return;
    m_FunctionDefinitions.insert(std::make_pair(name, body));
}

std::string ToMetal::GetOutputStructName() const
{
    switch (psContext->psShader->eShaderType)
    {
        case VERTEX_SHADER:
            return "Mtl_VertexOut";
        case PIXEL_SHADER:
            return "Mtl_FragmentOut";
        case HULL_SHADER:
            if (psContext->psShader->asPhases[psContext->currentPhase].ePhase == HS_FORK_PHASE ||
                psContext->psShader->asPhases[psContext->currentPhase].ePhase == HS_JOIN_PHASE)
                return "Mtl_PatchConstant";
            return "Mtl_ControlPoint";
        case DOMAIN_SHADER:
            return "Mtl_VertexOutPostTess";
        default:
            ASSERT(0);
            return "";
    }
}

std::string ToMetal::GetInputStructName() const
{
    switch (psContext->psShader->eShaderType)
    {
        case VERTEX_SHADER:
            return "Mtl_VertexIn";
        case PIXEL_SHADER:
            return "Mtl_FragmentIn";
        case COMPUTE_SHADER:
            return "Mtl_KernelIn";
        case HULL_SHADER:
            return "Mtl_HullIn";
        case DOMAIN_SHADER:
            return "Mtl_VertexInPostTess";
        default:
            ASSERT(0);
            return "";
    }
}

std::string ToMetal::GetCBName(const std::string& cbName) const
{
    std::string output = cbName;
    if (cbName[0] == '$')
    {
        // "$Globals" should have different names in different shaders so that CbKey can discretely identify a CB.
        switch (psContext->psShader->eShaderType)
        {
            case VERTEX_SHADER:
            case HULL_SHADER:
            case DOMAIN_SHADER:
                output[0] = 'V';
                break;
            case PIXEL_SHADER:
                output[0] = 'F';
                break;
            case COMPUTE_SHADER:
                output = cbName.substr(1);
                break;
            default:
                ASSERT(0);
                break;
        }
    }
    return output;
}

void ToMetal::SetIOPrefixes()
{
    switch (psContext->psShader->eShaderType)
    {
        case VERTEX_SHADER:
        case HULL_SHADER:
        case DOMAIN_SHADER:
            psContext->inputPrefix = "input.";
            psContext->outputPrefix = "output.";
            break;

        case PIXEL_SHADER:
            psContext->inputPrefix = "input.";
            psContext->outputPrefix = "output.";
            break;

        case COMPUTE_SHADER:
            psContext->inputPrefix = "";
            psContext->outputPrefix = "";
            break;
        default:
            ASSERT(0);
            break;
    }
}

void ToMetal::ClampPartialPrecisions()
{
    HLSLcc::ForEachOperand(psContext->psShader->asPhases[0].psInst.begin(), psContext->psShader->asPhases[0].psInst.end(), FEO_FLAG_ALL,
        [](std::vector<Instruction>::iterator &i, Operand *o, uint32_t flags)
        {
            if (o->eMinPrecision == OPERAND_MIN_PRECISION_FLOAT_2_8)
                o->eMinPrecision = OPERAND_MIN_PRECISION_FLOAT_16;
        });
}

void ToMetal::ReserveUAVBindingSlots(ShaderPhase *phase)
{
    for (uint32_t p = 0; p < phase->psDecl.size(); ++p)
    {
        uint32_t regNo = phase->psDecl[p].asOperands[0].ui32RegisterNumber;

        if (phase->psDecl[p].eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_RAW ||
            phase->psDecl[p].eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_STRUCTURED)
        {
            m_BufferSlots.ReserveBindingSlot(regNo, BindingSlotAllocator::RWBuffer);
        }
        else if (phase->psDecl[p].eOpcode == OPCODE_DCL_UNORDERED_ACCESS_VIEW_TYPED)
        {
            // Typed buffers are atm faked using structured buffers -> bind in buffer space
            if (phase->psDecl[p].value.eResourceDimension == RESOURCE_DIMENSION_BUFFER)
                m_BufferSlots.ReserveBindingSlot(regNo, BindingSlotAllocator::RWBuffer);
            else
                m_TextureSlots.ReserveBindingSlot(regNo, BindingSlotAllocator::UAV);
        }
    }
}
