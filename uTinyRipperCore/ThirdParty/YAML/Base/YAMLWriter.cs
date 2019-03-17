using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace uTinyRipper.YAML
{
	using Version = System.Version;

	public class YAMLWriter
	{
		public void AddDocument(YAMLDocument document)
		{
#if DEBUG
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			if (m_documents.Contains(document))
			{
				throw new ArgumentException($"Document {document} is added already", nameof(document));
			}
#endif
			m_documents.Add(document);
		}

		public void AddTag(string handle, string content)
		{
			if(m_tags.Any(t => t.Handle == handle))
			{
				throw new Exception($"Writer already contains tag {handle}");
			}
			YAMLTag tag = new YAMLTag(handle, content);
			m_tags.Add(tag);
		}

		public void Write(TextWriter output)
		{
			WriteHead(output);
			foreach (YAMLDocument doc in m_documents)
			{
				WriteDocument(doc);
			}
			WriteTail(output);
		}

		public void WriteHead(TextWriter output)
		{
			m_emitter = new Emitter(output);
			m_isWriteSeparator = false;

			if (IsWriteVersion)
			{
				m_emitter.WriteMeta(MetaType.YAML, Version.ToString());
				m_isWriteSeparator = true;
			}

			if (IsWriteDefaultTag)
			{
				m_emitter.WriteMeta(MetaType.TAG, DefaultTag.ToHeaderString());
				m_isWriteSeparator = true;
			}
			foreach (YAMLTag tag in m_tags)
			{
				m_emitter.WriteMeta(MetaType.TAG, tag.ToHeaderString());
				m_isWriteSeparator = true;
			}
		}

		public void WriteDocument(YAMLDocument doc)
		{
			doc.Emit(m_emitter, m_isWriteSeparator);
			m_isWriteSeparator = true;
		}

		public void WriteTail(TextWriter output)
		{
			output.Write('\n');
		}

		public static Version Version { get; } = new Version(1, 1);

		public const string DefaultTagHandle = "!u!";
		public const string DefaultTagContent = "tag:unity3d.com,2011:";

		public readonly YAMLTag DefaultTag = new YAMLTag(DefaultTagHandle, DefaultTagContent);

		public bool IsWriteVersion { get; set; } = true;
		public bool IsWriteDefaultTag { get; set; } = true;

		private readonly HashSet<YAMLDocument> m_documents = new HashSet<YAMLDocument>();
		private readonly List<YAMLTag> m_tags = new List<YAMLTag>();

		private Emitter m_emitter;
		private bool m_isWriteSeparator;
	}
}
