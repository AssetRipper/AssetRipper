using System;
using System.Text;

namespace AssetRipper.Core.Classes
{
	public interface ITextAsset : INamedObject
	{
		/// <summary>
		/// NOTE: According to the type trees, this is a string, but since binary files are serialized as TextAsset, we have to store its content as byte array
		/// </summary>
		byte[] Script { get; }
	}

	public static class TextAssetExtensions
	{
		/// <summary>
		/// Parse the script data with the an encoding
		/// </summary>
		/// <param name="textAsset">The relevant TextAsset</param>
		/// <param name="encoding">The encoding to parse with</param>
		/// <remarks>
		/// UTF8 is the default encoding used by Unity
		/// </remarks>
		/// <returns>The parsed string</returns>
		public static string ParseWithEncoding(this ITextAsset textAsset, Encoding encoding)
		{
			return encoding.GetString(textAsset.Script ?? Array.Empty<byte>());
		}

		/// <summary>
		/// Parse the script data with the UTF8 encoding
		/// </summary>
		/// <param name="textAsset">The relevant TextAsset</param>
		/// <remarks>
		/// UTF8 is the default encoding used by Unity
		/// </remarks>
		/// <returns>The parsed string</returns>
		public static string ParseWithUTF8(this ITextAsset textAsset) => textAsset.ParseWithEncoding(Encoding.UTF8);
	}
}
