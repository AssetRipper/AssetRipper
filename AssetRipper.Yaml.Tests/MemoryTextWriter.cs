using System.IO;
using System.Text;

namespace AssetRipper.Yaml.Tests
{
	public sealed class MemoryTextWriter : TextWriter
	{
		private readonly StringBuilder sb = new();

		public override Encoding Encoding { get; } = Encoding.UTF8;

		public override void Write(char value)
		{
			sb.Append(value);
		}

		public void Clear() => sb.Clear();

		public override string ToString() => sb.ToString();
	}
}
