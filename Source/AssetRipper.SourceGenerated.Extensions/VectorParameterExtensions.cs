using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Subclasses.VectorParameter;

namespace AssetRipper.SourceGenerated.Extensions;

public static class VectorParameterExtensions
{
	public static void SetValues(this IVectorParameter parameter, string name, ShaderParamType type, int index, int columns)
	{
		//parameter.Name = name;//Name doesn't exist
		parameter.NameIndex = -1;
		parameter.Index = index;
		parameter.ArraySize = 0;
		parameter.Type = (sbyte)type;
		parameter.Dim = (sbyte)columns;
	}

	public static ShaderParamType GetType_(this IVectorParameter parameter)
	{
		return (ShaderParamType)parameter.Type;
	}
}
