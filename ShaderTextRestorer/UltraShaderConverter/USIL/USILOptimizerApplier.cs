using ShaderTextRestorer.ShaderBlob;
using System;
using System.Collections.Generic;

namespace ShaderLabConvert
{
	public static class USILOptimizerApplier
	{
		// order is important
		// they should probably be separated into different lists in the future
		// when I work out what categories there will be
		private static readonly List<Type> OPTIMIZER_TYPES = new()
		{
            // do metadders first
            typeof(USILCBufferMetadder),
			typeof(USILSamplerMetadder),
			typeof(USILInputOutputMetadder),
			
			// do fixes (you really should have these enabled!)
			typeof(USILSamplerTypeFixer),
			typeof(USILGetDimensionsFixer),

            // do detection optimizers which usually depend on metadders
            //typeof(USILMatrixMulOptimizer), // I don't trust this code so it's commented for now
			
            // do simplification optimizers last when detection has been finished
            typeof(USILCompareOrderOptimizer),
			typeof(USILAddNegativeOptimizer),
			typeof(USILAndOptimizer),
			typeof(USILForLoopOptimizer)
		};

		public static void Apply(UShaderProgram shader, ShaderSubProgram shaderData)
		{
			foreach (Type optimizerType in OPTIMIZER_TYPES)
			{
				IUSILOptimizer optimizer = (IUSILOptimizer?)Activator.CreateInstance(optimizerType)
					?? throw new NullReferenceException($"Could not create an instance of type {optimizerType}");
				optimizer.Run(shader, shaderData);
			}
		}
	}
}
