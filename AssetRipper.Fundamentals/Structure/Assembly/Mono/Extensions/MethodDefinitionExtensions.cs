using Mono.Cecil;

namespace AssetRipper.Core.Structure.Assembly.Mono.Extensions
{
	public static class MethodDefinitionExtensions
	{
		/*
		 * IsVarArg
		 * GetSentinelPosition
		 */

		public static bool IsVarArg(this IMethodSignature self)
		{
			return (self.CallingConvention & MethodCallingConvention.VarArg) != 0;
		}

		public static int GetSentinelPosition(this IMethodSignature self)
		{
			if (!self.HasParameters)
			{
				return -1;
			}

			global::Mono.Collections.Generic.Collection<ParameterDefinition> parameters = self.Parameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				if (parameters[i].ParameterType.IsSentinel)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
