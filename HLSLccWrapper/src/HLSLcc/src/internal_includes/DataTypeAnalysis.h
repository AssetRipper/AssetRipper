#pragma once

#include "include/ShaderInfo.h"
#include <vector>

class HLSLCrossCompilerContext;
struct Instruction;

namespace HLSLcc
{
namespace DataTypeAnalysis
{
    void SetDataTypes(HLSLCrossCompilerContext* psContext, std::vector<Instruction> &instructions, uint32_t ui32TempCount, std::vector<SHADER_VARIABLE_TYPE> &results);
}
}
