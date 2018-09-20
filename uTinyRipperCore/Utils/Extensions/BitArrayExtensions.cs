using System.Collections;

namespace uTinyRipper
{
	public static class BitArrayExtensions
	{
		public static uint ToUInt32(this BitArray _this)
		{
			int value = 0;
			for(int i = 0; i < 8 * sizeof(uint); i++)
			{
				if(_this[i])
				{
					value |= (1 << i);
				}
			}
			return unchecked((uint)value);
		}
	}
}
