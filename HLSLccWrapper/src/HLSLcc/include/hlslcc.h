#ifndef HLSLCC_H_
#define HLSLCC_H_

#include <string>
#include <vector>
#include <map>
#include <algorithm>

#if defined(_WIN32) && defined(HLSLCC_DYNLIB)
    #define HLSLCC_APIENTRY __stdcall
    #if defined(libHLSLcc_EXPORTS)
        #define HLSLCC_API __declspec(dllexport)
    #else
        #define HLSLCC_API __declspec(dllimport)
    #endif
#else
    #define HLSLCC_APIENTRY
    #define HLSLCC_API
#endif

#include <stdint.h>
#include <string.h>

typedef enum
{
    LANG_DEFAULT,// Depends on the HLSL shader model.
    LANG_ES_100, LANG_ES_FIRST = LANG_ES_100,
    LANG_ES_300,
    LANG_ES_310, LANG_ES_LAST = LANG_ES_310,
    LANG_120, LANG_GL_FIRST = LANG_120,
    LANG_130,
    LANG_140,
    LANG_150,
    LANG_330,
    LANG_400,
    LANG_410,
    LANG_420,
    LANG_430,
    LANG_440, LANG_GL_LAST = LANG_440,
    LANG_METAL,
} GLLang;

typedef struct GlExtensions
{
    uint32_t ARB_explicit_attrib_location : 1;
    uint32_t ARB_explicit_uniform_location : 1;
    uint32_t ARB_shading_language_420pack : 1;
    uint32_t OVR_multiview : 1;
    uint32_t EXT_shader_framebuffer_fetch : 1;
} GlExtensions;

#include "ShaderInfo.h"
#include "UnityInstancingFlexibleArraySize.h"

typedef std::vector<std::string> TextureSamplerPairs;

typedef enum INTERPOLATION_MODE
{
    INTERPOLATION_UNDEFINED = 0,
    INTERPOLATION_CONSTANT = 1,
    INTERPOLATION_LINEAR = 2,
    INTERPOLATION_LINEAR_CENTROID = 3,
    INTERPOLATION_LINEAR_NOPERSPECTIVE = 4,
    INTERPOLATION_LINEAR_NOPERSPECTIVE_CENTROID = 5,
    INTERPOLATION_LINEAR_SAMPLE = 6,
    INTERPOLATION_LINEAR_NOPERSPECTIVE_SAMPLE = 7,
} INTERPOLATION_MODE;

#define PS_FLAG_VERTEX_SHADER   0x1
#define PS_FLAG_HULL_SHADER     0x2
#define PS_FLAG_DOMAIN_SHADER   0x4
#define PS_FLAG_GEOMETRY_SHADER 0x8
#define PS_FLAG_PIXEL_SHADER    0x10

#define TO_FLAG_NONE    0x0
#define TO_FLAG_INTEGER 0x1
#define TO_FLAG_NAME_ONLY 0x2
#define TO_FLAG_DECLARATION_NAME 0x4
#define TO_FLAG_DESTINATION 0x8 //Operand is being written to by assignment.
#define TO_FLAG_UNSIGNED_INTEGER 0x10
#define TO_FLAG_DOUBLE 0x20
// --- TO_AUTO_BITCAST_TO_FLOAT ---
//If the operand is an integer temp variable then this flag
//indicates that the temp has a valid floating point encoding
//and that the current expression expects the operand to be floating point
//and therefore intBitsToFloat must be applied to that variable.
#define TO_AUTO_BITCAST_TO_FLOAT 0x40
#define TO_AUTO_BITCAST_TO_INT 0x80
#define TO_AUTO_BITCAST_TO_UINT 0x100
// AUTO_EXPAND flags automatically expand the operand to at least (i/u)vecX
// to match HLSL functionality.
#define TO_AUTO_EXPAND_TO_VEC2 0x200
#define TO_AUTO_EXPAND_TO_VEC3 0x400
#define TO_AUTO_EXPAND_TO_VEC4 0x800
#define TO_FLAG_BOOL 0x1000
// These flags are only used for Metal:
// Force downscaling of the operand to match
// the other operand (Metal doesn't like mixing halfs with floats)
#define TO_FLAG_FORCE_HALF 0x2000

