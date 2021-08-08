#pragma once

#include <vector>
#include <set>
#include <map>
#include <string>
#include "growing_array.h"
#include <stdint.h>

//Reflection
#define MAX_RESOURCE_BINDINGS 256

typedef enum _SHADER_VARIABLE_TYPE
{
    SVT_VOID = 0,
    SVT_BOOL = 1,
    SVT_INT = 2,
    SVT_FLOAT = 3,
    SVT_STRING = 4,
    SVT_TEXTURE = 5,
    SVT_TEXTURE1D = 6,
    SVT_TEXTURE2D = 7,
    SVT_TEXTURE3D = 8,
    SVT_TEXTURECUBE = 9,
    SVT_SAMPLER = 10,
    SVT_PIXELSHADER = 15,
    SVT_VERTEXSHADER = 16,
    SVT_UINT = 19,
    SVT_UINT8 = 20,
    SVT_GEOMETRYSHADER = 21,
    SVT_RASTERIZER = 22,
    SVT_DEPTHSTENCIL = 23,
    SVT_BLEND = 24,
    SVT_BUFFER = 25,
    SVT_CBUFFER = 26,
    SVT_TBUFFER = 27,
    SVT_TEXTURE1DARRAY = 28,
    SVT_TEXTURE2DARRAY = 29,
    SVT_RENDERTARGETVIEW = 30,
    SVT_DEPTHSTENCILVIEW = 31,
    SVT_TEXTURE2DMS = 32,
    SVT_TEXTURE2DMSARRAY = 33,
    SVT_TEXTURECUBEARRAY = 34,
    SVT_HULLSHADER = 35,
    SVT_DOMAINSHADER = 36,
    SVT_INTERFACE_POINTER = 37,
    SVT_COMPUTESHADER = 38,
    SVT_DOUBLE = 39,
    SVT_RWTEXTURE1D = 40,
    SVT_RWTEXTURE1DARRAY = 41,
    SVT_RWTEXTURE2D = 42,
    SVT_RWTEXTURE2DARRAY = 43,
    SVT_RWTEXTURE3D = 44,
    SVT_RWBUFFER = 45,
    SVT_BYTEADDRESS_BUFFER = 46,
    SVT_RWBYTEADDRESS_BUFFER = 47,
    SVT_STRUCTURED_BUFFER = 48,
    SVT_RWSTRUCTURED_BUFFER = 49,
    SVT_APPEND_STRUCTURED_BUFFER = 50,
    SVT_CONSUME_STRUCTURED_BUFFER = 51,


    // Only used as a marker when analyzing register types
    SVT_FORCED_INT = 152,
    // Integer that can be either signed or unsigned. Only used as an intermediate step when doing data type analysis
    SVT_INT_AMBIGUOUS = 153,

    // Partial precision types. Used when doing type analysis
    SVT_FLOAT10 = 53, // Seems to be used in constant buffers
    SVT_FLOAT16 = 54,
    SVT_INT16 = 156,
    SVT_INT12 = 157,
    SVT_UINT16 = 158,

    SVT_FORCE_DWORD = 0x7fffffff
} SHADER_VARIABLE_TYPE;

typedef enum _SHADER_VARIABLE_CLASS
{
    SVC_SCALAR = 0,
    SVC_VECTOR = (SVC_SCALAR + 1),
    SVC_MATRIX_ROWS = (SVC_VECTOR + 1),
    SVC_MATRIX_COLUMNS = (SVC_MATRIX_ROWS + 1),
    SVC_OBJECT = (SVC_MATRIX_COLUMNS + 1),
    SVC_STRUCT = (SVC_OBJECT + 1),
    SVC_INTERFACE_CLASS = (SVC_STRUCT + 1),
    SVC_INTERFACE_POINTER = (SVC_INTERFACE_CLASS + 1),
    SVC_FORCE_DWORD = 0x7fffffff
} SHADER_VARIABLE_CLASS;


///////////////////////////////////////
// Types

