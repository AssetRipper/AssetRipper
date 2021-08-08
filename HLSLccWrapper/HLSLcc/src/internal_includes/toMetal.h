#pragma once
#include "internal_includes/Translator.h"
#include <map>
#include <vector>

struct SamplerDesc
{
    std::string name;
    uint32_t reg, slot;
};
struct TextureSamplerDesc
{
    std::string name;
    int textureBind, samplerBind;
    HLSLCC_TEX_DIMENSION dim;
    bool isMultisampled;
    bool isDepthSampler;
    bool uav;
};

class ToMetal : public Translator
{
public:
    explicit ToMetal(HLSLCrossCompilerContext *ctx)
        : Translator(ctx)
        , m_ShadowSamplerDeclared(false)
        , m_NeedFBOutputRemapDecl(false)
        , m_NeedFBInputRemapDecl(false)
    {}

    virtual bool Translate();
    virtual void TranslateDeclaration(const Declaration *psDecl);
    virtual bool TranslateSystemValue(const Operand *psOperand, const ShaderInfo::InOutSignature *sig, std::string &result, uint32_t *pui32IgnoreSwizzle, bool isIndexed, bool isInput, bool *outSkipPrefix = NULL, int *iIgnoreRedirect = NULL);
    std::string TranslateOperand(const Operand *psOp, uint32_t flags, uint32_t ui32ComponentMask = OPERAND_4_COMPONENT_MASK_ALL);

    virtual void SetIOPrefixes();

private:
    void TranslateInstruction(Instruction* psInst);

    void DeclareBuiltinInput(const Declaration *psDecl);
    void DeclareBuiltinOutput(const Declaration *psDecl);
    void DeclareClipPlanes(const Declaration* decl, unsigned declCount);
    void GenerateTexturesReflection(HLSLccReflection* refl);

    // Retrieve the name of the output struct for this shader
    std::string GetOutputStructName() const;
    std::string GetInputStructName() const;
    std::string GetCBName(const std::string& cbName) const;

    void DeclareHullShaderPassthrough();
    void HandleInputRedirect(const Declaration *psDecl, const std::string &typeName);
    void HandleOutputRedirect(const Declaration *psDecl, const std::string &typeName);

    void DeclareConstantBuffer(const ConstantBuffer *psCBuf, uint32_t ui32BindingPoint);
    void DeclareStructType(const std::string &name, const std::vector<ShaderVar> &contents, bool withinCB = false, uint32_t cumulativeOffset = 0, bool stripUnused = false);
    void DeclareStructType(const std::string &name, const std::vector<ShaderVarType> &contents, bool withinCB = false, uint32_t cumulativeOffset = 0);
    void DeclareStructVariable(const std::string &parentName, const ShaderVar &var, bool withinCB = false, uint32_t cumulativeOffset = 0, bool isUsed = true);
    void DeclareStructVariable(const std::string &parentName, const ShaderVarType &var, bool withinCB = false, uint32_t cumulativeOffset = 0, bool isUsed = true);
    void DeclareBufferVariable(const Declaration *psDecl, bool isRaw, bool isUAV);

    void DeclareResource(const Declaration *psDecl);
    void TranslateResourceTexture(const Declaration* psDecl, uint32_t samplerCanDoShadowCmp, HLSLCC_TEX_DIMENSION texDim);

    void DeclareOutput(const Declaration *decl);

    void PrintStructDeclarations(StructDefinitions &defs, const char *name = "");

    std::string ResourceName(ResourceGroup group, const uint32_t ui32RegisterNumber);

    // ToMetalOperand.cpp
    std::string TranslateOperandSwizzle(const Operand* psOperand, uint32_t ui32ComponentMask, int iRebase, bool includeDot = true);
    std::string TranslateOperandIndex(const Operand* psOperand, int index);
    std::string TranslateVariableName(const Operand* psOperand, uint32_t ui32TOFlag, uint32_t* pui32IgnoreSwizzle, uint32_t ui32CompMask, int *piRebase);

    // ToMetalInstruction.cpp

    void AddOpAssignToDestWithMask(const Operand* psDest,
        SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int& numParenthesis, uint32_t ui32CompMask);
    void AddAssignToDest(const Operand* psDest,
        SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int& numParenthesis);
    void AddAssignPrologue(int numParenthesis);

