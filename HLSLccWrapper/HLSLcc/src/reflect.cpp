#include "internal_includes/reflect.h"
#include "internal_includes/debug.h"
#include "internal_includes/decode.h"
#include "bstrlib.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

static void FormatVariableName(std::string & Name)
{
    /* MSDN http://msdn.microsoft.com/en-us/library/windows/desktop/bb944006(v=vs.85).aspx
       The uniform function parameters appear in the
       constant table prepended with a dollar sign ($),
       unlike the global variables. The dollar sign is
       required to avoid name collisions between local
       uniform inputs and global variables of the same name.*/

    /* Leave $ThisPointer, $Element and $Globals as-is.
       Otherwise remove $ character ($ is not a valid character for GLSL variable names). */
    if (Name[0] == '$')
    {
        if (strcmp(Name.c_str(), "$Element") != 0 &&
            strcmp(Name.c_str(), "$Globals") != 0 &&
            strcmp(Name.c_str(), "$ThisPointer") != 0)
        {
            Name[0] = '_';
        }
    }
}

static std::string ReadStringFromTokenStream(const uint32_t* tokens)
{
    char* charTokens = (char*)tokens;
    return std::string(charTokens);
}

static int MaskToRebaseOffset(const uint32_t mask)
{
    int res = 0;
    uint32_t m = mask;
    while ((m & 1) == 0)
    {
        res++;
        m = m >> 1;
    }
    return res;
}

static void ReadInputSignatures(const uint32_t* pui32Tokens,
    ShaderInfo* psShaderInfo,
    const int extended)
{
    uint32_t i;

    const uint32_t* pui32FirstSignatureToken = pui32Tokens;
    const uint32_t ui32ElementCount = *pui32Tokens++;
    /* const uint32_t ui32Key = * */ pui32Tokens++;

    psShaderInfo->psInputSignatures.clear();
    psShaderInfo->psInputSignatures.resize(ui32ElementCount);

    for (i = 0; i < ui32ElementCount; ++i)
    {
        uint32_t ui32ComponentMasks;
        ShaderInfo::InOutSignature* psCurrentSignature = &psShaderInfo->psInputSignatures[i];
        uint32_t ui32SemanticNameOffset;

        psCurrentSignature->ui32Stream = 0;
        psCurrentSignature->eMinPrec = MIN_PRECISION_DEFAULT;

        if (extended)
            psCurrentSignature->ui32Stream = *pui32Tokens++;

        ui32SemanticNameOffset = *pui32Tokens++;
        psCurrentSignature->ui32SemanticIndex = *pui32Tokens++;
        psCurrentSignature->eSystemValueType = (SPECIAL_NAME)*pui32Tokens++;
        psCurrentSignature->eComponentType = (INOUT_COMPONENT_TYPE)*pui32Tokens++;
        psCurrentSignature->ui32Register = *pui32Tokens++;

        ui32ComponentMasks = *pui32Tokens++;
        psCurrentSignature->ui32Mask = ui32ComponentMasks & 0x7F;
        //Shows which components are read
        psCurrentSignature->ui32ReadWriteMask = (ui32ComponentMasks & 0x7F00) >> 8;
        psCurrentSignature->iRebase = MaskToRebaseOffset(psCurrentSignature->ui32Mask);

        if (extended)
            psCurrentSignature->eMinPrec = (MIN_PRECISION)*pui32Tokens++;

        psCurrentSignature->semanticName = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstSignatureToken + ui32SemanticNameOffset));
    }
}

