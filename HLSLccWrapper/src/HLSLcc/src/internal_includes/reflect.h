#ifndef REFLECT_H
#define REFLECT_H

#include "hlslcc.h"

struct ShaderPhase_TAG;

typedef struct
{
    uint32_t* pui32Inputs;
    uint32_t* pui32Outputs;
    uint32_t* pui32Resources;
    uint32_t* pui32Interfaces;
    uint32_t* pui32Inputs11;
    uint32_t* pui32Outputs11;
    uint32_t* pui32OutputsWithStreams;
    uint32_t* pui32PatchConstants;
    uint32_t* pui32PatchConstants11;
} ReflectionChunks;

void LoadShaderInfo(const uint32_t ui32MajorVersion,
    const uint32_t ui32MinorVersion,
    const ReflectionChunks* psChunks,
    ShaderInfo* psInfo, uint32_t decodeFlags);

#endif
