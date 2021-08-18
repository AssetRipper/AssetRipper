#pragma once

#include "internal_includes/tokens.h"
#include <vector>
#include <memory>

enum { MAX_SUB_OPERANDS = 3 };
class Operand;
class HLSLCrossCompilerContext;
struct Instruction;

#if _MSC_VER
// We want to disable the "array will be default-initialized" warning, as that's exactly what we want
#pragma warning(disable: 4351)
#endif

class Operand
{
public:
    typedef std::shared_ptr<Operand> SubOperandPtr;

    Operand()
        :
        iExtended(),
        eType(),
        eModifier(),
        eMinPrecision(),
        iIndexDims(),
        iWriteMask(),
        iGSInput(),
        iPSInOut(),
        iWriteMaskEnabled(),
        iArrayElements(),
        iNumComponents(),
        eSelMode(),
        ui32CompMask(),
        ui32Swizzle(),
        aui32Swizzle(),
        aui32ArraySizes(),
        ui32RegisterNumber(),
        afImmediates(),
        adImmediates(),
        eSpecialName(),
        specialName(),
        eIndexRep(),
        m_SubOperands(),
        aeDataType(),
        m_Rebase(0),
        m_Size(0),
        m_Defines(),
        m_ForLoopInductorName(0)
#ifdef _DEBUG
        , id(0)
#endif
    {}

    // Retrieve the mask of all the components this operand accesses (either reads from or writes to).
    // Note that destination writemask does affect the effective access mask.
    uint32_t GetAccessMask() const;

    // Returns the index of the highest accessed component, based on component mask
    int GetMaxComponent() const;

    bool IsSwizzleReplicated() const;

    // Get the number of elements returned by operand, taking additional component mask into account
    //e.g.
    //.z = 1
    //.x = 1
    //.yw = 2
    uint32_t GetNumSwizzleElements(uint32_t ui32CompMask = OPERAND_4_COMPONENT_MASK_ALL) const;

    // When this operand is used as an input declaration, how many components does it have?
    int GetNumInputElements(const HLSLCrossCompilerContext *psContext) const;

    // Retrieve the operand data type.
    SHADER_VARIABLE_TYPE GetDataType(HLSLCrossCompilerContext* psContext, SHADER_VARIABLE_TYPE ePreferredTypeForImmediates = SVT_INT) const;

    // Returns 0 if the register used by the operand is per-vertex, or 1 if per-patch
    int GetRegisterSpace(const HLSLCrossCompilerContext *psContext) const;
    // Same as above but with explicit shader type and phase
    int GetRegisterSpace(SHADER_TYPE eShaderType, SHADER_PHASE_TYPE eShaderPhaseType) const;

    // Find the operand that contains the dynamic index for this operand (array in constant buffer).
    // When isAoS is true, we'll try to find the original index var to avoid additional calculations.
    // needsIndexCalcRevert output will tell if we need to divide the value to get the correct index.
    Operand* GetDynamicIndexOperand(HLSLCrossCompilerContext *psContext, const ShaderVarType* psVar, bool isAoS, bool *needsIndexCalcRevert) const;

    // Maps REFLECT_RESOURCE_PRECISION into OPERAND_MIN_PRECISION as much as possible
    static OPERAND_MIN_PRECISION ResourcePrecisionToOperandPrecision(REFLECT_RESOURCE_PRECISION ePrec);

    int iExtended;
    OPERAND_TYPE eType;
    OPERAND_MODIFIER eModifier;
    OPERAND_MIN_PRECISION eMinPrecision;
    int iIndexDims;
    int iWriteMask;
    int iGSInput;
    int iPSInOut;
    int iWriteMaskEnabled;
    int iArrayElements;
    int iNumComponents;

    OPERAND_4_COMPONENT_SELECTION_MODE eSelMode;
    uint32_t ui32CompMask;
    uint32_t ui32Swizzle;
    uint32_t aui32Swizzle[4];

    uint32_t aui32ArraySizes[3];
    uint32_t ui32RegisterNumber;
    //If eType is OPERAND_TYPE_IMMEDIATE32
    float afImmediates[4];
    //If eType is OPERAND_TYPE_IMMEDIATE64
    double adImmediates[4];

    SPECIAL_NAME eSpecialName;
    std::string specialName;

    OPERAND_INDEX_REPRESENTATION eIndexRep[3];

    SubOperandPtr m_SubOperands[MAX_SUB_OPERANDS];

    //One type for each component.
    SHADER_VARIABLE_TYPE aeDataType[4];

    uint32_t m_Rebase; // Rebase value, for constant array accesses.
    uint32_t m_Size; // Component count, only for constant array access.

    struct Define
    {
        Define() : m_Inst(0), m_Op(0) {}
        Define(const Define& a) = default;
        Define(Define&& a) = default;
        Define(Instruction* inst, Operand* op) : m_Inst(inst), m_Op(op) {}
        ~Define() = default;

        Define& operator=(const Define& other) = default;
        Define& operator=(Define&& other) = default;

        Instruction* m_Inst; // Instruction that writes to the temp
        Operand*     m_Op;   // The (destination) operand within that instruction.
    };

    std::vector<Define> m_Defines; // Array of instructions whose results this operand can use. (only if eType == OPERAND_TYPE_TEMP)
    uint32_t m_ForLoopInductorName; // If non-zero, this (eType==OPERAND_TYPE_TEMP) is an inductor variable used in for loop, and it has a special number as given here (overrides ui32RegisterNumber)

#ifdef _DEBUG
    uint64_t id;
#endif
};
