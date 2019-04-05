using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedTagMap : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_tags = new Dictionary<string, string>();

			m_tags.Read(reader);
		}

		public void Export(TextWriter writer, int intent)
		{
			if(Tags.Count != 0)
			{
				writer.WriteIndent(intent);
				writer.Write("Tags { ");
				foreach(var kvp in Tags)
				{
					writer.Write("\"{0}\" = \"{1}\" ", kvp.Key, kvp.Value);
				}
				writer.Write("}\n");
			}
		}

		public IReadOnlyDictionary<string, string> Tags => m_tags;

		private Dictionary<string, string> m_tags;
	}
}
