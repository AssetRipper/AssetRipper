using System.Reflection;
using System.Runtime.CompilerServices;

namespace AssetRipper.IO.Files.SourceGenerator;

internal static class ParameterExtensions
{
	public static bool IsParams(this ParameterInfo parameter)
	{
		return parameter.GetCustomAttribute<ParamCollectionAttribute>() is not null;
	}

	public static string GetParamsPrefix(this ParameterInfo parameter)
	{
		return parameter.IsParams() ? "params " : "";
	}
}
