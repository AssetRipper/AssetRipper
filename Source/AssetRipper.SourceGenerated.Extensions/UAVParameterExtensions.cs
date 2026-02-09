using AssetRipper.SourceGenerated.Subclasses.UAVParameter;

namespace AssetRipper.SourceGenerated.Extensions;

public static class UAVParameterExtensions
{
	public static void SetValues(this IUAVParameter parameter, string name, int index, int originalIndex)
	{
		//parameter.Name = name;//Name doesn't exist
		parameter.NameIndex = -1;
		parameter.Index = index;
		parameter.OriginalIndex = originalIndex;
	}
}
