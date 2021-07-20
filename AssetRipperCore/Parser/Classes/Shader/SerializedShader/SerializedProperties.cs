using AssetRipper.Extensions;
using AssetRipper.Parser.IO.Asset.Reader;
using System.IO;

namespace AssetRipper.Parser.Classes.Shader.SerializedShader
{
	public struct SerializedProperties : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Props = reader.ReadAssetArray<SerializedProperty>();
		}

		public void Export(TextWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("Properties {\n");
			foreach (SerializedProperty prop in Props)
			{
				prop.Export(writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		public SerializedProperty[] Props { get; set; }
	}
}
