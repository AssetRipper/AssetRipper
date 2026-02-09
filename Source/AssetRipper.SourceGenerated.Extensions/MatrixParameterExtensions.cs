using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Subclasses.MatrixParameter;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MatrixParameterExtensions
{
	public static void SetValues(this IMatrixParameter parameter, string name, ShaderParamType type, int index, int rowCount, int columnCount)
	{
		//parameter.Name = name;//Name doesn't exist
		parameter.NameIndex = -1;
		parameter.Index = index;
		parameter.ArraySize = 0;
		parameter.Type = (sbyte)type;
		parameter.RowCount = (sbyte)rowCount;
		//parameter.ColumnCount = (sbyte)columnCount;//doesn't exist; default value is 4
	}

	public static ShaderParamType GetType_(this IMatrixParameter parameter)
	{
		return (ShaderParamType)parameter.Type;
	}
}