static void ReadOutputSignatures(const uint32_t* pui32Tokens,
    ShaderInfo* psShaderInfo,
    const int minPrec,
    const int streams)
{
    uint32_t i;

    const uint32_t* pui32FirstSignatureToken = pui32Tokens;
    const uint32_t ui32ElementCount = *pui32Tokens++;
    /*const uint32_t ui32Key = * */ pui32Tokens++;

    psShaderInfo->psOutputSignatures.clear();
    psShaderInfo->psOutputSignatures.resize(ui32ElementCount);

    for (i = 0; i < ui32ElementCount; ++i)
    {
        uint32_t ui32ComponentMasks;
        ShaderInfo::InOutSignature* psCurrentSignature = &psShaderInfo->psOutputSignatures[i];
        uint32_t ui32SemanticNameOffset;

        psCurrentSignature->ui32Stream = 0;
        psCurrentSignature->eMinPrec = MIN_PRECISION_DEFAULT;

        if (streams)
            psCurrentSignature->ui32Stream = *pui32Tokens++;

        ui32SemanticNameOffset = *pui32Tokens++;
        psCurrentSignature->ui32SemanticIndex = *pui32Tokens++;
        psCurrentSignature->eSystemValueType = (SPECIAL_NAME)*pui32Tokens++;
        psCurrentSignature->eComponentType = (INOUT_COMPONENT_TYPE)*pui32Tokens++;
        psCurrentSignature->ui32Register = *pui32Tokens++;

        // Massage some special inputs/outputs to match the types of GLSL counterparts
        if (psCurrentSignature->eSystemValueType == NAME_RENDER_TARGET_ARRAY_INDEX)
        {
            psCurrentSignature->eComponentType = INOUT_COMPONENT_SINT32;
        }

        ui32ComponentMasks = *pui32Tokens++;
        psCurrentSignature->ui32Mask = ui32ComponentMasks & 0x7F;
        //Shows which components are NEVER written.
        psCurrentSignature->ui32ReadWriteMask = (ui32ComponentMasks & 0x7F00) >> 8;
        psCurrentSignature->iRebase = MaskToRebaseOffset(psCurrentSignature->ui32Mask);

        if (minPrec)
            psCurrentSignature->eMinPrec = (MIN_PRECISION)*pui32Tokens++;

        psCurrentSignature->semanticName = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstSignatureToken + ui32SemanticNameOffset));
    }
}

static void ReadPatchConstantSignatures(const uint32_t* pui32Tokens,
    ShaderInfo* psShaderInfo,
    const int minPrec,
    const int streams)
{
    uint32_t i;

    const uint32_t* pui32FirstSignatureToken = pui32Tokens;
    const uint32_t ui32ElementCount = *pui32Tokens++;
    /*const uint32_t ui32Key = * */ pui32Tokens++;

    psShaderInfo->psPatchConstantSignatures.clear();
    psShaderInfo->psPatchConstantSignatures.resize(ui32ElementCount);

    for (i = 0; i < ui32ElementCount; ++i)
    {
        uint32_t ui32ComponentMasks;
        ShaderInfo::InOutSignature* psCurrentSignature = &psShaderInfo->psPatchConstantSignatures[i];
        uint32_t ui32SemanticNameOffset;

        psCurrentSignature->ui32Stream = 0;
        psCurrentSignature->eMinPrec = MIN_PRECISION_DEFAULT;

        if (streams)
            psCurrentSignature->ui32Stream = *pui32Tokens++;

        ui32SemanticNameOffset = *pui32Tokens++;
        psCurrentSignature->ui32SemanticIndex = *pui32Tokens++;
        psCurrentSignature->eSystemValueType = (SPECIAL_NAME)*pui32Tokens++;
        psCurrentSignature->eComponentType = (INOUT_COMPONENT_TYPE)*pui32Tokens++;
        psCurrentSignature->ui32Register = *pui32Tokens++;

        // Massage some special inputs/outputs to match the types of GLSL counterparts
        if (psCurrentSignature->eSystemValueType == NAME_RENDER_TARGET_ARRAY_INDEX)
        {
            psCurrentSignature->eComponentType = INOUT_COMPONENT_SINT32;
        }

        ui32ComponentMasks = *pui32Tokens++;
        psCurrentSignature->ui32Mask = ui32ComponentMasks & 0x7F;
        //Shows which components are NEVER written.
        psCurrentSignature->ui32ReadWriteMask = (ui32ComponentMasks & 0x7F00) >> 8;
        psCurrentSignature->iRebase = MaskToRebaseOffset(psCurrentSignature->ui32Mask);

        if (minPrec)
            psCurrentSignature->eMinPrec = (MIN_PRECISION)*pui32Tokens++;

        psCurrentSignature->semanticName = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstSignatureToken + ui32SemanticNameOffset));
    }
}