typedef enum
{
    INVALID_SHADER = -1,
    PIXEL_SHADER,
    VERTEX_SHADER,
    GEOMETRY_SHADER,
    HULL_SHADER,
    DOMAIN_SHADER,
    COMPUTE_SHADER,
} SHADER_TYPE;

// Enum for texture dimension reflection data
typedef enum
{
    TD_FLOAT = 0,
    TD_INT,
    TD_2D,
    TD_3D,
    TD_CUBE,
    TD_2DSHADOW,
    TD_2DARRAY,
    TD_CUBEARRAY
} HLSLCC_TEX_DIMENSION;

// The prefix for all temporary variables used by the generated code.
// Using a texture or uniform name like this will cause conflicts
#define HLSLCC_TEMP_PREFIX "u_xlat"

typedef std::vector<std::pair<std::string, std::string> > MemberDefinitions;

// We store struct definition contents inside a vector of strings
struct StructDefinition
{
    StructDefinition() : m_Members(), m_Dependencies(), m_IsPrinted(false) {}

    MemberDefinitions m_Members; // A vector of strings with the struct members
    std::vector<std::string> m_Dependencies; // A vector of struct names this struct depends on.
    bool m_IsPrinted; // Has this struct been printed out yet?
};

typedef std::map<std::string, StructDefinition> StructDefinitions;

// Map of extra function definitions we need to add before the shader body but after the declarations.
typedef std::map<std::string, std::string> FunctionDefinitions;

// A helper class for allocating binding slots
// (because both UAVs and textures use the same slots in Metal, also constant buffers and other buffers etc)
class BindingSlotAllocator
{
    typedef std::map<uint32_t, uint32_t> SlotMap;
    SlotMap m_Allocations;
    uint32_t m_ShaderStageAllocations;
public:
    BindingSlotAllocator() : m_Allocations(), m_ShaderStageAllocations(0)
    {
        for (int i = MAX_RESOURCE_BINDINGS - 1; i >= 0; i--)
            m_FreeSlots.push_back(i);
    }

    enum BindType
    {
        ConstantBuffer = 0,
        RWBuffer,
        Texture,
        UAV
    };

    uint32_t GetBindingSlot(uint32_t regNo, BindType type)
    {
        // The key is regNumber with the bindtype stored to highest 16 bits
        uint32_t key = (m_ShaderStageAllocations + regNo) | (uint32_t(type) << 16);
        SlotMap::iterator itr = m_Allocations.find(key);
        if (itr == m_Allocations.end())
        {
            uint32_t slot = m_FreeSlots.back();
            m_FreeSlots.pop_back();
            m_Allocations.insert(std::make_pair(key, slot));
            return slot;
        }
        return itr->second;
    }

    // Func for reserving binding slots with the original reg number.
    // Used for fragment shader UAVs (SetRandomWriteTarget etc).
    void ReserveBindingSlot(uint32_t regNo, BindType type)
    {
        uint32_t key = regNo | (uint32_t(type) << 16);
        m_Allocations.insert(std::make_pair(key, regNo));

        // Remove regNo from free slots
        for (int i = m_FreeSlots.size() - 1; i >= 0; i--)
        {
            if (m_FreeSlots[i] == regNo)
            {
                m_FreeSlots.erase(m_FreeSlots.begin() + i);
                return;
            }
        }
    }

    uint32_t PeekFirstFreeSlot() const
    {
        return m_FreeSlots.back();
    }

    uint32_t SaveTotalShaderStageAllocationsCount()
    {
        m_ShaderStageAllocations = m_Allocations.size();
        return m_ShaderStageAllocations;
    }

private:
    std::vector<uint32_t> m_FreeSlots;
};

//The shader stages (Vertex, Pixel et al) do not depend on each other
//in HLSL. GLSL is a different story. HLSLCrossCompiler requires
//that hull shaders must be compiled before domain shaders, and
//the pixel shader must be compiled before all of the others.
//During compilation the GLSLCrossDependencyData struct will
//carry over any information needed about a different shader stage
//in order to construct valid GLSL shader combinations.


//Using GLSLCrossDependencyData is optional. However some shader
//combinations may show link failures, or runtime errors.
class GLSLCrossDependencyData
{
public:

