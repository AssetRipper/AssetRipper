#ifndef TO_GLSL_OPERAND_H
#define TO_GLSL_OPERAND_H

#include <stdint.h>
#include "bstrlib.h"
#include "ShaderInfo.h"

class HLSLCrossCompilerContext;

//void TranslateOperand(HLSLCrossCompilerContext* psContext, const Operand* psOperand, uint32_t ui32TOFlag);
// Translate operand but add additional component mask
//void TranslateOperandWithMask(HLSLCrossCompilerContext* psContext, const Operand* psOperand, uint32_t ui32TOFlag, uint32_t ui32ComponentMask);

void TranslateOperandSwizzle(HLSLCrossCompilerContext* psContext, const Operand* psOperand, int iRebase);
void TranslateOperandSwizzleWithMask(bstring glsl, HLSLCrossCompilerContext* psContext, const Operand* psOperand, uint32_t ui32ComponentMask, int iRebase);
void TranslateOperandSwizzleWithMask(HLSLCrossCompilerContext* psContext, const Operand* psOperand, uint32_t ui32ComponentMask, int iRebase);

void ResourceName(bstring targetStr, HLSLCrossCompilerContext* psContext, ResourceGroup group, const uint32_t ui32RegisterNumber, const int bZCompare);
std::string ResourceName(HLSLCrossCompilerContext* psContext, ResourceGroup group, const uint32_t ui32RegisterNumber, const int bZCompare);

std::string TextureSamplerName(ShaderInfo* psShaderInfo, const uint32_t ui32TextureRegisterNumber, const uint32_t ui32SamplerRegisterNumber, const int bZCompare);
void ConcatTextureSamplerName(bstring str, ShaderInfo* psShaderInfo, const uint32_t ui32TextureRegisterNumber, const uint32_t ui32SamplerRegisterNumber, const int bZCompare);

std::string UniformBufferInstanceName(HLSLCrossCompilerContext* psContext, const std::string& name);

#endif
