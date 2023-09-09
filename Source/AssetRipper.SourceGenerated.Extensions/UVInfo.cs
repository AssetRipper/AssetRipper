namespace AssetRipper.SourceGenerated.Extensions;

public readonly record struct UVInfo(uint Value)
{
	const int kInfoBitsPerUV = 4;
	const int kUVDimensionMask = 0b0011;
	const int kUVChannelExists = 0b0100;
	const uint uvChannelMask = (1u << kInfoBitsPerUV) - 1u;
	const int kMaxTexCoordShaderChannels = 8;

	public void GetChannelInfo(int index, out bool exists, out int dimension)
	{
		ValidateIndex(index);
		uint texCoordBits = GetTextureCoordinateBits(index);
		exists = (texCoordBits & kUVChannelExists) != 0;
		dimension = 1 + (int)(texCoordBits & kUVDimensionMask);
	}

	private uint GetTextureCoordinateBits(int index)
	{
		int bitOffset = index * kInfoBitsPerUV;
		uint texCoordBits = Value >> bitOffset & uvChannelMask;
		return texCoordBits;
	}

	public UVInfo AddChannelInfo(int index, bool exists, int dimension)
	{
		ValidateIndex(index);
		ValidateDimension(dimension);

		int bitOffset = index * kInfoBitsPerUV;
		uint texCoordBits = (exists ? kUVChannelExists : 0u) | (uint)(dimension - 1);
		return Value & ~(uvChannelMask << bitOffset) | texCoordBits << bitOffset;
	}

	private static void ValidateDimension(int dimension)
	{
		if (dimension < 1 || dimension > 1 + kUVDimensionMask)
		{
			throw new ArgumentOutOfRangeException(nameof(dimension));
		}
	}

	private static void ValidateIndex(int index)
	{
		if (index < 0 || index >= kMaxTexCoordShaderChannels)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	//Implicit conversions
	public static implicit operator uint(UVInfo uvInfo) => uvInfo.Value;
	public static implicit operator UVInfo(uint value) => new(value);

	public override string ToString() => Value.ToString();

	public static UVInfo Zero => default;
}
