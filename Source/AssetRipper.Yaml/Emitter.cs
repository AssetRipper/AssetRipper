using System.Buffers;

namespace AssetRipper.Yaml;

internal sealed class Emitter
{
	public Emitter(TextWriter writer, bool formatKeys)
	{
		ArgumentNullException.ThrowIfNull(writer);
		m_stream = writer;
		IsFormatKeys = formatKeys;
	}

	public Emitter IncreaseIndent()
	{
		m_indent++;
		return this;
	}

	public Emitter DecreaseIndent()
	{
		if (m_indent == 0)
		{
			throw new Exception($"Increase/decrease indent mismatch");
		}

		m_indent--;
		return this;
	}

	public Emitter Write(char value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter WriteRaw(char value)
	{
		m_stream.Write(value);
		return this;
	}

	/// <summary>
	/// Write a unicode character in the format \uXXXX.
	/// </summary>
	/// <remarks>
	/// Only used in <see cref="ScalarStyle.DoubleQuoted"/> strings.
	/// </remarks>
	/// <param name="value">The character to write.</param>
	/// <returns><see langword="this"/></returns>
	public Emitter WriteRawUnicode(char value)
	{
		m_stream.Write($"\\u{(ushort)value:X4}");
		return this;
	}

	public Emitter Write(byte value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(ushort value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(short value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(uint value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(int value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(ulong value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(long value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(float value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(double value)
	{
		WriteDelayed();
		m_stream.Write(value);
		return this;
	}

	public Emitter Write(ReadOnlySpan<char> value)
	{
		if (!value.IsEmpty)
		{
			WriteDelayed();
			m_stream.Write(value);
		}
		return this;
	}

	public Emitter WriteFormat(ReadOnlySpan<char> value)
	{
		if (value.Length > 0)
		{
			WriteDelayed();
			if (value.Length > 2 && value.StartsWith("m_", StringComparison.Ordinal))
			{
				int length = value.Length - 2;
				char[] buffer = ArrayPool<char>.Shared.Rent(length);

				value[2..].CopyTo(buffer);
				if (char.IsUpper(buffer[0]))
				{
					buffer[0] = char.ToLowerInvariant(buffer[0]);
				}
				m_stream.Write(buffer, 0, length);

				ArrayPool<char>.Shared.Return(buffer);
			}
			else
			{
				m_stream.Write(value);
			}
		}
		return this;
	}

	public Emitter WriteRaw(ReadOnlySpan<char> value)
	{
		m_stream.Write(value);
		return this;
	}

	public Emitter WriteClose(char @char)
	{
		m_isNeedSeparator = false;
		m_isNeedWhitespace = false;
		m_isNeedLineBreak = false;
		return Write(@char);
	}

	public Emitter WriteClose(ReadOnlySpan<char> @string)
	{
		m_isNeedSeparator = false;
		m_isNeedWhitespace = false;
		return Write(@string);
	}

	public Emitter WriteWhitespace()
	{
		m_isNeedWhitespace = true;
		return this;
	}

	public Emitter WriteSeparator()
	{
		m_isNeedSeparator = true;
		return this;
	}

	public Emitter WriteLine()
	{
		m_isNeedLineBreak = true;
		return this;
	}

	public void WriteMeta(MetaType type, string value)
	{
		Write('%').Write(type.ToStringRepresentation()).WriteWhitespace();
		Write(value).WriteLine();
	}

	public void WriteDelayed()
	{
		if (m_isNeedLineBreak)
		{
			m_stream.Write('\n');
			m_isNeedSeparator = false;
			m_isNeedWhitespace = false;
			m_isNeedLineBreak = false;
			WriteIndent();
		}
		if (m_isNeedSeparator)
		{
			m_stream.Write(',');
			m_isNeedSeparator = false;
		}
		if (m_isNeedWhitespace)
		{
			m_stream.Write(' ');
			m_isNeedWhitespace = false;
		}
	}

	private void WriteIndent()
	{
		if (m_indent > 0)
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThan(m_indent, 1000);
			Span<char> buffer = stackalloc char[m_indent * 2];
			buffer.Fill(' ');
			m_stream.Write(buffer);
		}
	}

	public bool IsFormatKeys { get; }
	internal bool IsKey { get; set; }

	private readonly TextWriter m_stream;

	private int m_indent = 0;
	private bool m_isNeedWhitespace = false;
	private bool m_isNeedSeparator = false;
	private bool m_isNeedLineBreak = false;
}