    struct GLSLBufferBindPointInfo
    {
        uint32_t slot;
        bool known;
    };

    // A container for a single Vulkan resource binding (<set, binding> pair)
    struct VulkanResourceBinding
    {
        uint32_t set;
        uint32_t binding;
    };

    enum GLSLBufferType
    {
        BufferType_ReadWrite,
        BufferType_Constant,
        BufferType_SSBO,
        BufferType_Texture,
        BufferType_UBO,

        BufferType_Count,
        BufferType_Generic = BufferType_ReadWrite
    };

private:
    //Required if PixelInterpDependency is true
    std::vector<INTERPOLATION_MODE> pixelInterpolation;

    // Map of varying locations, indexed by varying names.
    typedef std::map<std::string, uint32_t> VaryingLocations;

    static const int MAX_NAMESPACES = 6; // Max namespaces: vert input, hull input, domain input, geom input, ps input, (ps output)

    VaryingLocations varyingLocationsMap[MAX_NAMESPACES];
    uint32_t nextAvailableVaryingLocation[MAX_NAMESPACES];

    typedef std::map<std::string, VulkanResourceBinding> VulkanResourceBindings;
    VulkanResourceBindings m_VulkanResourceBindings;
    uint32_t m_NextAvailableVulkanResourceBinding[8]; // one per set.

    typedef std::map<std::string, uint32_t> GLSLResouceBindings;

public:
    GLSLResouceBindings m_GLSLResourceBindings;
    uint32_t m_NextAvailableGLSLResourceBinding[BufferType_Count];  // UAV, Constant and Buffers have seperate binding ranges
    uint32_t m_StructuredBufferBindPoints[MAX_RESOURCE_BINDINGS];   // for the old style bindings

    inline int GetVaryingNamespace(SHADER_TYPE eShaderType, bool isInput)
    {
        switch (eShaderType)
        {
            case VERTEX_SHADER:
                return isInput ? 0 : 1;

            case HULL_SHADER:
                return isInput ? 1 : 2;

            case DOMAIN_SHADER:
                return isInput ? 2 : 3;

            case GEOMETRY_SHADER:
                // The input depends on whether there's a tessellation shader before us
                if (isInput)
                {
                    return ui32ProgramStages & PS_FLAG_DOMAIN_SHADER ? 3 : 1;
                }
                return 4;

            case PIXEL_SHADER:
                // The inputs can come from geom shader, domain shader or directly from vertex shader
                if (isInput)
                {
                    if (ui32ProgramStages & PS_FLAG_GEOMETRY_SHADER)
                    {
                        return 4;
                    }
                    else if (ui32ProgramStages & PS_FLAG_DOMAIN_SHADER)
                    {
                        return 3;
                    }
                    else
                    {
                        return 1;
                    }
                }
                return 5; // This value never really used
            default:
                return 0;
        }
    }

public:
    GLSLCrossDependencyData()
        : eTessPartitioning(),
        eTessOutPrim(),
        fMaxTessFactor(64.0),
        numPatchesInThreadGroup(0),
        hasControlPoint(false),
        hasPatchConstant(false),
        ui32ProgramStages(0),
        m_ExtBlendModes()
    {
        memset(nextAvailableVaryingLocation, 0, sizeof(nextAvailableVaryingLocation));
        memset(m_NextAvailableVulkanResourceBinding, 0, sizeof(m_NextAvailableVulkanResourceBinding));
        memset(m_NextAvailableGLSLResourceBinding, 0, sizeof(m_NextAvailableGLSLResourceBinding));
    }

