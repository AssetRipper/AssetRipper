using System.Globalization;
using System.Text;

namespace AssetRipper.Export.Modules.Shaders.IO;

public class InvariantStreamWriter : StreamWriter
{
	public InvariantStreamWriter(Stream stream) : base(stream) { }

	public InvariantStreamWriter(string path) : base(path) { }

	public InvariantStreamWriter(Stream stream, Encoding encoding) : base(stream, encoding) { }

	public InvariantStreamWriter(string path, bool append) : base(path, append) { }

	public InvariantStreamWriter(Stream stream, Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize) { }

	public InvariantStreamWriter(string path, bool append, Encoding encoding) : base(path, append, encoding) { }

	public InvariantStreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen) : base(stream, encoding, bufferSize, leaveOpen) { }

	public InvariantStreamWriter(string path, bool append, Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize) { }

	public sealed override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
}
