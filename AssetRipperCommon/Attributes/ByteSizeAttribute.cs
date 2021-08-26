using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
	public class ByteSizeAttribute : Attribute
	{
		int Size { get; }

		public ByteSizeAttribute()
		{
			Size = -1;
		}

		public ByteSizeAttribute(int byteSize)
		{
			Size = byteSize;
		}
	}
}
