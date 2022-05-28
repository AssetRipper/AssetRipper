using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class BufferBinding : IAssetReadable, IYamlExportable
	{
		public BufferBinding() { }

		public BufferBinding(string name, int index)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			if (HasArraySize(reader.Version))
			{
				ArraySize = reader.ReadInt32();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_Index", Index);
			if (HasArraySize(container.ExportVersion))
			{
				node.Add("m_ArraySize", ArraySize);
			}

			return node;
		}

		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasArraySize(UnityVersion version) => version.IsGreaterEqual(2020);

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
	}
}