    // Retrieve the location for a varying with a given name.
    // If the name doesn't already have an allocated location, allocate one
    // and store it into the map.
    inline uint32_t GetVaryingLocation(const std::string &name, SHADER_TYPE eShaderType, bool isInput, bool keepLocation, uint32_t maxSemanticIndex)
    {
        int nspace = GetVaryingNamespace(eShaderType, isInput);
        VaryingLocations::iterator itr = varyingLocationsMap[nspace].find(name);
        if (itr != varyingLocationsMap[nspace].end())
            return itr->second;

        if (keepLocation)
        {
            // Try to generate consistent varying locations based on the semantic indices in the hlsl source, i.e "TEXCOORD11" gets assigned to layout(location = 11)

            // Inspect last 2 characters in name
            size_t len = name.length();

            if (len > 1)
            {
                if (isdigit(name[len - 1]))
                {
                    uint32_t index = 0;
                    if (isdigit(name[len - 2]))
                        index = atoi(&name[len - 2]); // 2-digits index
                    else
                        index = atoi(&name[len - 1]); // 1-digit index

                    if (index < 32) // Some platforms only allow 32 varying locations
                    {
                        // Check that index is not already used
                        bool canUseIndex = true;
                        for (VaryingLocations::iterator it = varyingLocationsMap[nspace].begin(); it != varyingLocationsMap[nspace].end(); ++it)
                        {
                            if (it->second == index)
                            {
                                canUseIndex = false;
                                break;
                            }
                        }

                        if (canUseIndex)
                        {
                            varyingLocationsMap[nspace].insert(std::make_pair(name, index));
                            return index;
                        }
                    }
                }
            }

            // fallback: pick an unused index (max of already allocated AND of semanticIndices found by SignatureAnalysis
            uint32_t maxIndexAlreadyAssigned = 0;
            for (VaryingLocations::iterator it = varyingLocationsMap[nspace].begin(); it != varyingLocationsMap[nspace].end(); ++it)
                maxIndexAlreadyAssigned = std::max(maxIndexAlreadyAssigned, it->second);

            uint32_t fallbackIndex = std::max(maxIndexAlreadyAssigned + 1, maxSemanticIndex + 1);
            varyingLocationsMap[nspace].insert(std::make_pair(name, fallbackIndex));
            return fallbackIndex;
        }
        else
        {
            uint32_t newKey = nextAvailableVaryingLocation[nspace];
            nextAvailableVaryingLocation[nspace]++;
            varyingLocationsMap[nspace].insert(std::make_pair(name, newKey));
            return newKey;
        }
    }

    // Retrieve the binding for a resource (texture, constant buffer, image) with a given name
    // If not found, allocate a new one (in set 0) and return that
    // The returned value is a pair of <set, binding>
    // If the name contains "hlslcc_set_X_bind_Y", those values (from the first found occurence in the name)
    // will be used instead, and all occurences of that string will be removed from name, so name parameter can be modified
    // if allocRoomForCounter is true, the following binding number in the same set will be allocated with name + '_counter'
    inline VulkanResourceBinding GetVulkanResourceBinding(std::string &name, bool allocRoomForCounter = false, uint32_t preferredSet = 0)
    {
        // scan for the special marker
        const char *marker = "Xhlslcc_set_%d_bind_%dX";
        uint32_t Set = 0, Binding = 0;
        size_t startLoc = name.find("Xhlslcc");
        if ((startLoc != std::string::npos) && (sscanf(name.c_str() + startLoc, marker, &Set, &Binding) == 2))
        {
            // Get rid of all markers
            while ((startLoc = name.find("Xhlslcc")) != std::string::npos)
            {
                size_t endLoc = name.find('X', startLoc + 1);
                if (endLoc == std::string::npos)
                    break;
                name.erase(startLoc, endLoc - startLoc + 1);
            }
            // Add to map
            VulkanResourceBinding newBind = { Set, Binding };
            m_VulkanResourceBindings.insert(std::make_pair(name, newBind));
            if (allocRoomForCounter)
            {
                VulkanResourceBinding counterBind = { Set, Binding + 1 };
                m_VulkanResourceBindings.insert(std::make_pair(name + "_counter", counterBind));
            }

            return newBind;
        }

        VulkanResourceBindings::iterator itr = m_VulkanResourceBindings.find(name);
        if (itr != m_VulkanResourceBindings.end())
            return itr->second;

        // Allocate a new one
        VulkanResourceBinding newBind = { preferredSet, m_NextAvailableVulkanResourceBinding[preferredSet] };
        m_NextAvailableVulkanResourceBinding[preferredSet]++;
        m_VulkanResourceBindings.insert(std::make_pair(name, newBind));
        if (allocRoomForCounter)
        {
            VulkanResourceBinding counterBind = { preferredSet, m_NextAvailableVulkanResourceBinding[preferredSet] };
            m_NextAvailableVulkanResourceBinding[preferredSet]++;
            m_VulkanResourceBindings.insert(std::make_pair(name + "_counter", counterBind));
        }
        return newBind;
    }

