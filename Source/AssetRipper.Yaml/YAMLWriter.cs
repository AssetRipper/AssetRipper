namespace AssetRipper.Yaml;

public class YamlWriter
{
	public void AddDocument(YamlDocument document)
	{
#if DEBUG
		ArgumentNullException.ThrowIfNull(document);
		if (m_documents.Contains(document))
		{
			throw new ArgumentException($"Document {document} is added already", nameof(document));
		}
#endif
		m_documents.Add(document);
	}

	public void AddTag(string handle, string content)
	{
		if (m_tags.Any(t => t.Handle == handle))
		{
			throw new Exception($"Writer already contains tag {handle}");
		}

		YamlTag tag = new YamlTag(handle, content);
		m_tags.Add(tag);
	}

	public void Write(TextWriter output)
	{
		WriteHead(output);
		foreach (YamlDocument doc in m_documents)
		{
			WriteDocument(doc);
		}

		WriteTail(output);
	}

	public void WriteHead(TextWriter output)
	{
		m_emitter = new Emitter(output, IsFormatKeys);
		m_isWriteSeparator = false;

		if (IsWriteVersion)
		{
			m_emitter.WriteMeta(MetaType.Yaml, Version.ToString());
			m_isWriteSeparator = true;
		}

		if (IsWriteDefaultTag)
		{
			m_emitter.WriteMeta(MetaType.Tag, DefaultTag.ToHeaderString());
			m_isWriteSeparator = true;
		}
		foreach (YamlTag tag in m_tags)
		{
			m_emitter.WriteMeta(MetaType.Tag, tag.ToHeaderString());
			m_isWriteSeparator = true;
		}
	}

	public void WriteDocument(YamlDocument doc)
	{
		ThrowIfNullEmitter();
		doc.Emit(m_emitter, m_isWriteSeparator);
		m_isWriteSeparator = true;
	}

	public void WriteTail(TextWriter output)
	{
		output.Write('\n');
	}

	[MemberNotNull(nameof(m_emitter))]
	private void ThrowIfNullEmitter()
	{
		if (m_emitter is null)
		{
			throw new NullReferenceException("Emitter cannot be null");
		}
	}

	public static Version Version { get; } = new Version(1, 1);

	public const string DefaultTagHandle = "!u!";
	public const string DefaultTagContent = "tag:unity3d.com,2011:";

	public readonly YamlTag DefaultTag = new YamlTag(DefaultTagHandle, DefaultTagContent);

	public bool IsWriteVersion { get; set; } = true;
	public bool IsWriteDefaultTag { get; set; } = true;
	public bool IsFormatKeys { get; set; }

	private readonly HashSet<YamlDocument> m_documents = [];
	private readonly List<YamlTag> m_tags = [];

	private Emitter? m_emitter;
	private bool m_isWriteSeparator;
}
