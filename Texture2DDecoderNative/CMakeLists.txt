cmake_minimum_required(VERSION 3.16)
project(Texture2DDecoderNative)

set(CMAKE_CXX_STANDARD 14)

# _T2D_DLL;NDEBUG;TEXTURE2DDECODERNATIVE_EXPORTS;_WINDOWS;_USRDLL
add_compile_definitions(_T2D_DLL)
add_compile_definitions(NDEBUG)
add_compile_definitions(TEXTURE2DDECODERNATIVE_EXPORTS)

include_directories(.)
include_directories(crunch)
include_directories(fp16)
include_directories(unitycrunch)

add_library(Texture2DDecoderNative SHARED
        crunch/crn_decomp.h
        crunch/crnlib.h
        fp16/bitcasts.h
        fp16/fp16.h
        unitycrunch/crn_decomp.h
        unitycrunch/crn_defs.h
        unitycrunch/crnlib.h
        astc.cpp
        astc.h
        atc.cpp
        atc.h
        bcn.cpp
        bcn.h
        bool32_t.h
        color.h
        crunch.cpp
        crunch.h
        dllexport.h
        dllmain.cpp
        endianness.h
        etc.cpp
        etc.h
        fp16.h
        pvrtc.cpp
        pvrtc.h
        resource.h
        unitycrunch.cpp
        unitycrunch.h)
