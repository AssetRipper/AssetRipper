using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class ConstantBuffer : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasStructParams(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		/// <summary>
		/// If on 2021, 2021.1.4 and greater. Otherwise, 2020.3.2 and greater.
		/// Not present in 2021.1.0 - 2021.1.3
		/// </summary>
		public static bool HasIsPartialCB(UnityVersion version) => version.Major == 2021 ? version.IsGreaterEqual(2021, 1, 4) : version.IsGreaterEqual(2020, 3, 2);

		public ConstantBuffer() { }

		public ConstantBuffer(string name, MatrixParameter[] matrices, VectorParameter[] vectors, StructParameter[] structs, int usedSize)
		{
			Name = name;
			NameIndex = -1;
			MatrixParams = matrices;
			VectorParams = vectors;
			StructParams = structs;
			Size = usedSize;
			IsPartialCB = false;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			MatrixParams = reader.ReadAssetArray<MatrixParameter>();
			VectorParams = reader.ReadAssetArray<VectorParameter>();
			if (HasStructParams(reader.Version))
			{
				StructParams = reader.ReadAssetArray<StructParameter>();
			}

			Size = reader.ReadInt32();
			if (HasIsPartialCB(reader.Version))
			{
				IsPartialCB = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_MatrixParams", MatrixParams.ExportYaml(container));
			node.Add("m_VectorParams", VectorParams.ExportYaml(container));
			if (HasStructParams(container.ExportVersion))
			{
				node.Add("m_StructParams", StructParams.ExportYaml(container));
			}

			node.Add("m_Size", Size);
			if (HasIsPartialCB(container.ExportVersion))
			{
				node.Add("m_IsPartialCB", IsPartialCB);
			}

			return node;
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public VectorParameter[] VectorParams { get; set; }
		public StructParameter[] StructParams { get; set; }
		public int Size { get; set; }
		public bool IsPartialCB { get; set; }
	}
}
