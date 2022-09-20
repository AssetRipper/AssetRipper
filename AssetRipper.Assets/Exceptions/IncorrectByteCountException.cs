namespace AssetRipper.Assets.Exceptions
{
	public sealed class IncorrectByteCountException : Exception
	{
		public IncorrectByteCountException(long actual, long expected) : base(GetMessage(actual, expected))
		{
		}

		private static string GetMessage(long actual, long expected)
		{
			return $"Incorrect number of bytes read. Read {actual} but expected {expected}";
		}

		public static void ThrowIf(Stream stream, long basePosition, long size)
		{
			if (stream.Position - basePosition != size)
			{
				throw new IncorrectByteCountException(stream.Position - basePosition, size);
			}
		}
	}
}
