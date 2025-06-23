using AssetRipper.SourceGenerated.Subclasses.GUID;

namespace AssetRipper.SourceGenerated.Extensions;

public static class GuidExtensions
{
	public static void CopyValues(this GUID destination, UnityGuid source)
	{
		destination.Data_0_ = source.Data0;
		destination.Data_1_ = source.Data1;
		destination.Data_2_ = source.Data2;
		destination.Data_3_ = source.Data3;
	}
}