static const uint32_t* ReadResourceBinding(ShaderInfo* psShaderInfo, const uint32_t* pui32FirstResourceToken, const uint32_t* pui32Tokens, ResourceBinding* psBinding, uint32_t decodeFlags)
{
    uint32_t ui32NameOffset = *pui32Tokens++;

    psBinding->name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstResourceToken + ui32NameOffset));
    FormatVariableName(psBinding->name);

    psBinding->eType = (ResourceType) * pui32Tokens++;
    psBinding->ui32ReturnType = (RESOURCE_RETURN_TYPE)*pui32Tokens++;
    psBinding->eDimension = (REFLECT_RESOURCE_DIMENSION)*pui32Tokens++;
    psBinding->ui32NumSamples = *pui32Tokens++; // fxc generates 2^32 - 1 for non MS images
    psBinding->ui32BindPoint = *pui32Tokens++;
    psBinding->ui32BindCount = *pui32Tokens++;
    psBinding->ui32Flags = *pui32Tokens++;
    if (((psShaderInfo->ui32MajorVersion >= 5) && (psShaderInfo->ui32MinorVersion >= 1)) ||
        (psShaderInfo->ui32MajorVersion > 5))
    {
        psBinding->ui32Space = *pui32Tokens++;
        psBinding->ui32RangeID = *pui32Tokens++;
    }

    psBinding->ePrecision = REFLECT_RESOURCE_PRECISION_UNKNOWN;

    if (decodeFlags & HLSLCC_FLAG_SAMPLER_PRECISION_ENCODED_IN_NAME)
    {
        if (psBinding->name.rfind("_highp") == psBinding->name.length() - 6)
        {
            psBinding->ePrecision = REFLECT_RESOURCE_PRECISION_HIGHP;
            psBinding->name.resize(psBinding->name.length() - 6);
        }
        else if (psBinding->name.rfind("_mediump") == psBinding->name.length() - 8)
        {
            psBinding->ePrecision = REFLECT_RESOURCE_PRECISION_MEDIUMP;
            psBinding->name.resize(psBinding->name.length() - 8);
        }
        else if (psBinding->name.rfind("_lowp") == psBinding->name.length() - 5)
        {
            psBinding->ePrecision = REFLECT_RESOURCE_PRECISION_LOWP;
            psBinding->name.resize(psBinding->name.length() - 5);
        }
    }

    return pui32Tokens;
}

//Read D3D11_SHADER_TYPE_DESC
static void ReadShaderVariableType(const uint32_t ui32MajorVersion,
    const uint32_t* pui32FirstConstBufToken,
    const uint32_t* pui32tokens, ShaderVarType* varType)
{
    const uint16_t* pui16Tokens = (const uint16_t*)pui32tokens;
    uint16_t ui32MemberCount;
    uint32_t ui32MemberOffset;
    const uint32_t* pui32MemberTokens;
    uint32_t i;

    varType->Class = (SHADER_VARIABLE_CLASS)pui16Tokens[0];
    varType->Type = (SHADER_VARIABLE_TYPE)pui16Tokens[1];
    varType->Rows = pui16Tokens[2];
    varType->Columns = pui16Tokens[3];
    varType->Elements = pui16Tokens[4];

    varType->MemberCount = ui32MemberCount = pui16Tokens[5];
    varType->Members.clear();

    if (varType->ParentCount)
    {
        // Add empty brackets for array parents. Indices are filled in later in the printing codes.
        if (varType->Parent->Elements > 1)
            varType->fullName = varType->Parent->fullName + "[]." + varType->name;
        else
            varType->fullName = varType->Parent->fullName + "." + varType->name;
    }

    if (ui32MemberCount)
    {
        varType->Members.resize(ui32MemberCount);

        ui32MemberOffset = pui32tokens[3];

        pui32MemberTokens = (const uint32_t*)((const char*)pui32FirstConstBufToken + ui32MemberOffset);

        for (i = 0; i < ui32MemberCount; ++i)
        {
            uint32_t ui32NameOffset = *pui32MemberTokens++;
            uint32_t ui32MemberTypeOffset = *pui32MemberTokens++;

            varType->Members[i].Parent = varType;
            varType->Members[i].ParentCount = varType->ParentCount + 1;

            varType->Members[i].Offset = *pui32MemberTokens++;

            varType->Members[i].name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstConstBufToken + ui32NameOffset));

            ReadShaderVariableType(ui32MajorVersion, pui32FirstConstBufToken,
                (const uint32_t*)((const char*)pui32FirstConstBufToken + ui32MemberTypeOffset), &varType->Members[i]);
        }
    }
}

