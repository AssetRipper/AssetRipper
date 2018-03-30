using System.Diagnostics.Contracts;

namespace UtinyRipper.Exporter.YAML
{
	public readonly struct YAMLTag
	{
		public YAMLTag(string handle, string content)
		{
			Handle = handle;
			Content = content;
		}

		public override string ToString()
		{
			return IsEmpty ? string.Empty : $"{Handle}{Content}";
		}

		[Pure]
		public string ToHeaderString()
		{
			return IsEmpty ? string.Empty : $"{Handle} {Content}";
		}

		public bool IsEmpty => string.IsNullOrEmpty(Handle);

		public string Handle { get; }
		public string Content { get; }
	}
}
