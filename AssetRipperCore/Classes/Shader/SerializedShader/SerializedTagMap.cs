using AssetRipper.Extensions;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Classes.Shader.SerializedShader
{
	public struct SerializedTagMap : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_tags = new Dictionary<string, string>();

			m_tags.Read(reader);
		}

		public void Export(TextWriter writer, int indent)
		{
			if (Tags.Count != 0)
			{
				writer.WriteIndent(indent);
				writer.Write("Tags { ");
				foreach (var kvp in Tags)
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