enum TESSELLATOR_PARTITIONING
{
    TESSELLATOR_PARTITIONING_UNDEFINED = 0,
    TESSELLATOR_PARTITIONING_INTEGER = 1,
    TESSELLATOR_PARTITIONING_POW2 = 2,
    TESSELLATOR_PARTITIONING_FRACTIONAL_ODD = 3,
    TESSELLATOR_PARTITIONING_FRACTIONAL_EVEN = 4
};

enum TESSELLATOR_OUTPUT_PRIMITIVE
{
    TESSELLATOR_OUTPUT_UNDEFINED = 0,
    TESSELLATOR_OUTPUT_POINT = 1,
    TESSELLATOR_OUTPUT_LINE = 2,
    TESSELLATOR_OUTPUT_TRIANGLE_CW = 3,
    TESSELLATOR_OUTPUT_TRIANGLE_CCW = 4
};

typedef enum TESSELLATOR_DOMAIN
{
    TESSELLATOR_DOMAIN_UNDEFINED = 0,
    TESSELLATOR_DOMAIN_ISOLINE = 1,
    TESSELLATOR_DOMAIN_TRI = 2,
    TESSELLATOR_DOMAIN_QUAD = 3
} TESSELLATOR_DOMAIN;

enum SPECIAL_NAME
{
    NAME_UNDEFINED = 0,
    NAME_POSITION = 1,
    NAME_CLIP_DISTANCE = 2,
    NAME_CULL_DISTANCE = 3,
    NAME_RENDER_TARGET_ARRAY_INDEX = 4,
    NAME_VIEWPORT_ARRAY_INDEX = 5,
    NAME_VERTEX_ID = 6,
    NAME_PRIMITIVE_ID = 7,
    NAME_INSTANCE_ID = 8,
    NAME_IS_FRONT_FACE = 9,
    NAME_SAMPLE_INDEX = 10,
    // The following are added for D3D11
    NAME_FINAL_QUAD_U_EQ_0_EDGE_TESSFACTOR = 11,
    NAME_FINAL_QUAD_V_EQ_0_EDGE_TESSFACTOR = 12,
    NAME_FINAL_QUAD_U_EQ_1_EDGE_TESSFACTOR = 13,
    NAME_FINAL_QUAD_V_EQ_1_EDGE_TESSFACTOR = 14,
    NAME_FINAL_QUAD_U_INSIDE_TESSFACTOR = 15,
    NAME_FINAL_QUAD_V_INSIDE_TESSFACTOR = 16,
    NAME_FINAL_TRI_U_EQ_0_EDGE_TESSFACTOR = 17,
    NAME_FINAL_TRI_V_EQ_0_EDGE_TESSFACTOR = 18,
    NAME_FINAL_TRI_W_EQ_0_EDGE_TESSFACTOR = 19,
    NAME_FINAL_TRI_INSIDE_TESSFACTOR = 20,
    NAME_FINAL_LINE_DETAIL_TESSFACTOR = 21,
    NAME_FINAL_LINE_DENSITY_TESSFACTOR = 22,
};


enum INOUT_COMPONENT_TYPE
{
    INOUT_COMPONENT_UNKNOWN = 0,
    INOUT_COMPONENT_UINT32 = 1,
    INOUT_COMPONENT_SINT32 = 2,
    INOUT_COMPONENT_FLOAT32 = 3
};

enum MIN_PRECISION
{
    MIN_PRECISION_DEFAULT = 0,
    MIN_PRECISION_FLOAT_16 = 1,
    MIN_PRECISION_FLOAT_2_8 = 2,
    MIN_PRECISION_RESERVED = 3,
    MIN_PRECISION_SINT_16 = 4,
    MIN_PRECISION_UINT_16 = 5,
    MIN_PRECISION_ANY_16 = 0xf0,
    MIN_PRECISION_ANY_10 = 0xf1
};

