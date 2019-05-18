using System;
using System.IO;

namespace uTinyRipper.YAML
{
	internal class Emitter
	{
		public Emitter(TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			m_stream = writer;
		}

		public Emitter IncreaseIntent()
		{
			m_indent++;
			return this;
		}

		public Emitter DecreaseIntent()
		{
			if (m_indent == 0)
			{
				throw new Exception($"Increase/decrease intent mismatch");
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

		public Emitter Write(string value)
		{
			if (value != string.Empty)
			{
				WriteDelayed();
				m_stream.Write(value);
			}
			return this;
		}

		public Emitter WriteRaw(string value)
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

		public Emitter WriteClose(string @string)
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
			Write("%").Write(type.ToString()).WriteWhitespace();
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
			for (int i = 0; i < m_indent * 2; i++)
			{
				m_stream.Write(' ');
			}
		}

		private readonly TextWriter m_stream;

		private int m_indent = 0;
		private bool m_isNeedWhitespace = false;
		private bool m_isNeedSeparator = false;
		private bool m_isNeedLineBreak = false;
	}
}
