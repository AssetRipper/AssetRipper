using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtinyRipper.Exporter.YAML
{
	using Version = System.Version;

	public class YAMLWriter
	{
		public void AddDocument(YAMLDocument document)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			if (m_documents.Contains(document))
			{
				throw new ArgumentException($"Document {document} is added already", nameof(document));
			}
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
			Emitter emitter = new Emitter(output);

			bool isWriteSeparator = false;
			if(IsWriteVersion)
			{
				emitter.WriteMeta(MetaType.YAML, Version.ToString());
				isWriteSeparator = true;
			}

			if(IsWriteDefaultTag)
			{
				emitter.WriteMeta(MetaType.TAG, DefaultTag.ToHeaderString());
				isWriteSeparator = true;
			}
			foreach(YAMLTag tag in m_tags)
			{
				emitter.WriteMeta(MetaType.TAG, tag.ToHeaderString());
				isWriteSeparator = true;
			}

			foreach(YAMLDocument doc in m_documents)
			{
				doc.Emit(emitter, isWriteSeparator);
				isWriteSeparator = true;
			}
			output.Write('\n');
		}

		public static Version Version { get; } = new Version(1, 1);

		public const string DefaultTagHandle = "!u!";
		public const string DefaultTagContent = "tag:unity3d.com,2011:";

		public readonly YAMLTag DefaultTag = new YAMLTag(DefaultTagHandle, DefaultTagContent);

		public bool IsWriteVersion { get; set; } = true;
		public bool IsWriteDefaultTag { get; set; } = true;

		private readonly List<YAMLDocument> m_documents = new List<YAMLDocument>();
		private readonly List<YAMLTag> m_tags = new List<YAMLTag>();
	}
}
