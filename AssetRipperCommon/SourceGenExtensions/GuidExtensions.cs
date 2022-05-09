using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Subclasses.GUID;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GuidExtensions
	{
		public static void SetValues(this GUID destination, UnityGUID source)
		{
			destination.m_Data_0_ = source.Data0;
			destination.m_Data_1_ = source.Data1;
			destination.m_Data_2_ = source.Data2;
			destination.m_Data_3_ = source.Data3;
		}

		public static void CopyValues(this GUID destination, GUID source)
		{
			destination.m_Data_0_ = source.m_Data_0_;
			destination.m_Data_1_ = source.m_Data_1_;
			destination.m_Data_2_ = source.m_Data_2_;
			destination.m_Data_3_ = source.m_Data_3_;
		}
	}
}
