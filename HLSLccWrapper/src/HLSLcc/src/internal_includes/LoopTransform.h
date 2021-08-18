#pragma once

class ShaderPhase;
class HLSLCrossCompilerContext;
namespace HLSLcc
{
    void DoLoopTransform(HLSLCrossCompilerContext *psContext, ShaderPhase &phase);
}