static const uint32_t* ReadConstantBuffer(ShaderInfo* psShaderInfo,
    const uint32_t* pui32FirstConstBufToken, const uint32_t* pui32Tokens, ConstantBuffer* psBuffer)
{
    uint32_t i;
    uint32_t ui32NameOffset = *pui32Tokens++;
    uint32_t ui32VarCount = *pui32Tokens++;
    uint32_t ui32VarOffset = *pui32Tokens++;
    const uint32_t* pui32VarToken = (const uint32_t*)((const char*)pui32FirstConstBufToken + ui32VarOffset);

    psBuffer->name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstConstBufToken + ui32NameOffset));
    FormatVariableName(psBuffer->name);

    psBuffer->asVars.clear();
    psBuffer->asVars.resize(ui32VarCount);

    for (i = 0; i < ui32VarCount; ++i)
    {
        //D3D11_SHADER_VARIABLE_DESC
        ShaderVar * const psVar = &psBuffer->asVars[i];

        uint32_t ui32TypeOffset;
        uint32_t ui32DefaultValueOffset;

        ui32NameOffset = *pui32VarToken++;

        psVar->name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstConstBufToken + ui32NameOffset));
        FormatVariableName(psVar->name);

        psVar->ui32StartOffset = *pui32VarToken++;
        psVar->ui32Size = *pui32VarToken++;

        //skip ui32Flags
        pui32VarToken++;

        ui32TypeOffset = *pui32VarToken++;

        psVar->sType.name = psVar->name;
        psVar->sType.fullName = psVar->name;
        psVar->sType.Parent = 0;
        psVar->sType.ParentCount = 0;
        psVar->sType.Offset = 0;
        psVar->sType.m_IsUsed = false;

        ReadShaderVariableType(psShaderInfo->ui32MajorVersion, pui32FirstConstBufToken,
            (const uint32_t*)((const char*)pui32FirstConstBufToken + ui32TypeOffset), &psVar->sType);

        ui32DefaultValueOffset = *pui32VarToken++;


        if (psShaderInfo->ui32MajorVersion  >= 5)
        {
            /*uint32_t StartTexture = * */ pui32VarToken++;
            /*uint32_t TextureSize = *  */ pui32VarToken++;
            /*uint32_t StartSampler = * */ pui32VarToken++;
            /*uint32_t SamplerSize = *  */ pui32VarToken++;
        }

        psVar->haveDefaultValue = 0;

        if (ui32DefaultValueOffset)
        {
            uint32_t i = 0;
            const uint32_t ui32NumDefaultValues = psVar->ui32Size / 4;
            const uint32_t* pui32DefaultValToken = (const uint32_t*)((const char*)pui32FirstConstBufToken + ui32DefaultValueOffset);

            //Always a sequence of 4-bytes at the moment.
            //bool const becomes 0 or 0xFFFFFFFF int, int & float are 4-bytes.
            ASSERT(psVar->ui32Size % 4 == 0);

            psVar->haveDefaultValue = 1;

            psVar->pui32DefaultValues.clear();
            psVar->pui32DefaultValues.resize(psVar->ui32Size / 4);

            for (i = 0; i < ui32NumDefaultValues; ++i)
            {
                psVar->pui32DefaultValues[i] = pui32DefaultValToken[i];
            }
        }
    }


    {
        psBuffer->ui32TotalSizeInBytes = *pui32Tokens++;

        //skip ui32Flags
        pui32Tokens++;
        //skip ui32BufferType
        pui32Tokens++;
    }

    return pui32Tokens;
}

