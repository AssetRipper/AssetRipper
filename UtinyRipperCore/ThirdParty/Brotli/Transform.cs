/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/
namespace Brotli
{
	/// <summary>Transformations on dictionary words.</summary>
	internal sealed class Transform
	{
		private readonly byte[] prefix;

		private readonly int type;

		private readonly byte[] suffix;

		internal Transform(string prefix, int type, string suffix)
		{
			this.prefix = ReadUniBytes(prefix);
			this.type = type;
			this.suffix = ReadUniBytes(suffix);
		}

		internal static byte[] ReadUniBytes(string uniBytes)
		{
			byte[] result = new byte[uniBytes.Length];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = unchecked((byte)uniBytes[i]);
			}
			return result;
		}

		internal static readonly Brotli.Transform[] Transforms = new Brotli.Transform[] { new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, string.Empty), new Brotli.Transform(string.Empty, 
			Brotli.WordTransformType.Identity, " "), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst1, string.Empty), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.UppercaseFirst, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " the "), new Brotli.Transform(" ", Brotli.WordTransformType.Identity
			, string.Empty), new Brotli.Transform("s ", Brotli.WordTransformType.Identity, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " of "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.UppercaseFirst, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " and "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst2, string.Empty), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.OmitLast1, string.Empty), new Brotli.Transform(", ", Brotli.WordTransformType.Identity, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity
			, ", "), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseFirst, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " in "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.Identity, " to "), new Brotli.Transform("e ", Brotli.WordTransformType.Identity, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "\""), new Brotli.Transform(string.Empty, 
			Brotli.WordTransformType.Identity, "."), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "\">"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "\n"), new 
			Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast3, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "]"), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.Identity, " for "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst3, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast2, string.Empty), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.Identity, " a "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " that "), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseFirst
			, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, ". "), new Brotli.Transform(".", Brotli.WordTransformType.Identity, string.Empty), new Brotli.Transform(" ", Brotli.WordTransformType
			.Identity, ", "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst4, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " with "), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.Identity, "'"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " from "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity
			, " by "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst5, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst6, string.Empty), new Brotli.Transform
			(" the ", Brotli.WordTransformType.Identity, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast4, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.Identity, ". The "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " on "), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.Identity, " as "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " is "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast7
			, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast1, "ing "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "\n\t"), new Brotli.Transform(string.Empty
			, Brotli.WordTransformType.Identity, ":"), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, ". "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "ed "), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.OmitFirst9, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitFirst7, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.OmitLast6, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "("), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst, ", "), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.OmitLast8, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, " at "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.Identity, "ly "), new Brotli.Transform(" the ", Brotli.WordTransformType.Identity, " of "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.OmitLast5, string.Empty), new Brotli.Transform(
			string.Empty, Brotli.WordTransformType.OmitLast9, string.Empty), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseFirst, ", "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst
			, "\""), new Brotli.Transform(".", Brotli.WordTransformType.Identity, "("), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.UppercaseFirst, "\">"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "=\""), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, "."), new Brotli.Transform(".com/", 
			Brotli.WordTransformType.Identity, string.Empty), new Brotli.Transform(" the ", Brotli.WordTransformType.Identity, " of the "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst
			, "'"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, ". This "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, ","), new Brotli.Transform(".", Brotli.WordTransformType
			.Identity, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst, "("), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst, "."), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.Identity, " not "), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, "=\""), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "er "
			), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseAll, " "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "al "), new Brotli.Transform(" ", Brotli.WordTransformType
			.UppercaseAll, string.Empty), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "='"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "\""), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.UppercaseFirst, ". "), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, "("), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, 
			"ful "), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseFirst, ". "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "ive "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.Identity, "less "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "'"), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "est "), new Brotli.Transform
			(" ", Brotli.WordTransformType.UppercaseFirst, "."), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "\">"), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, "='"
			), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst, ","), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity, "ize "), new Brotli.Transform(string.Empty, Brotli.WordTransformType
			.UppercaseAll, "."), new Brotli.Transform("\u00c2\u00a0", Brotli.WordTransformType.Identity, string.Empty), new Brotli.Transform(" ", Brotli.WordTransformType.Identity, ","), new Brotli.Transform(string.Empty
			, Brotli.WordTransformType.UppercaseFirst, "=\""), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "=\""), new Brotli.Transform(string.Empty, Brotli.WordTransformType.Identity
			, "ous "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, ", "), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseFirst, "='"), new Brotli.Transform(" ", 
			Brotli.WordTransformType.UppercaseFirst, ","), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseAll, "=\""), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseAll, ", "), new Brotli.Transform
			(string.Empty, Brotli.WordTransformType.UppercaseAll, ","), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "("), new Brotli.Transform(string.Empty, Brotli.WordTransformType.
			UppercaseAll, ". "), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseAll, "."), new Brotli.Transform(string.Empty, Brotli.WordTransformType.UppercaseAll, "='"), new Brotli.Transform(" ", Brotli.WordTransformType
			.UppercaseAll, ". "), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseFirst, "=\""), new Brotli.Transform(" ", Brotli.WordTransformType.UppercaseAll, "='"), new Brotli.Transform(" ", Brotli.WordTransformType
			.UppercaseFirst, "='") };

		internal static int TransformDictionaryWord(byte[] dst, int dstOffset, byte[] word, int wordOffset, int len, Brotli.Transform transform)
		{
			int offset = dstOffset;
			// Copy prefix.
			byte[] @string = transform.prefix;
			int tmp = @string.Length;
			int i = 0;
			// In most cases tmp < 10 -> no benefits from System.arrayCopy
			while (i < tmp)
			{
				dst[offset++] = @string[i++];
			}
			// Copy trimmed word.
			int op = transform.type;
			tmp = Brotli.WordTransformType.GetOmitFirst(op);
			if (tmp > len)
			{
				tmp = len;
			}
			wordOffset += tmp;
			len -= tmp;
			len -= Brotli.WordTransformType.GetOmitLast(op);
			i = len;
			while (i > 0)
			{
				dst[offset++] = word[wordOffset++];
				i--;
			}
			if (op == Brotli.WordTransformType.UppercaseAll || op == Brotli.WordTransformType.UppercaseFirst)
			{
				int uppercaseOffset = offset - len;
				if (op == Brotli.WordTransformType.UppercaseFirst)
				{
					len = 1;
				}
				while (len > 0)
				{
					tmp = dst[uppercaseOffset] & unchecked((int)(0xFF));
					if (tmp < unchecked((int)(0xc0)))
					{
						if (tmp >= 'a' && tmp <= 'z')
						{
							dst[uppercaseOffset] ^= unchecked((byte)32);
						}
						uppercaseOffset += 1;
						len -= 1;
					}
					else if (tmp < unchecked((int)(0xe0)))
					{
						dst[uppercaseOffset + 1] ^= unchecked((byte)32);
						uppercaseOffset += 2;
						len -= 2;
					}
					else
					{
						dst[uppercaseOffset + 2] ^= unchecked((byte)5);
						uppercaseOffset += 3;
						len -= 3;
					}
				}
			}
			// Copy suffix.
			@string = transform.suffix;
			tmp = @string.Length;
			i = 0;
			while (i < tmp)
			{
				dst[offset++] = @string[i++];
			}
			return offset - dstOffset;
		}
	}
}