enum ResourceType
{
    RTYPE_CBUFFER,//0
    RTYPE_TBUFFER,//1
    RTYPE_TEXTURE,//2
    RTYPE_SAMPLER,//3
    RTYPE_UAV_RWTYPED,//4
    RTYPE_STRUCTURED,//5
    RTYPE_UAV_RWSTRUCTURED,//6
    RTYPE_BYTEADDRESS,//7
    RTYPE_UAV_RWBYTEADDRESS,//8
    RTYPE_UAV_APPEND_STRUCTURED,//9
    RTYPE_UAV_CONSUME_STRUCTURED,//10
    RTYPE_UAV_RWSTRUCTURED_WITH_COUNTER,//11
    RTYPE_COUNT,
};

enum ResourceGroup
{
    RGROUP_CBUFFER,
    RGROUP_TEXTURE,
    RGROUP_SAMPLER,
    RGROUP_UAV,
    RGROUP_COUNT,
};

enum REFLECT_RESOURCE_DIMENSION
{
    REFLECT_RESOURCE_DIMENSION_UNKNOWN = 0,
    REFLECT_RESOURCE_DIMENSION_BUFFER = 1,
    REFLECT_RESOURCE_DIMENSION_TEXTURE1D = 2,
    REFLECT_RESOURCE_DIMENSION_TEXTURE1DARRAY = 3,
    REFLECT_RESOURCE_DIMENSION_TEXTURE2D = 4,
    REFLECT_RESOURCE_DIMENSION_TEXTURE2DARRAY = 5,
    REFLECT_RESOURCE_DIMENSION_TEXTURE2DMS = 6,
    REFLECT_RESOURCE_DIMENSION_TEXTURE2DMSARRAY = 7,
    REFLECT_RESOURCE_DIMENSION_TEXTURE3D = 8,
    REFLECT_RESOURCE_DIMENSION_TEXTURECUBE = 9,
    REFLECT_RESOURCE_DIMENSION_TEXTURECUBEARRAY = 10,
    REFLECT_RESOURCE_DIMENSION_BUFFEREX = 11,
};

enum REFLECT_RESOURCE_PRECISION
{
    REFLECT_RESOURCE_PRECISION_UNKNOWN = 0,
    REFLECT_RESOURCE_PRECISION_LOWP = 1,
    REFLECT_RESOURCE_PRECISION_MEDIUMP = 2,
    REFLECT_RESOURCE_PRECISION_HIGHP = 3,
};

enum RESOURCE_RETURN_TYPE
{
    RETURN_TYPE_UNORM = 1,
    RETURN_TYPE_SNORM = 2,
    RETURN_TYPE_SINT = 3,
    RETURN_TYPE_UINT = 4,
    RETURN_TYPE_FLOAT = 5,
    RETURN_TYPE_MIXED = 6,
    RETURN_TYPE_DOUBLE = 7,
    RETURN_TYPE_CONTINUED = 8,
    RETURN_TYPE_UNUSED = 9,
};

typedef std::map<std::string, REFLECT_RESOURCE_PRECISION> HLSLccSamplerPrecisionInfo;

struct ResourceBinding
{
    std::string name;
    ResourceType eType;
    uint32_t ui32BindPoint;
    uint32_t ui32BindCount;
    uint32_t ui32Flags;
    uint32_t ui32Space;
    uint32_t ui32RangeID;
    REFLECT_RESOURCE_DIMENSION eDimension;
    RESOURCE_RETURN_TYPE ui32ReturnType;
    uint32_t ui32NumSamples;
    REFLECT_RESOURCE_PRECISION ePrecision;
    int m_SamplerMode; // (SB_SAMPLER_MODE) For samplers, this is the sampler mode this sampler is declared with

