namespace uTinyRipper
{
	public enum EndianType
	{
		/// <summary>
		/// Inversed endian, most significant part first, 0x04 0x03 0x02 0x01 
		/// </summary>
		BigEndian,
		/// <summary>
		/// Ordinal endian, less significant part first, 0x01 0x02 0x03 0x4
		/// </summary>
		LittleEndian,
	}
}
