using System;
using System.Text;

namespace AssetRipper.Core.Classes
{
	public interface ITextAsset : INamedObject
	{
		byte[] Script { get; }
	}

	public static class TextAssetExtensions
	{
		public static string ParseWithEncoding(this ITextAsset textAsset, Encoding encoding)
		{
			return encoding.GetString(textAsset.Script ?? Array.Empty<byte>());
		}

		public static string ParseWithUTF8(this ITextAsset textAsset) => textAsset.ParseWithEncoding(Encoding.UTF8);
	}
}
