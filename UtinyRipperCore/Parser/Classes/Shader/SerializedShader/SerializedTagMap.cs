using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedTagMap : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_tags = new Dictionary<string, string>();

			m_tags.Read(stream);
		}

		public void Export(TextWriter writer, int intent)
		{
			if(Tags.Count != 0)
			{
				writer.WriteIntent(intent);
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
