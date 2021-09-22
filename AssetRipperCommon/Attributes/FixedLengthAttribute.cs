using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class FixedLengthAttribute : Attribute
	{
		int Length { get; }

		public FixedLengthAttribute(int length)
		{
			Length = length;
		}
	}
}
