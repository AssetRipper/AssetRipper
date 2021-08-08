#ifndef DECODE_H
#define DECODE_H

#include "internal_includes/Shader.h"

Shader* DecodeDXBC(uint32_t* data, uint32_t decodeFlags);

void UpdateOperandReferences(Shader* psShader, SHADER_PHASE_TYPE eShaderPhaseType, Instruction* psInst);

#endif
