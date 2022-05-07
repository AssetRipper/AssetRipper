﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class UAVParameter : IAssetReadable, IYamlExportable
	{
		public UAVParameter() { }

		public UAVParameter(string name, int index, int originalIndex)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			OriginalIndex = originalIndex;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			OriginalIndex = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_Index", Index);
			node.Add("m_OriginalIndex", OriginalIndex);
			return node;
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int OriginalIndex { get; set; }
	}
}