    // GLSL Bind point handling logic
    // Handles both 'old style' fill around fixed UAV and new style partitioned offsets with fixed UAV locations

    // HLSL has separate register spaces for UAV and structured buffers. GLSL has shared register space for all buffers.
    // The aim here is to preserve the UAV buffer bindings as they are and use remaining binding points for structured buffers.
    // In this step make m_structuredBufferBindPoints contain increasingly ordered uints starting from zero.
    // This is only used when we are doing old style binding setup
    void SetupGLSLResourceBindingSlotsIndices()
    {
        for (uint32_t i = 0; i < MAX_RESOURCE_BINDINGS; i++)
        {
            m_StructuredBufferBindPoints[i] = i;
        }
    }

    void RemoveBindPointFromAvailableList(uint32_t bindPoint)
    {
        for (uint32_t i = 0; i < MAX_RESOURCE_BINDINGS - 1 && m_StructuredBufferBindPoints[i] <= bindPoint; i++)
        {
            if (m_StructuredBufferBindPoints[i] == bindPoint) // Remove uav binding point from the list by copying array remainder here
            {
                memcpy(&m_StructuredBufferBindPoints[i], &m_StructuredBufferBindPoints[i + 1], (MAX_RESOURCE_BINDINGS - 1 - i) * sizeof(uint32_t));
                break;
            }
        }
    }

    void ReserveNamedBindPoint(const std::string &name, uint32_t bindPoint, GLSLBufferType type)
    {
        m_GLSLResourceBindings.insert(std::make_pair(name, bindPoint));
        RemoveBindPointFromAvailableList(bindPoint);
    }

    bool ShouldUseBufferSpecificBinding(GLSLBufferType bufferType)
    {
        return bufferType == BufferType_Constant || bufferType == BufferType_Texture || bufferType == BufferType_UBO;
    }

    uint32_t GetGLSLBufferBindPointIndex(GLSLBufferType bufferType)
    {
        uint32_t binding = -1;

        if (ShouldUseBufferSpecificBinding(bufferType))
        {
            binding = m_NextAvailableGLSLResourceBinding[bufferType];
        }
        else
        {
            binding = m_StructuredBufferBindPoints[m_NextAvailableGLSLResourceBinding[BufferType_Generic]];
        }

        return binding;
    }

    void UpdateResourceBindingIndex(GLSLBufferType bufferType)
    {
        if (ShouldUseBufferSpecificBinding(bufferType))
        {
            m_NextAvailableGLSLResourceBinding[bufferType]++;
        }
        else
        {
            m_NextAvailableGLSLResourceBinding[BufferType_Generic]++;
        }
    }

    inline GLSLBufferBindPointInfo GetGLSLResourceBinding(const std::string &name, GLSLBufferType bufferType)
    {
        GLSLResouceBindings::iterator itr = m_GLSLResourceBindings.find(name);
        if (itr != m_GLSLResourceBindings.end())
        {
            return GLSLBufferBindPointInfo{ itr->second, true };
        }

        uint32_t binding = GetGLSLBufferBindPointIndex(bufferType);
        UpdateResourceBindingIndex(bufferType);

        m_GLSLResourceBindings.insert(std::make_pair(name, binding));

        return GLSLBufferBindPointInfo{ binding, false };
    }

    //dcl_tessellator_partitioning and dcl_tessellator_output_primitive appear in hull shader for D3D,
    //but they appear on inputs inside domain shaders for GL.
    //Hull shader must be compiled before domain so the
    //ensure correct partitioning and primitive type information
    //can be saved when compiling hull and passed to domain compilation.
    TESSELLATOR_PARTITIONING eTessPartitioning;
    TESSELLATOR_OUTPUT_PRIMITIVE eTessOutPrim;
    float fMaxTessFactor;
    int numPatchesInThreadGroup;
    bool hasControlPoint;
    bool hasPatchConstant;

