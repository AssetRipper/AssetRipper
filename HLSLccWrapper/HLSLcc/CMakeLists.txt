cmake_minimum_required(VERSION 3.16)
project(HLSLcc)

set(CMAKE_CXX_STANDARD 14)

include_directories(.)
include_directories(include)
include_directories(src/)
include_directories(src/cbstring)
include_directories(src/internal_includes)

add_library(HLSLcc SHARED
        include/growing_array.h
        include/hlslcc.h
        include/hlslcc.hpp
        include/pstdint.h
        include/ShaderInfo.h
        include/UnityInstancingFlexibleArraySize.h
        src/cbstring/bsafe.c
        src/cbstring/bsafe.h
        src/cbstring/bstraux.c
        src/cbstring/bstraux.h
        src/cbstring/bstrlib.c
        src/cbstring/bstrlib.h
        src/internal_includes/ControlFlowGraph.h
        src/internal_includes/ControlFlowGraphUtils.h
        src/internal_includes/DataTypeAnalysis.h
        src/internal_includes/debug.h
        src/internal_includes/Declaration.h
        src/internal_includes/decode.h
        src/internal_includes/HLSLccToolkit.h
        src/internal_includes/HLSLCrossCompilerContext.h
        src/internal_includes/Instruction.h
        src/internal_includes/languages.h
        src/internal_includes/LoopTransform.h
        src/internal_includes/Operand.h
        src/internal_includes/reflect.h
        src/internal_includes/Shader.h
        src/internal_includes/toGLSL.h
        src/internal_includes/toGLSLOperand.h
        src/internal_includes/tokens.h
        src/internal_includes/toMetal.h
        src/internal_includes/toMetalDeclaration.h
        src/internal_includes/Translator.h
        src/internal_includes/UseDefineChains.h
        src/ControlFlowGraph.cpp
        src/ControlFlowGraphUtils.cpp
        src/DataTypeAnalysis.cpp
        src/Declaration.cpp
        src/decode.cpp
        src/HLSLcc.cpp
        src/HLSLccToolkit.cpp
        src/HLSLCrossCompilerContext.cpp
        src/Instruction.cpp
        src/LoopTransform.cpp
        src/Operand.cpp
        src/reflect.cpp
        src/Shader.cpp
        src/ShaderInfo.cpp
        src/toGLSL.cpp
        src/toGLSLDeclaration.cpp
        src/toGLSLInstruction.cpp
        src/toGLSLOperand.cpp
        src/toMetal.cpp
        src/toMetalDeclaration.cpp
        src/toMetalInstruction.cpp
        src/toMetalOperand.cpp
        src/UseDefineChains.cpp)
