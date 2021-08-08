#pragma once

// In Unity, instancing array sizes should be able to be dynamically patched at runtime by defining the macro.

#include <string>
#define UNITY_RUNTIME_INSTANCING_ARRAY_SIZE_MACRO "UNITY_RUNTIME_INSTANCING_ARRAY_SIZE"
#define UNITY_PRETRANSFORM_CONSTANT_NAME "UnityDisplayOrientationPreTransform"

const unsigned int kArraySizeConstantID = 0;
const unsigned int kPreTransformConstantID = 1;

// TODO: share with Runtime/GfxDevice/InstancingUtilities.h
inline bool IsUnityInstancingConstantBufferName(const char* cbName)
{
    static const char kInstancedCbNamePrefix[] = "UnityInstancing";
    return strncmp(cbName, kInstancedCbNamePrefix, sizeof(kInstancedCbNamePrefix) - 1) == 0;
}

inline bool IsPreTransformConstantBufferName(const char* cbName)
{
    static const char kPreTransformCbNamePrefix[] = "UnityDisplayOrientationPreTransformData";
    return strncmp(cbName, kPreTransformCbNamePrefix, sizeof(kPreTransformCbNamePrefix) - 1) == 0;
}
