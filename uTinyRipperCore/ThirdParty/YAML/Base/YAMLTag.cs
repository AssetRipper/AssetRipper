namespace uTinyRipper.YAML
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

		public string ToHeaderString()
		{
			return IsEmpty ? string.Empty : $"{Handle} {Content}";
		}

		public bool IsEmpty => string.IsNullOrEmpty(Handle);

		public string Handle { get; }
		public string Content { get; }
	}
}