    // Bitfield for the shader stages this program is going to include (see PS_FLAG_*).
    // Needed so we can construct proper shader input and output names
    uint32_t ui32ProgramStages;

    std::vector<std::string> m_ExtBlendModes; // The blend modes (from KHR_blend_equation_advanced) requested for this shader. See ext spec for list.

    inline INTERPOLATION_MODE GetInterpolationMode(uint32_t regNo)
    {
        if (regNo >= pixelInterpolation.size())
            return INTERPOLATION_UNDEFINED;
        else
            return pixelInterpolation[regNo];
    }

    inline void SetInterpolationMode(uint32_t regNo, INTERPOLATION_MODE mode)
    {
        if (regNo >= pixelInterpolation.size())
            pixelInterpolation.resize((regNo + 1) * 2, INTERPOLATION_UNDEFINED);

        pixelInterpolation[regNo] = mode;
    }

    struct CompareFirst
    {
        CompareFirst(std::string val) : m_Val(val) {}
        bool operator()(const std::pair<std::string, std::string>& elem) const
        {
            return m_Val == elem.first;
        }

    private:
        std::string m_Val;
    };

    inline bool IsMemberDeclared(const std::string &name)
    {
        if (std::find_if(m_SharedFunctionMembers.begin(), m_SharedFunctionMembers.end(), CompareFirst(name)) != m_SharedFunctionMembers.end())
            return true;
        return false;
    }

    MemberDefinitions m_SharedFunctionMembers;
    std::vector<std::string> m_SharedDependencies;
    BindingSlotAllocator m_SharedTextureSlots, m_SharedSamplerSlots;
    BindingSlotAllocator m_SharedBufferSlots;

    inline void ClearCrossDependencyData()
    {
        pixelInterpolation.clear();
        for (int i = 0; i < MAX_NAMESPACES; i++)
        {
            varyingLocationsMap[i].clear();
            nextAvailableVaryingLocation[i] = 0;
        }
        m_SharedFunctionMembers.clear();
        m_SharedDependencies.clear();
    }

    bool IsHullShaderInputAlreadyDeclared(const std::string& name)
    {
        bool isKnown = false;

        for (size_t idx = 0, end = m_hullShaderInputs.size(); idx < end; ++idx)
        {
            if (m_hullShaderInputs[idx] == name)
            {
                isKnown = true;
                break;
            }
        }

        return isKnown;
    }

    void RecordHullShaderInput(const std::string& name)
    {
        m_hullShaderInputs.push_back(name);
    }

    std::vector<std::string> m_hullShaderInputs;
};

struct GLSLShader
{
    int shaderType; //One of the GL enums.
    std::string sourceCode;
    ShaderInfo reflection;
    GLLang GLSLLanguage;
    TextureSamplerPairs textureSamplers;    // HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS fills this out
};

// Interface for retrieving reflection and diagnostics data
class HLSLccReflection
{
public:
    HLSLccReflection() {}
    virtual ~HLSLccReflection() {}

    // Called on errors or diagnostic messages
    virtual void OnDiagnostics(const std::string &error, int line, bool isError) {}

    virtual void OnInputBinding(const std::string &name, int bindIndex) {}

    // Returns false if this constant buffer is not needed for this shader. This info can be used for pruning unused
    // constant buffers and vars from compute shaders where we need broader context than a single kernel to know
    // if something can be dropped, as the constant buffers are shared between all kernels in a .compute file.
    virtual bool OnConstantBuffer(const std::string &name, size_t bufferSize, size_t memberCount) { return true; }

    // Returns false if this constant var is not needed for this shader. See above.
    virtual bool OnConstant(const std::string &name, int bindIndex, SHADER_VARIABLE_TYPE cType, int rows, int cols, bool isMatrix, int arraySize, bool isUsed) { return true; }

    virtual void OnConstantBufferBinding(const std::string &name, int bindIndex) {}
    virtual void OnTextureBinding(const std::string &name, int bindIndex, int samplerIndex, bool multisampled, HLSLCC_TEX_DIMENSION dim, bool isUAV) {}
    virtual void OnBufferBinding(const std::string &name, int bindIndex, bool isUAV) {}
    virtual void OnThreadGroupSize(unsigned int xSize, unsigned int ySize, unsigned int zSize) {}
    virtual void OnTessellationInfo(uint32_t tessPartitionMode, uint32_t tessOutputWindingOrder, uint32_t tessMaxFactor, uint32_t tessNumPatchesInThreadGroup) {}
    virtual void OnTessellationKernelInfo(uint32_t patchKernelBufferCount) {}

