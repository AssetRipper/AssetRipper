#include "hlslcc.h"

#include <memory>
#include <sstream>
#include "internal_includes/HLSLCrossCompilerContext.h"
#include "internal_includes/toGLSL.h"
#include "internal_includes/toMetal.h"
#include "internal_includes/Shader.h"
#include "internal_includes/decode.h"


#ifndef GL_VERTEX_SHADER_ARB
#define GL_VERTEX_SHADER_ARB              0x8B31
#endif
#ifndef GL_FRAGMENT_SHADER_ARB
#define GL_FRAGMENT_SHADER_ARB            0x8B30
#endif
#ifndef GL_GEOMETRY_SHADER
#define GL_GEOMETRY_SHADER 0x8DD9
#endif
#ifndef GL_TESS_EVALUATION_SHADER
#define GL_TESS_EVALUATION_SHADER 0x8E87
#endif
#ifndef GL_TESS_CONTROL_SHADER
#define GL_TESS_CONTROL_SHADER 0x8E88
#endif
#ifndef GL_COMPUTE_SHADER
#define GL_COMPUTE_SHADER 0x91B9
#endif

static bool CheckConstantBuffersNoDuplicateNames(const std::vector<ConstantBuffer>& buffers, HLSLccReflection& reflectionCallbacks)
{
    uint32_t count = buffers.size();
    for (uint32_t i = 0; i < count; ++i)
    {
        const ConstantBuffer& lhs = buffers[i];
        for (uint32_t j = i + 1; j < count; ++j)
        {
            const ConstantBuffer& rhs = buffers[j];
            if (lhs.name == rhs.name)
            {
                std::ostringstream oss;
                oss << "Duplicate constant buffer declaration: " << lhs.name;
                reflectionCallbacks.OnDiagnostics(oss.str(), 0, true);
                return false;
            }
        }
    }

    return true;
}

HLSLCC_API int HLSLCC_APIENTRY TranslateHLSLFromMem(const char* shader,
    unsigned int flags,
    GLLang language,
    const GlExtensions *extensions,
    GLSLCrossDependencyData* dependencies,
    HLSLccSamplerPrecisionInfo& samplerPrecisions,
    HLSLccReflection& reflectionCallbacks,
    GLSLShader* result)
{
    uint32_t* tokens;
    char* glslcstr = NULL;
    int GLSLShaderType = GL_FRAGMENT_SHADER_ARB;
    int success = 0;
    uint32_t i;

    tokens = (uint32_t*)shader;

    std::auto_ptr<Shader> psShader(DecodeDXBC(tokens, flags));

    if (psShader.get())
    {
        Shader* shader = psShader.get();
        if (!CheckConstantBuffersNoDuplicateNames(shader->sInfo.psConstantBuffers, reflectionCallbacks))
            return 0;

        HLSLCrossCompilerContext sContext(reflectionCallbacks);

        // Add shader precisions from the list
        psShader->sInfo.AddSamplerPrecisions(samplerPrecisions);

        if (psShader->ui32MajorVersion <= 3)
        {
            flags &= ~HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS;
        }

#ifdef _DEBUG
        flags |= HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS;
#endif

        sContext.psShader = shader;
        sContext.flags = flags;

        // If dependencies == NULL, we'll create a dummy object for it so that there's always something there.
        std::auto_ptr<GLSLCrossDependencyData> depPtr(NULL);
        if (dependencies == NULL)
        {
            depPtr.reset(new GLSLCrossDependencyData());
            sContext.psDependencies = depPtr.get();
            sContext.psDependencies->SetupGLSLResourceBindingSlotsIndices();
        }
        else
            sContext.psDependencies = dependencies;

        for (i = 0; i < psShader->asPhases.size(); ++i)
        {
            psShader->asPhases[i].hasPostShaderCode = 0;
        }

        if (language == LANG_METAL)
        {
            // Geometry shader is not supported
            if (psShader->eShaderType == GEOMETRY_SHADER)
            {
                result->sourceCode = "";
                return 0;
            }
            ToMetal translator(&sContext);
            if (!translator.Translate())
            {
                bdestroy(sContext.glsl);
                for (i = 0; i < psShader->asPhases.size(); ++i)
                {
                    bdestroy(psShader->asPhases[i].postShaderCode);
                    bdestroy(psShader->asPhases[i].earlyMain);
                }

                return 0;
            }
        }
        else
        {
            ToGLSL translator(&sContext);
            language = translator.SetLanguage(language);
            translator.SetExtensions(extensions);
            if (!translator.Translate())
            {
                bdestroy(sContext.glsl);
                for (i = 0; i < psShader->asPhases.size(); ++i)
                {
                    bdestroy(psShader->asPhases[i].postShaderCode);
                    bdestroy(psShader->asPhases[i].earlyMain);
                }

                return 0;
            }
        }

        switch (psShader->eShaderType)
        {
            case VERTEX_SHADER:
            {
                GLSLShaderType = GL_VERTEX_SHADER_ARB;
                break;
            }
            case GEOMETRY_SHADER:
            {
                GLSLShaderType = GL_GEOMETRY_SHADER;
                break;
            }
            case DOMAIN_SHADER:
            {
                GLSLShaderType = GL_TESS_EVALUATION_SHADER;
                break;
            }
            case HULL_SHADER:
            {
                GLSLShaderType = GL_TESS_CONTROL_SHADER;
                break;
            }
            case COMPUTE_SHADER:
            {
                GLSLShaderType = GL_COMPUTE_SHADER;
                break;
            }
            default:
            {
                break;
            }
        }

        glslcstr = bstr2cstr(sContext.glsl, '\0');
        result->sourceCode = glslcstr;
        bcstrfree(glslcstr);

        bdestroy(sContext.glsl);
        for (i = 0; i < psShader->asPhases.size(); ++i)
        {
            bdestroy(psShader->asPhases[i].postShaderCode);
            bdestroy(psShader->asPhases[i].earlyMain);
        }

        result->reflection = psShader->sInfo;

        result->textureSamplers = psShader->textureSamplers;

        success = 1;
    }

    shader = 0;
    tokens = 0;

    /* Fill in the result struct */

    result->shaderType = GLSLShaderType;
    result->GLSLLanguage = language;

    return success;
}

HLSLCC_API int HLSLCC_APIENTRY TranslateHLSLFromFile(const char* filename,
    unsigned int flags,
    GLLang language,
    const GlExtensions *extensions,
    GLSLCrossDependencyData* dependencies,
    HLSLccSamplerPrecisionInfo& samplerPrecisions,
    HLSLccReflection& reflectionCallbacks,
    GLSLShader* result)
{
    FILE* shaderFile;
    int length;
    size_t readLength;
    std::vector<char> shader;
    int success = 0;

    shaderFile = fopen(filename, "rb");

    if (!shaderFile)
    {
        return 0;
    }

    fseek(shaderFile, 0, SEEK_END);
    length = ftell(shaderFile);
    fseek(shaderFile, 0, SEEK_SET);

    shader.resize(length + 1);

    readLength = fread(&shader[0], 1, length, shaderFile);

    fclose(shaderFile);
    shaderFile = 0;

    shader[readLength] = '\0';

    success = TranslateHLSLFromMem(&shader[0], flags, language, extensions, dependencies, samplerPrecisions, reflectionCallbacks, result);

    return success;
}
