namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public enum USILInstructionType
{
	// math
	Move, //dx: mov
	MoveConditional, //dx: movc
	Add, //dx: add/iadd
	Subtract, //dx: --- (add/iadd)
	Multiply, //dx: mul/imul
	Divide, //dx: div
	MultiplyAdd, //dx: mad

	And, //dx: and
	Or, //dx: or
	Xor, //dx: xor
	Not, //dx: not

	ShiftLeft, //dx: ishl
	ShiftRight, //dx: ishr

	Floor, //dx: round_ni
	Ceiling, //dx: round_pi
	Round, //dx: round_ne
	Truncate, //dx: round_z
	IntToFloat, //dx: itof
	UIntToFloat, //dx: utof
	FloatToInt, //dx: ftoi
	FloatToUInt, //dx: ftou

	Minimum, //dx: min
	Maximum, //dx: max

	SquareRoot, //dx: sqrt
	SquareRootReciprocal, //dx: rsq

	Exponential, //dx: exp
	Logarithm2, //dx: log

	Sine, //dx: --- (sincos)
	Cosine, //dx: --- (sincos)

	DotProduct2, //dx: dp2
	DotProduct3, //dx: dp3
	DotProduct4, //dx: dp4

	Reciprocal, //dx: rcp
	Fractional, //dx: frc

	ResourceDimensionInfo, //dx: resinfo
	SampleCountInfo, //dx: sampleinfo

	// probably should be an argument
	DerivativeRenderTargetX, //dx: deriv_rtx
	DerivativeRenderTargetY, //dy: deriv_rty
	DerivativeRenderTargetXCoarse, //dx: deriv_rtx_coarse
	DerivativeRenderTargetYCoarse, //dx: deriv_rtx_coarse
	DerivativeRenderTargetXFine, //dy: deriv_rty_fine
	DerivativeRenderTargetYFine, //dy: deriv_rty_fine

	// comparisons
	Equal, //dx: eq
	NotEqual, //dx: ne
	GreaterThan, //dx: --- (ge/lt)
	GreaterThanOrEqual, //dx: ge
	LessThan, //dx: lt
	LessThanOrEqual, //dx: --- (ge/lt)

	// branching
	IfTrue, //dx: if(_z)
	IfFalse, //dx: if(_nz)
	Else, //dx: else
	EndIf, //dx: endif
	Return, //dx: return
	Loop, //dx: loop
	ForLoop, //dx: --- (loop, ige for example)
	EndLoop, //dx: endloop
	Break, //dx: break
	Continue, //dx: continue

	// graphics
	Discard, //dx: discard
	Sample, //dx: sample
	SampleLODBias, //dx: sample_b
	SampleComparison, //dx: sample_c
	SampleComparisonLODZero, //dx: sample_c_lz
	SampleLOD, //dx: sample_l
	SampleDerivative, //dx: sample_d
	LoadResource, //dx: ld
	LoadResourceMultisampled, //dx: ld2dms
	LoadResourceStructured, //dx: ld_structured

	// artifical instructions
	GetDimensions, //dx: --- (resinfo/sampleinfo)

	// math
	MultiplyMatrixByVector,

	// unity
	UnityObjectToClipPos,
	UnityObjectToWorldNormal,
	WorldSpaceViewDir,

	// extra
	Comment
}
