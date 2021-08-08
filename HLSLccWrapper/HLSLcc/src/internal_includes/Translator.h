#pragma once
#include "HLSLCrossCompilerContext.h"
#include "Shader.h"

struct Declaration;
// Base class for translator backend implenentations.
class Translator
{
protected:
    HLSLCrossCompilerContext *psContext;
public:
    explicit Translator(HLSLCrossCompilerContext *ctx) : psContext(ctx) {}
    virtual ~Translator() {}

    virtual bool Translate() = 0;

    virtual void TranslateDeclaration(const Declaration *psDecl) = 0;

    // Translate system value type to name, return true if succeeded and no further translation is necessary
    virtual bool TranslateSystemValue(const Operand *psOperand, const ShaderInfo::InOutSignature *sig, std::string &result, uint32_t *pui32IgnoreSwizzle, bool isIndexed, bool isInput, bool *outSkipPrefix = NULL, int *iIgnoreRedirect = NULL) = 0;

    // In GLSL, the input and output names cannot clash.
    // Also, the output name of previous stage must match the input name of the next stage.
    // So, do gymnastics depending on which shader we're running on and which other shaders exist in this program.
    //
    virtual void SetIOPrefixes() = 0;

    void SetExtensions(const struct GlExtensions *ext)
    {
        psContext->psShader->extensions = ext;
    }
};
