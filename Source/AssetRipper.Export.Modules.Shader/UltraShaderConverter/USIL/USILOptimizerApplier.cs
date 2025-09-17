using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Fixers;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Metadders;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL.Optimizers;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public static class USILOptimizerApplier
{
	/// <summary>
	/// An array of optimizers to apply.
	/// </summary>
	/// <remarks>
	/// Order is important. Calling <see cref="IUSILOptimizer.Run(UShaderProgram, ShaderSubProgram)"/> should not modify
	/// the state of the optimizer.
	/// </remarks>
	private static readonly IUSILOptimizer[] OPTIMIZER_TYPES =
	[
		// do metadders first
		new USILCBufferMetadder(),
		new USILSamplerMetadder(),
		new USILInputOutputMetadder(),
		
		// do fixes (you really should have these enabled!)
		new USILSamplerTypeFixer(),
		new USILGetDimensionsFixer(),

		// do detection optimizers which usually depend on metadders
		//new USILMatrixMulOptimizer(), // I don't trust this code so it's commented for now
		
		// do simplification optimizers last when detection has been finished
		new USILCompareOrderOptimizer(),
		new USILAddNegativeOptimizer(),
		new USILAndOptimizer(),
		new USILForLoopOptimizer(),
	];

	public static void Apply(UShaderProgram shader, ShaderSubProgram shaderData)
	{
		for (int i = 0; i < OPTIMIZER_TYPES.Length; i++)
		{
			OPTIMIZER_TYPES[i].Run(shader, shaderData);
		}
	}
}
