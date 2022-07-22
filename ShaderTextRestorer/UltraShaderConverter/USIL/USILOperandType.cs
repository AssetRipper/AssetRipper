﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderLabConvert
{
    public enum USILOperandType
    {
        None,
		Null,
        Comment,

        TempRegister,
		IndexableTempRegister,
        InputRegister,
        OutputRegister,
        ResourceRegister,
        SamplerRegister,

        ConstantBuffer,

        Sampler2D,
        Sampler3D,
        SamplerCube,
        Sampler2DArray,
        SamplerCubeArray,

		InputCoverageMask,
		InputThreadGroupID,
		InputThreadID,
		InputThreadIDInGroup,
		InputThreadIDInGroupFlattened,
		InputPrimitiveID,
		InputForkInstanceID,
		InputGSInstanceID,
		InputDomainPoint,
		OutputControlPointID,
		OutputDepth,
		OutputCoverageMask,
		OutputDepthGreaterEqual,
		OutputDepthLessEqual,
		StencilRef,

		ImmediateInt,
        ImmediateFloat,
		ImmediateConstantBuffer,
		Matrix,

        Multiple // i.e. fixed2(cb1, cb2)
    }
}
