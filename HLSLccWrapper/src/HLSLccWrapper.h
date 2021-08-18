#pragma once
#include <vector>
#include "hlslcc.h"

//Needs to be wrapped because the original is a bitfield
typedef struct WrappedGlExtensions
{
    uint32_t ARB_explicit_attrib_location;
    uint32_t ARB_explicit_uniform_location;
    uint32_t ARB_shading_language_420pack;
    uint32_t OVR_multiview;
    uint32_t EXT_shader_framebuffer_fetch;
} WrappedGlExtensions;

#ifdef __cplusplus
extern "C" {
#endif
#ifdef _WIN32
#  ifdef MODULE_API_EXPORTS
#    define MODULE_API __declspec(dllexport)
#  else
#    define MODULE_API __declspec(dllimport)
#  endif
#else
#  define MODULE_API
#endif

MODULE_API const char* ShaderTranslateFromFile(const char* filepath, GLLang language, WrappedGlExtensions extensions);
MODULE_API const char* ShaderTranslateFromMem(unsigned char data[], GLLang lang, WrappedGlExtensions extensions);

#ifdef __cplusplus
}
#endif