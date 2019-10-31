using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// DeprecatedLOD
	/// </summary>
	public struct LOD : IAsset
	{
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public static bool HasVertexCount(Version version) => version.IsLess(1, 5);

		public LOD Convert(IExportContainer container)
		{
			LOD instance = new LOD();
			instance.MeshData = MeshData.Select(t => t.Convert()).ToArray();
			if (HasVertexCount(container.ExportVersion))
			{
				instance.VertexCount = VertexCount;
				instance.NewVertexStart = NewVertexStart;
				instance.MeshError = MeshError;
				instance.MorphToVertex = MorphToVertex.ToArray();
			}
			return instance;
		}

		public void Read(AssetReader reader)
		{
			MeshData = reader.ReadAssetArray<MeshData>();
			if (HasVertexCount(reader.Version))
			{
				VertexCount = reader.ReadInt32();
				NewVertexStart = reader.ReadInt32();
				MeshError = reader.ReadSingle();
				MorphToVertex = reader.ReadUInt16Array();
			}
		}

		public void Write(AssetWriter writer)
		{
			MeshData.Write(writer);
			if (HasVertexCount(writer.Version))
			{
				writer.Write(VertexCount);
				writer.Write(NewVertexStart);
				writer.Write(MeshError);
				MorphToVertex.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MeshDataName, MeshData.ExportYAML(container));
			if (HasVertexCount(container.ExportVersion))
			{
				node.Add(VertexCountName, VertexCount);
				node.Add(NewVertexStartName, NewVertexStart);
				node.Add(MeshErrorName, MeshError);
				node.Add(MorphToVertexName, MorphToVertex.ExportYAML(false));
			}
			return node;
		}

		/// <summary>
		/// Array of submeshes
		/// </summary>
		public MeshData[] MeshData { get; set; }
		public int VertexCount { get; set; }
		public int NewVertexStart { get; set; }
		public float MeshError { get; set; }
		public ushort[] MorphToVertex { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string VertexCountName = "m_VertexCount";
		public const string NewVertexStartName = "m_NewVertexStart";
		public const string MeshErrorName = "m_MeshError";
		public const string MorphToVertexName = "m_MorphToVertex";
	}
}
