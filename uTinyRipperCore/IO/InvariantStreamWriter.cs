using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace uTinyRipper
{
	public class InvariantStreamWriter : StreamWriter
	{
		public InvariantStreamWriter(Stream stream):
			base(stream)
		{
		}

#if !NET_STANDARD
		public InvariantStreamWriter(string path):
			base(path)
		{
		}
#endif

		public InvariantStreamWriter(Stream stream, Encoding encoding) :
			base(stream, encoding)
		{
		}

#if !NET_STANDARD
		public InvariantStreamWriter(string path, bool append) :
			base(path, append)
		{
		}
#endif

		public InvariantStreamWriter(Stream stream, Encoding encoding, int bufferSize) :
			base(stream, encoding, bufferSize)
		{
		}

#if !NET_STANDARD
		public InvariantStreamWriter(string path, bool append, Encoding encoding) :
			base(path, append, encoding)
		{
		}
#endif

		public InvariantStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen) :
			base(stream, encoding, bufferSize, leaveOpen)
		{
		}

#if !NET_STANDARD
		public InvariantStreamWriter(string path, bool append, Encoding encoding, int bufferSize) :
			base(path, append, encoding, bufferSize)
		{
		}
#endif

		public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
	}
}
