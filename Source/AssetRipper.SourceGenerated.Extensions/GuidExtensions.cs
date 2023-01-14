using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Subclasses.GUID;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class GuidExtensions
	{
		public static void CopyValues(this GUID destination, UnityGUID source)
		{
			destination.m_Data_0_ = source.Data0;
			destination.m_Data_1_ = source.Data1;
			destination.m_Data_2_ = source.Data2;
			destination.m_Data_3_ = source.Data3;
		}
	}
}
