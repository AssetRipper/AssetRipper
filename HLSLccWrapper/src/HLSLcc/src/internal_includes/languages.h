#ifndef LANGUAGES_H
#define LANGUAGES_H

#include "hlslcc.h"
#include "HLSLCrossCompilerContext.h"
#include "Shader.h"

static int InOutSupported(const GLLang eLang)
{
    if (eLang == LANG_ES_100 || eLang == LANG_120)
    {
        return 0;
    }
    return 1;
}

static int WriteToFragData(const GLLang eLang)
{
    if (eLang == LANG_ES_100 || eLang == LANG_120)
    {
        return 1;
    }
    return 0;
}

static int ShaderBitEncodingSupported(const GLLang eLang)
{
    if (eLang != LANG_ES_300 &&
        eLang != LANG_ES_310 &&
        eLang < LANG_330)
    {
        return 0;
    }
    return 1;
}

static int HaveOverloadedTextureFuncs(const GLLang eLang)
{
    if (eLang == LANG_ES_100 || eLang == LANG_120)
    {
        return 0;
    }
    return 1;
}

static bool IsMobileTarget(const HLSLCrossCompilerContext *psContext)
{
    if ((psContext->flags & HLSLCC_FLAG_MOBILE_TARGET) != 0)
        return true;

    return false;
}

//Only enable for ES. Vulkan and Switch.
//Not present in 120, ignored in other desktop languages. Specifically enabled on Vulkan.
static int HavePrecisionQualifiers(const HLSLCrossCompilerContext *psContext)
{
    if ((psContext->flags & HLSLCC_FLAG_VULKAN_BINDINGS) != 0 || (psContext->flags & HLSLCC_FLAG_NVN_TARGET) != 0)
        return 1;

    const GLLang eLang = psContext->psShader->eTargetLanguage;
    if (eLang >= LANG_ES_100 && eLang <= LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int EmitLowp(const HLSLCrossCompilerContext *psContext)
{
    const GLLang eLang = psContext->psShader->eTargetLanguage;
    return eLang == LANG_ES_100 ? 1 : 0;
}

static int HaveCubemapArray(const GLLang eLang)
{
    if (eLang >= LANG_400 && eLang <= LANG_GL_LAST)
        return 1;
    return 0;
}

static bool IsESLanguage(const GLLang eLang)
{
    return (eLang >= LANG_ES_FIRST && eLang <= LANG_ES_LAST);
}

static bool IsDesktopGLLanguage(const GLLang eLang)
{
    return (eLang >= LANG_GL_FIRST && eLang <= LANG_GL_LAST);
}

//Only on vertex inputs and pixel outputs.
static int HaveLimitedInOutLocationQualifier(const GLLang eLang, const struct GlExtensions *extensions)
{
    if (eLang >= LANG_330 || eLang == LANG_ES_300 || eLang == LANG_ES_310 || (extensions && ((struct GlExtensions*)extensions)->ARB_explicit_attrib_location))
    {
        return 1;
    }
    return 0;
}

static int HaveInOutLocationQualifier(const GLLang eLang)
{
    if (eLang >= LANG_410 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

//layout(binding = X) uniform {uniformA; uniformB;}
//layout(location = X) uniform uniform_name;
static int HaveUniformBindingsAndLocations(const GLLang eLang, const struct GlExtensions *extensions, unsigned int flags)
{
    if (flags & HLSLCC_FLAG_DISABLE_EXPLICIT_LOCATIONS)
        return 0;

    if (eLang >= LANG_430 || eLang == LANG_ES_310 ||
        (extensions && ((struct GlExtensions*)extensions)->ARB_explicit_uniform_location && ((struct GlExtensions*)extensions)->ARB_shading_language_420pack))
    {
        return 1;
    }
    return 0;
}

static int DualSourceBlendSupported(const GLLang eLang)
{
    if (eLang >= LANG_330)
    {
        return 1;
    }
    return 0;
}

static int SubroutinesSupported(const GLLang eLang)
{
    if (eLang >= LANG_400)
    {
        return 1;
    }
    return 0;
}

//Before 430, flat/smooth/centroid/noperspective must match
//between fragment and its previous stage.
//HLSL bytecode only tells us the interpolation in pixel shader.
static int PixelInterpDependency(const GLLang eLang)
{
    if (eLang < LANG_430)
    {
        return 1;
    }
    return 0;
}

static int HaveUnsignedTypes(const GLLang eLang)
{
    switch (eLang)
    {
        case LANG_ES_100:
        case LANG_120:
            return 0;
        default:
            break;
    }
    return 1;
}

static int HaveBitEncodingOps(const GLLang eLang)
{
    switch (eLang)
    {
        case LANG_ES_100:
        case LANG_120:
            return 0;
        default:
            break;
    }
    return 1;
}

static int HaveNativeBitwiseOps(const GLLang eLang)
{
    switch (eLang)
    {
        case LANG_ES_100:
        case LANG_120:
            return 0;
        default:
            break;
    }
    return 1;
}

static int HaveDynamicIndexing(HLSLCrossCompilerContext *psContext, const Operand* psOperand = NULL)
{
    // WebGL only allows dynamic indexing with constant expressions, loop indices or a combination.
    // The only exception is for uniform access in vertex shaders, which can be indexed using any expression.

    switch (psContext->psShader->eTargetLanguage)
    {
        case LANG_ES_100:
        case LANG_120:
            if (psOperand != NULL)
            {
                if (psOperand->m_ForLoopInductorName)
                    return 1;

                if (psContext->psShader->eShaderType == VERTEX_SHADER && psOperand->eType == OPERAND_TYPE_CONSTANT_BUFFER)
                    return 1;
            }

            return 0;
        default:
            break;
    }
    return 1;
}

static int HaveGather(const GLLang eLang)
{
    if (eLang >= LANG_400 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HaveGatherNonConstOffset(const GLLang eLang)
{
    if (eLang >= LANG_420 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HaveQueryLod(const GLLang eLang)
{
    if (eLang >= LANG_400)
    {
        return 1;
    }
    return 0;
}

static int HaveQueryLevels(const GLLang eLang)
{
    if (eLang >= LANG_430)
    {
        return 1;
    }
    return 0;
}

static int HaveFragmentCoordConventions(const GLLang eLang)
{
    if (eLang >= LANG_150)
    {
        return 1;
    }
    return 0;
}

static int HaveGeometryShaderARB(const GLLang eLang)
{
    if (eLang >= LANG_150)
    {
        return 1;
    }
    return 0;
}

static int HaveAtomicCounter(const GLLang eLang)
{
    if (eLang >= LANG_420 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HaveAtomicMem(const GLLang eLang)
{
    if (eLang >= LANG_430 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HaveImageAtomics(const GLLang eLang)
{
    if (eLang >= LANG_420)
    {
        return 1;
    }
    return 0;
}

static int HaveCompute(const GLLang eLang)
{
    if (eLang >= LANG_430 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HaveImageLoadStore(const GLLang eLang)
{
    if (eLang >= LANG_420 || eLang == LANG_ES_310)
    {
        return 1;
    }
    return 0;
}

static int HavePreciseQualifier(const GLLang eLang)
{
    if (eLang >= LANG_400) // TODO: Add for ES when we're adding 3.2 lang
    {
        return 1;
    }
    return 0;
}

#endif
