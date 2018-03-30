using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporter.YAML
{
	internal class Emitter
	{
		public Emitter(TextWriter writer)
		{
			if(writer == null)
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

		public Emitter Write(char @char)
		{
			WriteDelayed();
			m_stream.Write(@char);

			return this;
		}

		public Emitter Write(string @string)
		{
			if(@string != string.Empty)
			{
				WriteDelayed();
				m_stream.Write(@string);
			}
			
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

		private void WriteDelayed()
		{
			if(m_isNeedLineBreak)
			{
				m_stream.Write('\n');
				m_isNeedSeparator = false;
				m_isNeedWhitespace = false;
				m_isNeedLineBreak = false;
				WriteIndent();
			}
			if(m_isNeedSeparator)
			{
				m_stream.Write(',');
				m_isNeedSeparator = false;
			}
			if(m_isNeedWhitespace)
			{
				m_stream.Write(' ');
				m_isNeedWhitespace = false;
			}
		}

		private void WriteIndent()
		{
			for(int i = 0; i < m_indent * 2; i++)
			{
				m_stream.Write(' ');
			}
		}
		
		private readonly TextWriter m_stream;

		//public bool IsMultyline { get; set; } = true;

		private int m_indent = 0;
		private bool m_isNeedWhitespace = false;
		private bool m_isNeedSeparator = false;
		private bool m_isNeedLineBreak = false;
	}
}
