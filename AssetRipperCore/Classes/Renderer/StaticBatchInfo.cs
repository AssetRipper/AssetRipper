using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Renderer
{
	public sealed class StaticBatchInfo
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(FirstSubMeshName, FirstSubMesh);
			node.Add(SubMeshCountName, SubMeshCount);
			return node;
		}

		public void Initialize(uint[] subsetIndices)
		{
			if (subsetIndices.Length == 0)
			{
				FirstSubMesh = 0;
				SubMeshCount = 0;
			}
			else
			{
				FirstSubMesh = (ushort)subsetIndices[0];
				SubMeshCount = (ushort)subsetIndices.Length;
				for (int i = 0, j = FirstSubMesh; i < SubMeshCount; i++, j++)
				{
					if (subsetIndices[i] != j)
					{
						throw new Exception("Can't create static batch info from subset indices");
					}
				}
			}
		}
	}
}
