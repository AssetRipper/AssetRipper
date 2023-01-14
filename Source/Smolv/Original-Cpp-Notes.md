smol-v - public domain - https://github.com/aras-p/smol-v
authored 2016-2020 by Aras Pranckevicius
no warranty implied; use at your own risk
See end of file for license information.


### OVERVIEW:

SMOL-V encodes Vulkan/Khronos SPIR-V format programs into a form that is smaller, and is more
compressible. Normally no changes to the programs are done; they decode
into exactly same program as what was encoded. Optionally, debug information
can be removed too.

SPIR-V is a very verbose format, several times larger than same programs expressed in other
shader formats (e.g. DX11 bytecode, GLSL, DX9 bytecode etc.). The SSA-form with ever increasing
IDs is not very appreciated by regular data compressors either. SMOL-V does several things
to improve this:
- Many words, especially ones that most often have small values, are encoded using
  "varint" scheme (1-5 bytes per word, with just one byte for values in 0..127 range).
  See https://developers.google.com/protocol-buffers/docs/encoding
- Some IDs used in the program are delta-encoded, relative to previously seen IDs (e.g. Result
  IDs). Often instructions reference things that were computed just before, so this results in
  small deltas. These values are also encoded using "varint" scheme.
- Reordering instruction opcodes so that the most common ones are the smallest values, for smaller
 varint encoding.
- Encoding several instructions in a more compact form, e.g. the "typical <=4 component swizzle"
 shape of a VectorShuffle instruction, or sequences of MemberDecorate instructions.

A somewhat similar utility is spirv-remap from glslang, see
https://github.com/KhronosGroup/glslang/blob/master/README-spirv-remap.txt


### USAGE:

Add source/smolv.h and source/smolv.cpp to your C++ project build.
Currently it might require C++11 or somesuch; I only tested with Visual Studio 2017/2019, Mac Xcode 11 and Gcc 5.4.

smolv::Encode and smolv::Decode is the basic functionality.

Other functions are for development/statistics purposes, to figure out frequencies and
distributions of the instructions.

There's a test + compression benchmarking suite in testing/testmain.cpp; using that needs adding
other files under testing/external to the build too (3rd party code: glslang remapper, Zstd, LZ4).


### LIMITATIONS / TODO:

- SPIR-V where the words got stored in big-endian layout is not supported yet.
- The whole thing might not work on Big-Endian CPUs. It might, but I'm not 100% sure.
- Not much prevention is done against malformed/corrupted inputs, TODO.
- Out of memory cases are not handled. The code will either throw exception
  or crash, depending on your compilation flags.