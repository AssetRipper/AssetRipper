using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Renderer
{
	public sealed class StaticBatchInfo : IStaticBatchInfo
	{
		public ushort FirstSubMesh { get; set; }
		public ushort SubMeshCount { get; set; }

		public const string FirstSubMeshName = "firstSubMesh";
		public const string SubMeshCountName = "subMeshCount";

		public StaticBatchInfo() { }
		public StaticBatchInfo(uint[] subsetIndices)
		{
			this.Initialize(subsetIndices);
		}

		public void Read(AssetReader reader)
		{
			FirstSubMesh = reader.ReadUInt16();
			SubMeshCount = reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			throw new System.NotImplementedException();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FirstSubMeshName, FirstSubMesh);
			node.Add(SubMeshCountName, SubMeshCount);
			return node;
		}
	}
}
