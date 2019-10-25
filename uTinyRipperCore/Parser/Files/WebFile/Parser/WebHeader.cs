using System;

namespace uTinyRipper.WebFiles
{
	public sealed class WebHeader : IEndianReadable
	{
		internal static bool IsWebHeader(EndianReader reader)
		{
			if (reader.BaseStream.Length - reader.BaseStream.Position > Signature.Length)
			{
				if (reader.ReadStringZeroTerm(Signature.Length, out string signature))
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
