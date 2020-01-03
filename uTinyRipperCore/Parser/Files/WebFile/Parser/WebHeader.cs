using System;

namespace uTinyRipper.WebFiles
{
	public sealed class WebHeader : IEndianReadable
	{
		internal static bool IsWebHeader(EndianReader reader)
		{
			if (reader.BaseStream.Length - reader.BaseStream.Position > Signature.Length)
			{
				long position = reader.BaseStream.Position;
				bool isRead = reader.ReadStringZeroTerm(Signature.Length + 1, out string signature);
				reader.BaseStream.Position = position;
				if (isRead)
				{
					return signature == Signature;
				}
			}
			return false;
		}

		public void Read(EndianReader reader)
		{
			string signature = reader.ReadStringZeroTerm();
			if (signature != Signature)
			{
				throw new Exception($"Signature '{signature}' doesn't match to '{Signature}'");
			}
		}

		private const string Signature = "UnityWebData1.0";
	}
}