static void ReadResources(const uint32_t* pui32Tokens,//in
    ShaderInfo* psShaderInfo,                //out
    uint32_t decodeFlags)
{
    ResourceBinding* psResBindings;
    ConstantBuffer* psConstantBuffers;
    const uint32_t* pui32ConstantBuffers;
    const uint32_t* pui32ResourceBindings;
    const uint32_t* pui32FirstToken = pui32Tokens;
    uint32_t i;

    const uint32_t ui32NumConstantBuffers = *pui32Tokens++;
    const uint32_t ui32ConstantBufferOffset = *pui32Tokens++;

    uint32_t ui32NumResourceBindings = *pui32Tokens++;
    uint32_t ui32ResourceBindingOffset = *pui32Tokens++;
    /*uint32_t ui32ShaderModel = * */ pui32Tokens++;
    /*uint32_t ui32CompileFlags = * */ pui32Tokens++;//D3DCompile flags? http://msdn.microsoft.com/en-us/library/gg615083(v=vs.85).aspx

    //Resources
    pui32ResourceBindings = (const uint32_t*)((const char*)pui32FirstToken + ui32ResourceBindingOffset);

    psShaderInfo->psResourceBindings.clear();
    psShaderInfo->psResourceBindings.resize(ui32NumResourceBindings);
    psResBindings = ui32NumResourceBindings == 0 ? NULL : &psShaderInfo->psResourceBindings[0];

    for (i = 0; i < ui32NumResourceBindings; ++i)
    {
        pui32ResourceBindings = ReadResourceBinding(psShaderInfo, pui32FirstToken, pui32ResourceBindings, psResBindings + i, decodeFlags);
        ASSERT(psResBindings[i].ui32BindPoint < MAX_RESOURCE_BINDINGS);
    }

    //Constant buffers
    pui32ConstantBuffers = (const uint32_t*)((const char*)pui32FirstToken + ui32ConstantBufferOffset);

    psShaderInfo->psConstantBuffers.clear();
    psShaderInfo->psConstantBuffers.resize(ui32NumConstantBuffers);
    psConstantBuffers = ui32NumConstantBuffers == 0 ? NULL : &psShaderInfo->psConstantBuffers[0];

    for (i = 0; i < ui32NumConstantBuffers; ++i)
    {
        pui32ConstantBuffers = ReadConstantBuffer(psShaderInfo, pui32FirstToken, pui32ConstantBuffers, psConstantBuffers + i);
    }

    //Map resource bindings to constant buffers
    if (psShaderInfo->psConstantBuffers.size())
    {
        /* HLSL allows the following:
         cbuffer A
         {...}
         cbuffer A
         {...}
         And both will be present in the assembly if used

         So we need to track which ones we matched already and throw an error if two buffers have the same name
        */
        std::vector<uint32_t> alreadyBound(ui32NumConstantBuffers, 0);
        for (i = 0; i < ui32NumResourceBindings; ++i)
        {
            ResourceGroup eRGroup;
            uint32_t cbufIndex = 0;

            eRGroup = ShaderInfo::ResourceTypeToResourceGroup(psResBindings[i].eType);

            //Find the constant buffer whose name matches the resource at the given resource binding point
            for (cbufIndex = 0; cbufIndex < psShaderInfo->psConstantBuffers.size(); cbufIndex++)
            {
                if (psConstantBuffers[cbufIndex].name == psResBindings[i].name && alreadyBound[cbufIndex] == 0)
                {
                    psShaderInfo->aui32ResourceMap[eRGroup][psResBindings[i].ui32BindPoint] = cbufIndex;
                    alreadyBound[cbufIndex] = 1;
                    break;
                }
            }
        }
    }
}