    SHADER_VARIABLE_TYPE GetDataType() const
    {
        switch (ePrecision)
        {
            case REFLECT_RESOURCE_PRECISION_LOWP:
                switch (ui32ReturnType)
                {
                    case RETURN_TYPE_UNORM:
                    case RETURN_TYPE_SNORM:
                    case RETURN_TYPE_FLOAT:
                        return SVT_FLOAT10;
                    case RETURN_TYPE_SINT:
                        return SVT_INT16;
                    case RETURN_TYPE_UINT:
                        return SVT_UINT16;
                    default:
//              ASSERT(0);
                        return SVT_FLOAT10;
                }

            case REFLECT_RESOURCE_PRECISION_MEDIUMP:
                switch (ui32ReturnType)
                {
                    case RETURN_TYPE_UNORM:
                    case RETURN_TYPE_SNORM:
                    case RETURN_TYPE_FLOAT:
                        return SVT_FLOAT16;
                    case RETURN_TYPE_SINT:
                        return SVT_INT16;
                    case RETURN_TYPE_UINT:
                        return SVT_UINT16;
                    default:
//              ASSERT(0);
                        return SVT_FLOAT16;
                }

            default:
                switch (ui32ReturnType)
                {
                    case RETURN_TYPE_UNORM:
                    case RETURN_TYPE_SNORM:
                    case RETURN_TYPE_FLOAT:
                        return SVT_FLOAT;
                    case RETURN_TYPE_SINT:
                        return SVT_INT;
                    case RETURN_TYPE_UINT:
                        return SVT_UINT;
                    case RETURN_TYPE_DOUBLE:
                        return SVT_DOUBLE;
                    default:
//              ASSERT(0);
                        return SVT_FLOAT;
                }
        }
    }
};

struct ShaderVarType
{
    ShaderVarType() :
        Class(),
        Type(),
        Rows(),
        Columns(),
        Elements(),
        MemberCount(),
        Offset(),
        ParentCount(),
        Parent(),
        m_IsUsed(false)
    {}

    SHADER_VARIABLE_CLASS   Class;
    SHADER_VARIABLE_TYPE    Type;
    uint32_t                Rows;
    uint32_t                Columns;
    uint32_t                Elements;
    uint32_t                MemberCount;
    uint32_t                Offset;
    std::string             name;

    uint32_t ParentCount;
    struct ShaderVarType * Parent;
    //Includes all parent names.
    std::string             fullName;

    std::vector<struct ShaderVarType> Members;

    bool m_IsUsed; // If not set, is not used in the shader code

    uint32_t GetMemberCount() const
    {
        if (Class == SVC_STRUCT)
        {
            uint32_t res = 0;
            std::vector<struct ShaderVarType>::const_iterator itr;
            for (itr = Members.begin(); itr != Members.end(); itr++)
            {
                res += itr->GetMemberCount();
            }
            return res;
        }
        else
            return 1;
    }
};

struct ShaderVar
{
    std::string name;
    int haveDefaultValue;
    std::vector<uint32_t> pui32DefaultValues;
    //Offset/Size in bytes.
    uint32_t ui32StartOffset;
    uint32_t ui32Size;

    ShaderVarType sType;
};

struct ConstantBuffer
{
    std::string name;

    std::vector<ShaderVar> asVars;

    uint32_t ui32TotalSizeInBytes;

    uint32_t GetMemberCount(bool stripUnused) const
    {
        uint32_t res = 0;
        std::vector<ShaderVar>::const_iterator itr;
        for (itr = asVars.begin(); itr != asVars.end(); itr++)
        {
            if (stripUnused && !itr->sType.m_IsUsed)
                continue;
            res += itr->sType.GetMemberCount();
        }
        return res;
    }
};

struct ClassType
{
    std::string name;
    uint16_t ui16ID;
    uint16_t ui16ConstBufStride;
    uint16_t ui16Texture;
    uint16_t ui16Sampler;
};

struct ClassInstance
{
    std::string name;
    uint16_t ui16ID;
    uint16_t ui16ConstBuf;
    uint16_t ui16ConstBufOffset;
    uint16_t ui16Texture;
    uint16_t ui16Sampler;
};

class Operand;

class ShaderInfo
{
public:

    struct InOutSignature
    {
        std::string semanticName;
        uint32_t ui32SemanticIndex;
        SPECIAL_NAME eSystemValueType;
        INOUT_COMPONENT_TYPE eComponentType;
        uint32_t ui32Register;
        uint32_t ui32Mask;
        uint32_t ui32ReadWriteMask;

        int iRebase; // If mask does not start from zero, this indicates the offset that needs to be subtracted from each swizzle

        uint32_t ui32Stream;
        MIN_PRECISION eMinPrec;