    // these are for now metal only (but can be trivially added for other backends if needed)
    // they are useful mostly for diagnostics as interim values are actually hidden from user
    virtual void OnVertexProgramOutput(const std::string& name, const std::string& semantic, int semanticIndex) {}
    virtual void OnBuiltinOutput(SPECIAL_NAME name) {}
    virtual void OnFragmentOutputDeclaration(int numComponents, int outputIndex) {}


    enum AccessType
    {
        ReadAccess = 1 << 0,
        WriteAccess = 1 << 1
    };

    virtual void OnStorageImage(int bindIndex, unsigned int access) {}
};


/*HLSL constant buffers are treated as default-block unform arrays by default. This is done
  to support versions of GLSL which lack ARB_uniform_buffer_object functionality.
  Setting this flag causes each one to have its own uniform block.
  Note: Currently the nth const buffer will be named UnformBufferN. This is likey to change to the original HLSL name in the future.*/
static const unsigned int HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT = 0x1;

static const unsigned int HLSLCC_FLAG_ORIGIN_UPPER_LEFT = 0x2;

static const unsigned int HLSLCC_FLAG_PIXEL_CENTER_INTEGER = 0x4;

static const unsigned int HLSLCC_FLAG_GLOBAL_CONSTS_NEVER_IN_UBO = 0x8;

//GS enabled?
//Affects vertex shader (i.e. need to compile vertex shader again to use with/without GS).
//This flag is needed in order for the interfaces between stages to match when GS is in use.
//PS inputs VtxGeoOutput
//GS outputs VtxGeoOutput
//Vs outputs VtxOutput if GS enabled. VtxGeoOutput otherwise.
static const unsigned int HLSLCC_FLAG_GS_ENABLED = 0x10;

static const unsigned int HLSLCC_FLAG_TESS_ENABLED = 0x20;

//Either use this flag or glBindFragDataLocationIndexed.
//When set the first pixel shader output is the first input to blend
//equation, the others go to the second input.
static const unsigned int HLSLCC_FLAG_DUAL_SOURCE_BLENDING = 0x40;

//If set, shader inputs and outputs are declared with their semantic name.
static const unsigned int HLSLCC_FLAG_INOUT_SEMANTIC_NAMES = 0x80;
//If set, shader inputs and outputs are declared with their semantic name appended.
static const unsigned int HLSLCC_FLAG_INOUT_APPEND_SEMANTIC_NAMES = 0x100;

//If set, combines texture/sampler pairs used together into samplers named "texturename_X_samplername".
static const unsigned int HLSLCC_FLAG_COMBINE_TEXTURE_SAMPLERS = 0x200;

//If set, attribute and uniform explicit location qualifiers are disabled (even if the language version supports that)
static const unsigned int HLSLCC_FLAG_DISABLE_EXPLICIT_LOCATIONS = 0x400;

//If set, global uniforms are not stored in a struct.
static const unsigned int HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT = 0x800;

//If set, image declarations will always have binding and format qualifiers.
static const unsigned int HLSLCC_FLAG_GLES31_IMAGE_QUALIFIERS = 0x1000;

// If set, treats sampler names ending with _highp, _mediump, and _lowp as sampler precision qualifiers
// Also removes that prefix from generated output
static const unsigned int HLSLCC_FLAG_SAMPLER_PRECISION_ENCODED_IN_NAME = 0x2000;

// If set, adds location qualifiers to intra-shader varyings.
static const unsigned int HLSLCC_FLAG_SEPARABLE_SHADER_OBJECTS = 0x4000; // NOTE: obsolete flag (behavior enabled by this flag began default in 83a16a1829cf)

// If set, wraps all uniform buffer declarations in a preprocessor macro #ifdef HLSLCC_ENABLE_UNIFORM_BUFFERS
// so that if that macro is undefined, all UBO declarations will become normal uniforms
static const unsigned int HLSLCC_FLAG_WRAP_UBO = 0x8000;

