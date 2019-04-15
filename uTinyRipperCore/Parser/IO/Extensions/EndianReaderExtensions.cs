namespace uTinyRipper
{
	public static class EndianReaderExtensions
	{
		public static string[][] ReadStringArrayArray(this EndianReader _this)
		{
			int count = _this.ReadInt32();
			string[][] array = new string[count][];
			for (int i = 0; i < count; i++)
			{
				array[i] = _this.ReadStringArray();
			}
			return array;
		}
	}
}
