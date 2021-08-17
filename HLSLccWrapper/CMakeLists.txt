cmake_minimum_required(VERSION 3.16)
project(HLSLccWrapper)

set(CMAKE_CXX_STANDARD 14)

include_directories(.)
include_directories(HLSLcc)
include_directories(HLSLcc/include)

add_library(HLSLccWrapper SHARED
        HLSLcc/include/growing_array.h
        HLSLcc/include/hlslcc.h
        HLSLcc/include/hlslcc.hpp
        HLSLcc/include/pstdint.h
        HLSLcc/include/ShaderInfo.h
        HLSLcc/include/UnityInstancingFlexibleArraySize.h
        HLSLccWrapper.cpp
        HLSLccWrapper.h)