// If set, skips all members of the $Globals constant buffer struct that are not referenced in the shader code
static const unsigned int HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS = 0x10000;

#define HLSLCC_TRANSLATE_MATRIX_FORMAT_STRING "hlslcc_mtx%dx%d"

// If set, translates all matrix declarations into vec4 arrays (as the DX bytecode treats them), and prefixes the name with 'hlslcc_mtx<rows>x<cols>'
static const unsigned int HLSLCC_FLAG_TRANSLATE_MATRICES = 0x20000;

// If set, emits Vulkan-style (set, binding) bindings, also captures that info from any declaration named "<Name>_hlslcc_set_X_bind_Y"
// Unless bindings are given explicitly, they are allocated into set 0 (map stored in GLSLCrossDependencyData)
static const unsigned int HLSLCC_FLAG_VULKAN_BINDINGS = 0x40000;

// If set, metal output will use linear sampler for shadow compares, otherwise point sampler.
static const unsigned int HLSLCC_FLAG_METAL_SHADOW_SAMPLER_LINEAR = 0x80000;

// If set, avoid emit atomic counter (ARB_shader_atomic_counters) and use atomic functions provided by ARB_shader_storage_buffer_object instead.
static const unsigned int HLSLCC_FLAG_AVOID_SHADER_ATOMIC_COUNTERS = 0x100000;

// Unused 0x200000;

// If set, this shader uses the GLSL extension EXT_shader_framebuffer_fetch
static const unsigned int HLSLCC_FLAG_SHADER_FRAMEBUFFER_FETCH = 0x400000;

// Build for Switch.
static const unsigned int HLSLCC_FLAG_NVN_TARGET = 0x800000;

// If set, generate an instance name for constant buffers. GLSL specs 4.5 disallows uniform variables from different constant buffers sharing the same name
// as long as they are part of the same final linked program. Uniform buffer instance names solve this cross-shader symbol conflict issue.
static const unsigned int HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT_WITH_INSTANCE_NAME = 0x1000000;

// Massage shader steps into Metal compute kernel from vertex/hull shaders + post-tessellation vertex shader from domain shader
static const unsigned int HLSLCC_FLAG_METAL_TESSELLATION = 0x2000000;

// Disable fastmath
static const unsigned int HLSLCC_FLAG_DISABLE_FASTMATH = 0x4000000;

//If set, uniform explicit location qualifiers are enabled (even if the language version doesn't support that)
static const unsigned int HLSLCC_FLAG_FORCE_EXPLICIT_LOCATIONS = 0x8000000;

// If set, each line of the generated source will be preceded by a comment specifying which DirectX bytecode instruction it maps to
static const unsigned int HLSLCC_FLAG_INCLUDE_INSTRUCTIONS_COMMENTS = 0x10000000;

// If set, try to generate consistent varying locations based on the semantic indices in the hlsl source, i.e "TEXCOORD11" gets assigned to layout(location = 11)
static const unsigned int HLSLCC_FLAG_KEEP_VARYING_LOCATIONS = 0x20000000;

// Code generation might vary for mobile targets, or using lower sampler precision than full by default
static const unsigned int HLSLCC_FLAG_MOBILE_TARGET = 0x40000000;

#ifdef __cplusplus
extern "C" {
#endif

HLSLCC_API int HLSLCC_APIENTRY TranslateHLSLFromFile(const char* filename,
    unsigned int flags,
    GLLang language,
    const GlExtensions *extensions,
    GLSLCrossDependencyData* dependencies,
    HLSLccSamplerPrecisionInfo& samplerPrecisions,
    HLSLccReflection& reflectionCallbacks,
    GLSLShader* result
);

HLSLCC_API int HLSLCC_APIENTRY TranslateHLSLFromMem(const char* shader,
    unsigned int flags,
    GLLang language,
    const GlExtensions *extensions,
    GLSLCrossDependencyData* dependencies,
    HLSLccSamplerPrecisionInfo& samplerPrecisions,
    HLSLccReflection& reflectionCallbacks,
    GLSLShader* result);

#ifdef __cplusplus
}
#endif

#endif