static const uint16_t* ReadClassType(const uint32_t* pui32FirstInterfaceToken, const uint16_t* pui16Tokens, ClassType* psClassType)
{
    const uint32_t* pui32Tokens = (const uint32_t*)pui16Tokens;
    uint32_t ui32NameOffset = *pui32Tokens;
    pui16Tokens += 2;

    psClassType->ui16ID = *pui16Tokens++;
    psClassType->ui16ConstBufStride = *pui16Tokens++;
    psClassType->ui16Texture = *pui16Tokens++;
    psClassType->ui16Sampler = *pui16Tokens++;

    psClassType->name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstInterfaceToken + ui32NameOffset));

    return pui16Tokens;
}

static const uint16_t* ReadClassInstance(const uint32_t* pui32FirstInterfaceToken, const uint16_t* pui16Tokens, ClassInstance* psClassInstance)
{
    uint32_t ui32NameOffset = *pui16Tokens++ << 16;
    ui32NameOffset |= *pui16Tokens++;

    psClassInstance->ui16ID = *pui16Tokens++;
    psClassInstance->ui16ConstBuf = *pui16Tokens++;
    psClassInstance->ui16ConstBufOffset = *pui16Tokens++;
    psClassInstance->ui16Texture = *pui16Tokens++;
    psClassInstance->ui16Sampler = *pui16Tokens++;

    psClassInstance->name = ReadStringFromTokenStream((const uint32_t*)((const char*)pui32FirstInterfaceToken + ui32NameOffset));

    return pui16Tokens;
}

static void ReadInterfaces(const uint32_t* pui32Tokens,
    ShaderInfo* psShaderInfo)
{
    uint32_t i;
    uint32_t ui32StartSlot;
    const uint32_t* pui32FirstInterfaceToken = pui32Tokens;
    const uint32_t ui32ClassInstanceCount = *pui32Tokens++;
    const uint32_t ui32ClassTypeCount = *pui32Tokens++;
    const uint32_t ui32InterfaceSlotRecordCount = *pui32Tokens++;
    /*const uint32_t ui32InterfaceSlotCount = * */ pui32Tokens++;
    const uint32_t ui32ClassInstanceOffset = *pui32Tokens++;
    const uint32_t ui32ClassTypeOffset = *pui32Tokens++;
    const uint32_t ui32InterfaceSlotOffset = *pui32Tokens++;

    const uint16_t* pui16ClassTypes = (const uint16_t*)((const char*)pui32FirstInterfaceToken + ui32ClassTypeOffset);
    const uint16_t* pui16ClassInstances = (const uint16_t*)((const char*)pui32FirstInterfaceToken + ui32ClassInstanceOffset);
    const uint32_t* pui32InterfaceSlots = (const uint32_t*)((const char*)pui32FirstInterfaceToken + ui32InterfaceSlotOffset);

    const uint32_t* pui32InterfaceSlotTokens = pui32InterfaceSlots;

    ClassType* psClassTypes;
    ClassInstance* psClassInstances;

    psShaderInfo->psClassTypes.clear();
    psShaderInfo->psClassTypes.resize(ui32ClassTypeCount);
    psClassTypes = &psShaderInfo->psClassTypes[0];

    for (i = 0; i < ui32ClassTypeCount; ++i)
    {
        pui16ClassTypes = ReadClassType(pui32FirstInterfaceToken, pui16ClassTypes, psClassTypes + i);
        psClassTypes[i].ui16ID = (uint16_t)i;
    }

    psShaderInfo->psClassInstances.clear();
    psShaderInfo->psClassInstances.resize(ui32ClassInstanceCount);
    psClassInstances = &psShaderInfo->psClassInstances[0];

    for (i = 0; i < ui32ClassInstanceCount; ++i)
    {
        pui16ClassInstances = ReadClassInstance(pui32FirstInterfaceToken, pui16ClassInstances, psClassInstances + i);
    }

    //Slots map function table to $ThisPointer cbuffer variable index
    ui32StartSlot = 0;
    for (i = 0; i < ui32InterfaceSlotRecordCount; ++i)
    {
        uint32_t k;

        const uint32_t ui32SlotSpan = *pui32InterfaceSlotTokens++;
        const uint32_t ui32Count = *pui32InterfaceSlotTokens++;
        const uint32_t ui32TypeIDOffset = *pui32InterfaceSlotTokens++;
        const uint32_t ui32TableIDOffset = *pui32InterfaceSlotTokens++;

        const uint16_t* pui16TypeID = (const uint16_t*)((const char*)pui32FirstInterfaceToken + ui32TypeIDOffset);
        const uint32_t* pui32TableID = (const uint32_t*)((const char*)pui32FirstInterfaceToken + ui32TableIDOffset);

        for (k = 0; k < ui32Count; ++k)
        {
            psShaderInfo->aui32TableIDToTypeID[*pui32TableID++] = *pui16TypeID++;
        }

        ui32StartSlot += ui32SlotSpan;
    }
}

