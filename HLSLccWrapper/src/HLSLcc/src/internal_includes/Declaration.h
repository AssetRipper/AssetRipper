#pragma once

#include <vector>
#include <set>
#include "internal_includes/tokens.h"
#include "internal_includes/Operand.h"

typedef struct ICBVec4_TAG
{
    uint32_t a;
    uint32_t b;
    uint32_t c;
    uint32_t d;
} ICBVec4;

#define ACCESS_FLAG_READ       0x1
#define ACCESS_FLAG_WRITE      0x2
#define ACCESS_FLAG_ATOMIC     0x4

struct Declaration
{
    Declaration() :
        eOpcode(OPCODE_INVALID),
        ui32NumOperands(0),
        ui32BufferStride(0),
        ui32TableLength(0),
        ui32IsShadowTex(0)
    {}

    OPCODE_TYPE eOpcode;

    uint32_t ui32NumOperands;

    Operand asOperands[2];

    std::vector<ICBVec4> asImmediateConstBuffer;
    //The declaration can set one of these
    //values depending on the opcode.
    union
    {
        uint32_t ui32GlobalFlags;
        uint32_t ui32NumTemps;
        RESOURCE_DIMENSION eResourceDimension;
        INTERPOLATION_MODE eInterpolation;
        PRIMITIVE_TOPOLOGY eOutputPrimitiveTopology;
        PRIMITIVE eInputPrimitive;
        uint32_t ui32MaxOutputVertexCount;
        TESSELLATOR_DOMAIN eTessDomain;
        TESSELLATOR_PARTITIONING eTessPartitioning;
        TESSELLATOR_OUTPUT_PRIMITIVE eTessOutPrim;
        uint32_t aui32WorkGroupSize[3];
        uint32_t ui32HullPhaseInstanceCount;
        float fMaxTessFactor;
        uint32_t ui32IndexRange;
        uint32_t ui32GSInstanceCount;
        SB_SAMPLER_MODE eSamplerMode; // For sampler declarations, the sampler mode.

        struct Interface_TAG
        {
            uint32_t ui32InterfaceID;
            uint32_t ui32NumFuncTables;
            uint32_t ui32ArraySize;
        } iface;
    } value;

    uint32_t ui32BufferStride;

    struct UAV_TAG
    {
        UAV_TAG() :
            ui32GloballyCoherentAccess(0),
            bCounter(0),
            Type(RETURN_TYPE_UNORM),
            ui32NumComponents(0),
            ui32AccessFlags(0)
        {
        }

        uint32_t ui32GloballyCoherentAccess;
        uint8_t bCounter;
        RESOURCE_RETURN_TYPE Type;
        uint32_t ui32NumComponents;
        uint32_t ui32AccessFlags;
    } sUAV;

    struct TGSM_TAG
    {
        uint32_t ui32Stride;
        uint32_t ui32Count;

        TGSM_TAG() :
            ui32Stride(0),
            ui32Count(0)
        {
        }
    } sTGSM;

    struct IndexableTemp_TAG
    {
        uint32_t ui32RegIndex;
        uint32_t ui32RegCount;
        uint32_t ui32RegComponentSize;

        IndexableTemp_TAG() :
            ui32RegIndex(0),
            ui32RegCount(0),
            ui32RegComponentSize(0)
        {
        }
    } sIdxTemp;

    uint32_t ui32TableLength;

    uint32_t ui32IsShadowTex;

    // Set indexed by sampler register number.
    std::set<uint32_t> samplersUsed;
};