    typedef enum
    {
        CMP_EQ,
        CMP_LT,
        CMP_GE,
        CMP_NE,
    } ComparisonType;

    void AddComparison(Instruction* psInst, ComparisonType eType,
        uint32_t typeFlag);

    bool CanForceToHalfOperand(const Operand *psOperand);

    void AddMOVBinaryOp(const Operand *pDest, Operand *pSrc, uint32_t precise);
    void AddMOVCBinaryOp(const Operand *pDest, const Operand *src0, Operand *src1, Operand *src2, uint32_t precise);
    void CallBinaryOp(const char* name, Instruction* psInst,
        int dest, int src0, int src1, SHADER_VARIABLE_TYPE eDataType);
    void CallTernaryOp(const char* op1, const char* op2, Instruction* psInst,
        int dest, int src0, int src1, int src2, uint32_t dataType);
    void CallHelper3(const char* name, Instruction* psInst,
        int dest, int src0, int src1, int src2, int paramsShouldFollowWriteMask, uint32_t ui32Flags);
    void CallHelper3(const char* name, Instruction* psInst,
        int dest, int src0, int src1, int src2, int paramsShouldFollowWriteMask);
    void CallHelper2(const char* name, Instruction* psInst,
        int dest, int src0, int src1, int paramsShouldFollowWriteMask);
    void CallHelper2Int(const char* name, Instruction* psInst,
        int dest, int src0, int src1, int paramsShouldFollowWriteMask);
    void CallHelper2UInt(const char* name, Instruction* psInst,
        int dest, int src0, int src1, int paramsShouldFollowWriteMask);
    void CallHelper1(const char* name, Instruction* psInst,
        int dest, int src0, int paramsShouldFollowWriteMask);
    void CallHelper1Int(
        const char* name,
        Instruction* psInst,
        const int dest,
        const int src0,
        int paramsShouldFollowWriteMask);
    void TranslateTexelFetch(
        Instruction* psInst,
        const ResourceBinding* psBinding,
        bstring glsl);
    void TranslateTexelFetchOffset(
        Instruction* psInst,
        const ResourceBinding* psBinding,
        bstring glsl);
    void TranslateTexCoord(
        const RESOURCE_DIMENSION eResDim,
        Operand* psTexCoordOperand);
    void GetResInfoData(Instruction* psInst, int index, int destElem);
    void TranslateTextureSample(Instruction* psInst,
        uint32_t ui32Flags);
    void TranslateDynamicComponentSelection(const ShaderVarType* psVarType,
        const Operand* psByteAddr, uint32_t offset, uint32_t mask);
    void TranslateShaderStorageStore(Instruction* psInst);
    void TranslateShaderStorageLoad(Instruction* psInst);
    void TranslateAtomicMemOp(Instruction* psInst);
    void TranslateConditional(
        Instruction* psInst,
        bstring glsl);

    // The map is keyed by struct name. The special name "" (empty string) is reserved for entry point function parameters
    StructDefinitions m_StructDefinitions;

    // A <function name, body text> map of extra helper functions we'll need.
    FunctionDefinitions m_FunctionDefinitions;

    BindingSlotAllocator m_TextureSlots, m_SamplerSlots;
    BindingSlotAllocator m_BufferSlots;

    struct BufferReflection
    {
        uint32_t bind;
        bool isUAV;
        bool hasCounter;
    };
    std::map<std::string, BufferReflection> m_BufferReflections;

    std::vector<SamplerDesc> m_Samplers;
    std::vector<TextureSamplerDesc> m_Textures;

    std::string m_ExtraGlobalDefinitions;

    // Flags for whether we need to add the declaration for the FB IO remaps
    bool m_NeedFBInputRemapDecl;
    bool m_NeedFBOutputRemapDecl;

    bool m_ShadowSamplerDeclared;

    void EnsureShadowSamplerDeclared();

    // Add an extra function to the m_FunctionDefinitions list, unless it's already there.
    void DeclareExtraFunction(const std::string &name, const std::string &body);

    // Move all lowp -> mediump
    void ClampPartialPrecisions();

    // Reseve UAV slots in advance to match the original HLSL bindings -> correct bindings in SetRandomWriteTarget()
    void ReserveUAVBindingSlots(ShaderPhase *phase);
};
