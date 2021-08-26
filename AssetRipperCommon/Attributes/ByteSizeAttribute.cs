using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
	public sealed class ByteSizeAttribute : Attribute
	{
		public int Size { get; }

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