        std::set<uint32_t> isIndexed; // Set of phases where this input/output is part of a index range.
        std::map<uint32_t, uint32_t> indexStart; // If indexed, contains the start index for the range
        std::map<uint32_t, uint32_t> index; // If indexed, contains the current index relative to the index start.
    };

    ShaderInfo() :
        ui32MajorVersion(),
        ui32MinorVersion(),
        psResourceBindings(),
        psConstantBuffers(),
        psThisPointerConstBuffer(),
        psClassTypes(),
        psClassInstances()
    {}

    SHADER_VARIABLE_TYPE GetTextureDataType(uint32_t regNo);

    int GetResourceFromBindingPoint(const ResourceGroup eGroup, const uint32_t ui32BindPoint, const ResourceBinding** ppsOutBinding) const;

    void GetConstantBufferFromBindingPoint(const ResourceGroup eGroup, const uint32_t ui32BindPoint, const ConstantBuffer** ppsConstBuf) const;

    int GetInterfaceVarFromOffset(uint32_t ui32Offset, ShaderVar** ppsShaderVar) const;

    int GetInputSignatureFromRegister(const uint32_t ui32Register, const uint32_t ui32Mask, const InOutSignature** ppsOut, bool allowNull = false) const;
    int GetInputSignatureFromType(const uint32_t uiType, const InOutSignature** ppsOut, bool allowNull = true) const;
    int GetPatchConstantSignatureFromRegister(const uint32_t ui32Register, const uint32_t ui32Mask, const InOutSignature** ppsOut, bool allowNull = false) const;
    int GetOutputSignatureFromRegister(const uint32_t ui32Register,
        const uint32_t ui32CompMask,
        const uint32_t ui32Stream,
        const InOutSignature** ppsOut,
        bool allowNull = false) const;

    int GetOutputSignatureFromSystemValue(SPECIAL_NAME eSystemValueType, uint32_t ui32SemanticIndex, const InOutSignature** ppsOut) const;

    static ResourceGroup ResourceTypeToResourceGroup(ResourceType);

    static uint32_t GetCBVarSize(const ShaderVarType* psType, bool matrixAsVectors, bool wholeArraySize = false);

    static int GetShaderVarFromOffset(const uint32_t ui32Vec4Offset,
        const uint32_t(&pui32Swizzle)[4],
        const ConstantBuffer* psCBuf,
        const ShaderVarType** ppsShaderVar,
        bool* isArray,
        std::vector<uint32_t>* arrayIndices,
        int32_t* pi32Rebase,
        uint32_t flags);

    static std::string GetShaderVarIndexedFullName(const ShaderVarType* psShaderVar, const std::vector<uint32_t>& indices, const std::string& dynamicIndex, bool revertDynamicIndexCalc, bool matrixAsVectors);

    // Apply shader precision information to resource bindings
    void AddSamplerPrecisions(HLSLccSamplerPrecisionInfo &info);

    uint32_t ui32MajorVersion;
    uint32_t ui32MinorVersion;

    std::vector<InOutSignature> psInputSignatures;
    std::vector<InOutSignature> psOutputSignatures;
    std::vector<InOutSignature> psPatchConstantSignatures;

    std::vector<ResourceBinding> psResourceBindings;

    std::vector<ConstantBuffer> psConstantBuffers;
    ConstantBuffer* psThisPointerConstBuffer;

    std::vector<ClassType> psClassTypes;
    std::vector<ClassInstance> psClassInstances;

    //Func table ID to class name ID.
    HLSLcc::growing_vector<uint32_t> aui32TableIDToTypeID;

    HLSLcc::growing_vector<uint32_t> aui32ResourceMap[RGROUP_COUNT];

    HLSLcc::growing_vector<ShaderVarType> sGroupSharedVarType;

    TESSELLATOR_PARTITIONING eTessPartitioning;
    TESSELLATOR_OUTPUT_PRIMITIVE eTessOutPrim;
    uint32_t ui32TessInputControlPointCount;
    uint32_t ui32TessOutputControlPointCount;
    TESSELLATOR_DOMAIN eTessDomain;
    bool bEarlyFragmentTests;
};
