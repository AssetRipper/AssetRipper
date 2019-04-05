using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedProperties : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_props = reader.ReadAssetArray<SerializedProperty>();
		}

		public void Export(TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach(SerializedProperty prop in Props)
			{
				prop.Export(writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		public IReadOnlyList<SerializedProperty> Props => m_props;

		private SerializedProperty[] m_props;
	}
}
