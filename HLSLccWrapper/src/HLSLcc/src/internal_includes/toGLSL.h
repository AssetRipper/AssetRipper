#pragma once

#include "hlslcc.h"
#include "internal_includes/Translator.h"

class HLSLCrossCompilerContext;

class ToGLSL : public Translator
{
protected:
    GLLang language;
    bool m_NeedUnityInstancingArraySizeDecl;
    bool m_NeedUnityPreTransformDecl;

public:
    explicit ToGLSL(HLSLCrossCompilerContext* ctx) :
        Translator(ctx),
        language(LANG_DEFAULT),
        m_NeedUnityInstancingArraySizeDecl(false),
        m_NeedUnityPreTransformDecl(false),
        m_NumDeclaredWhileTrueLoops(0)
    {}
    // Sets the target language according to given input. if LANG_DEFAULT, does autodetect and returns the selected language
    GLLang SetLanguage(GLLang suggestedLanguage);

    virtual bool Translate();
    virtual void TranslateDeclaration(const Declaration* psDecl);
    virtual bool TranslateSystemValue(const Operand *psOperand, const ShaderInfo::InOutSignature *sig, std::string &result, uint32_t *pui32IgnoreSwizzle, bool isIndexed, bool isInput, bool *outSkipPrefix = NULL, int *iIgnoreRedirect = NULL);
    virtual void SetIOPrefixes();

private:
    void TranslateOperand(bstring glsl, const Operand *psOp, uint32_t flags, uint32_t ui32ComponentMask = OPERAND_4_COMPONENT_MASK_ALL, bool forceNoConversion = false);
    void TranslateOperand(const Operand *psOp, uint32_t flags, uint32_t ui32ComponentMask = OPERAND_4_COMPONENT_MASK_ALL, bool forceNoConversion = false);
    void TranslateInstruction(Instruction* psInst, bool isEmbedded = false);

    void TranslateVariableNameWithMask(bstring glsl, const Operand* psOperand, uint32_t ui32TOFlag, uint32_t* pui32IgnoreSwizzle, uint32_t ui32CompMask, int *piRebase, bool forceNoConversion = false);
    void TranslateVariableNameWithMask(const Operand* psOperand, uint32_t ui32TOFlag, uint32_t* pui32IgnoreSwizzle, uint32_t ui32CompMask, int *piRebase, bool forceNoConversion = false);

    void TranslateOperandIndex(const Operand* psOperand, int index);
    void TranslateOperandIndexMAD(const Operand* psOperand, int index, uint32_t multiply, uint32_t add);

    void AddOpAssignToDestWithMask(const Operand* psDest,
        SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int *pNeedsParenthesis, uint32_t ui32CompMask);
    void AddAssignToDest(const Operand* psDest,
        SHADER_VARIABLE_TYPE eSrcType, uint32_t ui32SrcElementCount, uint32_t precise, int* pNeedsParenthesis);
    void AddAssignPrologue(int numParenthesis, bool isEmbedded = false);


    void AddBuiltinOutput(const Declaration* psDecl, int arrayElements, const char* builtinName);
    void AddBuiltinInput(const Declaration* psDecl, const char* builtinName);
    void HandleOutputRedirect(const Declaration *psDecl, const char *Precision);
    void HandleInputRedirect(const Declaration *psDecl, const char *Precision);

    void AddUserOutput(const Declaration* psDecl);
    void DeclareStructConstants(const uint32_t ui32BindingPoint, const ConstantBuffer* psCBuf, const Operand* psOperand, bstring glsl);
    void DeclareConstBufferShaderVariable(const char* varName, const struct ShaderVarType* psType, const struct ConstantBuffer* psCBuf, int unsizedArray, bool addUniformPrefix, bool reportInReflection);
    void PreDeclareStructType(const std::string &name, const struct ShaderVarType* psType);
    void DeclareUBOConstants(const uint32_t ui32BindingPoint, const ConstantBuffer* psCBuf, bstring glsl);

    void ReportStruct(const std::string &name, const struct ShaderVarType* psType);

    typedef enum
    {
        CMP_EQ,
        CMP_LT,
        CMP_GE,
        CMP_NE,
    } ComparisonType;

    void AddComparison(Instruction* psInst, ComparisonType eType,
        uint32_t typeFlag);

    void AddMOVBinaryOp(const Operand *pDest, Operand *pSrc, uint32_t precise, bool isEmbedded = false);
    void AddMOVCBinaryOp(const Operand *pDest, const Operand *src0, Operand *src1, Operand *src2, uint32_t precise);
    void CallBinaryOp(const char* name, Instruction* psInst,
        int dest, int src0, int src1, SHADER_VARIABLE_TYPE eDataType, bool isEmbedded = false);
    void CallTernaryOp(const char* op1, const char* op2, Instruction* psInst,
        int dest, int src0, int src1, int src2, uint32_t dataType);
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

    void HandleSwitchTransformation(Instruction* psInst, bstring glsl);