void LoadShaderInfo(const uint32_t ui32MajorVersion,
    const uint32_t ui32MinorVersion,
    const ReflectionChunks* psChunks,
    ShaderInfo* psInfo,
    uint32_t decodeFlags)
{
    const uint32_t* pui32Inputs = psChunks->pui32Inputs;
    const uint32_t* pui32Inputs11 = psChunks->pui32Inputs11;
    const uint32_t* pui32Resources = psChunks->pui32Resources;
    const uint32_t* pui32Interfaces = psChunks->pui32Interfaces;
    const uint32_t* pui32Outputs = psChunks->pui32Outputs;
    const uint32_t* pui32Outputs11 = psChunks->pui32Outputs11;
    const uint32_t* pui32OutputsWithStreams = psChunks->pui32OutputsWithStreams;
    const uint32_t* pui32PatchConstants = psChunks->pui32PatchConstants;
    const uint32_t* pui32PatchConstants11 = psChunks->pui32PatchConstants11;

    psInfo->eTessOutPrim = TESSELLATOR_OUTPUT_UNDEFINED;
    psInfo->eTessPartitioning = TESSELLATOR_PARTITIONING_UNDEFINED;
    psInfo->ui32TessInputControlPointCount = 0;
    psInfo->ui32TessOutputControlPointCount = 0;
    psInfo->eTessDomain = TESSELLATOR_DOMAIN_UNDEFINED;
    psInfo->bEarlyFragmentTests = false;

    psInfo->ui32MajorVersion = ui32MajorVersion;
    psInfo->ui32MinorVersion = ui32MinorVersion;


    if (pui32Inputs)
        ReadInputSignatures(pui32Inputs, psInfo, 0);
    if (pui32Inputs11)
        ReadInputSignatures(pui32Inputs11, psInfo, 1);
    if (pui32Resources)
        ReadResources(pui32Resources, psInfo, decodeFlags);
    if (pui32Interfaces)
        ReadInterfaces(pui32Interfaces, psInfo);
    if (pui32Outputs)
        ReadOutputSignatures(pui32Outputs, psInfo, 0, 0);
    if (pui32Outputs11)
        ReadOutputSignatures(pui32Outputs11, psInfo, 1, 1);
    if (pui32OutputsWithStreams)
        ReadOutputSignatures(pui32OutputsWithStreams, psInfo, 0, 1);
    if (pui32PatchConstants)
        ReadPatchConstantSignatures(pui32PatchConstants, psInfo, 0, 0);
    if (pui32PatchConstants11)
        ReadPatchConstantSignatures(pui32PatchConstants11, psInfo, 1, 1);

    {
        uint32_t i;
        for (i = 0; i < psInfo->psConstantBuffers.size(); ++i)
        {
            if (psInfo->psConstantBuffers[i].name == "$ThisPointer")
            {
                psInfo->psThisPointerConstBuffer = &psInfo->psConstantBuffers[i];
            }
        }
    }
}
