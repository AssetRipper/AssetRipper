using System;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct VariableBoneCountWeights : IAsset
	{
		public VariableBoneCountWeights(bool _)
		{
			Data = Array.Empty<uint>();
		}

		public VariableBoneCountWeights Convert(IExportContainer container)
		{
			VariableBoneCountWeights instance = new VariableBoneCountWeights();
			instance.Data = Data.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			Data = reader.ReadUInt32Array();
		}

		public void Write(AssetWriter writer)
		{
			Data.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(DataName, Data.ExportYAML(true));
			return node;
		}

		public uint[] Data { get; set; }

		public const string DataName = "m_Data";
	}
}