    // Add an extra function to the m_FunctionDefinitions list, unless it's already there.
    bool DeclareExtraFunction(const std::string &name, bstring body);
    void UseExtraFunctionDependency(const std::string &name);

    void DeclareDynamicIndexWrapper(const struct ShaderVarType* psType);
    void DeclareDynamicIndexWrapper(const char* psName, SHADER_VARIABLE_CLASS eClass, SHADER_VARIABLE_TYPE eType, uint32_t ui32Rows, uint32_t ui32Columns, uint32_t ui32Elements);

    bool RenderTargetDeclared(uint32_t input);

    std::string GetVulkanDummySamplerName();

    // A <function name, body text> map of extra helper functions we'll need.
    FunctionDefinitions m_FunctionDefinitions;
    std::vector<std::string> m_FunctionDefinitionsOrder;

    std::vector<std::string> m_AdditionalDefinitions;

    std::vector<std::string> m_DefinedStructs;

    std::set<uint32_t> m_DeclaredRenderTarget;
    int m_NumDeclaredWhileTrueLoops;

    struct SwitchConversion
    {
        /*
         IF (CONDITION1) BREAK; STATEMENT1; IF (CONDITION2) BREAK; STATEMENT2;... transforms to
         if (CONDITION1) {} ELSE { STATEMENT1; IF (CONDITION2) {} ELSE {STATEMENT2; ...} }
         thus, we need to count the "BREAK" statements we encountered in each IF on the same level inside a SWITCH.
         */
        struct ConditionalInfo
        {
            int breakCount;         // Count BREAK on the same level to emit enough closing braces afterwards
            bool breakEncountered;  // Just encountered a BREAK statment, potentially need to emit "ELSE"
            bool endifEncountered;  // We need to check for "ENDIF ELSE" sequence, and not emit "else" if we see it

            ConditionalInfo() :
                ConditionalInfo(0, false)
            {}

            explicit ConditionalInfo(int initialBreakCount) :
                ConditionalInfo(initialBreakCount, false)
            {}

            ConditionalInfo(int initialBreakCount, bool withEndif) :
                ConditionalInfo(initialBreakCount, withEndif, false)
            {}

            ConditionalInfo(int initialBreakCount, bool withEndif, bool withBreak) :
                breakCount(initialBreakCount),
                endifEncountered(withEndif),
                breakEncountered(withBreak)
            {}
        };

        bstring switchOperand;
        // We defer emitting if (condition) for each CASE statement to concatenate possible CASE A: CASE B:... into one if ().
        std::vector<bstring> currentCaseOperands;
        std::vector<ConditionalInfo> conditionalsInfo;
        int isInLoop;       // We don't count "BREAK" (end emit them) if we're in a loop.
        bool isFirstCase;

        SwitchConversion() :
            switchOperand(bfromcstr("")),
            isInLoop(0),
            isFirstCase(true)
        {}

        SwitchConversion(const SwitchConversion& other) :
            switchOperand(bstrcpy(other.switchOperand)),
            conditionalsInfo(other.conditionalsInfo),
            isInLoop(other.isInLoop),
            isFirstCase(other.isFirstCase)
        {
            currentCaseOperands.reserve(other.currentCaseOperands.size());
            for (size_t i = 0; i < other.currentCaseOperands.size(); ++i)
                currentCaseOperands.push_back(bstrcpy(other.currentCaseOperands[i]));
        }

        SwitchConversion(SwitchConversion&& other) :
            switchOperand(other.switchOperand),
            currentCaseOperands(std::move(other.currentCaseOperands)),
            conditionalsInfo(std::move(other.conditionalsInfo)),
            isInLoop(other.isInLoop),
            isFirstCase(other.isFirstCase)
        {
            other.switchOperand = nullptr;
        }

        ~SwitchConversion()
        {
            bdestroy(switchOperand);
            for (size_t i = 0; i < currentCaseOperands.size(); ++i)
                bdestroy(currentCaseOperands[i]);
        }

        SwitchConversion& operator=(const SwitchConversion& other)
        {
            if (this == &other)
                return *this;

            switchOperand = bstrcpy(other.switchOperand);
            conditionalsInfo = other.conditionalsInfo;
            isInLoop = other.isInLoop;
            isFirstCase = other.isFirstCase;
            currentCaseOperands.reserve(other.currentCaseOperands.size());
            for (size_t i = 0; i < other.currentCaseOperands.size(); ++i)
                currentCaseOperands.push_back(bstrcpy(other.currentCaseOperands[i]));

            return *this;
        }

        SwitchConversion& operator=(SwitchConversion&& other)
        {
            if (this == &other)
                return *this;

            switchOperand = other.switchOperand;
            conditionalsInfo = std::move(other.conditionalsInfo);
            isInLoop = other.isInLoop;
            isFirstCase = other.isFirstCase;
            currentCaseOperands = std::move(other.currentCaseOperands);

            other.switchOperand = nullptr;

            return *this;
        }
    };
    std::vector<SwitchConversion> m_SwitchStack;
};
