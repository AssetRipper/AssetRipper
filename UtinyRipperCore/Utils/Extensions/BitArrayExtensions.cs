using System.Collections;

namespace UtinyRipper
{
	public static class BitArrayExtensions
	{
		public static uint ToUInt32(this BitArray _this)
		{
			int[] bitArray = new int[1];
			_this.CopyTo(bitArray, 0);
			return unchecked((uint)bitArray[0]);
		}
	}
}
